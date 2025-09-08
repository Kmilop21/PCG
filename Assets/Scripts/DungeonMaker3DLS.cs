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
        movement.position = new Vector3(100, 20, 30);
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

        rules.Add(new Rule("F", "FHXF-RXF-"));

        rules.Add(new Rule("S", "F"));

        rules.Add(new Rule("RI", "HIF"));

        rules.Add(new Rule("RX", "-HTF+HTF"));

        rules.Add(new Rule("RC", string.Empty));

        rules.Add(new Rule("RT", "-HRFHIF+RXF"));

        rules.Add(new Rule("HI", "HIF"));

        rules.Add(new Rule("HX", "HIF-HXF"));

        rules.Add(new Rule("HT", "HIF+HRF"));

        rules.Add(new Rule("D", "HIF"));

        rules.Add(new Rule("+", "+"));

        rules.Add(new Rule("-", "-"));

        //AddPersistentRuleMeaning(this);
    }

    [RuleMeaning("[")]
    public void SavePosition() => bracketInfo = movement;
    [RuleMeaning("]")]
    public void LoadPosition() => movement = bracketInfo;
    [RuleMeaning("+")]
    public void RotateToLeft()
    {
        movement.dir = Quaternion.Euler(0, 90, 0) * movement.dir;
        //Debug.Log(movement.dir);
    }
    [RuleMeaning("-")]
    public void RotateToRight()
    {
        movement.dir = Quaternion.Euler(0, -90, 0) * movement.dir;
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

            Quaternion correction = Quaternion.Euler(-90, 0, 0);
            instance.transform.rotation = Quaternion.LookRotation(movement.dir, Vector3.up) * correction;
        } while (positions.Exists((pos) => (pos == instance.transform.position)));

        positions.Add(instance.transform.position);
    }

    [RuleMeaning("S")]
    public void SMeaning() => index = 0;

    [RuleMeaning("RI")]
    public void RIMeaning() => index = 1;

    [RuleMeaning("RX")]
    public void RXMeaning() => index = 2;

    [RuleMeaning("RC")]
    public void RCMeaning() => index = 3;

    [RuleMeaning("RT")]
    public void RTMeaning() => index = 4;

    [RuleMeaning("HI")]
    public void HIMeaning() => index = 5;

    [RuleMeaning("HX")]
    public void HXMeaning() => index = 6;

    [RuleMeaning("HT")]
    public void HTMeaning() => index = 7;

    [RuleMeaning("D")]
    public void DMeaning() => index = 8;
}
