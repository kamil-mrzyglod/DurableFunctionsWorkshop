using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableFunctions.External
{
    public static class External
    {
        [FunctionName("Orchestratio")]
        public static async Task<string> Orchestration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.StartNewAsync("Orchestration", null);
        }

        [FunctionName("Orchestration_Status")]
        public static async Task<DurableOrchestrationStatus> Orchestration_Status(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start_status/{id}")] HttpRequestMessage input,
            string id,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.GetStatusAsync(id, true, true);
        }

        [FunctionName("Orchestration_Raise")]
        public static async Task Orchestration_Raise(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start_raise/{id}/{event}")] HttpRequestMessage input,
            string id,
            string @event,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            await starter.RaiseEventAsync(id, @event, await input.Content.ReadAsStringAsync());
        }

        [FunctionName("Orchestration")]
        public static async Task<string> Orchestration_Start([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var @event = await context.WaitForExternalEvent<string>("MyEvent");

            var result = await context.CallActivityAsync<string>(nameof(Activity), @event);
            return result;
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
