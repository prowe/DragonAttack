using System;
using System.Threading.Tasks;
using Dragon.Shared;
using Orleans;
using Orleans.Streams;

namespace Dragon.Silo
{
    public class PlayerGrain : Grain, IPlayerGrain
    {
        private GameCharacterStatus status;
        private IAsyncStream<GameCharacterStatus> eventStream;

        public override Task OnActivateAsync()
        {
            this.status = new GameCharacterStatus
            {
                Id = this.GetGrainIdentity().PrimaryKey,
                Health = 15,
                MaxHealth = 15
            };

            var streamProvider = this.GetStreamProvider("Default");
            eventStream = streamProvider.GetStream<GameCharacterStatus>(status.Id, "PlayerGrain");

            return base.OnActivateAsync();
        }

        public Task<GameCharacterStatus> GetStatus()
        {
            Console.WriteLine($"{IdentityString}: Getting status");
            return Task.FromResult(status);
        }

        public Task BeAttacked(Guid attackerId)
        {
            var damage = 1;
            GetLogger().TrackTrace($"{IdentityString}: being attacked");

            status.DecrementHealth(damage);
            eventStream.OnNextAsync(status);
            return Task.CompletedTask;
        }
    }
}