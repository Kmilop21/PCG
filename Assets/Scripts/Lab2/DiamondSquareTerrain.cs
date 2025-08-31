using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSquareTerrain : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int size = 257;
    public float height = 20f;      
    public float roughness = 0.5f;  
    public float initialRange = 10f;

    private float[,] heightMap;

    void Start()
    {
        GenerateHeightMap();
        ApplyToTerrain();
    }
    void GenerateHeightMap()
    {
        heightMap = new float[size, size];

        //inicializar esquinas
        heightMap[0, 0] = Random.Range(-initialRange, initialRange);
        heightMap[0, size - 1] = Random.Range(-initialRange, initialRange);
        heightMap[size - 1, 0] = Random.Range(-initialRange, initialRange);
        heightMap[size - 1, size - 1] = Random.Range(-initialRange, initialRange);

        int step = size - 1;
        float range = initialRange;

        while (step > 1)
        {
            int halfStep = step / 2;

            //paso diamante
            for (int x = 0; x < size - 1; x += step)
            {
                for (int y = 0; y < size - 1; y += step)
                {
                    float avg = (heightMap[x, y] +
                                 heightMap[x + step, y] +
                                 heightMap[x, y + step] +
                                 heightMap[x + step, y + step]) / 4.0f;

                    heightMap[x + halfStep, y + halfStep] = avg + Random.Range(-range, range);
                }
            }

            //paso cuadrado
            for (int x = 0; x < size; x += halfStep)
            {
                for (int y = (x + halfStep) % step; y < size; y += step)
                {
                    float sum = 0f;
                    int count = 0;

                    if (x - halfStep >= 0) { sum += heightMap[x - halfStep, y]; count++; }
                    if (x + halfStep < size) { sum += heightMap[x + halfStep, y]; count++; }
                    if (y - halfStep >= 0) { sum += heightMap[x, y - halfStep]; count++; }
                    if (y + halfStep < size) { sum += heightMap[x, y + halfStep]; count++; }

                    float avg = sum / count;
                    heightMap[x, y] = avg + Random.Range(-range, range);
                }
            }

            step /= 2;
            range *= roughness;
        }

        //normalizar entre 0 y 1
        NormalizeHeightMap();


    }

    void NormalizeHeightMap()
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (float h in heightMap)
        {
            if (h < min) min = h;
            if (h > max) max = h;
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                heightMap[x, y] = Mathf.InverseLerp(min, max, heightMap[x, y]);
            }
        }
    }

    void ApplyToTerrain()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("Este GameObject no tiene un componente Terrain.");
            return;
        }

        terrain.terrainData.heightmapResolution = size;
        terrain.terrainData.size = new Vector3(size, height, size);
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }


}
