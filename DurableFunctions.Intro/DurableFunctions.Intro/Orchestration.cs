using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace DurableFunctions.Intro
{
    public static class Orchestration
    {
        [FunctionName("Foo")]
        public static async Task<HttpResponseMessage> Foo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "foo")] HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient starter)
        {
            var id = await starter.StartNewAsync(nameof(Foo_Orchestration), await req.Content.ReadAsStringAsync());
            return starter.CreateCheckStatusResponse(req, id);
        }

        [FunctionName("Foo_Orchestration")]
        public static async Task Foo_Orchestration([OrchestrationTrigger] DurableOrchestrationContext ctx)
        {
            var data = JsonConvert.DeserializeObject<Model>(ctx.GetInput<string>());
            await ctx.CallActivityAsync(nameof(Foo_Activity), data);
        }

        [FunctionName("Foo_Activity")]
        public static void Foo_Activity(
            [ActivityTrigger] DurableActivityContext ctx,
            TraceWriter log)
        {
            var model = ctx.GetInput<Model>();
            if (model.Name == "Foo")
            {
                // Do this...
            }
            else
            {
                // Do that
            }
        }
    }

    public class Model
    {
        public string Name { get; set; }
    }
}
