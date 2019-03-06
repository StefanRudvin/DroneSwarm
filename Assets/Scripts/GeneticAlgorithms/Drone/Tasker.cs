using System.Collections.Generic;
using UnityEngine;

namespace GeneticAlgorithms.Drone
{
    public class Tasker
    {
        public List<Task> _tasks = new List<Task>();

        public void addTask(Task task)
        {
            _tasks.Add(task);
        }
    }
}