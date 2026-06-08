using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Основной класс ComboEffect.
    /// </summary>
    public class ComboEffect(ElementType elementType, int comboLevel)
    {
        /// <summary>
        /// Элемент ElementType.
        /// </summary>
        public ElementType ElementType { get; set; } = elementType;
        /// <summary>
        /// Элемент ComboLevel.
        /// </summary>
        public int ComboLevel { get; set; } = comboLevel;
        public int Damage { get; set; }
        public int Healing { get; set; }
        public int Shield { get; set; }
        public bool IsAoE { get; set; }
        public StatusEffectData StatusEffect { get; set; }
    }

    /// <summary>
    /// Менеджер ComboSystem. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class ComboSystem : Node
    {
        [Signal]
        /// <summary>
        /// Элемент CombosProcessedEventHandler.
        /// </summary>
        public delegate void CombosProcessedEventHandler(int comboCount);

        [Signal]
        /// <summary>
        /// Элемент CascadeDetectedEventHandler.
        /// </summary>
        public delegate void CascadeDetectedEventHandler(int cascadeLevel);

        private List<ComboEffect> _lastProcessedEffects = [];
        private int _currentCascadeLevel;

        /// <summary>
        /// Возвращает LastProcessedEffects.
        /// </summary>
        public List<ComboEffect> GetLastProcessedEffects()
        {
            return _lastProcessedEffects;
        }

        /// <summary>
        /// Обрабатывает Matches.
        /// </summary>
        public List<ComboEffect> ProcessMatches(List<MatchResult> matches, bool isCascade = false)
        {
            if (isCascade)
            {
                _currentCascadeLevel++;
                _ = EmitSignal(SignalName.CascadeDetected, _currentCascadeLevel);
                GD.Print($"[ComboSystem] Cascade detected! Level: {_currentCascadeLevel}");
            }
            else
            {
                _currentCascadeLevel = 0;
            }

            List<ComboEffect> comboEffects = [];

            foreach (MatchResult match in matches)
            {
                ComboEffect effect = CreateComboEffect(match);
                if (effect != null)
                {
                    if (_currentCascadeLevel > 0)
                    {
                        float cascadeBonus = 1.0f + (_currentCascadeLevel * 0.2f); 
                        effect.Damage = Mathf.CeilToInt(effect.Damage * cascadeBonus);
                        effect.Healing = Mathf.CeilToInt(effect.Healing * cascadeBonus);
                        effect.Shield = Mathf.CeilToInt(effect.Shield * cascadeBonus);

                        GD.Print($"[ComboSystem] Cascade bonus applied: x{cascadeBonus:F1}");
                    }

                    comboEffects.Add(effect);
                }
            }

            if (comboEffects.Count > 0)
            {
                _lastProcessedEffects = comboEffects;
                _ = EmitSignal(SignalName.CombosProcessed, comboEffects.Count);
                GD.Print($"[ComboSystem] Processed {comboEffects.Count} combo effects");
            }

            return comboEffects;
        }

        /// <summary>
        /// Возвращает CascadeLevel.
        /// </summary>
        public int GetCascadeLevel()
        {
            return _currentCascadeLevel;
        }

        /// <summary>
        /// Сбрасывает Cascade.
        /// </summary>
        public void ResetCascade()
        {
            _currentCascadeLevel = 0;
        }

        private static ComboEffect CreateComboEffect(MatchResult match)
        {
            int comboLevel = match.GetComboLevel();
            if (comboLevel == 0)
            {
                return null;
            }

            ComboEffect effect = new(match.ElementType, comboLevel);

            switch (match.ElementType)
            {
                case ElementType.Fire:
                    ProcessFireCombo(effect, comboLevel);
                    break;

                case ElementType.Sword:
                    ProcessSwordCombo(effect, comboLevel);
                    break;

                case ElementType.Heal:
                    ProcessHealCombo(effect, comboLevel);
                    break;

                case ElementType.Shield:
                    ProcessShieldCombo(effect, comboLevel);
                    break;
            }

            return effect;
        }

        private static void ProcessFireCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1: 
                    effect.Damage = GameConstants.FIRE_3_DAMAGE;
                    effect.IsAoE = false;
                    break;

                case 2: 
                    effect.Damage = GameConstants.FIRE_4_DAMAGE;
                    effect.IsAoE = false;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Burning,
                        GameConstants.FIRE_4_BURN_DURATION,
                        GameConstants.FIRE_4_BURN_DAMAGE
                    );
                    break;

                case 3: 
                    effect.Damage = GameConstants.FIRE_5_DAMAGE;
                    effect.IsAoE = true; 
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Burning,
                        GameConstants.FIRE_5_BURN_DURATION,
                        GameConstants.FIRE_5_BURN_DAMAGE
                    );
                    break;
            }

            GD.Print($"[ComboSystem] Fire combo level {level}: {effect.Damage} damage" +
                     (effect.IsAoE ? " (AoE)" : "") +
                     (effect.StatusEffect != null ? " + Burning" : ""));
        }

        private static void ProcessSwordCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1: 
                    effect.Damage = GameConstants.SWORD_3_DAMAGE;
                    effect.IsAoE = false;
                    break;

                case 2: 
                    effect.Damage = GameConstants.SWORD_4_DAMAGE;
                    effect.IsAoE = false;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Bleeding,
                        GameConstants.SWORD_4_BLEED_DURATION,
                        GameConstants.SWORD_4_BLEED_DAMAGE
                    );
                    break;

                case 3: 
                    effect.Damage = GameConstants.SWORD_5_DAMAGE;
                    effect.IsAoE = false;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Stunned,
                        1, 
                        0
                    );
                    break;
            }

            GD.Print($"[ComboSystem] Sword combo level {level}: {effect.Damage} damage" +
                     (effect.StatusEffect?.Type == StatusEffect.Bleeding ? " + Bleeding" : "") +
                     (effect.StatusEffect?.Type == StatusEffect.Stunned ? " + Stun" : ""));
        }

        private static void ProcessHealCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1: 
                    effect.Healing = GameConstants.HEAL_3_AMOUNT;
                    break;

                case 2: 
                    effect.Healing = GameConstants.HEAL_4_AMOUNT;
                    break;

                case 3: 
                    effect.Healing = GameConstants.HEAL_5_AMOUNT;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Regeneration,
                        GameConstants.HEAL_5_REGEN_DURATION,
                        GameConstants.HEAL_5_REGEN_AMOUNT
                    );
                    break;
            }

            GD.Print($"[ComboSystem] Heal combo level {level}: {effect.Healing} HP" +
                     (level == 2 ? " + Cleanse" : "") +
                     (effect.StatusEffect != null ? " + Regeneration" : ""));
        }

        private static void ProcessShieldCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1: 
                    effect.Shield = GameConstants.SHIELD_3_AMOUNT;
                    break;

                case 2: 
                    effect.Shield = GameConstants.SHIELD_4_AMOUNT;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.ShieldReflect,
                        1, 
                        0,
                        GameConstants.SHIELD_4_REFLECT_PERCENT
                    );
                    break;

                case 3: 
                    effect.Shield = GameConstants.SHIELD_5_AMOUNT;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Immunity,
                        1, 
                        0
                    );
                    break;
            }

            GD.Print($"[ComboSystem] Shield combo level {level}: {effect.Shield} shield" +
                     (effect.StatusEffect?.Type == StatusEffect.ShieldReflect ? " + Reflect" : "") +
                     (effect.StatusEffect?.Type == StatusEffect.Immunity ? " + Immunity" : ""));
        }
    }
}
