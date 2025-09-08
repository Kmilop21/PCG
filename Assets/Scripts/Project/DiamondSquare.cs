using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[System.Serializable]
public struct DiamondSquare
{
    [SerializeField] public float initialRange;
    [SerializeField] public float noiseValue;
    public float[,] Map { private set; get; }
    [field: SerializeField] public int Size { set; get; }   
    public DiamondSquare(int size, float initialRange, float noiseValue)
    {
        Size = size;
        this.initialRange = initialRange;
        this.noiseValue = noiseValue;
        Map = new float[size, size];
    }
    public void Generate()
    {
        Map = new float[Size, Size];
        //inicializar esquinas
        Map[0, 0] = Random.Range(-initialRange, initialRange);
        Map[0, Size - 1] = Random.Range(-initialRange, initialRange);
        Map[Size - 1, 0] = Random.Range(-initialRange, initialRange);
        Map[Size - 1, Size - 1] = Random.Range(-initialRange, initialRange);

        int step = Size - 1;
        float range = initialRange;

        while (step > 1)
        {
            int halfStep = step / 2;

            //paso diamante
            for (int x = 0; x < Size - 1; x += step)
            {
                for (int y = 0; y < Size - 1; y += step)
                {
                    float avg = (Map[x, y] + Map[x + step, y] + Map[x, y + step] + Map[x + step, y + step]) / 4.0f;
                    Map[x + halfStep, y + halfStep] = avg + Random.Range(-range, range);
                }
            }

            //paso cuadrado
            for (int x = 0; x < Size; x += halfStep)
            {
                for (int y = (x + halfStep) % step; y < Size; y += step)
                {
                    float sum = 0f;
                    int count = 0;

                    if (x - halfStep >= 0)
                    { 
                        sum += Map[x - halfStep, y]; 
                        count++; 
                    }
                    if (x + halfStep < Size) 
                    { 
                        sum += Map[x + halfStep, y]; 
                        count++; 
                    }
                    if (y - halfStep >= 0) 
                    { 
                        sum += Map[x, y - halfStep]; 
                        count++; 
                    }
                    if (y + halfStep < Size) 
                    { 
                        sum += Map[x, y + halfStep]; 
                        count++; 
                    }

                    float avg = sum / count;
                    Map[x, y] = avg + Random.Range(-range, range);
                }
            }

            step /= 2;
            range *= noiseValue;
        }

        //normalizar entre 0 y 1
        NormalizeMap();

    }

    public void Generate(Vector2 startPos, Dictionary<Rect, float> valuesBSP)
    {
        Map = new float[Size, Size];
        //inicializar esquinas
        Map[0, 0] = Random.Range(-initialRange, initialRange);
        Map[0, Size - 1] = Random.Range(-initialRange, initialRange);
        Map[Size - 1, 0] = Random.Range(-initialRange, initialRange);
        Map[Size - 1, Size - 1] = Random.Range(-initialRange, initialRange);

        int step = Size - 1;
        float range = initialRange;

        while (step > 1)
        {
            int halfStep = step / 2;

            //paso diamante
            for (int x = 0; x < Size - 1; x += step)
            {
                for (int y = 0; y < Size - 1; y += step)
                {
                    Rect first = valuesBSP.Keys.First((r) => r.Contains(startPos + new Vector2(x, y)));
                    float avg = (Map[x, y] + Map[x + step, y] + Map[x, y + step] + Map[x + step, y + step]) / 4.0f;
                    Map[x + halfStep, y + halfStep] = avg + Random.Range(-range, range) * valuesBSP[first];
                }
            }

            //paso cuadrado
            for (int x = 0; x < Size; x += halfStep)
            {
                for (int y = (x + halfStep) % step; y < Size; y += step)
                {
                    float sum = 0f;
                    int count = 0;

                    if (x - halfStep >= 0)
                    {
                        sum += Map[x - halfStep, y];
                        count++;
                    }
                    if (x + halfStep < Size)
                    {
                        sum += Map[x + halfStep, y];
                        count++;
                    }
                    if (y - halfStep >= 0)
                    {
                        sum += Map[x, y - halfStep];
                        count++;
                    }
                    if (y + halfStep < Size)
                    {
                        sum += Map[x, y + halfStep];
                        count++;
                    }

                    float avg = sum / count;
                    Map[x, y] = avg + Random.Range(-range, range);
                }
            }

            step /= 2;
            range *= noiseValue;
        }

        //normalizar entre 0 y 1
        NormalizeMap();

    }

    private void NormalizeMap()
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (float h in Map)
        {
            if (h < min) 
                min = h;
            if (h > max) 
                max = h;
        }

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
                Map[x, y] = Mathf.InverseLerp(min, max, Map[x, y]);
        }
    }

    
}
