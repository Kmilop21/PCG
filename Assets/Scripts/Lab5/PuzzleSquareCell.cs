using Foike.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public struct PuzzleCellRedux
{
    private bool[] connections;
    public PuzzleCellRedux(PuzzleSquareCell source)
    {
        connections = source.GetConnections();
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
            bool[] copy = new bool[4];
            connections.CopyTo(copy, 0);
            connections[0] = copy[^1];
            for (int i = 1; i < connections.Length; i++)
                connections[i] = copy[i - 1];
        }
        else if (dir > 0)
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
}
public class PuzzleSquareCell : MonoBehaviour
{
    //[SerializeField] private PuzzleSquareCell[] neighbors = new PuzzleSquareCell[4];
    [SerializeField] private bool[] connections = new bool[4];
    public int value;

    public bool IsValid => true;
    public float Cost => 1;
    public bool Selected { private set; get; }

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
}
