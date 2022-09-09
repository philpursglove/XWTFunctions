using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using XWTFunctions.Messages;

namespace XWTFunctions.Queue
{
    public class RecordGameScore
    {
        [FunctionName("RecordGameScoreFromQueue")]
        public void Run([QueueTrigger("scoresonthedoors", Connection = "")]GameScoreMessage message, ILogger log)
        {
            
        }
    }
}
