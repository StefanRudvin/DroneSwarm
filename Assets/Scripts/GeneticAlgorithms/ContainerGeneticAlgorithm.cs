using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

/*
 *	This class serves as the backbone for the genetic algorithm.
 * 	
 */
public class ContainerGeneticAlgorithm
{
    private readonly int _height;
    private int _width;
    private readonly int _length;

    private bool debug = false;

    private readonly List<ContainerModel> _containerModels;

    // List of current chromosomes
    private List<List<List<ContainerModel>>> _population = new List<List<List<ContainerModel>>>();

    private List<List<List<ContainerModel>>> _matingPool = new List<List<List<ContainerModel>>>();

    private const int PopulationSize = 100;
    private int _generationCount = 1000;

    public ContainerGeneticAlgorithm(List<ContainerModel> containerModels, int height, int width, int length)
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

        if (debug) Debug.Log(string.Format("Population count: {0}", _population.Count.ToString()));

        EvolvePopulation();
        
        return GetBestChromosome();
    }

    private float GetChromosomeWeightDifference(List<List<ContainerModel>> chromosome)
    {
        var weights = new List<int>();

        // Then the length = 10
        for (var k = 0; k < _length; k++)
        {
            var first = chromosome[0][k];
            var second = chromosome[1][k];
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

    private ContainerPlan GetBestChromosome()
    {
        var bestChromosome = new List<List<ContainerModel>>();
        
        foreach (var chromosome in _population)
        {
            float weightDifference = GetChromosomeWeightDifference(chromosome);
            if (weightDifference != minWeightDifference) continue;
            bestChromosome = chromosome;
            break;
        }
        
        if (debug) Debug.Log(string.Format("Found best chromosome: {0}", bestChromosome));
        
        return new ContainerPlan(bestChromosome);
    }

    private void Mutation()
    {
    }

    private void NaturalSelection()
    {
        var newPopulation = new List<List<List<ContainerModel>>>();
        var random = new Random();

        foreach (var chromosome in _population)
        {
            // Select two random chromosomes from mating pool
            var parentA = _matingPool[random.Next(0, _matingPool.Count)];
            var parentB = _matingPool[random.Next(0, _matingPool.Count)];

            // Add their crossover to the new population
            newPopulation.Add(crossOver(parentA, parentB));
        }

        _population = newPopulation;
    }

    private List<List<ContainerModel>> crossOver(List<List<ContainerModel>> chromosomeA, List<List<ContainerModel>> chromosomeB)
    {
        // Create a new chromosome based on random pivot between 2 different chromosomes.

        var newChromosome = getEmptyChromosome();
        var random = new Random();

        int randomPivot = random.Next(0, _length);

        for (var i = 0; i < _length; i++)
        {
            if (i < randomPivot)
            {
                newChromosome[0][i] = chromosomeA[0][i];
                newChromosome[1][i] = chromosomeA[1][i];
            }
            else
            {
                newChromosome[0][i] = chromosomeB[0][i];
                newChromosome[1][i] = chromosomeB[1][i];
            }
        }
        return newChromosome;
    }

    private void SetMaxMinWeightDifference()
    {
        minWeightDifference = 999f;
        maxWeightDifference = 0f;

        foreach (var chromosome in _population)
        {
            float weightDifference = GetChromosomeWeightDifference(chromosome);
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
        // Add chromosomes to mating pool according to their fitness
        foreach (var chromosome in _population)
        {
            float difference = GetChromosomeWeightDifference(chromosome);

            float fitness = GetFitnessFromDifference(difference);
            fitnesses.Add(fitness);

            for (var k = 0; k < fitness; k++)
            {
                _matingPool.Add(chromosome);
            }
        }
        if (debug) Debug.Log(string.Format("Average fitness for population: {0}",
            (fitnesses.Sum() / fitnesses.Count).ToString()));
    }

    private List<List<ContainerModel>> getEmptyChromosome()
    {
        var chromosome = new List<List<ContainerModel>>();

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

        chromosome.Add(testContainers);
        chromosome.Add(testContainers);

        return chromosome;
    }

    private void CreateInitialPopulation()
    {
        // We have the 20 container models.

        // Time to create a population of e.g. 100 chromosomes that have a mixture of the populations.
        // Each chromosome is a container plan, i.e. a 2d array, 2* 10 containers.

        var random = new Random();

        for (var i = 0; i < PopulationSize; i++)
        {
            var temporaryContainers = new List<ContainerModel>(_containerModels);
            var chromosome = getEmptyChromosome();

            // First do height = 2
            for (var j = 0; j < _height; j++)
            {
                // Then the length = 10
                for (var k = 0; k < _length; k++)
                {
                    // So this happens 2 * 10 times = 20
                    int elementToPick = random.Next(0, temporaryContainers.Count - 1);

                    chromosome[j][k] = temporaryContainers[elementToPick];
                    temporaryContainers.RemoveAt(elementToPick);
                }
            }
            _population.Add(chromosome);
        }
    }
}