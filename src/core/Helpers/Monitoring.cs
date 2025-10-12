using System.Diagnostics;

namespace core.Helpers
{
    public class Monitoring
    {
        public static async Task<T> Log<T>(Func<Task<T>> action, string actionDescription)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await action();
                stopwatch.Stop();
                Console.WriteLine($"[INFO] '{actionDescription}' executed in {stopwatch.ElapsedMilliseconds} ms");
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"[ERROR] '{actionDescription}' failed after {stopwatch.ElapsedMilliseconds} ms with error: {ex.Message}");
                throw;
            }
        }
    }
}