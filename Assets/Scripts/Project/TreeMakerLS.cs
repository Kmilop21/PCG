using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "L-Systems/Tree Maker", fileName = "Tree Maker")]
public class TreeMakerLS : LSystem
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Vector3 scale = Vector3.one;

    private int index;
    private (Vector3 position, Vector3 dir) bracketInfo;
    private (Vector3 position, Vector3 dir) movement;
    [System.NonSerialized] private List<Vector3> positions;
    public TreeMakerLS()
    {
        positions = new List<Vector3>();
        prefabs = new GameObject[0];
        index = 0;
        bracketInfo = new(Vector3.zero, Vector3.up);
        LoadPosition();

        rules.Add(new Rule("[", string.Empty));

        rules.Add(new Rule("]", string.Empty));

        rules.Add(new Rule("F", "F[-F]F[+F][F]-"));

        rules.Add(new Rule("+", string.Empty));

        rules.Add(new Rule("-", string.Empty));
    }

    [RuleMeaning("[")]
    public void SavePosition() => bracketInfo = movement;
    [RuleMeaning("]")]
    public void LoadPosition() => movement = bracketInfo;
    [RuleMeaning("+")]
    public void RotateToLeft()
    {
        movement.dir = Quaternion.Euler(0, 0, 45) * movement.dir;
        //Debug.Log(movement.dir);
    }
    [RuleMeaning("-")]
    public void RotateToRight()
    {
        movement.dir = Quaternion.Euler(0, 0, -45) * movement.dir;
        //Debug.Log(movement.dir);
    }
    [RuleMeaning("F")]
    public void Forward()
    {
        //Debug.Log(positions.Count);
        GameObject instance = Instantiate(prefabs[index]);

        do
        {
            movement.position += new Vector3(movement.dir.x * scale.x, movement.dir.y * scale.y, movement.dir.z * scale.z);
            instance.transform.position = movement.position;
            instance.transform.rotation = Quaternion.identity * Quaternion.LookRotation(movement.dir);
        } while (positions.Exists((pos) => (pos == instance.transform.position)));

        positions.Add(instance.transform.position);
    }
}
