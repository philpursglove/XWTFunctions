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

            using (var timeOutCts = new CancellationTokenSource())
            {
                // Look up tournament date/time from db
                DateTime tournamentTime = DateTime.MaxValue;
                Task timeOutEvent = context.CreateTimer(tournamentTime, timeOutCts.Token);

                Task<bool> approvalEvent = context.WaitForExternalEvent<bool>("PlayerAcceptance");
                Task<bool> rejectionEvent = context.WaitForExternalEvent<bool>("PlayerRejection");
                Task<bool> cancellationEvent = context.WaitForExternalEvent<bool>("PlayerCancellation");

                Task completionEvent =
                    await Task.WhenAny(approvalEvent, rejectionEvent, cancellationEvent, timeOutEvent);

                if (completionEvent == approvalEvent)
                {
                    timeOutCts.Cancel();
                    // Send confirmation email to player
                    await context.CallActivityAsync("SendAcceptanceEmail", null);
                }

                if (completionEvent == rejectionEvent)
                {
                    timeOutCts.Cancel();
                    // Send rejection email to player
                    await context.CallActivityAsync("SendRejectionEmail", null);
                }

                if (completionEvent == cancellationEvent)
                {
                    timeOutCts.Cancel();
                    // Send cancellation email to TO
                    await context.CallActivityAsync("SendCancellationEmail", null);
                }

                if (completionEvent == timeOutEvent)
                {
                    // Send email to both, I guess
                }
            }
        }

        [FunctionName("RequestPlayerApproval")]
        public static void RequestPlayerApproval([ActivityTrigger] string name, ILogger log)
        {
            
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