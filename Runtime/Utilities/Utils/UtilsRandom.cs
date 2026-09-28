using SketchEngine.Core;
using UnityEngine;

namespace SketchEngine.Utilities.Utils
{
    public static class UtilsRandom
    {
        public static Direction2D RandomDirection2D()
        {
            return Random.Range(0, 2) == 0 ? Direction2D.Left : Direction2D.Right;
        }
        
        public static Direction4D RandomDirection4D()
        {
            int rdValue = Random.Range(1, 5);
            switch (rdValue)
            {
                case 1:
                    return Direction4D.Left;
                case 2:
                    return Direction4D.Right;
                case 3:
                    return Direction4D.Forward;
                case 4:
                    return Direction4D.Backward;
            }
            
            return Direction4D.Left;
        }
    }
}
