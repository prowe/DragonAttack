using System;
using System.Threading.Tasks;
using Dragon.Shared;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Streams;

namespace Dragon.Silo
{
    public class MobGrain : Grain, IMobGrain
    {
        private int maxHealth = 100;
        private int health = 100;
        private IAsyncStream<GameCharacterStatus> eventStream;

        public override Task OnActivateAsync()
        {
            var streamProvider = this.GetStreamProvider("Default");
            eventStream = streamProvider.GetStream<GameCharacterStatus>(Guid.Empty, "MobGrain");
            return Task.CompletedTask;
        }

        public Task<GameCharacterStatus> GetStatus()
        {
            GetLogger().TrackTrace($"{IdentityString}: Getting status");
            return Task.FromResult(Status);
        }

        public async Task BeAttacked(Guid attackerId)
        {
            GetLogger().TrackTrace($"{IdentityString}: being attacked");
            //TODO: add hate list
            health--;
            
            eventStream.OnNextAsync(Status);
        }

        private GameCharacterStatus Status => new GameCharacterStatus
        {
            Id = this.GetGrainIdentity().PrimaryKey,
            Health = health,
            MaxHealth = maxHealth
        };
    }
}