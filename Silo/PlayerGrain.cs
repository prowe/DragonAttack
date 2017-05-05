using System;
using System.Threading.Tasks;
using Dragon.Shared;
using Orleans;

namespace Dragon.Silo
{
    public class PlayerGrain : Grain, IPlayerGrain
    {
        private int maxHealth = 100;
        private int health = 100;

        public Task<GameCharacterStatus> GetStatus()
        {
            Console.WriteLine($"{IdentityString}: Getting status");
            return Task.FromResult(Status);
        }

        private GameCharacterStatus Status => new GameCharacterStatus
        {
            Id = this.GetGrainIdentity().PrimaryKey,
            Health = health,
            MaxHealth = maxHealth
        };
    }
}