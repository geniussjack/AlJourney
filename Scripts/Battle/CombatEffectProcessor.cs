using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Match3;
using AlJourney.Scripts.Utils;
using Godot;
using System.Linq;

namespace AlJourney.Scripts.Battle
{
    /// <summary>
    /// Сервис для применения эффектов комбо и урона во время битвы.
    /// </summary>
    public static class CombatEffectProcessor
    {
        public static void ApplyDamageEffect(ComboEffect effect, PlayerCharacter activeHero, BattleManager battleManager, CameraShake cameraShake)
        {
            int damage = activeHero.CalculateDamage(effect.Damage, effect.ElementType);

            AudioManager.Instance?.PlayAttackSound();

            if (effect.IsAoE)
            {
                cameraShake?.ShakeStrong();
                ComboParticles.SpawnComboEffect(battleManager, new Vector2(640, 360), effect.ElementType, effect.ComboLevel);

                foreach (Enemy enemy in battleManager.Enemies.Where(e => e.IsAlive))
                {
                    DealDamageToEnemy(enemy, damage, effect, activeHero, true, battleManager);
                }
            }
            else
            {
                Enemy target = battleManager.Enemies.FirstOrDefault(e => e.IsAlive);
                if (target != null)
                {
                    cameraShake?.ShakeMedium();
                    ComboParticles.SpawnComboEffect(battleManager, new Vector2(640, 300), effect.ElementType, effect.ComboLevel);
                    DealDamageToEnemy(target, damage, effect, activeHero, false, battleManager);
                }
            }
        }

        private static void DealDamageToEnemy(Enemy target, int damage, ComboEffect effect, PlayerCharacter activeHero, bool isAoE, BattleManager battleManager)
        {
            int reflected = target.TakeDamage(damage, activeHero.AttackType, canReflect: true);
            Vector2 particlePos = isAoE ? new Vector2(400, 200) : new Vector2(640, 250);

            AudioManager.Instance?.PlayHitSound();
            ComboParticles.SpawnDamageNumber(battleManager, particlePos, damage);

            if (effect.StatusEffect != null)
            {
                target.ApplyStatusEffect(effect.StatusEffect);
            }

            if (reflected > 0)
            {
                _ = activeHero.TakeDamage(reflected, target.AttackType, canReflect: false);
            }
        }

        public static void ApplyHealEffect(ComboEffect effect, DualHeroSystem heroSystem, BattleManager battleManager, CameraShake cameraShake)
        {
            int healing = PlayerCharacter.CalculateHealing(effect.Healing);
            cameraShake?.ShakeLight();
            ComboParticles.SpawnComboEffect(battleManager, new Vector2(640, 360), ElementType.Heal, effect.ComboLevel);

            heroSystem.Mage.Heal(healing);
            heroSystem.Warrior.Heal(healing);

            ComboParticles.SpawnHealNumber(battleManager, new Vector2(200, 100), healing);
            ComboParticles.SpawnHealNumber(battleManager, new Vector2(1000, 100), healing);

            if (effect.ComboLevel == 2)
            {
                heroSystem.Mage.ClearNegativeEffects();
                heroSystem.Warrior.ClearNegativeEffects();
            }

            if (effect.StatusEffect != null)
            {
                heroSystem.Mage.ApplyStatusEffect(effect.StatusEffect);
                heroSystem.Warrior.ApplyStatusEffect(effect.StatusEffect);
            }
        }

        public static void ApplyShieldEffect(ComboEffect effect, DualHeroSystem heroSystem, BattleManager battleManager, CameraShake cameraShake)
        {
            int shield = PlayerCharacter.CalculateShield(effect.Shield);
            cameraShake?.ShakeLight();
            ComboParticles.SpawnComboEffect(battleManager, new Vector2(640, 360), ElementType.Shield, effect.ComboLevel);

            heroSystem.Mage.AddShield(shield);
            heroSystem.Warrior.AddShield(shield);

            ComboParticles.SpawnShieldNumber(battleManager, new Vector2(200, 100), shield);
            ComboParticles.SpawnShieldNumber(battleManager, new Vector2(1000, 100), shield);

            if (effect.StatusEffect != null)
            {
                heroSystem.Mage.ApplyStatusEffect(effect.StatusEffect);
                heroSystem.Warrior.ApplyStatusEffect(effect.StatusEffect);
            }
        }
    }
}
