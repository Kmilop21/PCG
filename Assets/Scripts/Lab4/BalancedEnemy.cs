using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class BalancedEnemy : MonoBehaviour
{
    [SerializeField] private ESBalancedEnemySettler statsSettler;
    [NonSerialized] private Stats stats;

    private void Start()
    {
        stats = statsSettler.GetBalancedStats();
        Debug.Log("Enemy: " + stats);
    }
}
