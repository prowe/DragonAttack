
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dragon.Shared;
using System;
using Orleans;
using System.Threading.Tasks;

namespace Dragon.Web
{
    public class GameController : Controller
    {
        private readonly ILogger<GameController> logger;
        private readonly IGrainFactory grainFactory;

        public GameController(ILogger<GameController> logger, IGrainFactory grainFactory)
        {
            this.logger = logger;
            this.grainFactory = grainFactory;
        }

        [Route("/players/{playerId}")]
        [HttpPost]
        public async Task<JoinGameResponse> JoinGame(Guid playerId)
        {
            logger.LogInformation($"Player {playerId} joined game");
            return new JoinGameResponse {
                Player = await grainFactory.GetGrain<IPlayerGrain>(playerId).GetStatus()
            };
        }

        [Route("/players/{playerId}/attacks")]
        [HttpPost]
        public void Attack(Guid playerId)
        {
            logger.LogInformation($"Player {playerId} attacking");
        }
    }

    public class JoinGameResponse
    {
        public GameCharacterStatus Player { get; set; }
        public GameCharacterStatus Target { get; set; }
    }
}