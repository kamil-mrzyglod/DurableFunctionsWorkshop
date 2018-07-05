using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DurableFunctions.Timers
{
    public static class Timeout
    {
        [FunctionName("Orchestration_Client2")]
        public static async Task<string> Orchestration_Client2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start_timeout")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.StartNewAsync("Orchestration2", null);
        }

        [FunctionName("Orchestration_Client2_Status")]
        public static async Task<DurableOrchestrationStatus> Orchestration_Client_Status(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start_timeout_status/{id}")] HttpRequestMessage input,
            string id,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.GetStatusAsync(id);
        }

        [FunctionName("Orchestration2")]
        public static async Task<bool> Orchestration_Start2([OrchestrationTrigger] DurableOrchestrationContext context, TraceWriter log)
        {
            log.Info($"Scheduled at {context.CurrentUtcDateTime}");

            var timeout = TimeSpan.FromSeconds(10);
            var deadline = context.CurrentUtcDateTime.Add(timeout);

            using (var cts = new CancellationTokenSource())
            {
                var activityTask = context.CallActivityAsync(nameof(Activity2), context.CurrentUtcDateTime);
                var timeoutTask = context.CreateTimer(deadline, cts.Token);

                var winner = await Task.WhenAny(activityTask, timeoutTask);
                if (winner != activityTask) return false;
                cts.Cancel();
                return true;
            }
        }

        [FunctionName("Activity2")]
        public static void Activity2([ActivityTrigger] DurableActivityContext context, TraceWriter log)
        {
            var date = context.GetInput<DateTime>();
            log.Info($"Executed at {date}");
        }
    }
}
