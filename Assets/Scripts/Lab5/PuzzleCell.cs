using Foike.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public enum CellTag
{
    Empty = 0,
    Filled = 1,
    Start = -10,
    End = 10
}

public struct PuzzleCellRedux : IEquatable<PuzzleCellRedux>
{
    private bool[] connections;
    public CellTag Tag { private set; get; }
    public int PrefabIndex { private set; get; }
    public string Name;
    public int RotCount;
    public int Value;
    public PuzzleCellRedux(PuzzleCell source, int index)
    {
        Value = source.value;
        RotCount = 0;
        connections = source.GetConnections();
        Name = source.name;
        PrefabIndex = index;
        if(source is EmptyCell)
            Tag = CellTag.Empty;
        else if (source is StartCell)
            Tag = CellTag.Start;
        else if (source is EndCell)
            Tag = CellTag.End;
        else
            Tag = CellTag.Filled;
    }

    public PuzzleCellRedux(PuzzleCellRedux source)
    {
        Value = source.Value;
        RotCount = source.Value;
        connections = source.GetConnections();
        Name = source.Name;
        PrefabIndex = source.PrefabIndex;
        Tag = source.Tag;
    }
    public bool IsReachable()
    {
        bool isReachable = false;

        foreach (bool c in connections)
            isReachable |= c;

        return isReachable;
    }
    public void Rotate90Degrees(int dir = -1)
    {
        if (dir < 0)
        {
            RotCount -= 1;
            bool[] copy = new bool[4];
            connections.CopyTo(copy, 0);
            connections[0] = copy[^1];
            for (int i = 1; i < connections.Length; i++)
                connections[i] = copy[i - 1];
        }
        else if (dir > 0)
        {
            RotCount += 1;  
            bool[] copy = new bool[4];
            connections.CopyTo(copy, 0);
            connections[^1] = copy[0];
            for (int i = connections.Length - 1; i >= 0; i--)
                connections[i] = copy[i + 1];
        }
    }

    public bool[] GetConnections()
    {
        bool[] connections = new bool[4];
        this.connections.CopyTo(connections, 0);
        return connections;
    }

    public override bool Equals(object obj)
    {
        return obj is PuzzleCellRedux redux && Equals(redux);
    }

    public bool Equals(PuzzleCellRedux other)
    {
        return EqualityComparer<bool[]>.Default.Equals(connections, other.connections) &&
               Tag == other.Tag &&
               Name == other.Name &&
               PrefabIndex == other.PrefabIndex;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(connections, Tag, Name, PrefabIndex);
    }

    public static bool operator==(PuzzleCellRedux left, PuzzleCellRedux right)
    {
        for(int i = 0; i < 4; i++)
        {
            if (left.connections[i] != right.connections[i])
                return false;
        }
        return left.Name == right.Name && left.Tag == right.Tag && left.PrefabIndex == right.PrefabIndex;
    }

    public static bool operator!=(PuzzleCellRedux left, PuzzleCellRedux right)
    {
        return !(left == right);
    }


}
public class PuzzleCell : MonoBehaviour
{
    //[SerializeField] private PuzzleSquareCell[] neighbors = new PuzzleSquareCell[4];
    [SerializeField] private bool[] connections = new bool[4];
    public int value;

    // Start is called before the first frame update
    void Start()
    {
        int i = 0; 
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Rotate90Degrees(int dir = -1)
    {
        transform.Rotate(new Vector3(0, 0, 90) * dir, Space.Self);

        if(dir < 0)
        {
            bool[] copy = new bool[4];
            connections.CopyTo(copy, 0);
            connections[0] = copy[^1];
            for(int i = 1; i < connections.Length; i++)
                connections[i] = copy[i - 1];
        }
        else if(dir > 0)
        {
            bool[] copy = new bool[4];
            connections.CopyTo(copy, 0);
            connections[^1] = copy[0];
            for (int i = connections.Length - 1; i >= 0; i--)
                connections[i] = copy[i + 1];
        }
    }

    public bool[] GetConnections()
    {
        bool[] connections = new bool[4];
        this.connections.CopyTo(connections, 0);
        return connections;
    }

    public bool IsReachable()
    {
        bool isReachable = false;

        foreach(bool c in connections)
            isReachable |= c;

        return isReachable;
    }

    //public static bool operator==(PuzzleSquareCell left, PuzzleSquareCell right)
    //{
    //    if(left is null && right is null) return true;

    //    if(left is null || right is null) return false;

    //    for(int i = 0; i < 4; i++)
    //    {
    //        if (left.connections[i] != right.connections[i])
    //            return false;
    //    }

    //    return true;
    //}

    //public static bool operator !=(PuzzleSquareCell left, PuzzleSquareCell right) => !(left == right);
}
