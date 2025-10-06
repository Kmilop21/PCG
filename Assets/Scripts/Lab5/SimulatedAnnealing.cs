using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedAnnealing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public int[,] Run(FitnessFunction fitnessFunction, GenerateNeighbor generateNeighbor, int[,] startingSolution,float startingTemperature, float minTemperature, float coolingRate)
    //{
    //    int[,] currentSolution = startingSolution;
    //    float currentValue = fitnessFunction(currentSolution);
    //    float temperature = startingTemperature;

    //    while(temperature > minTemperature)
    //    {
    //        int[,] neighbor = generateNeighbor(currentSolution);
    //        float neighborValue = fitnessFunction(neighbor);

    //        float deltaE = neighborValue - currentValue;

    //        if(deltaE > 0)
    //        {
    //            currentSolution = neighbor;
    //            currentValue = neighborValue;
    //        }
    //        else
    //        {
    //            float prob = Mathf.Exp(deltaE/temperature);
    //            if (Random.Range(0, 1) < prob)
    //            {
    //                currentSolution = neighbor;
    //                currentValue = neighborValue;
    //            }
    //        }
    //        temperature = temperature * coolingRate;
    //    }
    //    return currentSolution;
    //}
}
