using Flurl.Http;

public class ExternalApiService
{
    public async Task<string> GetDataAsync()
    {
        // Example: GET from a JSON endpoint
        string result = await "http://date.jsontest.com"
            .WithHeader("Accept", "application/json")
            .GetStringAsync();
        return result;
    }
}
