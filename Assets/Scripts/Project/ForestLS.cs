using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "L-Systems/Forest", fileName = "Forest LS")]
public class ForestLS : LSystem
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Vector3 scale = 0.4f * Vector3.one;
    [SerializeField] private Vector3 axis = Vector3.up;
    [SerializeField] private float minScaleValue = 0.75f;
    [SerializeField] private float maxScaleValue = 1.5f;

    private (Vector3 position, Vector3 dir, Vector3 scale, int index) savedState;
    private (Vector3 position, Vector3 dir, Vector3 scale, int index) state;
    [System.NonSerialized] private List<Vector3> positions;

    private bool first;

    [NonSerialized] public RandomBiomeGenerator Ref;
    public ForestLS()
    {
        positions = new List<Vector3>();
        prefabs = new GameObject[0];
        savedState = new(Vector3.zero, Vector3.forward, Vector3.one, 0);

        LoadPosition();

        rules.Add(new Rule("[", string.Empty));

        rules.Add(new Rule("]", string.Empty));

        rules.Add(new Rule("F", "F[S-F]FS[+FPSF][PF]-"));

        rules.Add(new Rule("S", "S[+F-SF]PF[-F]PS"));

        rules.Add(new Rule("P", "P[FP-]-FPS+[-F]S+"));

        rules.Add(new Rule("+", string.Empty));

        rules.Add(new Rule("-", string.Empty));
    }

    public override void Initialize()
    {
        state.position = Ref.transform.position + new Vector3(257, 0, 257) / 2;
        state.dir = Vector3.forward;
        SavePosition();
    }
    [RuleMeaning("[")]
    public void SavePosition() => savedState = state;
    [RuleMeaning("]")]
    public void LoadPosition() => state = savedState;
    [RuleMeaning("+")]
    public void RotateToLeft()
    {
        Vector3 pos;
        do
        {
            state.dir = Quaternion.Euler(45 * axis) * state.dir;
            pos = state.position + new Vector3(state.dir.x * scale.x, state.dir.y * scale.y, state.dir.z * scale.z);
        } while (Ref.IsOutSide(pos.x, pos.z));
    }
    [RuleMeaning("-")]
    public void RotateToRight()
    {
        Vector3 pos;
        do
        {
            state.dir = Quaternion.Euler(-45 * axis) * state.dir;
            pos = state.position + new Vector3(state.dir.x * scale.x, state.dir.y * scale.y, state.dir.z * scale.z);
        } while (Ref.IsOutSide(pos.x, pos.z));
        //Debug.Log(movement.dir);
    }
    [RuleMeaning("F")]
    public void Forward()
    {
        Vector3 pos = state.position + new Vector3(state.dir.x * scale.x, state.dir.y * scale.y, state.dir.z * scale.z);

        if (Ref.IsOutSide(pos.x, pos.z))
        {
            RotateToLeft();
            pos = state.position + new Vector3(state.dir.x * scale.x, state.dir.y * scale.y, state.dir.z * scale.z);
        }

        state.position = pos;
        BiomeInfo currentBiome = Ref.GetCurrentBiome(state.position.x, state.position.z);
        if (currentBiome.Flora.Length > 0)
        {
            GameObject instance = Instantiate(currentBiome.Flora[0]);
            float y = Ref.GetHeight((int)state.position.x, (int)state.position.z);
            instance.transform.position = new Vector3(state.position.x, y, state.position.z);
        }
        else
            Debug.Log("No plant");
    }

    [RuleMeaning("S")]
    public void SetRandomScale() => state.scale = Vector3.one * UnityEngine.Random.Range(minScaleValue, maxScaleValue);
    [RuleMeaning("P")]
    public void SetRandomPrefab() => state.index = UnityEngine.Random.Range(0, prefabs.Length);
}
