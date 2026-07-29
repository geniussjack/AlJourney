using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Utils;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Battle
{
    /// <summary>
    /// Сервис для применения эффектов способностей во время пошагового боя:
    /// нанесение урона атакующими способностями и лечение/щит защитными/поддерживающими.
    /// </summary>
    public static class CombatEffectProcessor
    {
        /// <summary>
        /// Применяет атакующую способность ко всем разрешённым целям (одна цель либо все враги при AoE).
        /// </summary>
        public static void ApplyAttackAbility(AbilityData ability, PlayerCharacter caster, IReadOnlyList<Character> targets, BattleManager battleManager, CameraShake cameraShake)
        {
            if (targets.Count == 0)
            {
                return;
            }

            int damage = caster.CalculateDamage(ability.GetEffect("damage"));
            bool isAoE = targets.Count > 1;

            AudioManager.Instance?.PlayAttackSound();
            if (isAoE)
            {
                cameraShake?.ShakeStrong();
            }
            else
            {
                cameraShake?.ShakeMedium();
            }

            ComboParticles.SpawnComboEffect(battleManager, new Vector2(640, isAoE ? 200 : 300), ability.Element, 1);

            foreach (Character target in targets)
            {
                DealDamage(target, damage, caster, isAoE, battleManager);
            }
        }

        private static void DealDamage(Character target, int damage, PlayerCharacter caster, bool isAoE, BattleManager battleManager)
        {
            int reflected = target.TakeDamage(damage, caster.AttackType, canReflect: true);
            Vector2 particlePos = isAoE ? new Vector2(400, 200) : new Vector2(640, 250);

            AudioManager.Instance?.PlayHitSound();
            ComboParticles.SpawnDamageNumber(battleManager, particlePos, damage);

            if (reflected > 0)
            {
                _ = caster.TakeDamage(reflected, target.AttackType, canReflect: false);
            }
        }

        /// <summary>
        /// Применяет защитную/поддерживающую способность ко всем разрешённым целям
        /// (одна цель либо весь отряд при AoE). Поддерживает лечение и/или щит в зависимости от эффектов способности.
        /// </summary>
        public static void ApplySupportAbility(AbilityData ability, IReadOnlyList<Character> targets, DualHeroSystem heroSystem, BattleManager battleManager, CameraShake cameraShake)
        {
            if (targets.Count == 0)
            {
                return;
            }

            cameraShake?.ShakeLight();
            ComboParticles.SpawnComboEffect(battleManager, new Vector2(640, 360), ability.Element, 1);

            int heal = ability.GetEffect("heal");
            int shield = ability.GetEffect("shield");

            foreach (Character target in targets)
            {
                Vector2 position = GetAllyVfxPosition(target, heroSystem);

                if (heal > 0)
                {
                    int healedAmount = PlayerCharacter.CalculateHealing(heal);
                    target.Heal(healedAmount);
                    ComboParticles.SpawnHealNumber(battleManager, position, healedAmount);
                }

                if (shield > 0)
                {
                    int shieldAmount = PlayerCharacter.CalculateShield(shield);
                    target.AddShield(shieldAmount);
                    ComboParticles.SpawnShieldNumber(battleManager, position, shieldAmount);
                }
            }
        }

        /// <summary>
        /// Возвращает экранную позицию для визуальных эффектов над указанным участником отряда.
        /// Используется как боевыми способностями героев, так и атаками врагов по отряду.
        /// </summary>
        internal static Vector2 GetAllyVfxPosition(Character member, DualHeroSystem heroSystem)
        {
            if (member == heroSystem.Mage)
            {
                return new Vector2(200, 100);
            }

            if (member == heroSystem.Warrior)
            {
                return new Vector2(1000, 100);
            }

            // Резерв под слот наёмника (Companion), который в Этапе 1 ещё не используется.
            return new Vector2(600, 100);
        }
    }
}
