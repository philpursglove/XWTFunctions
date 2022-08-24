using NUnit.Framework.Internal;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using NSubstitute;
using XWTFunctions.Workflow;

namespace XWTFunctions.Tests
{
    public class AddPlayerToTournamentTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task HttpStart_returns_retryafter_header()
        {
            // Define constants
            const string functionName = "AddPlayerToTournament";
            const string instanceId = "7E467BDB-213F-407A-B86A-1954053D3C24";

            // Mock TraceWriter
            var loggerMock = Substitute.For<Microsoft.Extensions.Logging.ILogger>();

            // Mock DurableOrchestrationClientBase
            var clientMock = Substitute.For<IDurableClient>();

            // Mock StartNewAsync method
            clientMock.StartNewAsync(functionName).ReturnsForAnyArgs(instanceId);


            // Mock CreateCheckStatusResponse method
            clientMock.CreateCheckStatusResponse(new HttpRequestMessage() , instanceId, false).ReturnsForAnyArgs(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(string.Empty),
                Headers =
                {
                    RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10))
                }
            });

            // Call Orchestration trigger function
            var result = await AddPlayerToTournament.HttpStart(
                new HttpRequestMessage()
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json"),
                    RequestUri = new Uri("http://localhost:7071/orchestrators/AddPlayerToTournament"),
                },
                clientMock,
                loggerMock);

            // Validate that output is not null
            Assert.NotNull(result.Headers.RetryAfter);

            // Validate output's Retry-After header value
            Assert.That(result.Headers.RetryAfter.Delta, Is.EqualTo(TimeSpan.FromSeconds(10)));
        }

        [Test]
        public async Task When_A_Player_Is_Accepted_An_Acceptance_Email_Is_Sent()
        {
            var durableOrchestrationContextMock = Substitute.For<IDurableOrchestrationContext>();

            durableOrchestrationContextMock.WaitForExternalEvent("PlayerAcceptance")
                .Returns(new Task<string>(() => { return "Accept"; }));

            await AddPlayerToTournament.RunOrchestrator(durableOrchestrationContextMock);

            await durableOrchestrationContextMock.Received(1).CallActivityAsync("SendAcceptanceEmail", null);
        }

        [Test]
        public async Task When_A_Player_Is_Rejected_An_Rejection_Email_Is_Sent()
        {
            var durableOrchestrationContextMock = Substitute.For<IDurableOrchestrationContext>();

            durableOrchestrationContextMock.WaitForExternalEvent("PlayerRejection")
                .Returns(new Task<string>(() => { return "Reject"; }));

            await AddPlayerToTournament.RunOrchestrator(durableOrchestrationContextMock);

            await durableOrchestrationContextMock.Received(1).CallActivityAsync("SendRejectionEmail", null);
        }
    }
}