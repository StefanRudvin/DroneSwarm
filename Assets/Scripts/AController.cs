using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AController : MonoBehaviour
{
    public GameObject Target;
    public bool waitingAtTarget;
    public int level;
}