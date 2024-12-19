using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using System.Dynamic;
using System.Runtime.InteropServices;
using WebApp.UI.Exceptions;
using WebApp.UI.Services;
using WebApp.DAL.Entities;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;
using WebApp.UI.ViewModels;

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

            // 2. Bitwise operators
            int a = 0b_0011;
            int b = 0b_0101;
            int c = a & b; // 0001
            results.Add($"Bitwise AND: {Convert.ToString(a, 2)} & {Convert.ToString(b, 2)} = {Convert.ToString(c, 2)}");

            // 3.shift
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

            // 8. Advanced Collections: HashSet

            HashSet<string> set = new HashSet<string> { "apple", "banana" };

            set.Add("apple");

            results.Add("HashSet contains: " + string.Join(",", set));

            ViewBag.Results = results;
            return View();
        }

        public async Task<IActionResult> HeavyComputation(CancellationToken cancellationToken)
        {
            try
            {
                // Simulate heavy CPU-bound work
                int result = await System.Threading.Tasks.Task.Run(() =>
                {
                    // Check for cancellation
                    cancellationToken.ThrowIfCancellationRequested();
                    int sum = 0;
                    for (int i = 0; i < 1_000_000; i++)
                    {
                        sum += i;
                        if (i % 100_000 == 0 && cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }
                    }
                    return sum;
                }, cancellationToken);

                ViewBag.Result = result;
                return View();
            }
            catch (OperationCanceledException)
            {
                ViewBag.Result = "Computation was canceled.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Result = $"An error occurred: {ex.Message}";
                return View();
            }
        }
        public async Task<IActionResult> TplDemo()
        {
            var results = new List<string>();
            var tasks = new List<Task>
            {
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    lock (results)
                    {
                        results.Add("Task 1 completed.");
                    }
                }),
                Task.Run(() =>
                {
                    Thread.Sleep(1500);
                    lock (results)
                    {
                        results.Add("Task 2 completed.");
                    }
                }),
                Task.Run(() =>
                {
                    Thread.Sleep(500);
                    lock (results)
                    {
                        results.Add("Task 3 completed.");
                    }
                })
            };

            await Task.WhenAll(tasks);

            ViewBag.Results = results;
            return View();
        }
        public IActionResult RegexDemo()
        {
            return View(new RegexDemoViewModel());
        }


        [HttpPost, ActionName("RegexDemo")]
        [ValidateAntiForgeryToken]
        public IActionResult RegexDemo(RegexDemoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Validation: Check if InputText is a valid email
            string emailPattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
            model.IsValidEmail = Regex.IsMatch(model.InputText, emailPattern, RegexOptions.IgnoreCase);

            // 2. Find and Replace
            if (!string.IsNullOrWhiteSpace(model.FindPattern) && model.ReplaceWith != null)
            {
                try
                {
                    model.ReplaceResult = Regex.Replace(model.InputText, model.FindPattern, model.ReplaceWith, RegexOptions.IgnoreCase);
                }
                catch (RegexParseException ex)
                {
                    ModelState.AddModelError("", $"Find and Replace Error: {ex.Message}");
                }
            }

            // 3. Split String
            if (!string.IsNullOrWhiteSpace(model.SplitPattern))
            {
                try
                {
                    model.SplitResult = Regex.Split(model.InputText, model.SplitPattern, RegexOptions.IgnoreCase);
                }
                catch (RegexParseException ex)
                {
                    ModelState.AddModelError("", $"Split Error: {ex.Message}");
                }
            }

            // 4. Parse Data using Groups
            if (!string.IsNullOrWhiteSpace(model.ParsePattern))
            {
                try
                {
                    Match match = Regex.Match(model.InputText, model.ParsePattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        foreach (Group group in match.Groups)
                        {
                            if (group.Name != "0" && !string.IsNullOrEmpty(group.Value))
                            {
                                model.ParsedGroups.Add(new ParsedGroup
                                {
                                    GroupName = group.Name,
                                    Value = group.Value
                                });
                            }
                        }
                    }
                }
                catch (RegexParseException ex)
                {
                    ModelState.AddModelError("", $"Parse Error: {ex.Message}");
                }
            }

            return View(model);
        }
    }
}
