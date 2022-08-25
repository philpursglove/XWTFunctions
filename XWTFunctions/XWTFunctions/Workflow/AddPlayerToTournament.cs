using System;
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
    public static class AddPlayerToTournament
    {
        [FunctionName("AddPlayerToTournament")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync("RequestPlayerApproval", null);

            var approvalEvent = context.WaitForExternalEvent("PlayerAcceptance");
            var rejectionEvent = context.WaitForExternalEvent("PlayerRejection");
            var cancellationEvent = context.WaitForExternalEvent("PlayerCancellation");

            var completionEvent = Task.WhenAny(approvalEvent, rejectionEvent, cancellationEvent);

            if (completionEvent == approvalEvent)
            {
                // Send confirmation email to player
                await context.CallActivityAsync("SendAcceptanceEmail", null);
            }
            else if (completionEvent == rejectionEvent)
            {
                // Send rejection email to player
                await context.CallActivityAsync("SendRejectionEmail", null);
            }
            else if (completionEvent == cancellationEvent)
            {
                // Send cancellation email to TO
                await context.CallActivityAsync("SendCancellationEmail", null);
            }
        }


        [FunctionName("RequestPlayerApproval")]
        public static void RequestPlayerApproval([ActivityTrigger] string name, ILogger log)
        {
            // Send email to TO

        }

        [FunctionName("SendAcceptanceEmail")]
        public static void SendAcceptanceEmail([ActivityTrigger] string name, ILogger log)
        {

        }

        [FunctionName("SendRejectionEmail")]
        public static void SendRejectionEmail([ActivityTrigger] string name, ILogger log)
        {

        }

        [FunctionName("SendCancellationEmail")]
        public static void SendCancellationEmail([ActivityTrigger] string name, ILogger log)
        {

        }

        [FunctionName("AddPlayerToTournament_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("AddPlayerToTournament", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}