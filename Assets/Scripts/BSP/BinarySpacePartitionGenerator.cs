using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BinarySpacePartitionGenerator : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("Map Settings")]
    [SerializeField] private Vector2 size = Vector2.one * 50;
    [SerializeField] private Vector2 minSize = Vector2.one * 5;

    [Header("Generation Settings")]
    [SerializeField] private bool splitToMax = true;
    [SerializeField] private int iteration = 4;

    private BinarySpacePartitionTree tree;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if(!EditorApplication.isPlaying)
            tree = BinarySpacePartitionTree.Generate(new Rect((Vector2)transform.position, size), size);
#endif
        tree.OnDrawGizmos();
    }
    private void Start()
    {
        tree = BinarySpacePartitionTree.Generate(new Rect((Vector2)transform.position, size), minSize, 
            splitToMax ? -10 : iteration);
    }
}
