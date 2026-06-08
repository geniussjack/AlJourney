using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Основной класс Character.
    /// </summary>
    public partial class Character : Node
    {
        [Signal]
        /// <summary>
        /// Элемент HealthChangedEventHandler.
        /// </summary>
        public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

        [Signal]
        /// <summary>
        /// Элемент DamageTakenEventHandler.
        /// </summary>
        public delegate void DamageTakenEventHandler(int amount);

        [Signal]
        /// <summary>
        /// Элемент HealedEventHandler.
        /// </summary>
        public delegate void HealedEventHandler(int amount);

        [Signal]
        /// <summary>
        /// Элемент ShieldChangedEventHandler.
        /// </summary>
        public delegate void ShieldChangedEventHandler(int amount);

        [Signal]
        /// <summary>
        /// Элемент StatusEffectAppliedEventHandler.
        /// </summary>
        public delegate void StatusEffectAppliedEventHandler(StatusEffect effect);

        [Signal]
        /// <summary>
        /// Элемент StatusEffectRemovedEventHandler.
        /// </summary>
        public delegate void StatusEffectRemovedEventHandler(StatusEffect effect);

        [Signal]
        /// <summary>
        /// Элемент CharacterDiedEventHandler.
        /// </summary>
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
        /// Элемент CharacterName.
        /// </summary>
        public string CharacterName => _name;

        /// <summary>
        /// Элемент MaxHealth.
        /// </summary>
        public int MaxHealth => _maxHealth;

        /// <summary>
        /// Элемент CurrentHealth.
        /// </summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>
        /// Элемент BaseDamage.
        /// </summary>
        public int BaseDamage => _baseDamage;

        /// <summary>
        /// Элемент BaseDefense.
        /// </summary>
        public int BaseDefense => _baseDefense;

        /// <summary>
        /// Элемент TotalDefense.
        /// </summary>
        public virtual int TotalDefense => _baseDefense;

        /// <summary>
        /// Элемент TotalMaxHealth.
        /// </summary>
        public virtual int TotalMaxHealth => _maxHealth;

        /// <summary>
        /// Элемент CurrentShield.
        /// </summary>
        public int CurrentShield => _currentShield;

        /// <summary>
        /// Элемент AttackType.
        /// </summary>
        public AttackType AttackType => _attackType;

        /// <summary>
        /// Проверяет, является ли Alive.
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// Проверяет, является ли Stunned.
        /// </summary>
        public bool IsStunned => _activeEffects.Any(e => e.Type == StatusEffect.Stunned);

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            _activeEffects = [];
        }

        /// <summary>
        /// Инициализирует .
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

            _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);
        }

        /// <summary>
        /// Элемент TakeDamage.
        /// </summary>
        public virtual int TakeDamage(int damage, AttackType attackType, bool canReflect = true)
        {
            if (!IsAlive)
            {
                return 0;
            }

            if (HasStatusEffect(StatusEffect.Immunity))
            {
                GD.Print($"[{_name}] Immune to damage!");
                return 0;
            }

            int finalDamage = damage;

            int effectiveDefense = TotalDefense;
            
            if (HasStatusEffect(StatusEffect.Weakened))
            {
                effectiveDefense = Mathf.CeilToInt(effectiveDefense * 0.7f); 
                GD.Print($"[{_name}] Defense reduced by Weakened status: {effectiveDefense}");
            }
            
            finalDamage = Mathf.Max(1, finalDamage - effectiveDefense);

            if (_currentShield > 0)
            {
                int shieldAbsorbed = Mathf.Min(_currentShield, finalDamage);
                _currentShield -= shieldAbsorbed;
                finalDamage -= shieldAbsorbed;

                _ = EmitSignal(SignalName.ShieldChanged, _currentShield);
                GD.Print($"[{_name}] Shield absorbed {shieldAbsorbed} damage. Remaining shield: {_currentShield}");
            }

            if (finalDamage > 0)
            {
                _currentHealth = Mathf.Max(0, _currentHealth - finalDamage);
                _ = EmitSignal(SignalName.DamageTaken, finalDamage);
                _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);

                GD.Print($"[{_name}] Took {finalDamage} damage. HP: {_currentHealth}/{TotalMaxHealth}");

                if (!IsAlive)
                {
                    OnDeath();
                }
            }

            if (canReflect)
            {
                StatusEffectData reflectEffect = _activeEffects.FirstOrDefault(e => e.Type == StatusEffect.ShieldReflect);
                if (reflectEffect != null && finalDamage > 0)
                {
                    int reflectedDamage = Mathf.CeilToInt(damage * reflectEffect.ExtraData);
                    GD.Print($"[{_name}] Reflected {reflectedDamage} damage!");
                    return reflectedDamage;
                }
            }

            return 0;
        }

        /// <summary>
        /// Элемент Heal.
        /// </summary>
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
        /// Добавляет Shield.
        /// </summary>
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
        /// Применяет StatusEffect.
        /// </summary>
        public virtual void ApplyStatusEffect(StatusEffectData effect)
        {
            if (!IsAlive || effect == null)
            {
                return;
            }

            if (HasStatusEffect(StatusEffect.Immunity))
            {
                GD.Print($"[{_name}] Immune to status effect: {effect.Type}");
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
                _ = EmitSignal(SignalName.StatusEffectApplied, (int)effect.Type);
                GD.Print($"[{_name}] Applied status effect: {effect.Type} for {effect.Duration} turns");
            }
        }

        /// <summary>
        /// Элемент ClearNegativeEffects.
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
        /// Обрабатывает StatusEffects.
        /// </summary>
        public virtual void ProcessStatusEffects()
        {
            if (!IsAlive)
            {
                return;
            }

            List<StatusEffectData> effectsToRemove = [];

            foreach (StatusEffectData effect in _activeEffects)
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

                StatusEffectData updatedEffect = effect.TickDuration();
                if (updatedEffect.ShouldRemove)
                {
                    effectsToRemove.Add(effect);
                }
                else
                {
                    int index = _activeEffects.IndexOf(effect);
                    _activeEffects[index] = updatedEffect;
                }
            }

            foreach (StatusEffectData effect in effectsToRemove)
            {
                _ = _activeEffects.Remove(effect);
                _ = EmitSignal(SignalName.StatusEffectRemoved, (int)effect.Type);
                GD.Print($"[{_name}] Status effect expired: {effect.Type}");
            }

            if (!IsAlive)
            {
                OnDeath();
            }
        }

        /// <summary>
        /// Проверяет наличие StatusEffect.
        /// </summary>
        public bool HasStatusEffect(StatusEffect effectType)
        {
            return _activeEffects.Any(e => e.Type == effectType);
        }

        /// <summary>
        /// Возвращает ActiveEffects.
        /// </summary>
        public List<StatusEffectData> GetActiveEffects()
        {
            return [.. _activeEffects];
        }

        protected virtual void OnDeath()
        {
            _ = EmitSignal(SignalName.CharacterDied);
            GD.Print($"[{_name}] has died!");
        }

        /// <summary>
        /// Элемент IncreaseMaxHealth.
        /// </summary>
        public void IncreaseMaxHealth(int amount)
        {
            _maxHealth += amount;
            _currentHealth += amount; 
            _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);
        }

        /// <summary>
        /// Элемент IncreaseDamage.
        /// </summary>
        public void IncreaseDamage(int amount)
        {
            _baseDamage += amount;
        }

        /// <summary>
        /// Элемент IncreaseDefense.
        /// </summary>
        public void IncreaseDefense(int amount)
        {
            _baseDefense += amount;
        }
    }
}
