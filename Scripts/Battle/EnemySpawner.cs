using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Battle
{
    /// <summary>
    /// Сервис, отвечающий за логику генерации врагов для волн.
    /// </summary>
    public static class EnemySpawner
    {
        /// <summary>
        /// Возвращает список врагов для указанной волны.
        /// </summary>
        public static List<Enemy> GenerateWaveEnemies(int currentWave)
        {
            List<Enemy> enemies = [];
            int totalEnemies = ScalingSystem.GetEnemyCount(currentWave);

            if (currentWave <= 5)
            {
                int count = Mathf.Min(totalEnemies, 5);
                enemies.Add(SpawnEnemy(EnemyType.Slime, currentWave, count));
            }
            else if (currentWave <= 10)
            {
                int count = Mathf.Min(totalEnemies, 5);
                enemies.Add(SpawnEnemy(EnemyType.SkeletonWarrior, currentWave, count));
            }
            else
            {
                // Mix
                int slimes = Mathf.Min(totalEnemies / 2, 5);
                int skeletons = Mathf.Min(totalEnemies - slimes, 5);

                if (slimes > 0)
                {
                    enemies.Add(SpawnEnemy(EnemyType.Slime, currentWave, slimes));
                }

                if (skeletons > 0)
                {
                    enemies.Add(SpawnEnemy(EnemyType.SkeletonWarrior, currentWave, skeletons));
                }
            }

            GD.Print($"[EnemySpawner] Wave {currentWave}: {totalEnemies} enemies total, generated {enemies.Count} stacks");
            return enemies;
        }

        /// <summary>
        /// Вспомогательный метод для спавна врага определенного типа.
        /// </summary>
        public static Enemy SpawnEnemy(EnemyType type, int wave, int count = 1)
        {
            return Enemy.Create(type, wave, count);
        }
    }
}
