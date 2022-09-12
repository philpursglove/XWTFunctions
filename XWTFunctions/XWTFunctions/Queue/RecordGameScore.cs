using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
                log.LogError($"Game with id {message.GameId} not found");
                return;
            }

            game.Player1Score = message.Player1Score;
            game.Player2Score = message.Player2Score;
            game.Player1Concede = message.Player1Concede;
            game.Player2Concede = message.Player2Concede;

            TournamentPlayer player1 = tournamentPlayerRepository.Query().FirstOrDefault(g => g.Id == game.Player1Id);
            TournamentPlayer player2 = tournamentPlayerRepository.Query().FirstOrDefault(g => g.Id == game.Player2Id);

            if (game.Player1Concede || game.Player2Concede)
            {
                // Concession
                if (game.Player1Concede)
                {
                    // Player2 +3 points, 20 mission points
                    player2.Points += 3;
                    player2.MissionPoints += 20;
                }
                else
                {
                    // Player1 +3 points, 20 mission points
                    player1.Points += 3;
                    player1.MissionPoints += 20;
                }
            }
            else
            {
                player1.MissionPoints += message.Player1Score;
                player2.MissionPoints += message.Player2Score;

                if (game.Player1Score == game.Player2Score)
                {
                    // Draw
                    player1.Points += 1;
                    player2.Points += 1;

                }
                else
                {
                    if (game.Player1Score > game.Player2Score)
                    {
                        // Player 1 win
                        player1.Points += 3;
                    }
                    else
                    {
                        // Player 2 win
                        player2.Points += 3;
                    }
                }
            }

            // Drops
            if (message.Player1Drop)
            {
                player1.Dropped = true;
            }

            if (message.Player2Drop)
            {
                player2.Dropped = true;
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

        public int MissionPoints { get; set; }

        public bool Dropped { get; set; }
    }
}
