using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


public class BalancedEnemy : MonoBehaviour
{
    [SerializeField] private ESBalancedEnemySettler statsSettler;
    [NonSerialized] private Stats stats;
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        stats = statsSettler.GetBalancedStats();
        Debug.Log("Enemy: " + stats);
        if(text != null) text.text = "Enemy stats: " + stats.ToString();
    }
}
