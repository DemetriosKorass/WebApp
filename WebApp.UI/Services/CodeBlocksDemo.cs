namespace WebApp.UI.Services
{
    public class CodeBlocksDemo
    {
        // Func
        Func<int, int> square = x => x * x;

        // Action
        Action<string> print = message => Console.WriteLine(message);

        // Predicate
        Predicate<int> isEven = x => x % 2 == 0;

        public void Demo()
        {
            int result = square(5); 
            print("Square of 5 is " + result);

            bool checkEven = isEven(10); 
            print("10 is even: " + checkEven);

            // Local function
            int AddNumbers(int a, int b)
            {
                return a + b;
            }
            print("AddNumbers(3,4) = " + AddNumbers(3, 4));

            // Anonymous method
            Func<int, int> triple = delegate (int n) { return n * 3; };
            print("Triple(4) = " + triple(4));

            // Extension methods
            print("5 squared using extension = " + 5.Square());

            // Expression lambda
            var numbers = new List<int> { 1, 2, 3, 4, 5 };
            var evens = numbers.Where(n => n % 2 == 0).ToList();
            print("Evens: " + string.Join(",", evens));

            // Statement lambda
            Func<int, int> factorial = default!;
            factorial = n =>
            {
                if (n <= 1) return 1;
                return n * factorial(n - 1);
            };

            print("Factorial(5) = " + factorial(5));
        }
    }
}