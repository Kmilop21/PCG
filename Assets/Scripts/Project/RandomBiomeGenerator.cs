using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.TerrainTools;

public class RandomBiomeGenerator : MonoBehaviour
{
    [SerializeField] private float minSize = 20f;
    [SerializeField] private DiamondSquare heightNoise = new DiamondSquare(257, 10f, 0.5f);
    [SerializeField] private DiamondSquare alphaNoise = new DiamondSquare(257, 10f, 0.5f);
    [SerializeField] private float height = 20f;
    [SerializeField] private BiomeInfo[] biomes;
    private float noise = 1;
    [SerializeField] private LSystemReader dungeonReader;
    private Terrain terrain;
    private BinarySpacePartitionTree tree;
    public bool IsReady { private set; get; }
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
        Generate();
        //PaintRandom();
    }

    private IEnumerator SetTerrainToDungeon()
    {
        yield return new WaitUntil(() => dungeonReader.Initialized);

        DungeonMaker3DLS dungeonMaker = dungeonReader.LSystem as DungeonMaker3DLS;

        List<(int y, int x)> toLate = new List<(int y, int x)>();
        float minY = float.MaxValue;
        
        for(int x = 0; x < heightNoise.Size; x++)
        {
            for(int y = 0; y < heightNoise.Size; y++)
            {
                Vector3 terrainPos = transform.position + new Vector3(x, 0, y);
                bool check(Vector3 p) => Vector3.Distance(p, terrainPos) <= 15f;
                if (dungeonMaker.Positions.Exists(check))
                {
                    Vector3 pos = dungeonMaker.Positions.First(check);
                    if (minY > heightNoise.Map[y, x])
                        minY = heightNoise.Map[y, x];

                    toLate.Add((y, x));
                }
            }
        }

        foreach ((int y, int x) coord in toLate)
            heightNoise.Map[coord.y, coord.x] = minY;


        terrain.terrainData.SetHeights(0, 0, heightNoise.Map);
    }
    private void SetBiomes()
    {
        alphaNoise.Generate();
        terrain.terrainData.terrainLayers = biomes.Select((b) => b.TerrainLayer).ToArray();
        terrain.terrainData.alphamapResolution = alphaNoise.Size;

        Dictionary<Rect, int> dictionary = new Dictionary<Rect, int>();
        foreach (Rect subArea in tree.GetSubAreas())
        {
            int i = Random.Range(0, biomes.Length);
            dictionary[subArea] = i;
            biomes[i].Areas.Add(subArea);
        }

        float[,,] alphamap = new float[alphaNoise.Size, alphaNoise.Size, biomes.Length];

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
                alphamap[y, x, dictionary[first]] = alphaNoise.Map[y, x];
            }
        }
        terrain.terrainData.SetAlphamaps(0, 0, alphamap);
    }
    public float GetHeight(int x, int y)
    {
        return terrain.terrainData.GetHeight(x, y);
    }

    public BiomeInfo GetCurrentBiome(float x, float y)
    {
        return biomes.First((b) =>
        {
            foreach(Rect area in b.Areas)
            {
                if (area.Contains(new Vector2(x, y)))
                    return true;
            }
            return false;
        });
    }

    public bool IsOutSide(float x, float y)
    {
        Rect area = new Rect(transform.position, Vector2.one * heightNoise.Size);

        return !area.Contains(new Vector2(x, y));
    }

    public void Generate()
    {
        for (int i = 0; i < biomes.Length; i++)
            biomes[i].Areas = new List<Rect>();

        LSystemReader reader = GetComponent<LSystemReader>();
        ForestLS fls = reader.LSystem as ForestLS;
        fls.Ref = this;
        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("Este GameObject no tiene un componente Terrain.");
            return;
        }

        Rect area = new Rect(transform.position, Vector2.one * heightNoise.Size);
        tree = BinarySpacePartitionTree.Generate(area, Vector2.one * minSize);
        terrain.terrainData.size = new Vector3(heightNoise.Size, heightNoise.InitialRange, heightNoise.Size);
        heightNoise.Generate();
        terrain.terrainData.heightmapResolution = heightNoise.Size;
        terrain.terrainData.SetHeights(0, 0, heightNoise.Map);


        SetBiomes();

        reader.Initialize();

        StartCoroutine(SetTerrainToDungeon());
    }

    public void readHeight(string s)
    {
        if (float.TryParse(s, out float result))
        {
            heightNoise.InitialRange = result;
        }
    }
    public void readNoise(string s)
    {
        if (float.TryParse(s, out float result))
        {
            heightNoise.Roughness = result;
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
