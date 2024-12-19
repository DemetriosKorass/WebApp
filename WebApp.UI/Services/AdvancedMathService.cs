using System.Numerics;

namespace WebApp.UI.Services
{
    public class AdvancedMathService
    {
        public double ComputeLog(double value)
        {
            return Math.Log(value);
        }

        public BigInteger MultiplyLarge(BigInteger a, BigInteger b)
        {
            return a * b;
        }

        public int SafeAdd(int a, int b)
        {
            checked
            {
                return a + b;
            }
        }
    }
}