using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Dynamic;
using System.Runtime.InteropServices;
using WebApp.UI;
using WebApp.UI.Exceptions;
using WebApp.UI.Services;
using WebApp.DAL.Entities;

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

        public IActionResult EventDemo()
        {
            _mathService.PerformCalculation(10, 20); // triggers event
            ViewBag.LastResult = _lastResult;
            return View();
        }

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

        // 2. StructLayout
        // Example of a struct with explicit layout
        [StructLayout(LayoutKind.Sequential)]
        struct MyPoint
        {
            public int X;
            public int Y;
        }

        record Person(string FirstName, string LastName);

        record Point2D(int X, int Y)
        {
            public void Deconstruct(out int x, out int y) { x = X; y = Y; }
        }

        // Advanced Demo: Flags, StructLayout, bitwise, dynamic, ExpandoObject, Records, Deconstruction, Logical Patterns
        public IActionResult AdvancedDemo()
        {
            var results = new List<string>();

            // 1. Flags
            var perm = Permissions.Read | Permissions.Write;
            if (perm.HasFlag(Permissions.Read)) results.Add("Has Read Permission");
            if (perm.HasFlag(Permissions.Write)) results.Add("Has Write Permission");
            if (!perm.HasFlag(Permissions.Delete)) results.Add("No Delete Permission");

            var p = new MyPoint { X = 10, Y = 20 };
            results.Add($"MyPoint: X={p.X}, Y={p.Y}");

            // 3. Bitwise operators
            int a = 0b_0011;
            int b = 0b_0101;
            int c = a & b; // 0001
            results.Add($"Bitwise AND: {Convert.ToString(a, 2)} & {Convert.ToString(b, 2)} = {Convert.ToString(c, 2)}");

            // shift
            int shiftMe = 1;
            int leftShift = shiftMe << 3; // 8
            results.Add($"1 << 3 = {leftShift}");

            // 4. dynamic
            dynamic dyn = "Hello";
            results.Add($"Dynamic variable initially string: Length={dyn.Length}");
            dyn = 123;
            results.Add($"Dynamic variable changed to int: {dyn}");

            // ExpandoObject
            dynamic expando = new ExpandoObject();
            expando.FirstName = "John";
            expando.LastName = "Doe";
            results.Add($"ExpandoObject: {expando.FirstName} {expando.LastName}");

            // 5. Records & Nondestructive Mutations
            var person = new Person("Alice", "Smith");
            var mutated = person with { LastName = "Johnson" };
            results.Add($"Record original: {person.FirstName} {person.LastName}, mutated: {mutated.FirstName} {mutated.LastName}");

            // 6. Deconstruction of tuples & user-defined types
            var tuple = (X: 10, Y: 20);
            var (xVal, yVal) = tuple;
            results.Add($"Deconstructed tuple: X={xVal}, Y={yVal}");

            // Deconstructing a record by defining Deconstruct method:
            var pt = new Point2D(5, 15);
            var (px, py) = pt;
            results.Add($"Deconstructed record: X={px}, Y={py}");

            // 7. Logical Patterns
            int number = 42;
            string resultStr = number switch
            {
                < 0 => "Negative",
                > 0 and < 100 => "Positive less than 100",
                100 => "Exactly 100",
                _ => "Something else"
            };
            results.Add($"Logical pattern result: {number} is {resultStr}");

            // 8. Advanced Collections: HashSet, SortedSet, SortedDictionary, etc.
            // Integration into core app: We'll show a HashSet usage in `TasksController.Create` below.
            // Just a demo here:
            HashSet<string> set = new HashSet<string> { "apple", "banana" };
            set.Add("apple"); // duplicate won't add
            results.Add("HashSet contains: " + string.Join(",", set));

            ViewBag.Results = results;
            return View();
        }
    }
}
