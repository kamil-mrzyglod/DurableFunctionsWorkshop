using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableFunctions.Instance
{
    public static class Instance
    {
        [FunctionName("Orchestration_Client")]
        public static async Task<HttpResponseMessage> Orchestration_Client(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            var id = await starter.StartNewAsync(nameof(Orchestration), null);

            return starter.CreateCheckStatusResponse(input, id);
        }

        [FunctionName("Terminate")]
        public static Task Terminate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "terminate/{id}")] HttpRequestMessage input,
            string id,
            [OrchestrationClient] DurableOrchestrationClient client)
        {
            var reason = "Manual termination";
            return client.TerminateAsync(id, reason);
        }

        [FunctionName("Status")]
        public static async Task<IList<DurableOrchestrationStatus>> Status(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "status")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient client)
        {
            var status = await client.GetStatusAsync();
            return status;
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
