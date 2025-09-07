using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "L-Systems/Dungeon Maker 3D", fileName = "Dungeon Maker 3D")]
public class DungeonMaker3DLS : LSystem
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Vector3 scale = Vector3.one;

    private int index;
    private (Vector3 position, Vector3 dir) bracketInfo;
    private (Vector3 position, Vector3 dir) movement;
    [System.NonSerialized] private List<Vector3> positions;

    private const float step = 14f;

    private void OnEnable()
    {
        movement.position = Vector3.zero;
        movement.dir = Vector3.forward;
        index = 0;
    }

    public DungeonMaker3DLS()
    {
        positions = new List<Vector3>();
        prefabs = new GameObject[0];
        index = 0;
        bracketInfo = new(Vector2.zero, Vector2.up);
        LoadPosition();

        rules.Add(new Rule("[", string.Empty));

        rules.Add(new Rule("]", string.Empty));

        rules.Add(new Rule("S", "FHI"));

        rules.Add(new Rule("RI", "FHI"));

        rules.Add(new Rule("RX", "FHI+FHI"));

        rules.Add(new Rule("RC", string.Empty));

        rules.Add(new Rule("RT", "FHI"));

        rules.Add(new Rule("HI", "FHI"));

        rules.Add(new Rule("HX", "FHI-FHI"));

        rules.Add(new Rule("HT", "FHI+FHI"));

        rules.Add(new Rule("D", "FHI"));

        rules.Add(new Rule("+", string.Empty));

        rules.Add(new Rule("-", string.Empty));

        //AddPersistentRuleMeaning(this);
    }

    [RuleMeaning("[")]
    public void SavePosition() => bracketInfo = movement;
    [RuleMeaning("]")]
    public void LoadPosition() => movement = bracketInfo;
    [RuleMeaning("+")]
    public void RotateToLeft()
    {
        movement.dir = Quaternion.Euler(0, 0, 90) * movement.dir;
        //Debug.Log(movement.dir);
    }
    [RuleMeaning("-")]
    public void RotateToRight()
    {
        movement.dir = Quaternion.Euler(0, 0, -90) * movement.dir;
        //Debug.Log(movement.dir);
    }
    [RuleMeaning("F")]
    public void Forward()
    {
        //Debug.Log(positions.Count);
        GameObject instance = Instantiate(prefabs[index]);
        do
        {
            movement.position += movement.dir * step;
            instance.transform.position = movement.position;
            instance.transform.rotation = Quaternion.LookRotation(movement.dir, Vector3.up);
        } while (positions.Exists((pos) => (pos == instance.transform.position)));

        positions.Add(instance.transform.position);
    }
}
