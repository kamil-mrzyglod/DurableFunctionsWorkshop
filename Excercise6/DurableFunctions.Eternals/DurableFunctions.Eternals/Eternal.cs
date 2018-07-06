using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DurableFunctions.Eternals
{
    public static class Eternal
    {
        [FunctionName("Orchestration_Client")]
        public static async Task<string> Orchestration_Client(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.StartNewAsync("Orchestration", await input.Content.ReadAsStringAsync());
        }

        [FunctionName("Orchestration_Client_Status")]
        public static async Task<DurableOrchestrationStatus> Orchestration_Client_Status(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "status/{id}")] HttpRequestMessage input,
            string id,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.GetStatusAsync(id, true, true);
        }

        [FunctionName("Orchestration")]
        public static async Task Orchestration_Start([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var payload = context.GetInput<string>();
            await context.CallActivityAsync(nameof(Activity), payload);
            await context.CreateTimer(context.CurrentUtcDateTime.AddSeconds(10), CancellationToken.None);

            context.ContinueAsNew(null);
        }

        [FunctionName("Activity")]
        public static string Activity([ActivityTrigger] DurableActivityContext context, TraceWriter log)
        {
            var payload = context.GetInput<string>();
            log.Info("CALLED!");

            // This will be serialized and persisted to orchestration history
            return $"Current payload is {payload}!";
        }
    }
}