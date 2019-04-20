using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace GeneticAlgorithms.Container
{
    public class Chromosome
    {
        public float fitness = 0;
        public List<List<ContainerModel>> _models = new List<List<ContainerModel>>();
        public float weight;
    }
}
