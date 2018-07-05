using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableFunctions.Errors
{
    public static class Errors
    {
        public static Dictionary<Guid, int> Account = new Dictionary<Guid, int>();

        [FunctionName("Orchestration_Start")]
        public static async Task<string> Orchestration_Start(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start")] HttpRequestMessage input,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            var guid = Guid.NewGuid();
            Account.Add(guid, 1000);
            var id = await starter.StartNewAsync(nameof(Orchestration), (await input.Content.ReadAsStringAsync(), guid));

            return id;
        }

        [FunctionName("Orchestration_Status")]
        public static async Task<DurableOrchestrationStatus> Orchestration_Status(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status/{id}")] HttpRequestMessage input,
            string id,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            return await starter.GetStatusAsync(id);
        }

        [FunctionName("Orchestration")]
        public static async Task<int> Orchestration([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var payload = context.GetInput<(int, Guid)>();

            try
            {
                var total = await context.CallActivityAsync<int>(nameof(Transfer), payload);
                return total;
            }
            catch (Exception)
            {
                var total = await context.CallActivityAsync<int>(nameof(Refund), payload);
                return total;
            }
        }

        [FunctionName("Transfer")]
        public static int Transfer([ActivityTrigger] DurableActivityContext context)
        {
            (int Amount, Guid Id) payload = context.GetInput<(int, Guid)>();
            Account[payload.Id] = Account[payload.Id] - payload.Amount;

            //throw new Exception("Something went wrong!");

            return Account[payload.Id];
        }

        [FunctionName("Refund")]
        public static int Refund([ActivityTrigger] DurableActivityContext context)
        {
            (int Amount, Guid Id) payload = context.GetInput<(int, Guid)>();
            Account[payload.Id] = Account[payload.Id] + payload.Amount;

            return Account[payload.Id];
        }
    }
}
