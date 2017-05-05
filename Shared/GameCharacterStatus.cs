using System;

namespace Dragon.Shared
{
    public class GameCharacterStatus
    {
        public Guid Id { get; set; }
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
    }
}