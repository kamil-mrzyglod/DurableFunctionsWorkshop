using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace DurableFunctions.Status
{
    public class Status
    {
        [FunctionName("Orchestration_Client")]
        public static async Task<HttpResponseMessage> Orchestration_Client(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            var payload = JsonConvert.DeserializeObject<Payload>(await input.Content.ReadAsStringAsync());
            if (string.IsNullOrEmpty(payload.OrchestrationId) == false)
            {
                var status = await starter.GetStatusAsync(payload.OrchestrationId, true, true);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(status))
                };
            }

            var instanceId = await starter.StartNewAsync(nameof(Orchestration), null);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(instanceId)
            };
        }

        [FunctionName("Orchestration")]
        public static async Task<string> Orchestration([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            await context.CallActivityAsync(nameof(Activity), "John Doe");
            await context.CallActivityAsync(nameof(Activity), "Jane Doe");
            await context.CallActivityAsync(nameof(Activity), "Johann Doe");

            return "Orchestration completed!";
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