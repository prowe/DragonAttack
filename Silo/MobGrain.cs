using System;
using System.Threading.Tasks;
using Dragon.Shared;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Streams;

namespace Dragon.Silo
{
    public class MobGrain : Grain, IMobGrain, IRemindable
    {
        private GameCharacterStatus status;
        private IAsyncStream<GameCharacterStatus> eventStream;
        private HateList hateList;
        private IDisposable turnTimer;

        public override Task OnActivateAsync()
        {
            var streamProvider = this.GetStreamProvider("Default");
            eventStream = streamProvider.GetStream<GameCharacterStatus>(Guid.Empty, "MobGrain");

            Spawn();

            return base.OnActivateAsync();
        }

        public Task<GameCharacterStatus> GetStatus()
        {
            GetLogger().TrackTrace($"{IdentityString}: Getting status");
            return Task.FromResult(status);
        }

        public async Task BeAttacked(Guid attackerId)
        {
            var damage = 1;
            GetLogger().TrackTrace($"{IdentityString}: being attacked");

            status.DecrementHealth(damage);
            hateList.RegisterDamage(attackerId, damage);
            if(status.Health <= 0)
            {
                Die();
            }
            await eventStream.OnNextAsync(status);
        }

        private async Task TakeTurn(object payload)
        {
            GetLogger().TrackTrace($"{IdentityString}: taking a turn");

            if (status.Health < 50) 
            {
                await Heal();
            }
            else if (hateList.TopHated != null)
            {
                await Attack(hateList.TopHated.Value);
            }
            else
            {
                await Heal();                
            }
            hateList.FadeHate();
        }

        private async Task Heal()
        {
            status.IncrementHealth(20);
            await eventStream.OnNextAsync(status);
        }

        private Task Attack(Guid target)
        {
            var player = GrainFactory.GetGrain<IPlayerGrain>(target);
            return player.BeAttacked(this.GetGrainIdentity().PrimaryKey);
        }

        private void Spawn()
        {
            this.turnTimer = RegisterTimer(TakeTurn, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
            this.hateList = new HateList();
            this.status = new GameCharacterStatus
            {
                Id = this.GetGrainIdentity().PrimaryKey,
                Health = 100,
                MaxHealth = 100
            };
        }

        private void Die()
        {
            if(this.turnTimer != null)
            {
                this.turnTimer.Dispose();
            }
            RegisterOrUpdateReminder("Respawn", TimeSpan.FromSeconds(30), TimeSpan.MaxValue);
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            Spawn();
            return Task.CompletedTask;
        }
    }
}