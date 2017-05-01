
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dragon.Web
{
    public class GameController : Controller
    {
        private readonly ILogger<GameController> logger;

        public GameController(ILogger<GameController> logger)
        {
            this.logger = logger;
        }

        [Route("/players/{playerId}")]
        [HttpPost]
        public JoinGameResponse JoinGame(string playerId)
        {
            logger.LogInformation($"Player {playerId} joined game");
            return new JoinGameResponse();
        }

        [Route("/players/{playerId}/attacks")]
        [HttpPost]
        public void Attack(string playerId)
        {
            logger.LogInformation($"Player {playerId} attacking");
        }
    }

    public class JoinGameResponse
    {
        public GameCharacterStatus Player { get; set; }
        public GameCharacterStatus Target { get; set; }
    }

    public class GameCharacterStatus
    {
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
    }
}