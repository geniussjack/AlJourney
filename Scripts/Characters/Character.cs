using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Base class for every living creature in the game.
    /// Manages health, armor, shields, status effects and
    /// base stats.
    /// </summary>
    public partial class Character : Node
    {
        /// <summary>
        /// Raised when current or maximum health changes.
        /// </summary>
        [Signal]
        public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

        /// <summary>
        /// Raised when direct damage is taken.
        /// </summary>
        [Signal]
        public delegate void DamageTakenEventHandler(int amount);

        /// <summary>
        /// Raised when health is successfully restored.
        /// </summary>
        [Signal]
        public delegate void HealedEventHandler(int amount);

        /// <summary>
        /// Raised when the magic shield's strength changes.
        /// </summary>
        [Signal]
        public delegate void ShieldChangedEventHandler(int amount);

        /// <summary>
        /// Raised when a new status effect is applied to the character.
        /// </summary>
        [Signal]
        public delegate void StatusEffectAppliedEventHandler(StatusEffect effect);

        /// <summary>
        /// Raised when a status effect expires or is cleared.
        /// </summary>
        [Signal]
        public delegate void StatusEffectRemovedEventHandler(StatusEffect effect);

        /// <summary>
        /// Raised when the character's health reaches zero.
        /// </summary>
        [Signal]
        public delegate void CharacterDiedEventHandler();

        [Signal]
        public delegate void StatusEffectAddedEventHandler(int effectType, int duration, int power);

        protected string _name;
        protected int _maxHealth;
        protected int _currentHealth;
        protected int _baseDamage;
        protected int _baseDefense;
        protected int _currentShield;
        protected AttackType _attackType;
        protected List<StatusEffectData> _activeEffects;

        /// <summary>
        /// The character's display name.
        /// </summary>
        public string CharacterName => _name;

        /// <summary>
        /// Base maximum health.
        /// </summary>
        public int MaxHealth => _maxHealth;

        /// <summary>
        /// Current health points.
        /// </summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>
        /// Base damage dealt.
        /// </summary>
        public int BaseDamage => _baseDamage;

        /// <summary>
        /// Base defense stat.
        /// </summary>
        public int BaseDefense => _baseDefense;

        /// <summary>
        /// Total defense stat. May be overridden by heroes to account for equipment.
        /// </summary>
        public virtual int TotalDefense => _baseDefense;

        /// <summary>
        /// Total maximum health. May be overridden by heroes.
        /// </summary>
        public virtual int TotalMaxHealth => _maxHealth;

        /// <summary>
        /// The magic shield's current strength. The shield absorbs any damage before it affects health.
        /// </summary>
        public int CurrentShield => _currentShield;

        /// <summary>
        /// The character's attack type.
        /// </summary>
        public AttackType AttackType => _attackType;

        /// <summary>
        /// Returns True if the character is still alive.
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// Returns True if the character has a stun effect active.
        /// </summary>
        public bool IsStunned => _activeEffects.Any(e => e.Type == StatusEffect.Stunned);

        /// <summary>
        /// Initializes internal structures when the node is added to the scene.
        /// </summary>
        public override void _Ready()
        {
            _activeEffects = [];
        }

        /// <summary>
        /// Sets the character's starting stats.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="maxHealth">Starting health.</param>
        /// <param name="damage">Starting damage.</param>
        /// <param name="defense">Starting defense.</param>
        /// <param name="attackType">The attack type.</param>
        public virtual void Initialize(string name, int maxHealth, int damage, int defense, AttackType attackType = AttackType.Physical)
        {
            _name = name;
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _baseDamage = damage;
            _baseDefense = defense;
            _currentShield = 0;
            _attackType = attackType;
            _activeEffects = [];

            _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);
        }

        /// <summary>
        /// Deals damage to the character, accounting for armor, shields and status effects.
        /// </summary>
        /// <param name="damage">The raw incoming damage.</param>
        /// <param name="attackType">The incoming attack's type.</param>
        /// <param name="canReflect">Whether this damage can be reflected back at the attacker.</param>
        /// <returns>The amount of damage that was reflected back.</returns>
        public virtual int TakeDamage(int damage, AttackType attackType, bool canReflect = true)
        {
            if (!IsAlive || HasStatusEffect(StatusEffect.Immunity))
            {
                return 0;
            }

            int finalDamage = CalculateFinalDamage(damage);
            finalDamage = AbsorbWithShield(finalDamage);
            ApplyHealthDamage(finalDamage);

            return canReflect ? HandleDamageReflection(damage) : 0;
        }

        private int CalculateFinalDamage(int rawDamage)
        {
            int effectiveDefense = TotalDefense;

            if (HasStatusEffect(StatusEffect.Weakened))
            {
                effectiveDefense = Mathf.CeilToInt(effectiveDefense * 0.7f);
                GD.Print($"[{_name}] Defense reduced by Weakened status: {effectiveDefense}");
            }

            int finalDamage = Mathf.Max(1, rawDamage - effectiveDefense);

            if (HasStatusEffect(StatusEffect.Shock) || HasStatusEffect(StatusEffect.Vulnerable))
            {
                finalDamage = Mathf.CeilToInt(finalDamage * 1.5f);
                GD.Print($"[{_name}] Damage increased by Shock/Vulnerable status: {finalDamage}");
            }

            return finalDamage;
        }

        private int AbsorbWithShield(int damage)
        {
            if (_currentShield <= 0)
            {
                return damage;
            }

            int shieldAbsorbed = Mathf.Min(_currentShield, damage);
            _currentShield -= shieldAbsorbed;

            _ = EmitSignal(SignalName.ShieldChanged, _currentShield);
            GD.Print($"[{_name}] Shield absorbed {shieldAbsorbed} damage. Remaining shield: {_currentShield}");

            return damage - shieldAbsorbed;
        }

        private void ApplyHealthDamage(int damageToHealth)
        {
            if (damageToHealth <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damageToHealth);
            _ = EmitSignal(SignalName.DamageTaken, damageToHealth);
            _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);

            GD.Print($"[{_name}] Took {damageToHealth} damage. HP: {_currentHealth}/{TotalMaxHealth}");

            if (!IsAlive)
            {
                OnDeath();
            }
        }

        private int HandleDamageReflection(int originalDamage)
        {
            StatusEffectData reflectEffect = _activeEffects.FirstOrDefault(e => e.Type == StatusEffect.ShieldReflect);
            if (reflectEffect != null && originalDamage > 0)
            {
                int reflectedDamage = Mathf.CeilToInt(originalDamage * reflectEffect.ExtraData);
                GD.Print($"[{_name}] Reflected {reflectedDamage} damage!");
                return reflectedDamage;
            }
            return 0;
        }

        /// <summary>
        /// Restores the character's health, without exceeding the maximum.
        /// </summary>
        /// <param name="amount">The amount to heal.</param>
        public virtual void Heal(int amount)
        {
            if (!IsAlive)
            {
                return;
            }

            int actualHeal = Mathf.Min(amount, TotalMaxHealth - _currentHealth);
            if (actualHeal > 0)
            {
                _currentHealth += actualHeal;
                _ = EmitSignal(SignalName.Healed, actualHeal);
                _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);

                GD.Print($"[{_name}] Healed {actualHeal} HP. HP: {_currentHealth}/{TotalMaxHealth}");
            }
        }

        /// <summary>
        /// Applies a magic shield to the character. Shields can stack.
        /// </summary>
        /// <param name="amount">The strength of the shield to add.</param>
        public virtual void AddShield(int amount)
        {
            if (!IsAlive)
            {
                return;
            }

            _currentShield += amount;
            _ = EmitSignal(SignalName.ShieldChanged, _currentShield);

            GD.Print($"[{_name}] Gained {amount} shield. Total: {_currentShield}");
        }

        /// <summary>
        /// Applies a new status effect.
        /// If an effect of that type is already active, its duration is updated, provided the new
        /// duration is longer.
        /// </summary>
        /// <param name="effect">The status effect data.</param>
        public virtual void ApplyStatusEffect(StatusEffectData effect)
        {
            if (!IsAlive || HasStatusEffect(StatusEffect.Immunity))
            {
                return;
            }

            StatusEffectData existingEffect = _activeEffects.FirstOrDefault(e => e.Type == effect.Type);
            if (existingEffect != null)
            {
                if (effect.Duration > existingEffect.Duration)
                {
                    _ = _activeEffects.Remove(existingEffect);
                    _activeEffects.Add(effect);
                }
            }
            else
            {
                _activeEffects.Add(effect);
            }

            _ = EmitSignal(SignalName.StatusEffectAdded, (int)effect.Type, effect.Duration, effect.Power);
            GD.Print($"[{_name}] Applied status effect: {effect.Type} for {effect.Duration} turns");
        }

        /// <summary>
        /// Clears every negative effect.
        /// Typically triggered by a powerful heal.
        /// </summary>
        public virtual void ClearNegativeEffects()
        {
            StatusEffect[] negativeEffects = [StatusEffect.Burning, StatusEffect.Bleeding, StatusEffect.Weakened, StatusEffect.Stunned];
            List<StatusEffectData> toRemove = [.. _activeEffects.Where(e => negativeEffects.Contains(e.Type))];

            foreach (StatusEffectData effect in toRemove)
            {
                _ = _activeEffects.Remove(effect);
                _ = EmitSignal(SignalName.StatusEffectRemoved, (int)effect.Type);
                GD.Print($"[{_name}] Removed negative effect: {effect.Type}");
            }
        }

        /// <summary>
        /// Called every turn. Processes damage/heal-over-time effects
        /// and ticks down effect duration counters.
        /// </summary>
        public virtual void ProcessStatusEffects()
        {
            if (!IsAlive)
            {
                return;
            }

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                StatusEffectData effect = _activeEffects[i];
                ApplyEffectTick(effect);

                StatusEffectData updatedEffect = effect.TickDuration();
                if (updatedEffect.ShouldRemove)
                {
                    _activeEffects.RemoveAt(i);
                    _ = EmitSignal(SignalName.StatusEffectRemoved, (int)effect.Type);
                    GD.Print($"[{_name}] Status effect expired: {effect.Type}");
                }
                else
                {
                    _activeEffects[i] = updatedEffect;
                }
            }

            if (!IsAlive)
            {
                OnDeath();
            }
        }

        private void ApplyEffectTick(StatusEffectData effect)
        {
            switch (effect.Type)
            {
                case StatusEffect.Burning:
                case StatusEffect.Bleeding:
                    int dotDamage = effect.Power;
                    _currentHealth = Mathf.Max(0, _currentHealth - dotDamage);
                    _ = EmitSignal(SignalName.DamageTaken, dotDamage);
                    _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);
                    GD.Print($"[{_name}] {effect.Type} dealt {dotDamage} damage. HP: {_currentHealth}/{TotalMaxHealth}");
                    break;

                case StatusEffect.Regeneration:
                    Heal(effect.Power);
                    break;
            }
        }

        /// <summary>
        /// Checks whether a specific status effect is active on the character.
        /// </summary>
        /// <param name="effectType">The effect type to look for.</param>
        /// <returns>True if the effect is found.</returns>
        public bool HasStatusEffect(StatusEffect effectType)
        {
            return _activeEffects.Any(e => e.Type == effectType);
        }

        /// <summary>
        /// Returns the list of every effect currently active.
        /// A copy is returned to prevent accidental modification.
        /// </summary>
        public List<StatusEffectData> GetActiveEffects()
        {
            return [.. _activeEffects];
        }

        /// <summary>
        /// Called when the character dies.
        /// </summary>
        protected virtual void OnDeath()
        {
            _ = EmitSignal(SignalName.CharacterDied);
            GD.Print($"[{_name}] has died!");
        }

        /// <summary>
        /// Permanently increases the character's base maximum health.
        /// Current health increases proportionally.
        /// </summary>
        /// <param name="amount">The amount to increase by.</param>
        public void IncreaseMaxHealth(int amount)
        {
            _maxHealth += amount;
            _currentHealth += amount;
            _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);
        }

        /// <summary>
        /// Permanently increases base damage.
        /// </summary>
        /// <param name="amount">The amount to increase by.</param>
        public void IncreaseDamage(int amount)
        {
            _baseDamage += amount;
        }

        /// <summary>
        /// Permanently increases base armor.
        /// </summary>
        /// <param name="amount">The amount to increase by.</param>
        public void IncreaseDefense(int amount)
        {
            _baseDefense += amount;
        }
    }
}
