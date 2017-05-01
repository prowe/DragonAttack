using System;
using System.Threading.Tasks;
using Dragon.Shared;
using Orleans;

namespace Dragon.Silo
{
    public class MobGrain : Grain, IMobGrain
    {
        private int maxHealth = 100;
        private int health = 100;

        public Task<GameCharacterStatus> GetStatus()
        {
            Console.WriteLine($"{IdentityString}: Getting status");
            return Task.FromResult(new GameCharacterStatus {
               Health = health,
               MaxHealth = maxHealth 
            });
        }

        public Task BeAttacked(Guid attackerId)
        {
            //TODO: add hate list
            Console.WriteLine($"{IdentityString}: being attacked");
            health--;
            return Task.CompletedTask;
        }
    }
}