using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using XWTFunctions.Messages;

namespace XWTFunctions.Queue
{
    public class RecordPlayerRegistration
    {
        [FunctionName("RecordPlayerRegistration")]
        public void Run([QueueTrigger("myqueue-items", Connection = "")]PlayerRegistrationMessage message, ILogger log)
        {
            // Look up tournament

            // Register player for tournament
        }
    }
}
