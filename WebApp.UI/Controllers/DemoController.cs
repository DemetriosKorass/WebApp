using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using WebApp.UI.Exceptions;
using WebApp.UI.Services;

namespace WebApp.UI.Controllers
{
    public class DemoController : Controller
    {
        private readonly MathService _mathService;
        private double _lastResult;
        private readonly ILogger<DemoController> _logger;

        public DemoController(MathService mathService, ILogger<DemoController> logger)
        {
            _mathService = mathService;
            _logger = logger;
            _mathService.CalculationCompleted += OnCalculationCompleted;
        }

        private void OnCalculationCompleted(object? sender, CalculationEventArgs e)
        {
            _lastResult = e.Result;
            _logger.LogInformation("Calculation completed: {Result}", e.Result);
        }

        public IActionResult Index()
        {
            return View();
        }

        // Event Handling Demo
        public IActionResult EventDemo()
        {
            _mathService.PerformCalculation(10, 20); // triggers event
            ViewBag.LastResult = _lastResult;
            return View();
        }

        // Math Operations Demo
        public IActionResult MathDemo()
        {
            var advMath = new AdvancedMathService();
            double logOf10 = advMath.ComputeLog(10);
            var bigNum = advMath.MultiplyLarge(new BigInteger(1234567890123456789), new BigInteger(9876543210987654321));
            int safeSum = 0;
            try
            {
                safeSum = advMath.SafeAdd(int.MaxValue, 1);
            }
            catch (OverflowException)
            {
                safeSum = -1;
            }

            ViewBag.Log = logOf10;
            ViewBag.BigNum = bigNum;
            ViewBag.SafeSum = safeSum;
            return View();
        }

        // Custom Exceptions Demo
        public IActionResult ExceptionDemo()
        {
            var validationService = new UserValidationService();
            try
            {
                validationService.ValidateUserEmail("invalidEmailWithoutAt");
            }
            catch (InvalidUserOperationException ex) when (ex.Message.Contains("invalid"))
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        // Code Blocks Demo (delegates, Func, Action, Predicate, etc.)
        public IActionResult BlocksDemo()
        {
            var results = new List<string>();

            Action<string> print = msg => results.Add(msg);

            int result = ((Func<int, int>)(x => x * x))(5); // 25
            print("Square of 5 is " + result);

            bool checkEven = ((Predicate<int>)(x => x % 2 == 0))(10);
            print("10 is even: " + checkEven);

            int AddNumbers(int a, int b) => a + b;
            print("AddNumbers(3,4) = " + AddNumbers(3, 4));

            Func<int, int> triple = delegate (int n) { return n * 3; };
            print("Triple(4) = " + triple(4));

            print("5 squared using extension = " + 5.Square());

            var numbers = new List<int> { 1, 2, 3, 4, 5 };
            var evens = numbers.Where(n => n % 2 == 0).ToList();
            print("Evens: " + string.Join(",", evens));

            Func<int, int> factorial = null;
            factorial = n => (n <= 1) ? 1 : n * factorial(n - 1);
            print("Factorial(5) = " + factorial(5));

            ViewBag.Results = results;
            return View();
        }
    }
}
