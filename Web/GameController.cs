
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
            var playerStatusTask = grainFactory.GetGrain<IPlayerGrain>(playerId).GetStatus();
            var targetStatusTask = grainFactory.GetGrain<IMobGrain>("dragon").GetStatus();
            await Task.WhenAll(playerStatusTask, targetStatusTask);
            return new JoinGameResponse {
                Player = playerStatusTask.Result,
                Target = targetStatusTask.Result
            };
        }

        [Route("/players/{playerId}/attacks")]
        [HttpPost]
        public async Task Attack(Guid playerId)
        {
            logger.LogInformation($"Player {playerId} attacking");
            await grainFactory.GetGrain<IMobGrain>("dragon").BeAttacked(playerId);
        }
    }

    public class JoinGameResponse
    {
        public GameCharacterStatus Player { get; set; }
        public GameCharacterStatus Target { get; set; }
    }
}