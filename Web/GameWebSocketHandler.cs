
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
using Microsoft.AspNetCore.Http;

namespace Dragon.Web
{
    public class GameWebSocketHandler
    {
        private readonly ILogger<GameWebSocketHandler> logger;
        private readonly IStreamProvider streamProvider;

        public GameWebSocketHandler(IStreamProvider streamProvider, ILogger<GameWebSocketHandler> logger)
        {
            this.streamProvider = streamProvider;
            this.logger = logger;
        }

        public Task Handle(HttpContext httpContext, Func<Task> next)
        {
            return httpContext.WebSockets.IsWebSocketRequest
                ? HandleWebsocket(httpContext)
                : next();
        }

        private async Task HandleWebsocket(HttpContext httpContext)
        {
            var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            logger.LogInformation("Accepted web socket");

            var playerId = GetPlayerId(httpContext);

            var observer = new WebsocketDelegatingObserver(webSocket);
            var handles = await Task.WhenAll(
                streamProvider.GetStream<GameCharacterStatus>(Guid.Empty, "MobGrain").SubscribeAsync(observer),
                streamProvider.GetStream<GameCharacterStatus>(playerId, "PlayerGrain").SubscribeAsync(observer)
            );

            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<Byte>(new Byte[4096]);
                var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                logger.LogInformation("Got: " + received);
            }

            logger.LogInformation("Closed Socket");

            await Task.WhenAll(handles.Select(Unsubscribe));
            logger.LogInformation("Unsubscribed from streams");
        }

        private Guid GetPlayerId(HttpContext httpContext)
        {
            return Guid.Parse(httpContext.Request.Query["playerId"]);
        }

        private Task Unsubscribe(StreamSubscriptionHandle<GameCharacterStatus> handle)
        {
            return handle.UnsubscribeAsync();
        }
    }

    public class WebsocketDelegatingObserver : IAsyncObserver<GameCharacterStatus>
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
            if (webSocket?.State == WebSocketState.Open)
            {
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