using Godot;
using System.Collections.Generic;
using AltarionsJourney.Core;
using AltarionsJourney.Data;

namespace AltarionsJourney.Match3
{
    /// <summary>
    /// Data structure representing a combo effect to be applied in battle.
    /// </summary>
    public class ComboEffect
    {
        public ElementType ElementType { get; set; }
        public int ComboLevel { get; set; }
        public int Damage { get; set; }
        public int Healing { get; set; }
        public int Shield { get; set; }
        public bool IsAoE { get; set; }
        public StatusEffectData StatusEffect { get; set; }

        public ComboEffect(ElementType elementType, int comboLevel)
        {
            ElementType = elementType;
            ComboLevel = comboLevel;
        }
    }

    /// <summary>
    /// Processes match-3 combos and converts them into battle effects.
    /// </summary>
    public partial class ComboSystem : Node
    {
        [Signal]
        public delegate void ComboProcessedEventHandler(ComboEffect effect);

        [Signal]
        public delegate void AllCombosProcessedEventHandler(List<ComboEffect> effects);

        /// <summary>
        /// Processes all match results and converts them to combat effects.
        /// </summary>
        public List<ComboEffect> ProcessMatches(List<MatchResult> matches)
        {
            var comboEffects = new List<ComboEffect>();

            foreach (var match in matches)
            {
                var effect = CreateComboEffect(match);
                if (effect != null)
                {
                    comboEffects.Add(effect);
                    EmitSignal(SignalName.ComboProcessed, effect);
                }
            }

            if (comboEffects.Count > 0)
            {
                EmitSignal(SignalName.AllCombosProcessed, comboEffects);
                GD.Print($"[ComboSystem] Processed {comboEffects.Count} combo effects");
            }

            return comboEffects;
        }

        /// <summary>
        /// Creates a combo effect based on match result.
        /// </summary>
        private ComboEffect CreateComboEffect(MatchResult match)
        {
            int comboLevel = match.GetComboLevel();
            if (comboLevel == 0) return null;

            var effect = new ComboEffect(match.ElementType, comboLevel);

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

        /// <summary>
        /// Processes Fire (Fireball) combo effects.
        /// </summary>
        private void ProcessFireCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1: // 3-match
                    effect.Damage = GameConstants.FIRE_3_DAMAGE;
                    effect.IsAoE = false;
                    break;

                case 2: // 4-match
                    effect.Damage = GameConstants.FIRE_4_DAMAGE;
                    effect.IsAoE = false;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Burning,
                        GameConstants.FIRE_4_BURN_DURATION,
                        GameConstants.FIRE_4_BURN_DAMAGE
                    );
                    break;

                case 3: // 5-match
                    effect.Damage = GameConstants.FIRE_5_DAMAGE;
                    effect.IsAoE = true; // Hits all enemies
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

        /// <summary>
        /// Processes Sword (Axe) combo effects.
        /// </summary>
        private void ProcessSwordCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1: // 3-match
                    effect.Damage = GameConstants.SWORD_3_DAMAGE;
                    effect.IsAoE = false;
                    break;

                case 2: // 4-match
                    effect.Damage = GameConstants.SWORD_4_DAMAGE;
                    effect.IsAoE = false;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Bleeding,
                        GameConstants.SWORD_4_BLEED_DURATION,
                        GameConstants.SWORD_4_BLEED_DAMAGE
                    );
                    break;

                case 3: // 5-match
                    effect.Damage = GameConstants.SWORD_5_DAMAGE;
                    effect.IsAoE = false;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Stunned,
                        1, // 1 turn stun
                        0
                    );
                    break;
            }

            GD.Print($"[ComboSystem] Sword combo level {level}: {effect.Damage} damage" +
                     (effect.StatusEffect?.Type == StatusEffect.Bleeding ? " + Bleeding" : "") +
                     (effect.StatusEffect?.Type == StatusEffect.Stunned ? " + Stun" : ""));
        }

        /// <summary>
        /// Processes Heal combo effects.
        /// </summary>
        private void ProcessHealCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1: // 3-match
                    effect.Healing = GameConstants.HEAL_3_AMOUNT;
                    break;

                case 2: // 4-match
                    effect.Healing = GameConstants.HEAL_4_AMOUNT;
                    // Also removes negative effects (handled in battle system)
                    break;

                case 3: // 5-match
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

        /// <summary>
        /// Processes Shield combo effects.
        /// </summary>
        private void ProcessShieldCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1: // 3-match
                    effect.Shield = GameConstants.SHIELD_3_AMOUNT;
                    break;

                case 2: // 4-match
                    effect.Shield = GameConstants.SHIELD_4_AMOUNT;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.ShieldReflect,
                        1, // Lasts 1 turn
                        0,
                        GameConstants.SHIELD_4_REFLECT_PERCENT
                    );
                    break;

                case 3: // 5-match
                    effect.Shield = GameConstants.SHIELD_5_AMOUNT;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Immunity,
                        1, // Lasts 1 turn
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