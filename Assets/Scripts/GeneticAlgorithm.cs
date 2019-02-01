using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

/*
 *	This class serves as the backbone for the genetic algorithm.
 * 	
 */
public class GeneticAlgorithm
{
    private readonly int _height;
    private int _width;
    private readonly int _length;

    private readonly List<ContainerModel> _containerModels;

    // List of current genes
    private List<List<List<ContainerModel>>> _population = new List<List<List<ContainerModel>>>();

    private List<List<List<ContainerModel>>> _matingPool = new List<List<List<ContainerModel>>>();

    private const int PopulationSize = 100;
    private int _generationCount = 1000;

    public GeneticAlgorithm(List<ContainerModel> containerModels, int height, int width, int length)
    {
        _containerModels = containerModels;
        _height = height;
        _width = width;
        _length = length;
    }

    private float maxWeightDifference = 0f;
    private float minWeightDifference = 9999f;

    public ContainerPlan CreateOptimalContainerPlan()
    {
        CreateInitialPopulation();

        Debug.Log(string.Format("Population count: {0}", _population.Count.ToString()));

        EvolvePopulation();
        
        return GetBestGene();
    }

    private float GetGeneWeightDifference(List<List<ContainerModel>> gene)
    {
        var weights = new List<int>();

        // Then the length = 10
        for (var k = 0; k < _length; k++)
        {
            var first = gene[0][k];
            var second = gene[1][k];
            weights.Add(first._weight + second._weight);
        }

        // First check the pivot difference.
        // Then the difference in quarters.
        // End up with 3 differences.

        float halfDifference = Math.Abs(weights.Take(weights.Count / 2).Sum() - weights.Skip(weights.Count / 2).Sum());

        int weightsLength = weights.Count;
        int halfWeightsLength = weightsLength / 2;
        int quarterWeightsLength = halfWeightsLength / 2;

        float firstHalfDifference = Math.Abs(weights.GetRange(0, quarterWeightsLength).Sum() -
                                             weights.GetRange(quarterWeightsLength, quarterWeightsLength).Sum());
        float secondHalfDifference = Math.Abs(weights.GetRange(halfWeightsLength, quarterWeightsLength).Sum() -
                                              weights.GetRange(halfWeightsLength + quarterWeightsLength,
                                                  quarterWeightsLength).Sum());

        float averageDifference = (halfDifference + firstHalfDifference + secondHalfDifference) / 3;

        return averageDifference;
    }

    private void EvolvePopulation()
    {
        for (var i = 0; i < _generationCount; i++)
        {
            SetMaxMinWeightDifference();

            if (maxWeightDifference == minWeightDifference)
            {
                // Optimized as well as it can.
                break;
            }

            CreateMatingPool();

            NaturalSelection();

            Mutation();
        }
    }

    private ContainerPlan GetBestGene()
    {
        var bestGene = new List<List<ContainerModel>>();
        
        foreach (var gene in _population)
        {
            float weightDifference = GetGeneWeightDifference(gene);
            if (weightDifference != minWeightDifference) continue;
            bestGene = gene;
            break;
        }
        
        Debug.Log(string.Format("Found best gene: {0}", bestGene));
        
        return new ContainerPlan(bestGene);
    }

    private void Mutation()
    {
    }

    private void NaturalSelection()
    {
        var newPopulation = new List<List<List<ContainerModel>>>();
        var random = new Random();

        foreach (var gene in _population)
        {
            // Select two random genes from mating pool
            var parentA = _matingPool[random.Next(0, _matingPool.Count)];
            var parentB = _matingPool[random.Next(0, _matingPool.Count)];

            // Add their crossover to the new population
            newPopulation.Add(crossOver(parentA, parentB));
        }

        _population = newPopulation;
    }

    private List<List<ContainerModel>> crossOver(List<List<ContainerModel>> geneA, List<List<ContainerModel>> geneB)
    {
        // Create a new gene based on random pivot between 2 different genes.

        var newGene = getEmptyGene();
        var random = new Random();

        int randomPivot = random.Next(0, _length);

        for (int i = 0; i < _length; i++)
        {
            if (i < randomPivot)
            {
                newGene[0][i] = geneA[0][i];
                newGene[1][i] = geneA[1][i];
            }
            else
            {
                newGene[0][i] = geneB[0][i];
                newGene[1][i] = geneB[1][i];
            }
        }
        return newGene;
    }

    private void SetMaxMinWeightDifference()
    {
        minWeightDifference = 999f;
        maxWeightDifference = 0f;

        foreach (var gene in _population)
        {
            float weightDifference = GetGeneWeightDifference(gene);
            if (weightDifference > maxWeightDifference)
            {
                maxWeightDifference = weightDifference;
            }

            if (weightDifference < minWeightDifference)
            {
                minWeightDifference = weightDifference;
            }
        }
    }

    private float GetFitnessFromDifference(float difference)
    {
        if (minWeightDifference == maxWeightDifference && difference == minWeightDifference)
        {
            return 100;
        }
        float normalizedDifference = (difference - minWeightDifference) / (maxWeightDifference - minWeightDifference);
        return (1 - normalizedDifference) * 100;
    }

    private void CreateMatingPool()
    {
        _matingPool.Clear();

        var fitnesses = new List<float>();
        // Add genes to mating pool according to their fitness
        foreach (var gene in _population)
        {
            float difference = GetGeneWeightDifference(gene);

            float fitness = GetFitnessFromDifference(difference);
            fitnesses.Add(fitness);

            for (var k = 0; k < fitness; k++)
            {
                _matingPool.Add(gene);
            }
        }
        Debug.Log(string.Format("Average fitness for population: {0}",
            (fitnesses.Sum() / fitnesses.Count).ToString()));
    }

    private List<List<ContainerModel>> getEmptyGene()
    {
        var gene = new List<List<ContainerModel>>();

        List<ContainerModel> testContainers = new List<ContainerModel>
        {
            _containerModels[0],
            _containerModels[0],
            _containerModels[0],
            _containerModels[0],
            _containerModels[0],
            _containerModels[0],
            _containerModels[0],
            _containerModels[0],
            _containerModels[0],
            _containerModels[0]
        };

        gene.Add(testContainers);
        gene.Add(testContainers);

        return gene;
    }

    private void CreateInitialPopulation()
    {
        // We have the 20 container models.

        // Time to create a population of e.g. 100 Genes that have a mixture of the populations.
        // Each Gene is a container plan, i.e. a 2d array, 2* 10 containers.

        var random = new Random();

        for (var i = 0; i < PopulationSize; i++)
        {
            var temporaryContainers = new List<ContainerModel>(_containerModels);
            var gene = getEmptyGene();

            // First do height = 2
            for (var j = 0; j < _height; j++)
            {
                // Then the length = 10
                for (var k = 0; k < _length; k++)
                {
                    // So this happens 2 * 10 times = 20
                    int elementToPick = random.Next(0, temporaryContainers.Count - 1);

                    gene[j][k] = temporaryContainers[elementToPick];
                    temporaryContainers.RemoveAt(elementToPick);
                }
            }
            _population.Add(gene);
        }
    }
}