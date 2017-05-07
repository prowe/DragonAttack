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
        private GameCharacterStatus status;
        private IAsyncStream<GameCharacterStatus> eventStream;
        private readonly HateList hateList = new HateList();

        public override Task OnActivateAsync()
        {
            var streamProvider = this.GetStreamProvider("Default");
            eventStream = streamProvider.GetStream<GameCharacterStatus>(Guid.Empty, "MobGrain");

            RegisterTimer(TakeTurn, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));

            this.status = new GameCharacterStatus
            {
                Id = this.GetGrainIdentity().PrimaryKey,
                Health = 100,
                MaxHealth = 100
            };

            return base.OnActivateAsync();
        }

        public Task<GameCharacterStatus> GetStatus()
        {
            GetLogger().TrackTrace($"{IdentityString}: Getting status");
            return Task.FromResult(status);
        }

        public Task BeAttacked(Guid attackerId)
        {
            var damage = 1;
            GetLogger().TrackTrace($"{IdentityString}: being attacked");

            status.DecrementHealth(damage);
            hateList.RegisterDamage(attackerId, damage);
            eventStream.OnNextAsync(status);
            return Task.CompletedTask;
        }

        private async Task TakeTurn(object payload)
        {
            GetLogger().TrackTrace($"{IdentityString}: taking a turn");

            if (status.Health < 50) 
            {
                Heal();
            }
            else if (hateList.TopHated != null)
            {
                await Attack(hateList.TopHated.Value);
            }
            else
            {
                Heal();                
            }
        }

        private void Heal()
        {
            status.IncrementHealth(20);
            eventStream.OnNextAsync(status);
        }

        private Task Attack(Guid target)
        {
            var player = GrainFactory.GetGrain<IPlayerGrain>(target);
            return player.BeAttacked(this.GetGrainIdentity().PrimaryKey);
        }
    }
}