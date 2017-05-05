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

            RegisterTimer(TakeTurn, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));

            return base.OnActivateAsync();
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

        private Task TakeTurn(object payload)
        {
            GetLogger().TrackTrace($"{IdentityString}: taking a turn");
            Heal();
            return Task.CompletedTask;
        }

        private void Heal()
        {
            health = Math.Min(maxHealth, health + 20);
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