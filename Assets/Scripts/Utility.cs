using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    public static Color createColorFromWeight(int weight)
    {
        return new Color(
            1000 / 255 * weight,
            1000 / 255 * weight,
            1000 / 255 * weight
        );
    }
}