using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableFunctions.Bindings
{
    public static class Orchestration
    {
        [FunctionName("Orchestration_Client")]
        public static async Task<string> Orchestration_Client(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.StartNewAsync("Orchestration", await input.Content.ReadAsStringAsync());
        }

        [FunctionName("Orchestration")]
        public static async Task Orchestration_Start([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var payload = context.GetInput<string>();
            await context.CallActivityAsync(nameof(Activity), payload);
        }

        [FunctionName("Activity")]
        public static string Activity([ActivityTrigger] DurableActivityContext context)
        {
            var payload = context.GetInput<string>();

            // This will be serialized and persisted to orchestration history
            return $"Current payload is {payload}!";
        }
    }
}
