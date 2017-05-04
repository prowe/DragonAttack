
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dragon.Shared;
using System;
using Orleans;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Orleans.Streams;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Dragon.Web
{
    public class GameController : Controller
    {
        private readonly ILogger<GameController> logger;
        private readonly IGrainFactory grainFactory;
        private readonly IStreamProvider streamProvider;

        private static readonly ISet<WebsocketDelegatingObserver> streamObservers = new HashSet<WebsocketDelegatingObserver>();

        public GameController(ILogger<GameController> logger, IGrainFactory grainFactory, IStreamProvider streamProvider)
        {
            this.logger = logger;
            this.grainFactory = grainFactory;
            this.streamProvider = streamProvider;
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

        [Route("/players/{playerId}/notifications")]
        [HttpGet]
        public async void SubscribeToPlayerNotifications(Guid playerId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                logger.LogInformation("Got web socket: " + webSocket + webSocket.State);
                var payload = new ArraySegment<byte>(Encoding.UTF8.GetBytes("hello"));
                await webSocket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);

                await webSocket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
            
                var observer = new WebsocketDelegatingObserver(webSocket, this);
                var stream = streamProvider.GetStream<GameCharacterStatus>(Guid.Empty, "MobGrain");
                await stream.SubscribeAsync(observer);
                    

            }
            else
            {
                logger.LogWarning("Invalid websocket request");
                HttpContext.Response.StatusCode = 400;
            }
        }
        private class WebsocketDelegatingObserver : IAsyncObserver<GameCharacterStatus>
        {
            private readonly WebSocket webSocket;
            private readonly GameController controller;

            public WebsocketDelegatingObserver(WebSocket webSocket, GameController gameController)
            {
                this.webSocket = webSocket;
                this.controller = gameController;
            }

            public Task OnCompletedAsync()
            {
                return Task.CompletedTask;
            }

            public Task OnErrorAsync(Exception ex)
            {
                return Task.CompletedTask;
            }

            public Task OnNextAsync(GameCharacterStatus item, StreamSequenceToken token = null)
            {
                if (webSocket?.State == WebSocketState.Open) {
                    string json = JsonConvert.SerializeObject(item);
                    var payload = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
                    return webSocket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                return Task.CompletedTask;
            }
        }
    }


    public class JoinGameResponse
    {
        public GameCharacterStatus Player { get; set; }
        public GameCharacterStatus Target { get; set; }
    }
}