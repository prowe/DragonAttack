using System;
using System.Linq;
using System.Collections.Generic;

namespace Dragon.Silo
{
    public class HateList
    {
        private IDictionary<Guid, int> damageByAttacker = new Dictionary<Guid, int>();
        private int hateSizeLimit = 5;

        public void RegisterDamage(Guid attacker, int damage)
        {
            if (damageByAttacker.ContainsKey(attacker))
            {
                damageByAttacker[attacker] = damageByAttacker[attacker] + damage;
            }
            else
            {
                damageByAttacker[attacker] = damage;
                cleanupHateList();
            }
        }

        public Guid? TopHated 
        {
            get
            {
                return !damageByAttacker.Any()
                    ? null
                    : (Guid?)damageByAttacker.OrderByDescending(kv => kv.Value).First().Key;
            }
        }

        public void FadeHate()
        {
            damageByAttacker = damageByAttacker
                .ToDictionary(kv => kv.Key, kv => kv.Value - 1);
        }

        private void cleanupHateList()
        {
            damageByAttacker = damageByAttacker
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Take(hateSizeLimit)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

    }
}