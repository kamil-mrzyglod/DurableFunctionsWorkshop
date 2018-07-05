using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DurableFunctions.Errors
{
    public static class Retry
    {
        public static Dictionary<Guid, int> Account = new Dictionary<Guid, int>();

        [FunctionName("Orchestration2_Start")]
        public static async Task<string> Orchestration2_Start(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start2")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            var guid = Guid.NewGuid();
            Account.Add(guid, 1000);
            var id = await starter.StartNewAsync(nameof(Orchestration2), (await input.Content.ReadAsStringAsync(), guid));

            return id;
        }

        [FunctionName("Orchestration2_Status")]
        public static async Task<DurableOrchestrationStatus> Orchestration2_Status(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status2/{id}")] HttpRequestMessage input,
            string id,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.GetStatusAsync(id);
        }

        [FunctionName("Orchestration2")]
        public static async Task<int> Orchestration2([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var payload = context.GetInput<(int, Guid)>();

            try
            {
                var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), 5);
                var total = await context.CallActivityWithRetryAsync<int>(nameof(Transfer2),
                    retryOptions, payload);

                return total;
            }
            catch (Exception)
            {
                var total = await context.CallActivityAsync<int>(nameof(Refund2), payload);
                return total;
            }
        }

        [FunctionName("Transfer2")]
        public static int Transfer2([ActivityTrigger] DurableActivityContext context, TraceWriter log)
        {
            log.Info("Attempt...");

            (int Amount, Guid Id) payload = context.GetInput<(int, Guid)>();
            Account[payload.Id] = Account[payload.Id] - payload.Amount;

            throw new Exception("Something went wrong!");

            return Account[payload.Id];
        }

        [FunctionName("Refund2")]
        public static int Refund2([ActivityTrigger] DurableActivityContext context)
        {
            (int Amount, Guid Id) payload = context.GetInput<(int, Guid)>();
            Account[payload.Id] = Account[payload.Id] + payload.Amount;

            return Account[payload.Id];
        }
    }
}