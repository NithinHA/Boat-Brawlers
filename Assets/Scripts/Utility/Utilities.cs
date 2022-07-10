using System.Linq;
using UnityEngine;

public class Utilities
{
    /// <summary>
    /// Performs roulette selection based on weights.
    /// </summary>
    public static int GetWeightedRandomIndex(float[] weights)
    {
        float weightSum = weights.Sum();
        int index = 0;
        int lastIndex = weights.Length - 1;
        while (index < lastIndex)
        {
            if (Random.Range(0, weightSum) < weights[index])
                return index;
            weightSum -= weights[index++];
        }

        return index;
    }
}
