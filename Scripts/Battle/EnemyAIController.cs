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
    /// Сервис для управления ИИ врагов и обработки их действий.
    /// </summary>
    public static class EnemyAIController
    {
        public static void PerformEnemyAction(Enemy enemy, BattleManager battleManager, CameraShake cameraShake)
        {
            if (enemy.IsStunned)
            {
                return;
            }

            List<PlayerCharacter> aliveHeroes = [];
            if (battleManager.HeroSystem.Mage.IsAlive)
            {
                aliveHeroes.Add(battleManager.HeroSystem.Mage);
            }

            if (battleManager.HeroSystem.Warrior.IsAlive)
            {
                aliveHeroes.Add(battleManager.HeroSystem.Warrior);
            }

            if (aliveHeroes.Count == 0)
            {
                return;
            }

            PlayerCharacter target = SelectTarget(aliveHeroes, enemy);

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

                AudioManager.Instance?.PlayHitSound();
                Vector2 targetPos = target == battleManager.HeroSystem.Mage ? new Vector2(200, 100) : new Vector2(1000, 100);
                ComboParticles.SpawnDamageNumber(battleManager, targetPos, damage);

                if (reflected > 0)
                {
                    _ = enemy.TakeDamage(reflected, target.AttackType, canReflect: false);
                }
            }
        }

        private static PlayerCharacter SelectTarget(List<PlayerCharacter> aliveHeroes, Enemy enemy)
        {
            PlayerCharacter wounded = aliveHeroes
                .Where(h => h.CurrentHealth < h.MaxHealth * 0.3f)
                .OrderBy(h => h.CurrentHealth)
                .FirstOrDefault();

            return wounded ?? (enemy.IsMiniboss || enemy.IsBoss
                ? aliveHeroes.OrderBy(h => h.BaseDefense).First()
                : aliveHeroes[GD.RandRange(0, aliveHeroes.Count - 1)]);
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
                ExecuteNecromancerDarkBolt(necromancer, target);
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

                // Рассчитываем позицию для нового скелета, чтобы он не накладывался на других.
                int maxEnemies = battleManager.Enemies.Count + 1;
                float totalWidth = (maxEnemies - 1) * 150f;
                float startX = 640f - (totalWidth / 2f) + 150f;
                float xOffset = startX + (battleManager.Enemies.Count * 150f);

                battleManager.Enemies.Add(skeleton);
                battleManager.AddChild(skeleton);
            }
        }

        private static void ExecuteNecromancerDarkBolt(Enemy necromancer, PlayerCharacter target)
        {
            int damage = necromancer.PerformAttack();
            int reflected = target.TakeDamage(damage, AttackType.Magical, canReflect: true);
            if (reflected > 0)
            {
                _ = necromancer.TakeDamage(reflected, target.AttackType, canReflect: false);
            }
        }

        private static void ExecuteNecromancerWeaken(BattleManager battleManager)
        {
            Data.StatusEffectData weakenEffect = new(StatusEffect.Weakened, 1, 0);
            battleManager.HeroSystem.Mage.ApplyStatusEffect(weakenEffect);
            battleManager.HeroSystem.Warrior.ApplyStatusEffect(weakenEffect);
        }
    }
}
