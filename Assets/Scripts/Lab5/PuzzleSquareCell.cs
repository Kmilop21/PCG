using Foike.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    //public void SetNeighbors(int index, PuzzleSquareCell neighbor) => neighbors[index] = neighbor;
    //public int IndexOf(PuzzleSquareCell cell)
    //{
    //    for(int i = 0; i < 4; i++)
    //    {
    //        if (neighbors[i] == cell) 
    //            return i;
    //    }

    //    return -1;
    //}
    //public PuzzleSquareCell GetNeighbor(int index) => neighbors[index]; 
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
