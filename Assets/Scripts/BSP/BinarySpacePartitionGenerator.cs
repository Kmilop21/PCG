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
    private Rect[] zones;
    private DiamondSquareTerrain DSterrain;

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
        DSterrain = GetComponent<DiamondSquareTerrain>();
        size = new Vector2(DSterrain.size, DSterrain.size);

        tree = BinarySpacePartitionTree.Generate(
            new Rect((Vector2)transform.position, size), 
            minSize, 
            splitToMax ? -10 : iteration);

        zones = tree.GetSubAreas();
        MarkZones();
    }

    private void MarkZones()
    {
        if (zones == null) return;

        Terrain terrain = Terrain.activeTerrain;
        Vector3 terrainPos = terrain.transform.position;

        foreach (var zone in zones)
        {
            float x = UnityEngine.Random.Range(zone.xMin, zone.xMax);
            float z = UnityEngine.Random.Range(zone.yMin, zone.yMax);

            float y = terrain.SampleHeight(new Vector3(x, 0, z));

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.transform.position = new Vector3(x, y, z);
            marker.transform.localScale = Vector3.one * 2;
        }
    }
}
