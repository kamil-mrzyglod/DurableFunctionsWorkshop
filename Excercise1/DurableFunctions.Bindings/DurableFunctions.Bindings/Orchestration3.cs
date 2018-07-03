using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableFunctions.Bindings
{
    public static class Orchestration3
    {
        [FunctionName("Orchestration_Client3")]
        public static async Task<HttpResponseMessage> Orchestration_Client(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start_check_wait")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            var id = await starter.StartNewAsync("Orchestration3", await input.Content.ReadAsStringAsync());

            return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(input, id, TimeSpan.FromSeconds(5));
        }

        [FunctionName("Orchestration3")]
        public static async Task<string> Orchestration_Start([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var payload = context.GetInput<string>();
            var result = await context.CallActivityAsync<string>(nameof(Activity), payload);

            return result;
        }

        [FunctionName("Activity3")]
        public static string Activity([ActivityTrigger] DurableActivityContext context)
        {
            var payload = context.GetInput<string>();

            // This will be serialized and persisted to orchestration history
            return $"Current payload is {payload}!";
        }
    }
}
