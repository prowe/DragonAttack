using System;
using System.Linq;

namespace Dragon.Silo
{
    public class HateList
    {
        private Guid? lastAttackedBy;

        public void RegisterDamage(Guid attacker, int damage)
        {
            lastAttackedBy = attacker;
        }

        public Guid? TopHated 
        {
            get
            {
                return lastAttackedBy;
            }
        }
    }
}