using Hangfire;

public class BackgroundJobService
{
    public void EnqueueJob()
    {
        // This job will run in the background
        BackgroundJob.Enqueue(() => Console.WriteLine("Hello from a background job!"));
    }
}
