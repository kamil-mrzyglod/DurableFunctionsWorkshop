using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DurableFunctions.Timers
{
    public static class Delay
    {
        [FunctionName("Orchestration_Client")]
        public static async Task<string> Orchestration_Client(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.StartNewAsync("Orchestration", null);
        }

        [FunctionName("Orchestration")]
        public static async Task Orchestration_Start([OrchestrationTrigger] DurableOrchestrationContext context, TraceWriter log)
        {
            log.Info($"Scheduled at {context.CurrentUtcDateTime}");

            await context.CreateTimer(context.CurrentUtcDateTime.AddSeconds(10), CancellationToken.None);
            await context.CallActivityAsync(nameof(Activity), context.CurrentUtcDateTime);
        }

        [FunctionName("Activity")]
        public static void Activity([ActivityTrigger] DurableActivityContext context, TraceWriter log)
        {
            var date = context.GetInput<DateTime>();
            log.Info($"Executed at {date}");
        }
    }
}