namespace WebApp.UI.Services.ExtensionMethods
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IList<T> source, int size)
        {
            for (var i = 0; i < (float)source.Count / size; i++)
            {
                yield return source.Skip(i * size).Take(size);
            }
        }
    }
}
