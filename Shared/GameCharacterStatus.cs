using System;

namespace Dragon.Shared
{
    public class GameCharacterStatus
    {
        public Guid Id { get; set; }
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;

        public void IncrementHealth(int amount)
        {
            Health += amount;
            Health = Math.Min(Health, MaxHealth);
            Health = Math.Max(Health, 0);
        }

        public void DecrementHealth(int amount) 
            => IncrementHealth(-1 * amount);
    }
}