
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
using Newtonsoft.Json.Serialization;

namespace Dragon.Web
{
    public class GameController : Controller
    {
        private readonly ILogger<GameController> logger;
        private readonly IGrainFactory grainFactory;
        private readonly IStreamProvider streamProvider;

        private static readonly ISet<WebSocket> sockets = new HashSet<WebSocket>();

        public GameController(ILogger<GameController> logger, IGrainFactory grainFactory, IStreamProvider streamProvider)
        {
            this.logger = logger;
            this.grainFactory = grainFactory;
            this.streamProvider = streamProvider;
        }

        [Route("/players")]
        [HttpPost]
        public async Task<JoinGameResponse> JoinGame()
        {
            Guid playerId = Guid.NewGuid();
            logger.LogInformation($"Player {playerId} joined game");
            var playerStatusTask = grainFactory.GetGrain<IPlayerGrain>(playerId).GetStatus();
            var targetStatusTask = grainFactory.GetGrain<IMobGrain>(Guid.Empty).GetStatus();
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
            await grainFactory.GetGrain<IMobGrain>(Guid.Empty).BeAttacked(playerId);
        }

        [Route("/players/{playerId}/notifications")]
        [HttpGet]
        public async void SubscribeToPlayerNotifications(Guid playerId)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                logger.LogWarning("Invalid websocket request");
                HttpContext.Response.StatusCode = 400;
                return;
            }

            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            logger.LogInformation("Got web socket: " + webSocket + webSocket.State);
            
            var targetStream = streamProvider.GetStream<GameCharacterStatus>(Guid.Empty, "MobGrain");
            var playerStream = streamProvider.GetStream<GameCharacterStatus>(playerId, "PlayerGrain");
            StreamSubscriptionHandle<GameCharacterStatus> targetStreamHandle = null;
            /*Task.Run(async () => {
                var observer = new WebsocketDelegatingObserver(webSocket);
                targetStreamHandle = await targetStream.SubscribeAsync(observer);
                //playerStream.SubscribeAsync(observer);
            });
            */

            var observer = new WebsocketDelegatingObserver(webSocket);
            var subscribeToTargetTask = SubscribeToStream(observer, Guid.Empty, "MobGrain");

            while (webSocket.State == WebSocketState.Open)
            {
                Thread.Sleep(5000);
            }
            
            
        }

        private Task<StreamSubscriptionHandle<GameCharacterStatus>> SubscribeToStream(IAsyncObserver<GameCharacterStatus> observer, Guid streamId, string streamNamespace)
        {
            var stream = streamProvider.GetStream<GameCharacterStatus>(streamId, streamNamespace);
            return Task.Run(() => {
                return stream.SubscribeAsync(observer);
            });
        }

        private class WebsocketDelegatingObserver : IAsyncObserver<GameCharacterStatus>
        {
            private readonly WebSocket webSocket;

            public WebsocketDelegatingObserver(WebSocket webSocket)
            {
                this.webSocket = webSocket;
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
                    string json = ToJson(item);
                    var payload = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
                    return webSocket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                return Task.CompletedTask;
            }

            private string ToJson(GameCharacterStatus item)
            {
                return JsonConvert.SerializeObject(
                    item,
                    Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
                );
            }
        }
    }

    public class JoinGameResponse
    {
        public GameCharacterStatus Player { get; set; }
        public GameCharacterStatus Target { get; set; }
    }
}