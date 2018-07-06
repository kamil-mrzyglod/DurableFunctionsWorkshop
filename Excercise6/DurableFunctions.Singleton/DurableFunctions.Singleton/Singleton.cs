using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DurableFunctions.Singleton
{
    public class Singleton
    {
        [FunctionName("Orchestration_Client")]
        public static async Task<HttpResponseMessage> Orchestration_Client(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start/{id}")] HttpRequestMessage input,
            string id,
            [OrchestrationClient] DurableOrchestrationClient starter,
            TraceWriter log)
        {
            var status = await starter.GetStatusAsync(id);
            if (status == null)
            {
                var orchestration = await starter.StartNewAsync("Orchestration", id, await input.Content.ReadAsStringAsync());
                log.Info($"Starting orchestration with id {orchestration}");

                return starter.CreateCheckStatusResponse(input, orchestration);
            }

            return input.CreateErrorResponse(HttpStatusCode.Conflict,
                $"Cannot start an orchestration with ID {id} - it is already running!");
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