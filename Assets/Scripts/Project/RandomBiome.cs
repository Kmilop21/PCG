using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.TerrainTools;

public class RandomBiome : MonoBehaviour
{
    [SerializeField] private float minSize = 20f;
    [SerializeField] private DiamondSquare heightNoise = new DiamondSquare(257, 10f, 0.5f);
    [SerializeField] private DiamondSquare alphaNoise = new DiamondSquare(257, 10f, 0.5f);
    [SerializeField] private float height = 20f;
    [SerializeField] private TerrainLayer[] terrainLayers;
    [SerializeField] private float noise = 1; 
    private Terrain terrain;
    private BinarySpacePartitionTree tree;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            Vector2 size = Vector2.one * heightNoise.Size;
            tree = BinarySpacePartitionTree.Generate(new Rect((Vector2)transform.position, size), size);
        }
#endif
        tree.OnDrawGizmos(true);
    }

    private void Start()
    {
        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("Este GameObject no tiene un componente Terrain.");
            return;
        }

        Rect area = new Rect(transform.position, Vector2.one * heightNoise.Size);
        tree = BinarySpacePartitionTree.Generate(area, Vector2.one * minSize);


        terrain.terrainData.size = new Vector3(heightNoise.Size, height, heightNoise.Size);

        heightNoise.Generate();
        terrain.terrainData.heightmapResolution = heightNoise.Size;
        terrain.terrainData.SetHeights(0, 0, heightNoise.Map);


        alphaNoise.Generate();
        terrain.terrainData.terrainLayers = terrainLayers;
        terrain.terrainData.alphamapResolution = alphaNoise.Size;
        
        Dictionary<Rect, int> dictionary = new Dictionary<Rect, int>();
        foreach (Rect subArea in tree.GetSubAreas())
            dictionary[subArea] = UnityEngine.Random.Range(0, terrainLayers.Length);

        float[,,] alphamap = new float[alphaNoise.Size, alphaNoise.Size, terrainLayers.Length];

        for(int x = 0; x < alphaNoise.Size; x++)
        {
            for (int y = 0; y < alphaNoise.Size; y++)
            {
                Vector2 pos = new Vector2(transform.position.x, transform.position.z);
                Vector2 rand;
                float rx = 0, ry = 0;
                if (x >= noise && x < alphaNoise.Size - noise)
                    rx = Random.Range(-noise, noise);
                if(y >= noise && y < alphaNoise.Size - noise)
                    ry = Random.Range(-noise, noise);
                rand = new Vector2(rx, ry); 
                Rect first = dictionary.Keys.First((r) => r.Contains(pos + new Vector2(x, y) + rand));
                alphamap[x, y, dictionary[first]] = alphaNoise.Map[x, y];
            }
        }
        terrain.terrainData.SetAlphamaps(0, 0, alphamap);

        //PaintRandom();
    }

    public void RePaint()
    {
        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("Este GameObject no tiene un componente Terrain.");
            return;
        }

        Rect area = new Rect(transform.position, Vector2.one * heightNoise.Size);
        tree = BinarySpacePartitionTree.Generate(area, Vector2.one * minSize);


        terrain.terrainData.size = new Vector3(heightNoise.Size, height, heightNoise.Size);

        heightNoise.Generate();
        terrain.terrainData.heightmapResolution = heightNoise.Size;
        terrain.terrainData.SetHeights(0, 0, heightNoise.Map);


        alphaNoise.Generate();
        terrain.terrainData.terrainLayers = terrainLayers;
        terrain.terrainData.alphamapResolution = alphaNoise.Size;

        Dictionary<Rect, int> dictionary = new Dictionary<Rect, int>();
        foreach (Rect subArea in tree.GetSubAreas())
            dictionary[subArea] = UnityEngine.Random.Range(0, terrainLayers.Length);

        float[,,] alphamap = new float[alphaNoise.Size, alphaNoise.Size, terrainLayers.Length];

        for (int x = 0; x < alphaNoise.Size; x++)
        {
            for (int y = 0; y < alphaNoise.Size; y++)
            {
                Vector2 pos = new Vector2(transform.position.x, transform.position.z);
                Vector2 rand;
                float rx = 0, ry = 0;
                if (x >= noise && x < alphaNoise.Size - noise)
                    rx = Random.Range(-noise, noise);
                if (y >= noise && y < alphaNoise.Size - noise)
                    ry = Random.Range(-noise, noise);
                rand = new Vector2(rx, ry);
                Rect first = dictionary.Keys.First((r) => r.Contains(pos + new Vector2(x, y) + rand));
                alphamap[x, y, dictionary[first]] = alphaNoise.Map[x, y];
            }
        }
        terrain.terrainData.SetAlphamaps(0, 0, alphamap);
    }

    private void SetHeightMapToBSP()
    {
        Dictionary<Rect, float> dictionary = new Dictionary<Rect, float>();

        foreach (Rect subArea in tree.GetSubAreas())
            dictionary[subArea] = UnityEngine.Random.Range(-1f, 1f);

        for (int x = 0; x < heightNoise.Size; x++)
        {
            for (int y = 0; y < heightNoise.Size; y++)
            {
                Vector2 pos = new Vector2(transform.position.x, transform.position.z);
                Rect first = dictionary.Keys.First((r) => r.Contains(pos + new Vector2(x, y)));
                heightNoise.Map[x, y] += heightNoise.Map[x, y] + dictionary[first];
            }
        }
    }

    private void PaintRandom()
    {
        foreach (Rect subArea in tree.GetSubAreas())
        {
            TerrainLayer selected = terrainLayers[UnityEngine.Random.Range(0, terrainLayers.Length)];
            PaintContext ctx = TerrainPaintUtility.BeginPaintTexture(terrain, subArea, selected);
            TerrainPaintUtility.EndPaintTexture(ctx, "RandomTerrain");
        }
    }

    public void readHeight(string s)
    {
        if (float.TryParse(s, out float result))
        {
            heightNoise.initialRange = result;
        }
    }
    public void readNoise(string s)
    {
        if (float.TryParse(s, out float result))
        {
            heightNoise.noiseValue = result;
        }
    }
    public void readSize(string s)
    {
        if (float.TryParse(s, out float result))
        {
            heightNoise.Size = (int)result;
        }
    }
}
