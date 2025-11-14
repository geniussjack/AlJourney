using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Base class for all battle characters (players and enemies).
    /// </summary>
    public partial class Character : Node
    {
        [Signal]
        public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

        [Signal]
        public delegate void DamageTakenEventHandler(int amount);

        [Signal]
        public delegate void HealedEventHandler(int amount);

        [Signal]
        public delegate void ShieldChangedEventHandler(int amount);

        [Signal]
        public delegate void StatusEffectAppliedEventHandler(StatusEffect effect);

        [Signal]
        public delegate void StatusEffectRemovedEventHandler(StatusEffect effect);

        [Signal]
        public delegate void CharacterDiedEventHandler();

        protected string _name;
        protected int _maxHealth;
        protected int _currentHealth;
        protected int _baseDamage;
        protected int _baseDefense;
        protected int _currentShield;
        protected AttackType _attackType;

        protected List<StatusEffectData> _activeEffects;

        /// <summary>
        /// Character's display name.
        /// </summary>
        public string CharacterName => _name;

        /// <summary>
        /// Maximum health points.
        /// </summary>
        public int MaxHealth => _maxHealth;

        /// <summary>
        /// Current health points.
        /// </summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>
        /// Base damage value.
        /// </summary>
        public int BaseDamage => _baseDamage;

        /// <summary>
        /// Base defense value.
        /// </summary>
        public int BaseDefense => _baseDefense;

        /// <summary>
        /// Current shield/armor points.
        /// </summary>
        public int CurrentShield => _currentShield;

        /// <summary>
        /// Type of attack (Physical or Magical).
        /// </summary>
        public AttackType AttackType => _attackType;

        /// <summary>
        /// Is character still alive.
        /// </summary>
        public bool IsAlive => _currentHealth > 0;

        /// <summary>
        /// Is character stunned and cannot act.
        /// </summary>
        public bool IsStunned => _activeEffects.Any(e => e.Type == StatusEffect.Stunned);

        public override void _Ready()
        {
            _activeEffects = [];
        }

        /// <summary>
        /// Initializes character with base stats.
        /// </summary>
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

            EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);
        }

        /// <summary>
        /// Takes damage with defense and shield calculation.
        /// </summary>
        public virtual int TakeDamage(int damage, AttackType attackType)
        {
            if (!IsAlive) return 0;

            // Check immunity
            if (HasStatusEffect(StatusEffect.Immunity))
            {
                GD.Print($"[{_name}] Immune to damage!");
                return 0;
            }

            int finalDamage = damage;

            // Apply defense reduction
            finalDamage = Mathf.Max(1, finalDamage - _baseDefense);

            // Apply shield
            if (_currentShield > 0)
            {
                int shieldAbsorbed = Mathf.Min(_currentShield, finalDamage);
                _currentShield -= shieldAbsorbed;
                finalDamage -= shieldAbsorbed;

                EmitSignal(SignalName.ShieldChanged, _currentShield);
                GD.Print($"[{_name}] Shield absorbed {shieldAbsorbed} damage. Remaining shield: {_currentShield}");
            }

            // Apply remaining damage to health
            if (finalDamage > 0)
            {
                _currentHealth = Mathf.Max(0, _currentHealth - finalDamage);
                EmitSignal(SignalName.DamageTaken, finalDamage);
                EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);

                GD.Print($"[{_name}] Took {finalDamage} damage. HP: {_currentHealth}/{_maxHealth}");

                if (!IsAlive)
                {
                    OnDeath();
                }
            }

            // Check for reflect damage
            StatusEffectData reflectEffect = _activeEffects.FirstOrDefault(e => e.Type == StatusEffect.ShieldReflect);
            if (reflectEffect != null && finalDamage > 0)
            {
                int reflectedDamage = Mathf.CeilToInt(damage * reflectEffect.ExtraData);
                GD.Print($"[{_name}] Reflected {reflectedDamage} damage!");
                return reflectedDamage;
            }

            return 0;
        }

        /// <summary>
        /// Heals the character.
        /// </summary>
        public virtual void Heal(int amount)
        {
            if (!IsAlive) return;

            int actualHeal = Mathf.Min(amount, _maxHealth - _currentHealth);
            if (actualHeal > 0)
            {
                _currentHealth += actualHeal;
                EmitSignal(SignalName.Healed, actualHeal);
                EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);

                GD.Print($"[{_name}] Healed {actualHeal} HP. HP: {_currentHealth}/{_maxHealth}");
            }
        }

        /// <summary>
        /// Adds shield/armor points.
        /// </summary>
        public virtual void AddShield(int amount)
        {
            if (!IsAlive) return;

            _currentShield += amount;
            EmitSignal(SignalName.ShieldChanged, _currentShield);

            GD.Print($"[{_name}] Gained {amount} shield. Total: {_currentShield}");
        }

        /// <summary>
        /// Applies a status effect to the character.
        /// </summary>
        public virtual void ApplyStatusEffect(StatusEffectData effect)
        {
            if (!IsAlive || effect == null) return;

            // Check immunity
            if (HasStatusEffect(StatusEffect.Immunity))
            {
                GD.Print($"[{_name}] Immune to status effect: {effect.Type}");
                return;
            }

            // Check if effect already exists
            StatusEffectData existingEffect = _activeEffects.FirstOrDefault(e => e.Type == effect.Type);
            if (existingEffect != null)
            {
                // Refresh duration if new effect is longer
                if (effect.Duration > existingEffect.Duration)
                {
                    existingEffect.Duration = effect.Duration;
                    existingEffect.Power = effect.Power;
                    existingEffect.ExtraData = effect.ExtraData;
                }
            }
            else
            {
                _activeEffects.Add(effect);
                EmitSignal(SignalName.StatusEffectApplied, (int)effect.Type);
                GD.Print($"[{_name}] Applied status effect: {effect.Type} for {effect.Duration} turns");
            }
        }

        /// <summary>
        /// Removes all negative status effects (for 4-match heal).
        /// </summary>
        public virtual void ClearNegativeEffects()
        {
            StatusEffect[] negativeEffects = [StatusEffect.Burning, StatusEffect.Bleeding, StatusEffect.Weakened, StatusEffect.Stunned];

            var toRemove = _activeEffects.Where(e => negativeEffects.Contains(e.Type)).ToList();
            foreach (StatusEffectData effect in toRemove)
            {
                _activeEffects.Remove(effect);
                EmitSignal(SignalName.StatusEffectRemoved, (int)effect.Type);
                GD.Print($"[{_name}] Removed negative effect: {effect.Type}");
            }
        }

        /// <summary>
        /// Processes status effects at start of turn (DoT effects).
        /// </summary>
        public virtual void ProcessStatusEffects()
        {
            if (!IsAlive) return;

            var effectsToRemove = new List<StatusEffectData>();

            foreach (StatusEffectData effect in _activeEffects)
            {
                switch (effect.Type)
                {
                    case StatusEffect.Burning:
                    case StatusEffect.Bleeding:
                        // Apply damage over time
                        int dotDamage = effect.Power;
                        _currentHealth = Mathf.Max(0, _currentHealth - dotDamage);
                        EmitSignal(SignalName.DamageTaken, dotDamage);
                        EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);
                        GD.Print($"[{_name}] {effect.Type} dealt {dotDamage} damage. HP: {_currentHealth}/{_maxHealth}");
                        break;

                    case StatusEffect.Regeneration:
                        // Apply healing over time
                        Heal(effect.Power);
                        break;
                }

                // Tick duration
                if (effect.TickDuration())
                {
                    effectsToRemove.Add(effect);
                }
            }

            // Remove expired effects
            foreach (StatusEffectData effect in effectsToRemove)
            {
                _activeEffects.Remove(effect);
                EmitSignal(SignalName.StatusEffectRemoved, (int)effect.Type);
                GD.Print($"[{_name}] Status effect expired: {effect.Type}");
            }

            if (!IsAlive)
            {
                OnDeath();
            }
        }

        /// <summary>
        /// Checks if character has a specific status effect.
        /// </summary>
        public bool HasStatusEffect(StatusEffect effectType)
        {
            return _activeEffects.Any(e => e.Type == effectType);
        }

        /// <summary>
        /// Gets all active status effects.
        /// </summary>
        public List<StatusEffectData> GetActiveEffects()
        {
            return [.. _activeEffects];
        }

        /// <summary>
        /// Called when character dies.
        /// </summary>
        protected virtual void OnDeath()
        {
            EmitSignal(SignalName.CharacterDied);
            GD.Print($"[{_name}] has died!");
        }

        /// <summary>
        /// Increases max health permanently.
        /// </summary>
        public void IncreaseMaxHealth(int amount)
        {
            _maxHealth += amount;
            _currentHealth += amount; // Also heal by the same amount
            EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);
        }

        /// <summary>
        /// Increases base damage permanently.
        /// </summary>
        public void IncreaseDamage(int amount)
        {
            _baseDamage += amount;
        }

        /// <summary>
        /// Increases base defense permanently.
        /// </summary>
        public void IncreaseDefense(int amount)
        {
            _baseDefense += amount;
        }
    }
}