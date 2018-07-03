using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace DurableFunctions.Status
{
    public class Custom
    {
        [FunctionName("Orchestration_Client2")]
        public static async Task<HttpResponseMessage> Orchestration_Client(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start_custom")] HttpRequestMessage input,
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

            var instanceId = await starter.StartNewAsync(nameof(Orchestration2), null);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(instanceId)
            };
        }

        [FunctionName("Orchestration2")]
        public static async Task<string> Orchestration2([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            await context.CallActivityAsync(nameof(Activity2), "John Doe");
            context.SetCustomStatus("This was John Doe!");

            await context.CallActivityAsync(nameof(Activity2), "Jane Doe");
            context.SetCustomStatus("This was jane Doe!");

            await context.CallActivityAsync(nameof(Activity2), "Johann Doe");
            context.SetCustomStatus("This was Johann Doe!");

            return "Orchestration completed!";
        }

        [FunctionName("Activity2")]
        public static string Activity2([ActivityTrigger] DurableActivityContext context)
        {
            var payload = context.GetInput<string>();

            // This will be serialized and persisted to orchestration history
            return $"Hello {payload}!";
        }
    }
}