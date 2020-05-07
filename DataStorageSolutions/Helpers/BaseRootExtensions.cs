using UnityEngine;

namespace DataStorageSolutions.Helpers
{
    internal static class BaseRootExtensions
    {
        public static float GetDistanceToPlayer(this BaseRoot baseRoot)
        {
            Int3 u = baseRoot.baseComp.WorldToGrid(Player.main.transform.position);
            int num = int.MaxValue;
            Int3 @int = new Int3(int.MaxValue);
            foreach (Int3 int2 in baseRoot.baseComp.AllCells)
            {
                int num2 = (u - int2).SquareMagnitude();
                if (num2 < num)
                {
                    @int = int2;
                    num = num2;
                }
            }

            baseRoot.baseComp.GridToWorld(@int, UWE.Utils.half3, out var vector);
            return (Player.main.transform.position - vector).magnitude;
        }
    }
}
