using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DurableFunctions.SubOrchestrations
{
    public class Suborchestration
    {
        [FunctionName("FullOrchestration_Start")]
        public static async Task<HttpResponseMessage> FullOrchestration_Start(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start_full")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter,
            TraceWriter log)
        {
            var id = await starter.StartNewAsync(nameof(FullOrchestration), await input.Content.ReadAsStringAsync());
            return starter.CreateCheckStatusResponse(input, id);
        }

        [FunctionName("FullOrchestration_Start_Status")]
        public static async Task<DurableOrchestrationStatus> FullOrchestration_Start_Status(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "status/{id}")] HttpRequestMessage input,
            string id,
            [OrchestrationClient] DurableOrchestrationClient starter,
            TraceWriter log)
        {
            return await starter.GetStatusAsync(id, true, true);
        }

        [FunctionName("FullOrchestration")]
        public static async Task FullOrchestration([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var payload = context.GetInput<string>();

            await context.CallSubOrchestratorAsync(nameof(ProvisioningOrchestration), payload);
            await context.CallSubOrchestratorAsync(nameof(FirmwareOrchestration), payload);
        }

        [FunctionName("ProvisioningOrchestration")]
        public static async Task ProvisioningOrchestration([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var payload = context.GetInput<string>();
            await context.CallActivityAsync(nameof(InstallDevice), payload);
            await context.CallActivityAsync(nameof(UpdateNetwork), payload);
        }

        [FunctionName("FirmwareOrchestration")]
        public static async Task FirmwareOrchestration([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var payload = context.GetInput<string>();
            await context.CallActivityAsync(nameof(UpdateFirmware), payload);
        }

        [FunctionName("InstallDevice")]
        public static string InstallDevice([ActivityTrigger] DurableActivityContext context, TraceWriter log)
        {
            log.Info("Installing device in infrastructure...");
            return Guid.NewGuid().ToString();
        }

        [FunctionName("UpdateNetwork")]
        public static object UpdateNetwork([ActivityTrigger] DurableActivityContext context, TraceWriter log)
        {
            log.Info("Updating network...");
            return new {Ip = "192.168.12.123"};
        }

        [FunctionName("UpdateFirmware")]
        public static string UpdateFirmware([ActivityTrigger] DurableActivityContext context, TraceWriter log)
        {
            log.Info("Updating firmware...");
            return new Version(1, 1, 123).ToString();
        }
    }
}