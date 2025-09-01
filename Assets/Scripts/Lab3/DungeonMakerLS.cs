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
    private List<GameObject> instances = new List<GameObject>();
    public DungeonMakerLS()
    {
        prefabs = new GameObject[0];
        index = 0;
        bracketInfo = new(Vector2.zero, Vector2.up);
        LoadPosition();

        grammars.Add(new Grammar('[', string.Empty));

        grammars.Add(new Grammar(']', string.Empty));

        grammars.Add(new Grammar('F', "F[T+F]+T"));

        grammars.Add(new Grammar('T', "-[FF-T]F"));

        grammars.Add(new Grammar('+', string.Empty));

        grammars.Add(new Grammar('-', string.Empty));
    }

    public void SavePosition() => bracketInfo = movement;
    public void LoadPosition() => movement = bracketInfo;
    public void RotateToLeft()
    {
        movement.dir = Quaternion.Euler(0, 0, 90) * movement.dir;
        Debug.Log(movement.dir);
    }
    public void RotateToRight()
    {
        movement.dir = Quaternion.Euler(0, 0, -90) * movement.dir;
        Debug.Log(movement.dir);
    }
    public void Forward()
    {
        GameObject instance = Instantiate(prefabs[index]);
        instance.transform.position = movement.position;
        movement.position += movement.dir * scale;
    }

    public void RandomTile() => index = Random.Range(0, prefabs.Length);
}
