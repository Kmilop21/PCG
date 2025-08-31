using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinarySpacePartition : MonoBehaviour
{

    [Header("Map Settings")]
    public int mapWidth = 64;
    public int mapHeight = 64;
    public int minPartitionSize = 16;
    public int maxIterations = 4;

    [Header("Room Settings")]
    public int minRoomSize = 4;
    public int maxRoomSize = 12;

    [Header("Debug")]
    public bool drawGizmos = true;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    private Node rootNode;
    private List<Node> leaves = new List<Node>();

    /// <summary>
    /// clase para las particiones
    /// </summary>
    class Node
    {
        public RectInt area;
        public Node left, right;
        public RectInt? room;
        public List<Vector2Int> corridor = new List<Vector2Int>();

        public Node(RectInt area)
        {
            this.area = area;
        }

        public bool IsLeaf() => left == null && right == null;
    }

    void Start()
    {
        GenerateMap();
    }

    /// <summary>
    /// funcion que genera el mapa
    /// </summary>
    void GenerateMap()
    {
        //nodo raiz
        rootNode = new Node(new RectInt(0, 0, mapWidth, mapHeight));

        //division de area recursivamente
        SplitNode(rootNode, maxIterations);

        //crear habitaciones
        CreateRooms(rootNode);

        //conectar habitaciones de corredores
        ConnectRooms(rootNode);

        //construir el mapa
        BuildMap();
    }

    /// <summary>
    /// clase encargada de hacer las divisiones tanto horizontales como verticales del espacio
    /// </summary>
    void SplitNode(Node node, int iterations)
    {
        if (iterations <= 0 || node.area.width < minPartitionSize * 2 || node.area.height < minPartitionSize * 2)
        {
            leaves.Add(node);
            return;
        }

        bool splitHorizontally = Random.value > 0.5f;

        if (splitHorizontally)
        {
            int splitY = Random.Range(minPartitionSize, node.area.height - minPartitionSize);
            node.left = new Node(new RectInt(node.area.x, node.area.y, node.area.width, splitY));
            node.right = new Node(new RectInt(node.area.x, node.area.y + splitY, node.area.width, node.area.height - splitY));
        }
        else
        {
            int splitX = Random.Range(minPartitionSize, node.area.width - minPartitionSize);
            node.left = new Node(new RectInt(node.area.x, node.area.y, splitX, node.area.height));
            node.right = new Node(new RectInt(node.area.x + splitX, node.area.y, node.area.width - splitX, node.area.height));
        }

        SplitNode(node.left, iterations - 1);
        SplitNode(node.right, iterations - 1);
    }

    /// <summary>
    /// funcion que cra las habitaciones
    /// </summary>
    void CreateRooms(Node node)
    {
        if (node.IsLeaf())
        {
            int roomWidth = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, node.area.width));
            int roomHeight = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, node.area.height));
            int roomX = Random.Range(node.area.x, node.area.xMax - roomWidth);
            int roomY = Random.Range(node.area.y, node.area.yMax - roomHeight);

            node.room = new RectInt(roomX, roomY, roomWidth, roomHeight);
        }
        else
        {
            if (node.left != null) CreateRooms(node.left);
            if (node.right != null) CreateRooms(node.right);
        }
    }

    /// <summary>
    /// conexion de habitaciones
    /// </summary>
    void ConnectRooms(Node node)
    {
        if (node.left != null && node.right != null)
        {
            Vector2Int roomA = GetRoomCenter(node.left);
            Vector2Int roomB = GetRoomCenter(node.right);

            //dibuja corredor recto entre centros
            if (Random.value > 0.5f)
            {
                CreateCorridor(roomA, new Vector2Int(roomB.x, roomA.y));
                CreateCorridor(new Vector2Int(roomB.x, roomA.y), roomB);
            }
            else
            {
                CreateCorridor(roomA, new Vector2Int(roomA.x, roomB.y));
                CreateCorridor(new Vector2Int(roomA.x, roomB.y), roomB);
            }

            ConnectRooms(node.left);
            ConnectRooms(node.right);
        }
    }

    /// <summary>
    /// crea el inicio y el fin del camino
    /// </summary>
    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        int xDir = start.x < end.x ? 1 : -1;
        int yDir = start.y < end.y ? 1 : -1;

        for (int x = start.x; x != end.x; x += xDir)
            rootNode.corridor.Add(new Vector2Int(x, start.y));

        for (int y = start.y; y != end.y; y += yDir)
            rootNode.corridor.Add(new Vector2Int(end.x, y));
    }

    /// <summary>
    /// obtiene el centro de la sala
    /// </summary>
    Vector2Int GetRoomCenter(Node node)
    {
        if (node.IsLeaf() && node.room.HasValue)
            return new Vector2Int(node.room.Value.x + node.room.Value.width / 2,
                                  node.room.Value.y + node.room.Value.height / 2);

        if (node.left != null) return GetRoomCenter(node.left);
        if (node.right != null) return GetRoomCenter(node.right);

        return Vector2Int.zero;
    }

    /// <summary>
    /// dibuja el mapa usando gizmos
    /// </summary>
    void OnDrawGizmos()
    {
        if (!drawGizmos || rootNode == null) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector3(mapWidth / 2f, 0, mapHeight / 2f), new Vector3(mapWidth, 0, mapHeight));

        foreach (var leaf in leaves)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(leaf.area.center.x, 0, leaf.area.center.y),
                                new Vector3(leaf.area.width, 0, leaf.area.height));

            if (leaf.room.HasValue)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(new Vector3(leaf.room.Value.center.x, 0, leaf.room.Value.center.y),
                                new Vector3(leaf.room.Value.width, 0.1f, leaf.room.Value.height));
            }
        }

        Gizmos.color = Color.red;
        foreach (var c in rootNode.corridor)
            Gizmos.DrawCube(new Vector3(c.x, 0, c.y), Vector3.one * 0.5f);
    }

    void BuildMap()
    {
        foreach (var leaf in leaves)
        {
            if(!leaf.room.HasValue) continue;
            RectInt room = leaf.room.Value;

            //Piso
            for (int x = room.x; x < room.xMax; x++)
                for (int y = room.y; y < room.yMax; y++)
                    Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity);

            //Paredes horizontales
            for(int x = room.x -1; x <= room.xMax; x++)
            {
                Instantiate(wallPrefab, new Vector3(x, 1, room.y-1), Quaternion.identity);
                Instantiate(wallPrefab, new Vector3(x, 1, room.yMax), Quaternion.identity);
            }

            //Paredes verticales
            for (int y = room.y - 1; y <= room.yMax; y++)
            {
                Instantiate(wallPrefab, new Vector3(room.x - 1, 1, y), Quaternion.identity);
                Instantiate(wallPrefab, new Vector3(room.xMax, 1, y), Quaternion.identity);
            }
        }

        //Corredores
        DrawCorridor(rootNode);
    }

    void DrawCorridor(Node node)
    {
        if (node == null) return;
        foreach (var c in node.corridor)
            Instantiate(floorPrefab, new Vector3(c.x, 0, c.y), Quaternion.identity);

        DrawCorridor(node.left);
        DrawCorridor(node.right);
    }
}
