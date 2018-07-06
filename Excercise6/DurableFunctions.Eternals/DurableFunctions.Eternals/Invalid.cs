using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DurableFunctions.Eternals
{
    public static class Invalid
    {
        [FunctionName("Orchestration_Client2")]
        public static async Task<string> Orchestration_Client2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start_invalid")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.StartNewAsync("Orchestration2", await input.Content.ReadAsStringAsync());
        }

        [FunctionName("Orchestration_Client_Status2")]
        public static async Task<DurableOrchestrationStatus> Orchestration_Client_Status2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "status_invalid/{id}")] HttpRequestMessage input,
            string id,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.GetStatusAsync(id, true, true);
        }

        [FunctionName("Orchestration2")]
        public static async Task Orchestration_Start2([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var payload = context.GetInput<string>();
            while (true)
            {
                await context.CallActivityAsync(nameof(Activity2), payload);
                await context.CreateTimer(context.CurrentUtcDateTime.AddSeconds(10), CancellationToken.None);
            }
        }

        [FunctionName("Activity2")]
        public static string Activity2([ActivityTrigger] DurableActivityContext context, TraceWriter log)
        {
            var payload = context.GetInput<string>();
            log.Info("CALLED!");

            // This will be serialized and persisted to orchestration history
            return $"Current payload is {payload}!";
        }
    }
}