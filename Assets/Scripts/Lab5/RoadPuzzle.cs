using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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

public class RoadPuzzle : MonoBehaviour, IEvolutionaryStrategy<PuzzleCellRedux[,]>
{
    [System.Serializable]
    public enum Mode
    {
        NONE = -1,
        NORMAL = 0,
        EVOLUTIONARY = 1,
        EVSEMPERTINE = 2
    }

    [SerializeField] private StartCell startPrefab;
    [SerializeField] private EndCell endPrefab;
    [SerializeField] private PuzzleCell emptyPrefab;
    [SerializeField] private PuzzleCell[] buildingPrefabs;
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField, HideInInspector] private ArrayWrapper<PuzzleCell>[] matrixCells;
    [SerializeField] private RoadPuzzle annealing;
    [SerializeField] private int pathLength = 10;


    [SerializeField] private Mode mode = Mode.EVOLUTIONARY;
    [SerializeField] private bool executeAnnealing = true;
    //Evolution Strategy
    private int maxPopulation = 25;
    public int MaxPopulation => maxPopulation;

    private int maxIterations = 15;
    private float presition = 0.75f;

    //Simulated Annealing
    private float startingTemp = 100;
    private float minTemp = 1;
    private float coolingRate = 0.75f;
    [SerializeField] private Slider coolingSlider;
    [SerializeField] private TextMeshProUGUI coolingText;

    public RoadPuzzle()
    {
        matrixCells = new ArrayWrapper<PuzzleCell>[height];
        for (int i = 0; i < width; i++)
            matrixCells[i] = new ArrayWrapper<PuzzleCell>(new PuzzleCell[width]);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (annealing != null)
        {
            switch (mode)
            {
                case Mode.EVSEMPERTINE:
                case Mode.EVOLUTIONARY:  LoadFromModel(this.GenerateBestIndividual(maxIterations)); break;
                case Mode.NORMAL: LoadFromModel(BuildSolutionModel(maxIterations)); break;
            }

            ArrayWrapper<PuzzleCell>[] matrix = executeAnnealing && mode != Mode.NONE ?
                SimulateAnnealing(matrixCells, startingTemp, minTemp, coolingRate) : CloneMatrix(matrixCells);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if(annealing.matrixCells[y][x] is EmptyCell)
                        Destroy(annealing.matrixCells[y][x]);
                    PuzzleCell clone = Instantiate(matrix[y][x], annealing.transform);
                    clone.transform.position = (Vector2)annealing.transform.position
                        + new Vector2(x, y) * emptyPrefab.transform.localScale;
                }
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        //    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
        //    if (hit.collider != null)
        //    {
        //        if (hit.collider.TryGetComponent(out PuzzleSquareCell cell))
        //        {
        //            cell.GetComponent<SpriteRenderer>().color = Color.black;
        //        }
        //    }
        //}
    }

    private void Swap(PuzzleCell first, PuzzleCell second)
    {
        (int i, int j) x = IndexOf(first);
        (int i, int j) y = IndexOf(second);

        (matrixCells[x.i][x.j], matrixCells[y.i][y.j]) = (matrixCells[y.i][y.j], matrixCells[x.i][x.j]);
        (second.transform.position, first.transform.position) = (first.transform.position, second.transform.position);
    }
    
    private void LoadFromModel(PuzzleCellRedux[,] board)
    {
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                PuzzleCell prefab = null;

                switch (board[y, x].Tag) 
                {
                    case CellTag.Start: prefab = startPrefab; break;
                    case CellTag.End: prefab = endPrefab; break;
                    case CellTag.Empty: prefab = emptyPrefab; break;
                    case CellTag.Filled: prefab = buildingPrefabs[board[y, x].PrefabIndex]; break;
                }

                PuzzleCell cell = Instantiate(prefab, matrixCells[y][x].transform.position, matrixCells[y][x].transform.rotation,
                    transform);
                
                for (int i = 0; i < Mathf.Abs(board[y, x].RotCount); i++)
                    cell.Rotate90Degrees();
                cell.name = board[y, x].Name;
                Destroy(matrixCells[y][x].gameObject);
                matrixCells[y][x] = cell;
            }
        }
    }

    private (int y, int x) IndexOfBoard(PuzzleCellRedux[,] board, PuzzleCellRedux cell)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (board[y, x] == cell)
                    return (y, x);
            }
        }

        return (-1, -1);
    }

    private PuzzleCellRedux[,] BuildSolutionModel(int pathLength)
    {
        PuzzleCellRedux[,] board = new PuzzleCellRedux[height, width];

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
                board[y, x] = new PuzzleCellRedux(matrixCells[y][x], -1);
        }



        (int y, int x) coords = (Random.Range(0, height), Random.Range(0, width));

        bool IsConnected(PuzzleCellRedux previous, PuzzleCellRedux next)
        {
            bool[] previousConnections = previous.GetConnections();
            bool[] nextConnections = next.GetConnections();

            (int y, int x) p = IndexOfBoard(board, previous);
            (int y, int x) n = coords;
            Debug.Log(p);
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
        
        bool InvalidCell(PuzzleCellRedux current)
        {
            bool[] connections = current.GetConnections();
            bool invalidLeftMove = connections[0] && (coords.x - 1 < 0);
            bool invalidUpMove = connections[1] && (coords.y + 1 >= height);
            bool invalidRightMove = connections[2] && (coords.x + 1 >= width);
            bool invalidDownMove = connections[3] && (coords.y - 1 < 0);
            return invalidLeftMove || invalidUpMove || invalidRightMove || invalidDownMove;
        }

        bool InvalidCoord((int y, int x) coord)
        {
            bool invalidLeft = coord.x < 0;
            bool invalidUp = coord.y >= height;
            bool invalidRight = coord.x >= width;
            bool invalidDown = coord.y < 0;

            return invalidLeft || invalidUp || invalidRight || invalidDown || board[coord.y, coord.x].Tag != CellTag.Empty;
        }

        bool IsDeadEnd(PuzzleCellRedux cell)
        {
            bool condition = true;
            bool[] connections = cell.GetConnections();

            if (coords.x > 0)
                condition &= !connections[0] || board[coords.y, coords.x - 1].Tag != CellTag.Empty;
            if (coords.y > 0)
                condition &= !connections[3] || board[coords.y - 1, coords.x].Tag != CellTag.Empty;
            if (coords.x < width - 1)
                condition &= !connections[2] || board[coords.y, coords.x + 1].Tag != CellTag.Empty;
            if (coords.y < height - 1)
                condition &= !connections[1] || board[coords.y + 1, coords.x].Tag != CellTag.Empty;

            return condition;
        }

        void Advance(PuzzleCellRedux instance)
        {
            Debug.Log(instance.Name);
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
            Debug.Log(indices.Count);
            do
            {
                int i = indices[Random.Range(0, indices.Count)];
                newCoord = (coords.y + dirs[i].y, coords.x + dirs[i].x);
                indices.Remove(i);
            } while (InvalidCoord(newCoord) && indices.Count > 0);

            coords = newCoord;
        }

        PuzzleCellRedux PutStart()
        {
            PuzzleCellRedux old = board[coords.y, coords.x];

            PuzzleCellRedux instance = new PuzzleCellRedux(startPrefab, -1);
            //StartCell instance = Instantiate(startPrefab, old.transform.position,
            //    old.transform.rotation, transform);

            int count = 0;
            while (InvalidCell(instance) && count < 4)
            {
                instance.Rotate90Degrees();
                count++;
            }
            instance.Name = old.Name;
            board[coords.y, coords.x] = instance;
            Advance(instance);
            return instance;
        }

        PuzzleCellRedux PutRoad(PuzzleCellRedux previous)
        {
            List<int> indices = new List<int>();

            for (int j = 0; j < buildingPrefabs.Length; j++)
                indices.Add(j);

            int i = indices.ElementAt(Random.Range(0, indices.Count));

            PuzzleCellRedux old = board[coords.y, coords.x];
            PuzzleCellRedux instance = new PuzzleCellRedux(buildingPrefabs[i], i);

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
                    //options.Add(instance);
                    indices.Remove(i);
                    if (indices.Count == 0)
                    {
                        instance = new PuzzleCellRedux(emptyPrefab, -1);
                        break;
                    }
                    i = indices[Random.Range(0, indices.Count)];
                    instance = new PuzzleCellRedux(buildingPrefabs[i], i);
                    rotCount = 0;
                }
            }


            if (instance.Tag != CellTag.Empty)
            {
                int count = 0;
                while ((IsDeadEnd(instance) || !IsConnected(previous, instance)) && count < 4)
                {
                    instance.Rotate90Degrees();
                    count++;
                }
                instance.Name = old.Name;
                board[coords.y, coords.x] = instance;
            }

            return instance;
        }

        PuzzleCellRedux PutEnd(PuzzleCellRedux previous)
        {
            PuzzleCellRedux old = board[coords.y, coords.x];
            PuzzleCellRedux instance = new PuzzleCellRedux(endPrefab, -1);

            int count = 0;
            while (!IsConnected(previous, instance) && count < 4)
            {
                instance.Rotate90Degrees();
                count++;
            }
            instance.Name = old.Name;
            board[coords.y, coords.x] = instance;
            return instance;
        }

        PuzzleCellRedux start = PutStart();
        PuzzleCellRedux previous = start;
        int count = 0;
        int errorThreshHold = 0;
        while (count < pathLength && errorThreshHold < 500f)
        {
            PuzzleCellRedux instance = PutRoad(previous);
            if (instance.Tag != CellTag.Empty)
            {
                Debug.Log(instance.Tag);
                Advance(instance);
                previous = instance;
                count++;
            }
            errorThreshHold++;
        }
        PutEnd(previous);

        return board;
    }
    public IEnumerable<PuzzleCell> GetNeighbors(PuzzleCell cell)
    {
        (int y, int x) = IndexOf(cell);

        if (x > 0)
            yield return matrixCells[y][x - 1];
        if (y > 0)
            yield return matrixCells[y - 1][x];
        if (x < width - 1)
            yield return matrixCells[y][x + 1];
        if (y < height - 1)
            yield return matrixCells[y + 1][x];
    }
    private (int y, int x) IndexOf(PuzzleCell cell)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (matrixCells[y][x] == cell)
                    return (y, x);
            }
        }

        return (-1, -1);
    }
    private void Rebuild()
    {
        for(int y = 0; y < matrixCells.Length; y++)
        {
            for (int x = 0; x < matrixCells[y].Count; x++)
            {
                if (matrixCells[y][x] == null)
                    continue;
#if UNITY_EDITOR
                DestroyImmediate(matrixCells[y][x].gameObject, false);
#else
                Destroy(matrixCells[y][x].gameObject);
#endif
            }
        }

        matrixCells = new ArrayWrapper<PuzzleCell>[height];

        for(int i = 0; i < height; i++)
            matrixCells[i] = new ArrayWrapper<PuzzleCell>(new PuzzleCell[width]);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzleCell cell = Instantiate(emptyPrefab, transform);
                cell.name = "Cell (" + y + ", " + x + ")";
                cell.transform.position = (Vector2)transform.position + new Vector2(x, y) * emptyPrefab.transform.localScale;
                matrixCells[y][x] = cell;
            }
        }
    }
    
    //private void ChangeCellPosition()
    //{
    //    for (int y = 0; y < height; y++)
    //    {
    //        for (int x = 0; x < width; x++)
    //                matrixCells[y][x].transform.position = (Vector2)transform.position 
    //                + new Vector2(x, y) * emptyPrefab.transform.localScale;
    //    }
    //}

    public ArrayWrapper<PuzzleCell>[] CloneMatrix(ArrayWrapper<PuzzleCell>[] source)
    {
        ArrayWrapper<PuzzleCell>[] copy = new ArrayWrapper<PuzzleCell>[height];
        for (int y = 0; y < height; y++)
        {
            PuzzleCell[] rowCopy = new PuzzleCell[width];
            for (int x = 0; x < width; x++)
            {
                rowCopy[x] = source[y][x];
            }
            copy[y] = new ArrayWrapper<PuzzleCell>(rowCopy);
        }
        return copy;
    } 

    public ArrayWrapper<PuzzleCell>[] SimulateAnnealing(ArrayWrapper<PuzzleCell>[] startingSolution, float startingTemperature, float minTemperature, float coolingRate)
    {
        var currentSolution = startingSolution;
        float currentValue = AnnealingFitnessFunction(currentSolution);
        float temperature = startingTemperature;

        while (temperature > minTemperature)
        {
            var neighbor = GenerateNeighbor(currentSolution);
            float neighborValue = AnnealingFitnessFunction(neighbor);

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
            temperature *= coolingRate;

            Debug.Log(temperature + "° enfriando...");
        }
        return currentSolution;
    }

    private float AnnealingFitnessFunction(ArrayWrapper<PuzzleCell>[] subject)
    {
        float fitness = 0;
        for(int y = 0; y < subject.Length; y++)
        {
            for(int x = 0; x < subject[y].Count; x++)
            {
                if (matrixCells[y][x] != subject[y][x]
                    && (matrixCells[y][x] is not EmptyCell || subject[y][x] is not EmptyCell))
                {
                    (int y, int x) coord = IndexOf(subject[y][x]);
                    (int y, int x) delta = (Mathf.Abs(coord.x - x), Mathf.Abs(coord.y - y));
                    fitness += delta.y + delta.x;
                }
            }
        }
        return fitness;
    }

    private ArrayWrapper<PuzzleCell>[] GenerateNeighbor(ArrayWrapper<PuzzleCell>[] state)
    {
        var newState = CloneMatrix(state);
        List<PuzzleCell> neighbors = new List<PuzzleCell>();
        PuzzleCell chosen;

        bool IsSwappable(PuzzleCell cell)
        {
            if (cell is StartCell || cell is EndCell)
                return false;

            if (chosen is EmptyCell)
                return cell is not EmptyCell;

            return cell is EmptyCell;
        }

        do
        {
            do
            {
                (int y, int x) = (Random.Range(0, height), Random.Range(0, width));

                chosen = newState[y][x];
            } while (chosen is StartCell || chosen is EndCell);

            neighbors = GetNeighborsFromState(state, chosen).Where(IsSwappable).ToList();
        } while (neighbors.Count == 0);
        
        if(neighbors.Count > 0)
        {
            PuzzleCell target = neighbors[Random.Range(0, neighbors.Count)];

            (int i, int j) a = IndexOfState(state, chosen);
            (int i, int j) b = IndexOfState(state, target);

            (newState[a.i][a.j], newState[b.i][b.j]) = (newState[b.i][b.j], newState[a.i][a.j]);
        }

        return newState;
    }

    private (int y, int x) IndexOfState(ArrayWrapper<PuzzleCell>[] state, PuzzleCell cell)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (state[y][x] == cell)
                    return (y, x);
            }
        }
        return (-1, -1);
    }
    private IEnumerable<PuzzleCell> GetNeighborsFromState(ArrayWrapper<PuzzleCell>[] state,
        PuzzleCell cell)
    {
        (int y, int x) = IndexOfState(state, cell);
        if (x > 0)
            yield return state[y][x - 1];
        if (y > 0)
            yield return state[y - 1][x];
        if (x < width - 1)
            yield return state[y][x + 1];
        if (y < height - 1)
            yield return state[y + 1][x];
    }

    IEnumerable<PuzzleCellRedux[,]> IEvolutionaryStrategy<PuzzleCellRedux[,]>.Initialize()
    {
        for (int i = 0; i < MaxPopulation; i++)
            yield return BuildSolutionModel(pathLength);
    }

    PuzzleCellRedux[,] IEvolutionaryStrategy<PuzzleCellRedux[,]>.Mutate(PuzzleCellRedux[,] parent)
    {
        return MutateSolutionModel(parent, Random.Range(2, pathLength));
    }

    private PuzzleCellRedux[,] MutateSolutionModel(PuzzleCellRedux[,] parent, int deleteCellCount)
    {
        PuzzleCellRedux[,] board = new PuzzleCellRedux[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                board[y, x] = new PuzzleCellRedux(parent[y, x]);
        }

        (int y, int x) coords = (-1, -1);

        PuzzleCellRedux GetConnectedCell(PuzzleCellRedux cell)
        {
            bool IsConnected(PuzzleCellRedux previous, PuzzleCellRedux next)
            {
                bool[] previousConnections = previous.GetConnections();
                bool[] nextConnections = next.GetConnections();

                (int y, int x) p = IndexOfBoard(parent, previous);
                (int y, int x) n = IndexOfBoard(parent, next);
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

            foreach (PuzzleCellRedux neighbor in GetNeighborsFromBoard(parent, cell))
            {
                if (IsConnected(neighbor, cell))
                    return neighbor;
            }
            return cell;
        }

        (int y, int x) Delete()
        {
            (int y, int x) currentCoord = (-1, -1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (board[y, x].Tag == CellTag.End)
                    {
                        currentCoord = (y, x);
                        break;
                    }
                }
            }

            for (int i = 0; i < deleteCellCount; i++)
            {
                PuzzleCellRedux previous = GetConnectedCell(parent[currentCoord.y, currentCoord.x]);
                board[currentCoord.y, currentCoord.x] = new PuzzleCellRedux(emptyPrefab, -1);
                currentCoord = IndexOfBoard(parent, previous);
            }

            return currentCoord;
        }


        bool IsConnected(PuzzleCellRedux previous, PuzzleCellRedux next)
        {
            bool[] previousConnections = previous.GetConnections();
            bool[] nextConnections = next.GetConnections();

            (int y, int x) p = IndexOfBoard(board, previous);
            (int y, int x) n = coords;
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


        bool InvalidCell(PuzzleCellRedux current)
        {
            bool[] connections = current.GetConnections();
            bool invalidLeftMove = connections[0] && (coords.x - 1 < 0);
            bool invalidUpMove = connections[1] && (coords.y + 1 >= height);
            bool invalidRightMove = connections[2] && (coords.x + 1 >= width);
            bool invalidDownMove = connections[3] && (coords.y - 1 < 0);
            return invalidLeftMove || invalidUpMove || invalidRightMove || invalidDownMove;
        }

        bool InvalidCoord((int y, int x) coord)
        {
            bool invalidLeft = coord.x < 0;
            bool invalidUp = coord.y >= height;
            bool invalidRight = coord.x >= width;
            bool invalidDown = coord.y < 0;

            return invalidLeft || invalidUp || invalidRight || invalidDown || board[coord.y, coord.x].Tag != CellTag.Empty;
        }

        bool IsDeadEnd(PuzzleCellRedux cell)
        {
            bool condition = true;
            bool[] connections = cell.GetConnections();

            if (coords.x > 0)
                condition &= !connections[0] || board[coords.y, coords.x - 1].Tag != CellTag.Empty;
            if (coords.y > 0)
                condition &= !connections[3] || board[coords.y - 1, coords.x].Tag != CellTag.Empty;
            if (coords.x < width - 1)
                condition &= !connections[2] || board[coords.y, coords.x + 1].Tag != CellTag.Empty;
            if (coords.y < height - 1)
                condition &= !connections[1] || board[coords.y + 1, coords.x].Tag != CellTag.Empty;

            return condition;
        }

        void Advance(PuzzleCellRedux instance)
        {
            Debug.Log(instance.Name);
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
            Debug.Log(indices.Count);
            do
            {
                int i = indices[Random.Range(0, indices.Count)];
                newCoord = (coords.y + dirs[i].y, coords.x + dirs[i].x);
                indices.Remove(i);
            } while (InvalidCoord(newCoord) && indices.Count > 0);

            coords = newCoord;
        }

        PuzzleCellRedux PutStart()
        {
            PuzzleCellRedux old = board[coords.y, coords.x];

            PuzzleCellRedux instance = new PuzzleCellRedux(startPrefab, -1);
            //StartCell instance = Instantiate(startPrefab, old.transform.position,
            //    old.transform.rotation, transform);

            int count = 0;
            while (InvalidCell(instance) && count < 4)
            {
                instance.Rotate90Degrees();
                count++;
            }
            instance.Name = old.Name;
            board[coords.y, coords.x] = instance;
            Advance(instance);
            return instance;
        }

        PuzzleCellRedux PutRoad(PuzzleCellRedux previous)
        {
            List<int> indices = new List<int>();

            for (int j = 0; j < buildingPrefabs.Length; j++)
                indices.Add(j);

            int i = indices.ElementAt(Random.Range(0, indices.Count));

            PuzzleCellRedux old = board[coords.y, coords.x];
            PuzzleCellRedux instance = new PuzzleCellRedux(buildingPrefabs[i], i);

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
                    //options.Add(instance);
                    indices.Remove(i);
                    if (indices.Count == 0)
                    {
                        instance = new PuzzleCellRedux(emptyPrefab, -1);
                        break;
                    }
                    i = indices[Random.Range(0, indices.Count)];
                    instance = new PuzzleCellRedux(buildingPrefabs[i], i);
                    rotCount = 0;
                }
            }


            if (instance.Tag != CellTag.Empty)
            {
                int count = 0;
                while ((IsDeadEnd(instance) || !IsConnected(previous, instance)) && count < 4)
                {
                    instance.Rotate90Degrees();
                    count++;
                }
                instance.Name = old.Name;
                board[coords.y, coords.x] = instance;
            }

            return instance;
        }

        PuzzleCellRedux PutEnd(PuzzleCellRedux previous)
        {
            PuzzleCellRedux old = board[coords.y, coords.x];
            PuzzleCellRedux instance = new PuzzleCellRedux(endPrefab, -1);

            int count = 0;
            while (!IsConnected(previous, instance) && count < 4)
            {
                instance.Rotate90Degrees();
                count++;
            }
            instance.Name = old.Name;
            board[coords.y, coords.x] = instance;
            return instance;
        }

        bool ExistStart()
        {
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    if (board[y, x].Tag == CellTag.Start)
                        return true;
                }
            }

            return false;
        }

        coords = Delete();

        PuzzleCellRedux previous = ExistStart() ? board[coords.y, coords.x] : PutStart();
        int count = 0;
        int errorThreshHold = 0;
        
        while (count < deleteCellCount - 1 && errorThreshHold < 500f)
        {
            PuzzleCellRedux instance = PutRoad(previous);
            if (instance.Tag != CellTag.Empty)
            {
                Debug.Log(instance.Tag);
                Advance(instance);
                previous = instance;
                count++;
            }
            errorThreshHold++;
        }
        
        PutEnd(previous);

        return board;
    }

    public float FitnessEvaluation(PuzzleCellRedux[,] individual)
    { 
        var result = EvaluatePath(individual);

        float lengthScore = result.lenght;
        float varietyScore = result.variety;


        float lengthCriteria = 0;

        float diffPath = Mathf.Abs(pathLength - lengthScore + 2);
        
        if (diffPath != 0)
            lengthCriteria = -diffPath;

        float fitness = lengthCriteria + varietyScore;
        Debug.Log("Fitness: " + fitness);

        return fitness;
    }

    private (bool solvable, int lenght, float variety) EvaluatePath(PuzzleCellRedux[,] individual)
    {
        PuzzleCellRedux? start = null;
        PuzzleCellRedux? end = null;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (individual[y, x].Tag == CellTag.Start)
                    start = individual[y, x];
                else if (individual[y, x].Tag == CellTag.End)
                    end = individual[y, x];

                if (start != null && end != null)
                    break;
            }
        }

        bool AreConnected(PuzzleCellRedux previous, PuzzleCellRedux next)
        {
            bool[] previousConnections = previous.GetConnections();
            bool[] nextConnections = next.GetConnections();

            (int y, int x) p = IndexOfBoard(individual, previous);
            (int y, int x) n = IndexOfBoard(individual, next);

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

        Queue<(PuzzleCellRedux cell, int distance, int pathValue)> queue = new Queue<(PuzzleCellRedux, int, int)>();
        HashSet<PuzzleCellRedux> visited = new HashSet<PuzzleCellRedux>();

        queue.Enqueue((start.Value, 0, start.Value.Value));
        visited.Add(start.Value);
        int sameCount = 0;

        Dictionary<int, int> repeated = new Dictionary<int, int>();

        float CalculateBias()
        {
            if (mode == Mode.EVSEMPERTINE)
                return 0;

            repeated.Remove(0);

            if (repeated.Keys.Count <= 1)
                return -pathLength;

            List<int> values = new List<int>(repeated.Values);

            int diff = 0;

            do
            {
                int max = values.Max();
                diff += max - values.Min();
                values.Remove(max);
            } while (values.Count > 1);

            return -diff;
        }

        while (queue.Count > 0)
        {
            //Debug.Log("Sigo aqui");
            var (current, dist, pathValue) = queue.Dequeue();

            if (current == end) return (true, dist, pathValue + CalculateBias());

            foreach (var neighbor in GetNeighborsFromBoard(individual, current))
            {
                if (visited.Contains(neighbor)) continue;

                if (AreConnected(current, neighbor))
                {
                    if (!repeated.ContainsKey(neighbor.Value))
                        repeated[neighbor.Value] = 0;
                    repeated[neighbor.Value]++;
                    queue.Enqueue((neighbor, dist + 1, pathValue + neighbor.Value));
                    visited.Add(neighbor);
                }
            }
        }

        return (false, 0, 0);
    }
    private IEnumerable<PuzzleCellRedux> GetNeighborsFromBoard(PuzzleCellRedux[,] board, PuzzleCellRedux cell)
    {
        (int y, int x) = IndexOfBoard(board, cell);

        if (x > 0)
            yield return board[y, x - 1];
        if (y > 0)
            yield return board[y - 1, x];
        if (x < width - 1)
            yield return board[y, x + 1];
        if (y < height - 1)
            yield return board[y + 1, x];
    }
    bool IEvolutionaryStrategy<PuzzleCellRedux[,]>.TerminationCriteria(IEnumerable<PuzzleCellRedux[,]> population)
    {
        return false;
    }

    public void ReadPopulationSize(string s)
    {
        if (float.TryParse(s, out var size))
            maxPopulation = (int)size;
    }

    public void ReadIterationAmount(string s)
    {
        if (float.TryParse(s, out var amount))
            maxIterations = (int)amount;
    }

    public void ReadStartingTemp(string s)
    {
        if (float.TryParse(s, out var temp))
            startingTemp = temp;
    }

    public void ReadMinTemp(string s)
    {
        if (float.TryParse(s, out var temp))
            minTemp = temp;
    }

    public void ReadCoolingRate(float value)
    {
        coolingRate = Mathf.Round(coolingSlider.value*10)/10;
        coolingText.text = coolingRate.ToString("F1");
        coolingRate /= 100;
    }

    public void ReadHeight(string s)
    {
        if (float.TryParse(s, out var value))
            height = (int)value;
    }

    public void ReadWidth(string s)
    {
        if (float.TryParse(s, out var value))
            width = (int)value;
    }

    public void ReadPathLenght(string s)
    {
        if (float.TryParse(s, out var value))
            pathLength = (int)value;
    }

    public void ReadModeChange(int index)
    {
        mode = (Mode)index;
    }

    public void RebuildButton()
    {
        Rebuild();

        if (annealing != null)
        {
            switch (mode)
            {
                case Mode.EVSEMPERTINE:
                case Mode.EVOLUTIONARY: LoadFromModel(this.GenerateBestIndividual(maxIterations)); break;
                case Mode.NORMAL: LoadFromModel(BuildSolutionModel(maxIterations)); break;
            }

            ArrayWrapper<PuzzleCell>[] matrix = executeAnnealing && mode != Mode.NONE ?
                SimulateAnnealing(matrixCells, startingTemp, minTemp, coolingRate) : CloneMatrix(matrixCells);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Destroy(annealing.matrixCells[y][x]);
                    PuzzleCell clone = Instantiate(matrix[y][x], annealing.transform);
                    clone.transform.position = (Vector2)annealing.transform.position
                        + new Vector2(x, y) * emptyPrefab.transform.localScale;
                }
            }
        }
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
