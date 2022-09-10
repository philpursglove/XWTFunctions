using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using XWTFunctions.Messages;

namespace XWTFunctions.Queue
{
    public class RecordGameScore
    {
        private readonly IRepository<Game> gameRepository;
        private readonly IRepository<TournamentPlayer> tournamentPlayerRepository;
        public RecordGameScore(IRepository<Game> gameRepo, IRepository<TournamentPlayer> tournamentPlayerRepo)
        {
            gameRepository = gameRepo;
            tournamentPlayerRepository = tournamentPlayerRepo;
        }

        [FunctionName("RecordGameScoreFromQueue")]
        public void Run([QueueTrigger("scoresonthedoors", Connection = "")]GameScoreMessage message, ILogger log)
        {
            Game game = gameRepository.Query().First(g => g.Id == message.GameId);
            if (game == null)
            {
                log.LogError($"Game with id {message.GameId.ToString()} not found");
                return;
            }

            game.Player1Score = message.Player1Score;
            game.Player2Score = message.Player2Score;
            game.Player1Concede = message.Player1Concede;
            game.Player2Concede = message.Player2Concede;

            if (game.Player1Concede || game.Player2Concede)
            {
                // Concession
                if (game.Player1Concede)
                {

                }
                else
                {
                    
                }
            }
            else
            {
                if (game.Player1Score == game.Player2Score)
                {
                    // Draw
                }
                else
                {
                    if (game.Player1Score > game.Player2Score)
                    {
                        // Player 1 win
                    }
                    else
                    {
                        // Player 2 win
                    }
                }
            }

            // Drops
            if (message.Player1Drop)
            {}

            if (message.Player2Drop)
            {

            }
        }
    }

    public interface IRepository<T>
    {
        IQueryable<T> Query();
    }

    public class Game
    {
        public Guid Id { get; set; }

        public Guid Player1Id { get; set; }
        public Guid Player2Id { get; set; }

        public int Player1Score { get; set; }
        public int Player2Score { get; set; }

        public int Turns { get; set; }
        public bool Player1Concede { get; internal set; }
        public bool Player2Concede { get; internal set; }
    }

    public class TournamentPlayer
    {
        public Guid Id { get; set; }

        public int Points { get; set; }
    }
}
