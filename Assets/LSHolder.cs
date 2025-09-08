using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSHolder : MonoBehaviour
{
    public static LSHolder instance;
    public List<GameObject> list;

    private void Awake()
    {
        instance = this;
    }
}
