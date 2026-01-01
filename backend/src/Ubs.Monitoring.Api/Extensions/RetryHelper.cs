namespace Ubs.Monitoring.Api.Startup;

public static class RetryHelper
{
    /// <summary>
    /// Executes an asynchronous operation with retries and a fixed delay between attempts.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="maxRetries">Maximum number of attempts before failing.</param>
    /// <param name="delayMs">Delay in milliseconds between attempts.</param>
    /// <param name="ct">Cancellation token used to cancel the operation.</param>
    /// <returns>A task that completes when the operation succeeds.</returns>
    public static async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        int maxRetries = 5,
        int delayMs = 1000,
        CancellationToken ct = default)
    {
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await operation(ct);
                return;
            }
            catch when (attempt < maxRetries)
            {
                await Task.Delay(delayMs, ct);
            }
        }

        throw new Exception("Operation failed after multiple retries.");
    }
}
