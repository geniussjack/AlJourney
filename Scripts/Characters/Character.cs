using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Базовый класс для всех живых существ в игре.
    /// Управляет здоровьем, броней, щитами, статусными эффектами и 
    /// базовыми характеристиками.
    /// </summary>
    public partial class Character : Node
    {
        /// <summary>
        /// Вызывается при изменении текущего или максимального здоровья.
        /// </summary>
        [Signal]
        public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

        /// <summary>
        /// Вызывается при получении прямого урона.
        /// </summary>
        [Signal]
        public delegate void DamageTakenEventHandler(int amount);

        /// <summary>
        /// Вызывается при успешном восстановлении здоровья.
        /// </summary>
        [Signal]
        public delegate void HealedEventHandler(int amount);

        /// <summary>
        /// Вызывается при изменении прочности магического щита.
        /// </summary>
        [Signal]
        public delegate void ShieldChangedEventHandler(int amount);

        /// <summary>
        /// Вызывается, когда на персонажа накладывается новый статусный эффект.
        /// </summary>
        [Signal]
        public delegate void StatusEffectAppliedEventHandler(StatusEffect effect);

        /// <summary>
        /// Вызывается, когда статусный эффект завершает действие или очищается.
        /// </summary>
        [Signal]
        public delegate void StatusEffectRemovedEventHandler(StatusEffect effect);

        /// <summary>
        /// Вызывается, когда здоровье персонажа опускается до нуля.
        /// </summary>
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
        /// Отображаемое имя персонажа.
        /// </summary>
        public string CharacterName => _name;

        /// <summary>
        /// Базовое максимальное здоровье.
        /// </summary>
        public int MaxHealth => _maxHealth;

        /// <summary>
        /// Текущее количество очков здоровья.
        /// </summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>
        /// Базовый наносимый урон.
        /// </summary>
        public int BaseDamage => _baseDamage;

        /// <summary>
        /// Базовый показатель защиты.
        /// </summary>
        public int BaseDefense => _baseDefense;

        /// <summary>
        /// Суммарный показатель защиты. Может переопределяться у героев для учета экипировки.
        /// </summary>
        public virtual int TotalDefense => _baseDefense;

        /// <summary>
        /// Суммарное максимальное здоровье. Может переопределяться у героев.
        /// </summary>
        public virtual int TotalMaxHealth => _maxHealth;

        /// <summary>
        /// Текущая прочность магического щита. Щит поглощает любой урон до того, как он затронет здоровье.
        /// </summary>
        public int CurrentShield => _currentShield;

        /// <summary>
        /// Тип атаки персонажа.
        /// </summary>
        public AttackType AttackType => _attackType;

        /// <summary>
        /// Возвращает True, если персонаж еще жив.
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// Возвращает True, если на персонаже висит эффект оглушения.
        /// </summary>
        public bool IsStunned => _activeEffects.Any(e => e.Type == StatusEffect.Stunned);

        /// <summary>
        /// Инициализация внутренних структур при добавлении узла на сцену.
        /// </summary>
        public override void _Ready()
        {
            _activeEffects = [];
        }

        /// <summary>
        /// Устанавливает начальные характеристики персонажа.
        /// </summary>
        /// <param name="name">Имя.</param>
        /// <param name="maxHealth">Стартовое здоровье.</param>
        /// <param name="damage">Стартовый урон.</param>
        /// <param name="defense">Стартовая броня.</param>
        /// <param name="attackType">Тип атаки.</param>
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
        /// Наносит урон персонажу с учетом его брони, щитов и статусных эффектов.
        /// </summary>
        /// <param name="damage">Сырой входящий урон.</param>
        /// <param name="attackType">Тип входящей атаки.</param>
        /// <param name="canReflect">Можно ли отразить этот урон обратно атакующему.</param>
        /// <returns>Количество урона, которое было отражено обратно.</returns>
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

            return Mathf.Max(1, rawDamage - effectiveDefense);
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
        /// Восстанавливает здоровье персонажа, не превышая максимального лимита.
        /// </summary>
        /// <param name="amount">Количество очков лечения.</param>
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
        /// Накладывает магический щит на персонажа. Щиты могут стакаться.
        /// </summary>
        /// <param name="amount">Прочность добавляемого щита.</param>
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
        /// Применяет новый статусный эффект.
        /// Если эффект такого типа уже висит, обновляет его длительность при условии, что новая длительность больше.
        /// </summary>
        /// <param name="effect">Объект с данными статусного эффекта.</param>
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
        /// Снимает все негативные эффекты.
        /// Обычно вызывается при мощном исцелении.
        /// </summary>
        public virtual void ClearNegativeEffects()
        {
            StatusEffect[] negativeEffects = { StatusEffect.Burning, StatusEffect.Bleeding, StatusEffect.Weakened, StatusEffect.Stunned };
            List<StatusEffectData> toRemove = _activeEffects.Where(e => negativeEffects.Contains(e.Type)).ToList();

            foreach (StatusEffectData effect in toRemove)
            {
                _ = _activeEffects.Remove(effect);
                _ = EmitSignal(SignalName.StatusEffectRemoved, (int)effect.Type);
                GD.Print($"[{_name}] Removed negative effect: {effect.Type}");
            }
        }

        /// <summary>
        /// Вызывается каждый ход. Обрабатывает периодический урон, регенерацию
        /// и уменьшает счетчики длительности эффектов.
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
                ApplyEffectTick(effect);

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
        /// Проверяет, действует ли на персонажа конкретный статусный эффект.
        /// </summary>
        /// <param name="effectType">Тип искомого эффекта.</param>
        /// <returns>True, если эффект найден.</returns>
        public bool HasStatusEffect(StatusEffect effectType)
        {
            return _activeEffects.Any(e => e.Type == effectType);
        }

        /// <summary>
        /// Возвращает список всех активных на данный момент эффектов.
        /// Возвращается копия, чтобы предотвратить случайные модификации.
        /// </summary>
        public List<StatusEffectData> GetActiveEffects()
        {
            return _activeEffects.ToList();
        }

        /// <summary>
        /// Вызывается при смерти персонажа.
        /// </summary>
        protected virtual void OnDeath()
        {
            _ = EmitSignal(SignalName.CharacterDied);
            GD.Print($"[{_name}] has died!");
        }

        /// <summary>
        /// Перманентно увеличивает базовое максимальное здоровье персонажа.
        /// Текущее здоровье увеличивается пропорционально.
        /// </summary>
        /// <param name="amount">Размер прибавки.</param>
        public void IncreaseMaxHealth(int amount)
        {
            _maxHealth += amount;
            _currentHealth += amount;
            _ = EmitSignal(SignalName.HealthChanged, _currentHealth, TotalMaxHealth);
        }

        /// <summary>
        /// Перманентно увеличивает базовый урон.
        /// </summary>
        /// <param name="amount">Размер прибавки.</param>
        public void IncreaseDamage(int amount)
        {
            _baseDamage += amount;
        }

        /// <summary>
        /// Перманентно увеличивает базовую броню.
        /// </summary>
        /// <param name="amount">Размер прибавки.</param>
        public void IncreaseDefense(int amount)
        {
            _baseDefense += amount;
        }
    }
}
