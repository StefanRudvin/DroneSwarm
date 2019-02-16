using System.Collections.Generic;
using UnityEngine;

public class Tasker
{
    public List<Task> _tasks = new List<Task>();

    public void addTask(GameObject gameObject, Vector3 startLocation, Vector3 endLocation, float length, Task nextTask = null)
    {
        _tasks.Add(new Task(gameObject, startLocation, endLocation, length, nextTask));
    }
    
    public void addTask(Task task)
    {
        _tasks.Add(task);
    }
}