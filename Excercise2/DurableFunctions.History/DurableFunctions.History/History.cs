using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableFunctions.History
{
    public class History
    {
        [FunctionName("Orchestration_Client")]
        public static async Task<string> Orchestration_Client(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.StartNewAsync(nameof(Orchestration), null);
        }

        [FunctionName("Orchestration")]
        public static async Task Orchestration([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            await context.CallActivityAsync(nameof(Activity), "John Doe");
            await context.CallActivityAsync(nameof(Activity), "Jane Doe");
            await context.CallActivityAsync(nameof(Activity), "Johann Doe");
        }

        [FunctionName("Activity")]
        public static string Activity([ActivityTrigger] DurableActivityContext context)
        {
            var payload = context.GetInput<string>();

            // This will be serialized and persisted to orchestration history
            return $"Hello {payload}!";
        }
    }
}