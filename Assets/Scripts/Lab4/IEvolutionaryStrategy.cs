using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public interface IEvolutionaryStrategy<TIndividual>
{
    int MaxPopulation { get; }
    protected IEnumerable<TIndividual> Initialize();
    protected TIndividual Mutate(TIndividual parent);
    float FitnessEvaluation(TIndividual individual);
    protected bool TerminationCriteria(IEnumerable<TIndividual> population);
    public sealed TIndividual[] GeneratePopulation(int maxIteration = -1)
    {
        int fitnessComparison(TIndividual x, TIndividual y)
        {
            float xValue = FitnessEvaluation(x);
            float yValue = FitnessEvaluation(y);

            if (xValue > yValue)
                return -1;

            if (xValue < yValue)
                return 1;

            return 0;
        }

        TIndividual[] population = Initialize().ToArray();
        population.QuickSort(fitnessComparison);
        int count = 0;
        while (!TerminationCriteria(population) && (maxIteration == -1 || count < maxIteration))
        {
            TIndividual[] offspring = new TIndividual[population.Length];
            for(int i = 0; i < offspring.Length; i++)
                offspring[i] = Mutate(population[i]);

            TIndividual[] combinedPopulation = population.Concat(offspring).ToArray();
            combinedPopulation.QuickSort(fitnessComparison);

            int length = MaxPopulation;
            TIndividual[] bestIndividuals = new TIndividual[length];
            for (int i = 0; i < length; i++)
            {
                Debug.Log("Gen " + count + " -> Individual " + i + " - Fitness: " + FitnessEvaluation(combinedPopulation[i]));
                bestIndividuals[i] = combinedPopulation[i];
            }

            population = bestIndividuals;
            count++;
        }

        return population;
    }

    public sealed TIndividual GenerateBestIndividual(int maxIteration = -1)
        => GeneratePopulation(maxIteration).First();
}
