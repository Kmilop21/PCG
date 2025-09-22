using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class Player : MonoBehaviour
{
    [field: SerializeField] public Stats MaxStats { private set; get; } 

    public Player()
    {
        MaxStats = new Stats(100, 100, 100);
    }
}
