using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public struct BiomeInfo
{
    [NonSerialized] public List<Rect> Areas;
    [field: SerializeField] public string Name { private set; get; }
    [field: SerializeField] public GameObject[] Flora { private set; get; }
    [field: SerializeField] public TerrainLayer TerrainLayer { private set; get; }  
}
