using System;
using System.Threading.Tasks;
using Dragon.Shared;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Streams;
using Microsoft.Extensions.Logging;

namespace Dragon.Silo
{
    public class MobGrain : Grain, IMobGrain, IRemindable
    {
        private readonly ILogger<MobGrain> logger;
        private GameCharacterStatus status;
        private IAsyncStream<GameCharacterStatus> eventStream;
        private HateList hateList;
        private IDisposable turnTimer;

        public MobGrain(ILogger<MobGrain> logger)
        {
            this.logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            var streamProvider = this.GetStreamProvider("Default");
            eventStream = streamProvider.GetStream<GameCharacterStatus>(Guid.Empty, "MobGrain");

            Spawn();

            await base.OnActivateAsync();
        }

        public Task<GameCharacterStatus> GetStatus()
        {
            logger.LogTrace($"{IdentityString}: Getting status");
            return Task.FromResult(status);
        }

        public async Task BeAttacked(Guid attackerId)
        {
            var damage = 1;
            logger.LogTrace($"{IdentityString}: being attacked");

            status.DecrementHealth(damage);
            hateList.RegisterDamage(attackerId, damage);
            if(status.Health <= 0)
            {
                await Die();
            }
            await eventStream.OnNextAsync(status);
        }

        private async Task TakeTurn(object payload)
        {
            logger.LogTrace($"{IdentityString}: taking a turn");

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
            logger.LogTrace($"Resulting state: Health={status.Health} {hateList}");
        }

        private async Task Heal()
        {
            logger.LogTrace($"{IdentityString}: Healing");
            status.IncrementHealth(20);
            await eventStream.OnNextAsync(status);
        }

        private Task Attack(Guid target)
        {
            logger.LogTrace($"{IdentityString}: Attacking {target}");
            var player = GrainFactory.GetGrain<IPlayerGrain>(target);
            return player.BeAttacked(this.GetGrainIdentity().PrimaryKey);
        }

        private void Spawn()
        {
            logger.LogTrace($"{IdentityString}: Spawning");
            this.turnTimer = RegisterTimer(TakeTurn, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
            this.hateList = new HateList();
            this.status = new GameCharacterStatus
            {
                Id = this.GetGrainIdentity().PrimaryKey,
                Health = 100,
                MaxHealth = 100
            };
        }

        private async Task Die()
        {
            if(this.turnTimer != null)
            {
                this.turnTimer.Dispose();
            }
            await RegisterOrUpdateReminder("Respawn", TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(90));
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            Spawn();
            var reminder = await GetReminder(reminderName);
            await UnregisterReminder(reminder);
        }
    }
}