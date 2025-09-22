using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ESBalancedEnemySettler : MonoBehaviour, IEvolutionaryStrategy<Stats>
{
    [SerializeField] private int maxPopulation = 10;
    [SerializeField] private Player playerRef;
    [SerializeField] private int variationRange = 10;
    [SerializeField] private int iterations = -1;
    [SerializeField] private float precision = 0.95f;
    public int MaxPopulation => maxPopulation;
    IEnumerable<Stats> IEvolutionaryStrategy<Stats>.Initialize()
    {
        Stats[] population = new Stats[MaxPopulation];
        for (int i = 0; i < population.Length; i++)
            population[i] = new Stats(Random.Range(1, playerRef.MaxStats.Str + 1), 
                Random.Range(1, playerRef.MaxStats.HP + 1), Random.Range(1, 
                playerRef.MaxStats.Str + 1));
        return population;
    }

    Stats IEvolutionaryStrategy<Stats>.Mutate(Stats parent)
    {
        return new Stats(parent.HP + Random.Range(-variationRange, variationRange), parent.Str
            + Random.Range(-variationRange, variationRange), parent.Def + Random.Range(-variationRange, variationRange));
    }

    public float FitnessEvaluation(Stats individual)
    {
        if (individual.HP <= 0)
            return 0;

        float str = individual.GetEffectiveStr(playerRef.MaxStats);

        if (str <= 0)
            return 0;

        float hp = individual.HP;

        float playerStr = playerRef.MaxStats.GetEffectiveStr(individual);
        float playerHP = playerRef.MaxStats.HP;

        static float fitness(float x, float min, float max)
        {
            if (x >= min && x <= max)
                return (x - min) / (max - min);

            return 0;
        }

        return (fitness(str, playerHP / 3, playerHP / 2) + fitness(playerStr, hp / 2, 3 * hp / 4)) / 2;
    }

    bool IEvolutionaryStrategy<Stats>.TerminationCriteria(IEnumerable<Stats> population)
    {
        return population.Any((individual) => FitnessEvaluation(individual) >= precision); 
    }

    public Stats GetBalancedStats()
    {
        IEvolutionaryStrategy<Stats> ies = this;
        return ies.GenerateBestIndividual(iterations);
    }
}

