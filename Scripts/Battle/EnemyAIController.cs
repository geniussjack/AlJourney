using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Utils;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Battle
{
    /// <summary>
    /// Service for controlling enemy AI and processing their actions.
    /// Works with an arbitrary composition of the player's living party (2 heroes + an optional
    /// mercenary), rather than a hardcoded Mage/Warrior pair.
    /// </summary>
    public static class EnemyAIController
    {
        public static void PerformEnemyAction(Enemy enemy, BattleManager battleManager, CameraShake cameraShake)
        {
            if (enemy.IsStunned)
            {
                return;
            }

            List<PlayerCharacter> aliveMembers = [.. battleManager.HeroSystem.GetAliveMembers()];
            if (aliveMembers.Count == 0)
            {
                return;
            }

            PlayerCharacter target = SelectTarget(aliveMembers, enemy);

            if (enemy.IsBoss)
            {
                PerformNecromancerAction(enemy, target, battleManager);
            }
            else
            {
                ExecuteStandardEnemyAttack(enemy, target, battleManager, cameraShake);
            }
        }

        private static void ExecuteStandardEnemyAttack(Enemy enemy, PlayerCharacter target, BattleManager battleManager, CameraShake cameraShake)
        {
            int damage = enemy.PerformAttack();
            if (damage > 0)
            {
                AudioManager.Instance?.PlayAttackSound();
                cameraShake?.ShakeLight();
                int reflected = target.TakeDamage(damage, enemy.AttackType, canReflect: true);
                battleManager.AddUltimateCharge(BattleManager.UltimateChargePerAction);

                AudioManager.Instance?.PlayHitSound();
                Vector2 targetPos = CombatEffectProcessor.GetAllyVfxPosition(target, battleManager.HeroSystem);
                ComboParticles.SpawnDamageNumber(battleManager, targetPos, damage);

                if (reflected > 0)
                {
                    _ = enemy.TakeDamage(reflected, target.AttackType, canReflect: false);
                }
            }
        }

        private static PlayerCharacter SelectTarget(List<PlayerCharacter> aliveMembers, Enemy enemy)
        {
            PlayerCharacter wounded = aliveMembers
                .Where(h => h.CurrentHealth < h.MaxHealth * 0.3f)
                .OrderBy(h => h.CurrentHealth)
                .FirstOrDefault();

            return wounded ?? (enemy.IsMiniboss || enemy.IsBoss
                ? aliveMembers.OrderBy(h => h.BaseDefense).First()
                : aliveMembers[GD.RandRange(0, aliveMembers.Count - 1)]);
        }

        private static void PerformNecromancerAction(Enemy necromancer, PlayerCharacter target, BattleManager battleManager)
        {
            if (necromancer.IsStunned)
            {
                return;
            }

            battleManager.IncrementNecromancerTurnCount();
            Enemy.NecromancerAbility ability = necromancer.GetNecromancerAbility(battleManager.NecromancerTurnCount);

            if (ability == Enemy.NecromancerAbility.SummonSkeleton)
            {
                ExecuteNecromancerSummon(battleManager);
            }
            else if (ability == Enemy.NecromancerAbility.DarkBolt)
            {
                ExecuteNecromancerDarkBolt(necromancer, target, battleManager);
            }
            else if (ability == Enemy.NecromancerAbility.WeakeningDarkness)
            {
                ExecuteNecromancerWeaken(battleManager);
            }
        }

        private static void ExecuteNecromancerSummon(BattleManager battleManager)
        {
            if (battleManager.Enemies.Count < Core.GameConstants.MAX_ENEMIES_PER_WAVE)
            {
                Enemy skeleton = EnemySpawner.SpawnEnemy(EnemyType.SkeletonWarrior, battleManager.CurrentWave);
                skeleton.CharacterDied += () => battleManager.OnEnemyDied(skeleton);

                battleManager.Enemies.Add(skeleton);
                battleManager.AddChild(skeleton);
            }
        }

        private static void ExecuteNecromancerDarkBolt(Enemy necromancer, PlayerCharacter target, BattleManager battleManager)
        {
            int damage = necromancer.PerformAttack();
            int reflected = target.TakeDamage(damage, AttackType.Magical, canReflect: true);
            battleManager.AddUltimateCharge(BattleManager.UltimateChargePerAction);

            if (reflected > 0)
            {
                _ = necromancer.TakeDamage(reflected, target.AttackType, canReflect: false);
            }
        }

        private static void ExecuteNecromancerWeaken(BattleManager battleManager)
        {
            Data.StatusEffectData weakenEffect = new(StatusEffect.Weakened, 1, 0);
            foreach (PlayerCharacter member in battleManager.HeroSystem.GetAliveMembers())
            {
                member.ApplyStatusEffect(weakenEffect);
            }
        }
    }
}
