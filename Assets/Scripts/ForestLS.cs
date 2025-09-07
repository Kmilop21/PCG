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
    private (Vector3 position, Vector3 dir, Vector3 scale, int index) currentState;
    [System.NonSerialized] private List<Vector3> positions;
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

    [RuleMeaning("[")]
    public void SavePosition() => savedState = currentState;
    [RuleMeaning("]")]
    public void LoadPosition() => currentState = savedState;
    [RuleMeaning("+")]
    public void RotateToLeft()
    {
        currentState.dir = Quaternion.Euler(45 * axis) * currentState.dir;
        //Debug.Log(movement.dir);
    }
    [RuleMeaning("-")]
    public void RotateToRight()
    {
        currentState.dir = Quaternion.Euler(-45 * axis) * currentState.dir;
        //Debug.Log(movement.dir);
    }
    [RuleMeaning("F")]
    public void Forward()
    {
        //Debug.Log(positions.Count);
        GameObject instance = Instantiate(prefabs[currentState.index]);

        do
        {
            currentState.position += new Vector3(currentState.dir.x * scale.x, currentState.dir.y * scale.y, currentState.dir.z * scale.z);
            instance.transform.position = currentState.position;
            instance.transform.localScale = currentState.scale;
        } while (positions.Exists((pos) => (pos == instance.transform.position)));

        positions.Add(instance.transform.position);
    }

    [RuleMeaning("S")]
    public void SetRandomScale() => currentState.scale = Vector3.one * UnityEngine.Random.Range(minScaleValue, maxScaleValue);
    [RuleMeaning("P")]
    public void SetRandomPrefab() => currentState.index = UnityEngine.Random.Range(0, prefabs.Length);
}
