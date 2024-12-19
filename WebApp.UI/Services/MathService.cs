namespace WebApp.UI.Services
{
    public class CalculationEventArgs : EventArgs
    {
        public double Result { get; }
        public CalculationEventArgs(double result)
        {
            Result = result;
        }
    }

    public class MathService
    {
        public event EventHandler<CalculationEventArgs>? CalculationCompleted;

        public void PerformCalculation(double a, double b)
        {
            double result = a + b;
            OnCalculationCompleted(result);
        }

        protected virtual void OnCalculationCompleted(double result)
        {
            CalculationCompleted?.Invoke(this, new CalculationEventArgs(result));
        }
    }
}
