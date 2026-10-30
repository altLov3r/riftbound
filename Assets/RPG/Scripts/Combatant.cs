using System;
using UnityEngine;

namespace Riftbound
{
    public class Combatant : MonoBehaviour
    {
        public string displayName = "Forest raider";
        public float maxHealth = 100;
        public bool isPlayer;
        public float flatDamageReduction;
        [System.NonSerialized] public float incomingDamageMultiplier = 1;
        public float Health { get; private set; }
        public bool Alive => Health > 0;
        public event Action<Combatant> Died;
        public event Action<float> Damaged, Healed;
        public float LastHit { get; private set; } = -10;

        void Awake() { Health = maxHealth; gameObject.AddComponent<CombatFeedback>(); }
        public void Damage(float amount)
        {
            if (!Alive || amount <= 0) return;
            amount = Mathf.Max(1, (amount - flatDamageReduction) * Mathf.Clamp01(incomingDamageMultiplier));
            float previous = Health;
            Health = Mathf.Max(0, Health - amount);
            LastHit = Time.time;
            Damaged?.Invoke(previous - Health);
            if (!Alive) Died?.Invoke(this);
        }
        public void Heal(float amount)
        {
            if (!Alive || amount <= 0) return;
            float previous = Health; Health = Mathf.Min(maxHealth, Health + amount);
            float actualHeal = Health - previous;
            if (actualHeal > 0)
            {
                CombatTextManager.SpawnHeal(transform.position, actualHeal);
                Healed?.Invoke(actualHeal);
            }
        }
        public void Restore() { Health = maxHealth; }
        public void ConfigureVitals(float maximum, float reduction)
        {
            maxHealth = Mathf.Max(1, maximum); flatDamageReduction = Mathf.Max(0, reduction);
            Health = Mathf.Min(Health, maxHealth);
        }
    }
}
