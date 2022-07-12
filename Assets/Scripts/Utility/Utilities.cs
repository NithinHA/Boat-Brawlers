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

        if (weightSum == 0)
            return Random.Range(0, weights.Length);

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
