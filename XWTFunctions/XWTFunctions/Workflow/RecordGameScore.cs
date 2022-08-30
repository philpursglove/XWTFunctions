using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace XWTFunctions.Workflow
{
    public static class RecordGameScore
    {
        [FunctionName("RecordGameScore")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync("RecordFirstPlayerResult", null);

            Task player2Approval = context.WaitForExternalEvent("Player2Approval");
            Task toApproval = context.WaitForExternalEvent("TOApproval");

            Task approval = await Task.WhenAny(player2Approval, toApproval);

            if (approval == player2Approval || approval == toApproval)
            {
                await context.CallActivityAsync("RecordFinalResult", null);
            }
        }

        [FunctionName("RecordFirstPlayerResult")]
        public static void RecordFirstPlayerResult()
        {

        }

        [FunctionName("RecordFinalResult")]
        public static void RecordFinalResult()
        {

        }


        [FunctionName("RecordGameScore_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("RecordGameScore", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}