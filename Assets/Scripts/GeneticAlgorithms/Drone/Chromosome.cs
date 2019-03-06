using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace GeneticAlgorithms.Drone
{
    public class Chromosome
    {
        public List<DroneCollection> _droneCollection = new List<DroneCollection>();
        public float _fitness;

        public float _weight;

        public Chromosome()
        {
        }

        public Chromosome(List<DroneCollection> droneCollection)
        {
            _droneCollection = droneCollection;
        }
    }
}