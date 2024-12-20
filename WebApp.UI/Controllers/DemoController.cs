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
using Swashbuckle.AspNetCore.Annotations;
using Flurl.Http;
using WebApp.DAL;

namespace WebApp.UI.Controllers
{
    /// <summary>
    /// Controller for demonstrating various functionalities including events, math operations, exceptions, blocks, advanced features, and regex operations.
    /// </summary>
    public class DemoController : Controller
    {
        private readonly MathService _mathService;
        private double _lastResult;
        private readonly ILogger<DemoController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoController"/> class.
        /// </summary>
        /// <param name="mathService">The math service for performing calculations.</param>
        /// <param name="logger">The logger instance.</param>
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

        /// <summary>
        /// Displays the home page.
        /// </summary>
        /// <returns>The home view.</returns>
        [HttpGet("[Controller]")]
        [SwaggerOperation(Summary = "Display Home Page", Description = "Returns the home view.")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Demonstrates event handling by performing a calculation.
        /// </summary>
        /// <returns>The event demonstration view with the last result.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Event Demonstration", Description = "Performs a calculation to trigger an event and displays the last result.")]
        public IActionResult EventDemo()
        {
            _mathService.PerformCalculation(10, 20); // triggers event
            ViewBag.LastResult = _lastResult;
            return View();
        }

        /// <summary>
        /// Demonstrates various math operations including logarithms, large number multiplication, and safe addition.
        /// </summary>
        /// <returns>The math demonstration view with computed results.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Math Demonstration", Description = "Performs various math operations and displays the results.")]
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

        /// <summary>
        /// Demonstrates exception handling by validating a user email.
        /// </summary>
        /// <returns>The exception demonstration view with error messages if any.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Exception Demonstration", Description = "Validates a user email and handles exceptions.")]
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

        /// <summary>
        /// Demonstrates the use of blocks, delegates, and LINQ operations.
        /// </summary>
        /// <returns>The blocks demonstration view with results.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Blocks Demonstration", Description = "Shows the use of blocks, delegates, and LINQ operations.")]
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


        /// Demonstrates advanced features including flags, bitwise operations, dynamic types, records, deconstruction, and logical patterns.
        /// </summary>
        /// <returns>The advanced demonstration view with results.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Advanced Demonstration", Description = "Shows advanced programming features such as flags, bitwise operations, dynamic types, records, deconstruction, and logical patterns.")]
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

        /// <summary>
        /// Performs a heavy computation asynchronously, supporting cancellation.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>The heavy computation view with the result or cancellation message.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Heavy Computation", Description = "Performs a CPU-bound heavy computation asynchronously.")]
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

        /// <summary>
        /// Demonstrates Task Parallel Library (TPL) by running multiple tasks concurrently.
        /// </summary>
        /// <returns>The TPL demonstration view with task results.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "TPL Demonstration", Description = "Runs multiple tasks concurrently and displays their completion messages.")]
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

        /// <summary>
        /// Displays the regex demonstration view.
        /// </summary>
        /// <returns>The regex demonstration view with a <see cref="RegexDemoViewModel"/> model.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Regex Demonstration", Description = "Displays the regex demonstration view.")]
        public IActionResult RegexDemo()
        {
            return View(new RegexDemoViewModel());
        }

        /// <summary>
        /// Processes the regex demonstration form submission.
        /// </summary>
        /// <param name="model">The regex demo view model containing user input.</param>
        /// <returns>The regex demonstration view with updated results or validation errors.</returns>
        [HttpPost("[Controller]/[Action]"), ActionName("RegexDemo")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Process Regex Demo", Description = "Processes user input for regex operations and displays the results.")]
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

        /// <summary>
        /// Displays the httpClient demonstration view.
        /// </summary>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "HTTP Client Demo", Description = "Demonstrates calling an external API using Flurl.")]
        public async Task<IActionResult> HttpClientDemo()
        {
            var apiUrl = "https://api.github.com/repos/dotnet/runtime"; 
                                                                        
            var data = await apiUrl
                .WithHeader("User-Agent", "MyWebApp")
                .GetJsonAsync<dynamic>();

            return View(data);
        }

        /// <summary>
        /// Displays the Hangfire demonstration view.
        /// </summary>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Task Scheduling Demo", Description = "Demonstrates scheduling a background task using Hangfire.")]
        public IActionResult TaskSchedulingDemo()
        {
            var bgService = new BackgroundJobService();
            bgService.EnqueueJob();
            return View();
        }

        /// <summary>
        /// Displays the transaction demonstration view.
        /// </summary>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Transactions Demo", Description = "Demonstrates wrapping operations in a transaction for data consistency.")]
        public async Task<IActionResult> TransactionsDemo([FromServices] AppDbContext dbContext)
        {
            var transactionService = new TransactionService(dbContext);
            await transactionService.UpdateMultipleEntitiesAsync();
            return View();
        }

    }
}
