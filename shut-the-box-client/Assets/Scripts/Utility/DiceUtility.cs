using System;
using Random = UnityEngine.Random;

namespace Utility
{
    public static class DiceUtility
    {
        public static void GenerateDiceSum(ref int[] rolls, int fCount, int target)
        {
            var dCount = rolls.Length;
            if (target < rolls.Length || target > dCount * fCount)
            {
                throw new ArgumentException(
                    $"Target sum is not achievable with the given number of dice and face count: {target}"
                );
            }

            var remainder = target;
            for (var i = 0; i < dCount; i++)
            {
                var rd = dCount - (i + 1);
                var min = Math.Max(remainder - rd * fCount, 1);
                var max = Math.Min(remainder - rd, fCount);
                rolls[i] = Random.Range(min, max + 1);
                remainder -= rolls[i];
            }

            for (var i = rolls.Length - 1; i > 0; i--)
            {
                var swapIndex = Random.Range(0, i + 1);
                (rolls[i], rolls[swapIndex]) = (rolls[swapIndex], rolls[i]);
            }
        }
    }
}
