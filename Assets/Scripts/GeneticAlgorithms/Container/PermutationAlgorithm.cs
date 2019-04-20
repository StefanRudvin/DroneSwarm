using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;
using UnityEngineInternal;
using Random = System.Random;

/*
 *	This class serves as the backbone for the genetic algorithm.
 * 	
 */

namespace GeneticAlgorithms.Container
{
    public class PermutationAlgorithm
    {
        
        private readonly int _height;
        private int _width;
        private readonly int _length;
        
        private string filePath = "Assets/Results/containerResults.csv";
        
        private Dictionary<int, float> _resultsDictionary = new Dictionary<int, float>();

        private bool debug = false;
        
        public List<float> lastGenFitnesses = new List<float>();

        private readonly List<ContainerModel> _containerModels;

        // List of current chromosomes
        private List<Chromosome> _population = new List<Chromosome>();

        private List<Chromosome> _matingPool = new List<Chromosome>();

        private const int PopulationSize = 100;
        private int _generationCount = 100;

        private Chromosome _fittestChromosome;
        private float _fittestChromosomeWeight = 99999999999999f;

        public PermutationAlgorithm(List<ContainerModel> containerModels, int height, int width, int length)
        {
            _containerModels = containerModels;
            _height = height;
            _width = width;
            _length = length;
        }

        private float maxWeightDifference = 0f;
        private float minWeightDifference = 9999f;
        
        private void Reset()
        {
            _matingPool.Clear();
            _population.Clear();
            _fittestChromosome = null;
            _fittestChromosomeWeight = 1000000000;
            _resultsDictionary.Clear();
        }

        public ContainerPlan CreateOptimalContainerPlan()
        {
            Reset();
            CreateInitialPopulation();

            if (debug)
            {
                DateTime dateTime = DateTime.Now;

                EvolvePopulation();

                TimeSpan permutationTime = DateTime.Now.Subtract(dateTime);

                WriteResultsToFile();
            }
            else
            {
                EvolvePopulation();
            }

            return GetBestChromosome();
        }
        
        private void WriteResultsToFile()
        {
            lastGenFitnesses.Add(_resultsDictionary[99]);

            string[] arrLine = File.ReadAllLines(filePath);

            int counter = 0;

            foreach (var entry in _resultsDictionary)
            {
                arrLine[counter] = String.Format("{0}, {1}", arrLine[counter].ToString(), entry.Value.ToString());
                counter++;
            }

            File.WriteAllLines(filePath, arrLine);
        }

        private float GetChromosomeWeightDifference(Chromosome chromosome)
        {
            List<int> weights = new List<int>();

            // Then the length = 10
            for (int k = 0; k < _length; k++)
            {
                ContainerModel first = chromosome._models[0][k];
                ContainerModel second = chromosome._models[1][k];
                weights.Add(first._weight + second._weight);
            }

            // First check the pivot difference.
            // Then the difference in quarters.
            // End up with 3 differences.

            float halfDifference =
                Math.Abs(weights.Take(weights.Count / 2).Sum() - weights.Skip(weights.Count / 2).Sum());

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
                SetChromosomeFitnesses();

//                if (maxWeightDifference == minWeightDifference) break;

                CreateMatingPool();

                NaturalSelection();
                
                _resultsDictionary.Add(i, _fittestChromosomeWeight);
                
                Mutation();
            }
        }

        private void SetChromosomeFitnesses()
        {
            SetMinMaxValues();

            foreach (var chromosome in _population)
            {
                chromosome.fitness = GetFitnessFromChromosome(chromosome);
            }
        }

        private ContainerPlan GetBestChromosome()
        {
            return new ContainerPlan(_fittestChromosome._models);
        }

        private void Mutation()
        {
            /*
             * Do some swapping with container models.
             * Up and down perchance.
             */
        }

        private void NaturalSelection()
        {
            var newPopulation = new List<Chromosome>();
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

        private Chromosome crossOver(Chromosome chromosomeA,
            Chromosome chromosomeB)
        {
            // Create a new chromosome based on random pivot between 2 different chromosomes.

            var newChromosome = getEmptyChromosome();
            var random = new Random();

            int randomPivot = random.Next(0, _length);

            for (var i = 0; i < _length; i++)
            {
                if (i < randomPivot)
                {
                    newChromosome._models[0][i] = chromosomeA._models[0][i];
                    newChromosome._models[1][i] = chromosomeA._models[1][i];
                }
                else
                {
                    newChromosome._models[0][i] = chromosomeB._models[0][i];
                    newChromosome._models[1][i] = chromosomeB._models[1][i];
                }
            }

            return newChromosome;
        }

        private void SetMinMaxValues()
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

        private float GetWeightFitnessFromDifference(float difference)
        {
            if (minWeightDifference == maxWeightDifference && difference == minWeightDifference)
            {
                return 100;
            }

            float normalizedDifference =
                (difference - minWeightDifference) / (maxWeightDifference - minWeightDifference);
            return (1 - normalizedDifference) * 100;
        }

        private float GetFitnessFromChromosome(Chromosome chromosome)
        {
            float difference = GetChromosomeWeightDifference(chromosome);

            float weightFitness = GetWeightFitnessFromDifference(difference);

            float priorityWeight = GetPriorityWeight(chromosome);
            
            // This is as low as possible
            float normalizedPriorityFitness = (priorityWeight - 0) / (3000 - 0);
            
            // So revert is with 1 - fitness
            float priorityFitness = (1 - normalizedPriorityFitness) * 100;

            // 50 / 50 weighting for final value.
            float averageFitness = (priorityFitness + weightFitness) / 2;

            chromosome.weight = priorityWeight + difference;


            if (chromosome.weight < _fittestChromosomeWeight)
            {
                _fittestChromosome = chromosome;
                _fittestChromosomeWeight = chromosome.weight;
            }
            
            return averageFitness;
        }

        private float GetPriorityWeight(Chromosome chromosome)
        {
            float weight = 0f;
            
            for (int i = 0; i < chromosome._models.Count; i++)
            {
                List<ContainerModel> containerModels = chromosome._models[i];

                // Top layer has a higher penalty, so higher priorities go higher up on the boat.
                int multiplier = i == 0 ? 1 : 2;
                
                foreach (ContainerModel containerModel in containerModels)
                {
                    weight += (100 - containerModel._orderPriority) * multiplier;
                }
            }
            return weight;
        }

        private void CreateMatingPool()
        {
            _matingPool.Clear();

            var fitnesses = new List<float>();
            // Add chromosomes to mating pool according to their fitness
            foreach (var chromosome in _population)
            {
                fitnesses.Add(chromosome.fitness);

                for (var k = 0; k < chromosome.fitness; k++)
                {
                    _matingPool.Add(chromosome);
                }
            }

            if (debug)
                Debug.Log(string.Format("Average fitness for population: {0}",
                    (fitnesses.Sum() / fitnesses.Count).ToString()));
        }

        private Chromosome getEmptyChromosome()
        {
            var chromosome = new Chromosome();

            List<ContainerModel> testContainersA = new List<ContainerModel>
            {
                _containerModels[10],
                _containerModels[11],
                _containerModels[12],
                _containerModels[13],
                _containerModels[14],
                _containerModels[15],
                _containerModels[16],
                _containerModels[17],
                _containerModels[18],
                _containerModels[19]
            };

            List<ContainerModel> testContainersB = new List<ContainerModel>
            {
                _containerModels[0],
                _containerModels[1],
                _containerModels[2],
                _containerModels[3],
                _containerModels[4],
                _containerModels[5],
                _containerModels[6],
                _containerModels[7],
                _containerModels[8],
                _containerModels[9]
            };

            chromosome._models.Add(testContainersA);
            chromosome._models.Add(testContainersB);

            return chromosome;
        }

        private void CreateInitialPopulation()
        {
            // We have the 20 container models.

            // Time to create a population of e.g. 100 chromosomes that have a mixture of the populations.
            // Each chromosome is a container plan, i.e. a 2d array, 2* 10 containers.

            var random = new Random();

            for (int i = 0; i < PopulationSize; i++)
            {
                List<ContainerModel> temporaryContainers = new List<ContainerModel>(_containerModels);
                Chromosome chromosome = getEmptyChromosome();

                // First do height = 2
                for (int j = 0; j < _height; j++)
                {
                    // Then the length = 10
                    for (int k = 0; k < _length; k++)
                    {
                        // So this happens 2 * 10 times = 20
                        int elementToPick = random.Next(0, temporaryContainers.Count - 1);

                        chromosome._models[j][k] = temporaryContainers[elementToPick];
                        temporaryContainers.RemoveAt(elementToPick);
                    }
                }

                _population.Add(chromosome);
            }
        }
        
    }
}