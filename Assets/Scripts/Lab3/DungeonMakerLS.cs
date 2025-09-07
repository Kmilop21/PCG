using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "L-Systems/Dungeon Maker", fileName = "Dungeon Maker")]
public class DungeonMakerLS : LSystem
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Vector2 scale = Vector2.one;

    private int index;
    private (Vector2 position, Vector2 dir) bracketInfo;
    private (Vector2 position, Vector2 dir) movement;
    [System.NonSerialized] private List<Vector3> positions;
    public DungeonMakerLS()
    {
        positions = new List<Vector3>();
        prefabs = new GameObject[0];
        index = 0;
        bracketInfo = new(Vector2.zero, Vector2.up);
        LoadPosition();

        rules.Add(new Rule("[", string.Empty));

        rules.Add(new Rule("]", string.Empty));

        rules.Add(new Rule("F", "F[T+F]+T"));

        rules.Add(new Rule("T", "-[FF-T]F"));

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
            movement.position += movement.dir * scale;
            instance.transform.position = movement.position;
        } while (positions.Exists((pos) => (pos == instance.transform.position)));

        positions.Add(instance.transform.position);
    }
    [RuleMeaning("T")]
    public void RandomTile() => index = Random.Range(0, prefabs.Length);
}
