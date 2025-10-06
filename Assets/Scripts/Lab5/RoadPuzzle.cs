using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[System.Serializable]
public struct ArrayWrapper<T> : IList<T>
{
    [SerializeReference] private T[] array;
    public int Count => ((ICollection<T>)array).Count;

    public bool IsReadOnly => ((ICollection<T>)array).IsReadOnly;

    public T this[int index]
    {
        set => array[index] = value;
        get => array[index];
    }
    public ArrayWrapper(T[] array)
    {
        this.array = array;
    }

    public int IndexOf(T item)
    {
        return ((IList<T>)array).IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        ((IList<T>)array).Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        ((IList<T>)array).RemoveAt(index);
    }

    public void Add(T item)
    {
        ((ICollection<T>)array).Add(item);
    }

    public void Clear()
    {
        ((ICollection<T>)array).Clear();
    }

    public bool Contains(T item)
    {
        return ((ICollection<T>)array).Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ((ICollection<T>)this.array).CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return ((ICollection<T>)array).Remove(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)array).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return array.GetEnumerator();
    }
}

public class RoadPuzzle : MonoBehaviour
{
    [SerializeField] private StartCell startPrefab;
    [SerializeField] private EndCell endPrefab;
    [SerializeField] private PuzzleSquareCell emptyPrefab;
    [SerializeField] private PuzzleSquareCell[] buildingPrefabs;
    [SerializeField] private int length = 5;
    [SerializeField] private int height = 5;
    [SerializeField, HideInInspector] private ArrayWrapper<PuzzleSquareCell>[] matrixCells;

    [System.NonSerialized] private StartCell start;
    [System.NonSerialized] private EndCell end;

    public RoadPuzzle()
    {
        matrixCells = new ArrayWrapper<PuzzleSquareCell>[height];
        for (int i = 0; i < length; i++)
            matrixCells[i] = new ArrayWrapper<PuzzleSquareCell>(new PuzzleSquareCell[length]);
    }
    // Start is called before the first frame update
    void Start()
    {
        SolutionBuilder(10);
        matrixCells = SimulateAnnealing(matrixCells, 10, 1, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
            if (hit.collider != null)
            {
                if (hit.collider.TryGetComponent(out PuzzleSquareCell cell))
                {
                    cell.GetComponent<SpriteRenderer>().color = Color.black;
                }
            }
        }
    }

    private void Swap(PuzzleSquareCell first, PuzzleSquareCell second)
    {
        (int i, int j) x = IndexOf(first);
        (int i, int j) y = IndexOf(second);

        (matrixCells[x.i][x.j], matrixCells[y.i][y.j]) = (matrixCells[y.i][y.j], matrixCells[x.i][x.j]);
        (second.transform.position, first.transform.position) = (first.transform.position, second.transform.position);
    }

    private void SolutionBuilder(int pathLength)
    {
        (int y, int x) coords = (Random.Range(0, height), Random.Range(0, length));


        bool IsConnected(PuzzleSquareCell previous, PuzzleSquareCell next)
        {
            bool[] previousConnections = previous.GetConnections();
            bool[] nextConnections = next.GetConnections();

            (int y, int x) p = IndexOf(previous);
            (int y, int x) n = coords;

            Debug.Log(p + " - " + n);
            (int dy, int dx) = (Mathf.Abs(p.y - n.y), Mathf.Abs(p.x - n.x));

            if (dx == dy)
                return false;

            if (p.x < n.x)
                return previousConnections[2] && nextConnections[0];

            if (p.x > n.x)
                return previousConnections[0] && nextConnections[2];

            if (p.y < n.y)
                return previousConnections[1] && nextConnections[3];

            if (p.y > n.y)
                return previousConnections[3] && nextConnections[1];

            return false;
        }
        bool InvalidCell(PuzzleSquareCell current)
        {

            Debug.Log(matrixCells[0][0].GetType());
            bool[] connections = current.GetConnections();
            bool invalidLeftMove = connections[0] && (coords.x - 1 < 0);
            bool invalidUpMove = connections[1] && (coords.y + 1 >= height);
            bool invalidRightMove = connections[2] && (coords.x + 1 >= length);
            bool invalidDownMove = connections[3] && (coords.y - 1 < 0);
            return invalidLeftMove || invalidUpMove || invalidRightMove || invalidDownMove;
        }

        bool InvalidCoord((int y, int x) coord)
        {
            bool invalidLeft = coord.x < 0;
            bool invalidUp = coord.y >= height;
            bool invalidRight = coord.x >= length;
            bool invalidDown = coord.y < 0;

            return invalidLeft || invalidUp || invalidRight || invalidDown || matrixCells[coord.y][coord.x] is not EmptyCell;
        }

        bool IsDeadEnd(PuzzleSquareCell cell)
        {
            bool condition = true;
            bool[] connections = cell.GetConnections();

            if (coords.x > 0)
                condition &= !connections[0] || matrixCells[coords.y][coords.x - 1] is not EmptyCell;
            if (coords.y > 0)
                condition &= !connections[3] || matrixCells[coords.y - 1][coords.x] is not EmptyCell;
            if (coords.x < length - 1)
                condition &= !connections[2] || matrixCells[coords.y][coords.x + 1] is not EmptyCell;
            if (coords.y < height - 1)
                condition &= !connections[1] || matrixCells[coords.y + 1][coords.x] is not EmptyCell;

            return condition;
        }

        void Advance(PuzzleSquareCell instance)
        {
            bool[] connections = instance.GetConnections();

            List<(int y, int x)> dirs = new List<(int y, int x)>();
            for (int i = 0; i < 4; i++)
            {
                if (connections[i])
                {
                    (int y, int x) dir = (0, 0);
                    switch (i)
                    {
                        case 0: dir = (0, -1); break;
                        case 1: dir = (1, 0); break;
                        case 2: dir = (0, 1); break;
                        case 3: dir = (-1, 0); break;
                    }
                    dirs.Add(dir);
                }
            }

            (int y, int x) newCoord;

            List<int> indices = new List<int>();
            for (int i = 0; i < dirs.Count; i++)
                indices.Add(i);

            do
            {
                int i = indices[Random.Range(0, indices.Count)];
                newCoord = (coords.y + dirs[i].y, coords.x + dirs[i].x);
                indices.Remove(i);
            } while (InvalidCoord(newCoord) && indices.Count > 0);

            coords = newCoord;
        }

        StartCell PutStart()
        {
            PuzzleSquareCell old = matrixCells[coords.y][coords.x];
            StartCell instance = Instantiate(startPrefab, old.transform.position,
                old.transform.rotation, transform);

            int count = 0;
            while (InvalidCell(instance) && count < 4)
            {
                instance.Rotate90Degrees();
                count++;
            }
            instance.name = old.name;
            matrixCells[coords.y][coords.x] = instance;
            Destroy(old.gameObject);
            Advance(instance);
            return instance;
        }

        PuzzleSquareCell PutRoad(PuzzleSquareCell previous)
        {
            List<int> indices = new List<int>();

            for (int j = 0; j < buildingPrefabs.Length; j++)
                indices.Add(j);

            int i = indices.ElementAt(Random.Range(0, indices.Count));

            PuzzleSquareCell old = matrixCells[coords.y][coords.x];
            PuzzleSquareCell instance = Instantiate(buildingPrefabs[i], old.transform.position,
                old.transform.rotation, transform);

            int rotCount = 0;
            while (InvalidCell(instance) || IsDeadEnd(instance) || !IsConnected(previous, instance))
            {
                if (rotCount < 4)
                {
                    instance.Rotate90Degrees();
                    rotCount++;
                }
                else
                {
                    Destroy(instance.gameObject);
                    //options.Add(instance);
                    indices.Remove(i);
                    if (indices.Count == 0)
                    {
                        instance = null;
                        break;
                    }
                    i = indices[Random.Range(0, indices.Count)];
                    instance = Instantiate(buildingPrefabs[i], old.transform.position,
                        old.transform.rotation, transform);
                    rotCount = 0;
                }
            }


            if (instance != null)
            {
                int count = 0;
                while ((IsDeadEnd(instance) || !IsConnected(previous, instance)) && count < 4)
                {
                    instance.Rotate90Degrees();
                    count++;
                }
                instance.name = old.name;
                matrixCells[coords.y][coords.x] = instance;
                Destroy(old.gameObject);
            }

            return instance;
        }

        EndCell PutEnd(PuzzleSquareCell previous)
        {
            List<int> indices = new List<int>();

            for (int j = 0; j < buildingPrefabs.Length; j++)
                indices.Add(j);

            int i = indices.ElementAt(Random.Range(0, indices.Count));

            PuzzleSquareCell old = matrixCells[coords.y][coords.x];
            EndCell instance = Instantiate(endPrefab, old.transform.position,
                old.transform.rotation, transform);

            int count = 0;
            while (!IsConnected(previous, instance) && count < 4)
            {
                instance.Rotate90Degrees();
                count++;
            }
            instance.name = old.name;
            matrixCells[coords.y][coords.x] = instance;
            Destroy(old.gameObject);

            return instance;
        }

        start = PutStart();
        PuzzleSquareCell previous = start;
        int count = 0;
        int errorThreshHold = 0;
        while (count < pathLength && errorThreshHold < 500f)
        {
            PuzzleSquareCell instance = PutRoad(previous);
            if (instance != null)
            {
                Advance(instance);
                previous = instance;
                count++;
            }
            errorThreshHold++;
        }
        end = PutEnd(previous);
    }


    private bool TryGetConnection((int i, int j) first, (int i, int j) second, out int index)
    {
        index = -1;
        (int i, int j) delta = (first.i - second.i, first.j - second.j);

        (int i, int j) absDelta = (Mathf.Abs(delta.i), Mathf.Abs(delta.j));

        if (absDelta.i > 1 || absDelta.j > 1 || delta.i == delta.j)
            return false;

        if (delta.j < 0)
            index = 0;
        else if (delta.i < 0)
            index = 1;
        else if (delta.j > 0)
            index = 2;
        else if (delta.i > 0)
            index = 3;

        if (index == -1)
            return false;

        return true;
    }
    public IEnumerable<PuzzleSquareCell> GetNeighbors(PuzzleSquareCell cell)
    {
        (int y, int x) = IndexOf(cell);

        if (x > 0)
            yield return matrixCells[y][x - 1];
        if (y > 0)
            yield return matrixCells[y - 1][x];
        if (x < length - 1)
            yield return matrixCells[y][x + 1];
        if (y < height - 1)
            yield return matrixCells[y + 1][x];
    }
    private (int y, int x) IndexOf(PuzzleSquareCell cell)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < length; x++)
            {
                if (matrixCells[y][x] == cell)
                    return (y, x);
            }
        }

        return (-1, -1);
    }
    private void Rebuild()
    {
        ArrayWrapper<PuzzleSquareCell>[] copy = new ArrayWrapper<PuzzleSquareCell>[height];

        for (int i = 0; i < matrixCells.Length; i++)
        {
            PuzzleSquareCell[] aux = new PuzzleSquareCell[length];
            matrixCells[i].CopyTo(aux, 0);
            copy[i] = new ArrayWrapper<PuzzleSquareCell>(aux);
        }

        for (int i = matrixCells.Length; i < height; i++)
            copy[i] = new ArrayWrapper<PuzzleSquareCell>(new PuzzleSquareCell[length]);

        matrixCells = copy;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < length; x++)
            {
                if (matrixCells[y][x] == null)
                {
                    PuzzleSquareCell cell = Instantiate(emptyPrefab, transform);
                    cell.name = "Cell (" + y + ", " + x + ")";
                    cell.transform.position = (Vector2)transform.position + new Vector2(x, y) * emptyPrefab.transform.localScale;
                    matrixCells[y][x] = cell;
                }
            }
        }
    }

    public ArrayWrapper<PuzzleSquareCell>[] CloneMatrix(ArrayWrapper<PuzzleSquareCell>[] source)
    {
        ArrayWrapper<PuzzleSquareCell>[] copy = new ArrayWrapper<PuzzleSquareCell>[height];
        for (int y = 0; y < height; y++)
        {
            PuzzleSquareCell[] rowCopy = new PuzzleSquareCell[length];
            for (int x = 0; x < length; x++)
            {
                rowCopy[x] = source[y][x];
            }
            copy[y] = new ArrayWrapper<PuzzleSquareCell>(rowCopy);
        }
        return copy;
    } 

    public ArrayWrapper<PuzzleSquareCell>[] SimulateAnnealing(ArrayWrapper<PuzzleSquareCell>[] startingSolution, float startingTemperature, float minTemperature, float coolingRate)
    {
        var currentSolution = startingSolution;
        float currentValue = FitnessFunction(currentSolution);
        float temperature = startingTemperature;

        while (temperature > minTemperature)
        {
            var neighbor = GenerateNeighbor(currentSolution);
            float neighborValue = FitnessFunction(neighbor);

            float deltaE = neighborValue - currentValue;

            if (deltaE > 0)
            {
                currentSolution = neighbor;
                currentValue = neighborValue;
            }
            else
            {
                float prob = Mathf.Exp(deltaE / temperature);
                if (Random.Range(0, 1) < prob)
                {
                    currentSolution = neighbor;
                    currentValue = neighborValue;
                }
            }
            temperature = temperature * coolingRate;

            Debug.Log(temperature + "° enfriando...");
        }
        return currentSolution;
    }

    private (bool solvable, int lenght, int variety) EvaluatePath(ArrayWrapper<PuzzleSquareCell>[] state)
    {
        (int y, int x) IndexOfInState(PuzzleSquareCell cell)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    if (state[y][x] == cell)
                        return (y, x);
                }
            }

            return (-1, -1);
        }
        IEnumerable<PuzzleSquareCell> GetStateNeighbors(PuzzleSquareCell cell)
        {
            (int y, int x) = IndexOfInState(cell);
            if (x > 0)
                yield return state[y][x - 1];
            if (y > 0)
                yield return state[y - 1][x];
            if (x < length - 1)
                yield return state[y][x + 1];
            if (y < height - 1)
                yield return state[y + 1][x];
        }

        PuzzleSquareCell start = null;
        PuzzleSquareCell end = null;

        for(int y = 0; y < state.Length; y++)
        {
            for(int x = 0; x < state[y].Count; x++)
            {
                if (state[y][x] is StartCell)
                    start = state[y][x];
                else if (state[y][x] is EndCell)
                    end = state[y][x];

                if (start != null && end != null)
                    break;
            }
        }

        bool AreConnected(PuzzleSquareCell previous, PuzzleSquareCell next)
        {
            bool[] previousConnections = previous.GetConnections();
            bool[] nextConnections = next.GetConnections();

            (int y, int x) p = IndexOfInState(previous);
            (int y, int x) n = IndexOfInState(next);

            (int dy, int dx) = (Mathf.Abs(p.y - n.y), Mathf.Abs(p.x - n.x));

            if (dx == dy || dx > 1 || dy > 1)
                return false;

            if (p.x < n.x)
                return previousConnections[2] && nextConnections[0];

            if (p.x > n.x)
                return previousConnections[0] && nextConnections[2];

            if (p.y < n.y)
                return previousConnections[1] && nextConnections[3];

            if (p.y > n.y)
                return previousConnections[3] && nextConnections[1];

            return false;
        }

        if (start == null || end == null) return (false, 0, 0);

        Queue<(PuzzleSquareCell cell, int distance, int pathValue)> queue = new Queue<(PuzzleSquareCell, int, int)>();
        HashSet<PuzzleSquareCell> visited = new HashSet<PuzzleSquareCell>();

        queue.Enqueue((start, 0, start.value));
        visited.Add(start);

        while(queue.Count > 0)
        {
            //Debug.Log("Sigo aqui");
            var (current, dist, pathValue) = queue.Dequeue();

            if (current == end) return (true, dist, pathValue);

            foreach (var neighbor in GetStateNeighbors(current))
            {
                if(visited.Contains(neighbor)) continue;

                if (AreConnected(current, neighbor))
                {
                    queue.Enqueue((neighbor, dist+1, pathValue + neighbor.value)); 
                    visited.Add(neighbor);
                }
            }
        }

        return (false, 0, 0);
    }

    private float FitnessFunction(ArrayWrapper<PuzzleSquareCell>[] subject)
    {
        var result = EvaluatePath(subject);
        if(!result.solvable) return -1;

        float lenghtScore = result.lenght;
        float varietyScore = result.variety;

        float fitness = lenghtScore + varietyScore;
        Debug.Log("Fitness: " + fitness);

        return fitness;
    }

    private ArrayWrapper<PuzzleSquareCell>[] GenerateNeighbor(ArrayWrapper<PuzzleSquareCell>[] subject)
    {
        var newState = CloneMatrix(subject);

        int y = Random.Range(0, height);
        int x = Random.Range(0, length);
        PuzzleSquareCell chosen = newState[y][x];

        var neighbors = GetNeighbors(chosen).ToList();
        if(neighbors.Count > 0)
        {
            PuzzleSquareCell target = neighbors[Random.Range(0, neighbors.Count)];

            (int i, int j) a = IndexOf(chosen);
            (int i, int j) b = IndexOf(target);

            (newState[a.i][a.j], newState[b.i][b.j]) = (newState[b.i][b.j], newState[a.i][a.j]);
        }

        return newState;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RoadPuzzle)), CanEditMultipleObjects]
    public class MyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button(new GUIContent("Rebuild")))
            {
                RoadPuzzle square = (RoadPuzzle)target;
                square.Rebuild();
            }
        }
    }
#endif
}
