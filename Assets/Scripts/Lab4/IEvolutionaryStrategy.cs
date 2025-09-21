using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

public interface IEvolutionaryStrategy<TIndividual>
{
    protected IEnumerable<TIndividual> Initialize();
    protected TIndividual Mutate(TIndividual parent);
    protected float FitnessEvaluation(TIndividual individual);
    protected bool TerminationCriteria(IEnumerable<TIndividual> population);
    public sealed TIndividual Generate(int populationMax, int maxIteration = -1)
    {
        int compareIndividuals(TIndividual x, TIndividual y)
        {
            float xValue = FitnessEvaluation(x);
            float yValue = FitnessEvaluation(y);

            if (xValue > yValue)
                return 1;

            if (xValue < yValue)
                return -1;

            return 0;
        }

        TIndividual[] population = Initialize().ToArray();
        population.QuickSort(compareIndividuals);

        int count = 0;
        while (!TerminationCriteria(population) && (maxIteration == -1 || count < maxIteration))
        {
            TIndividual[] offspring = new TIndividual[population.Length];
            for(int i = 0; i < offspring.Length; i++)
                offspring[i] = Mutate(population[i]);

            TIndividual[] combinedPopulation = population.Concat(offspring).ToArray();
            combinedPopulation.QuickSort(compareIndividuals);

            int length = populationMax < combinedPopulation.Length ? populationMax : combinedPopulation.Length;
            TIndividual[] bestIndividuals = new TIndividual[length];
            for (int i = 0; i < length; i++)
                bestIndividuals[i] = combinedPopulation[i];

            population = bestIndividuals;
            count++;
        }

        return population.First();
    }
}
