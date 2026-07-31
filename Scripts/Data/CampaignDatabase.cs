using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Статическая база данных карты кампании: локации, основная линейная цепочка уровней от руин
    /// деревни до логова некроманта, и ответвления с мини-боссами.
    /// Это рабочий, легко донастраиваемый набор уровней — конкретный состав волн будет
    /// уточняться отдельно по мере балансировки (см. REDESIGN_NOTES.md, Этап 3).
    /// </summary>
    public static class CampaignDatabase
    {
        /// <summary>
        /// Первый уровень кампании, доступный без каких-либо условий разблокировки.
        /// </summary>
        public const string FirstLevelId = "village_ruins_1";

        /// <summary>
        /// Все уровни кампании по порядку добавления (основная линия и ответвления вперемешку),
        /// в том порядке, в котором они объявлены ниже в <see cref="BuildLevels"/>.
        /// </summary>
        public static readonly IReadOnlyList<LevelDefinition> Levels = BuildLevels();

        private static List<LevelDefinition> BuildLevels()
        {
            List<LevelDefinition> levels = [];

            // --- Локация 1: Руины деревни ---
            // Стартовая локация. Простейшие противники: слаймы и зомби.
            AddMainLevel(levels, LocationId.VillageRuins, 1, difficulty: 1, requiredLevelId: null,
                Wave(Spawn(EnemyType.Slime, 2)));
            AddMainLevel(levels, LocationId.VillageRuins, 2, difficulty: 2,
                Wave(Spawn(EnemyType.Slime, 3)));
            AddMainLevel(levels, LocationId.VillageRuins, 3, difficulty: 3,
                Wave(Spawn(EnemyType.Zombie, 1), Spawn(EnemyType.Slime, 2)));
            AddMainLevel(levels, LocationId.VillageRuins, 4, difficulty: 4,
                Wave(Spawn(EnemyType.Zombie, 2)),
                Wave(Spawn(EnemyType.Zombie, 1), Spawn(EnemyType.Slime, 2)));

            // --- Локация 2: Тёмный лес ---
            // Вводятся скелеты (воин и лучник), зомби остаются проходным противником.
            AddMainLevel(levels, LocationId.DarkForest, 1, difficulty: 5, requiredLevelId: LevelId(LocationId.VillageRuins, 4),
                Wave(Spawn(EnemyType.SkeletonWarrior, 2)));
            AddMainLevel(levels, LocationId.DarkForest, 2, difficulty: 6,
                Wave(Spawn(EnemyType.SkeletonWarrior, 1), Spawn(EnemyType.SkeletonArcher, 2)));
            AddMainLevel(levels, LocationId.DarkForest, 3, difficulty: 7,
                Wave(Spawn(EnemyType.SkeletonWarrior, 2), Spawn(EnemyType.Zombie, 1)),
                Wave(Spawn(EnemyType.SkeletonArcher, 2)));
            AddMainLevel(levels, LocationId.DarkForest, 4, difficulty: 8,
                Wave(Spawn(EnemyType.SkeletonWarrior, 2), Spawn(EnemyType.SkeletonArcher, 2)),
                Wave(Spawn(EnemyType.Zombie, 2)));
            // Ответвление: первый мини-босс, Генерал Драугров.
            AddBranchLevel(levels, LocationId.DarkForest, "dark_forest_branch_1", difficulty: 7,
                requiredLevelId: LevelId(LocationId.DarkForest, 1),
                Wave(Spawn(EnemyType.SkeletonWarrior, 2)),
                Wave(Spawn(EnemyType.GeneralOfDraugr)));

            // --- Локация 3: Погребённые катакомбы ---
            // Вводится триада Драугров, скелеты остаются проходными.
            AddMainLevel(levels, LocationId.BuriedCatacombs, 1, difficulty: 9, requiredLevelId: LevelId(LocationId.DarkForest, 4),
                Wave(Spawn(EnemyType.DraugrWarrior, 2)));
            AddMainLevel(levels, LocationId.BuriedCatacombs, 2, difficulty: 10,
                Wave(Spawn(EnemyType.DraugrDefender, 1), Spawn(EnemyType.SkeletonArcher, 2)));
            AddMainLevel(levels, LocationId.BuriedCatacombs, 3, difficulty: 11,
                Wave(Spawn(EnemyType.DraugrCaster, 2), Spawn(EnemyType.SkeletonWarrior, 1)),
                Wave(Spawn(EnemyType.DraugrWarrior, 2)));
            AddMainLevel(levels, LocationId.BuriedCatacombs, 4, difficulty: 12,
                Wave(Spawn(EnemyType.DraugrWarrior, 1), Spawn(EnemyType.DraugrDefender, 1), Spawn(EnemyType.DraugrCaster, 1)),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrCaster, 1)));
            // Ответвление: второй мини-босс, Архискелет.
            AddBranchLevel(levels, LocationId.BuriedCatacombs, "buried_catacombs_branch_1", difficulty: 11,
                requiredLevelId: LevelId(LocationId.BuriedCatacombs, 1),
                Wave(Spawn(EnemyType.SkeletonWarrior, 2), Spawn(EnemyType.SkeletonArcher, 1)),
                Wave(Spawn(EnemyType.Arhiskeleton)));

            // --- Локация 4: Ледяные пустоши ---
            // Самые тяжёлые "рядовые" смешанные волны перед логовом некроманта.
            AddMainLevel(levels, LocationId.FrozenWastes, 1, difficulty: 13, requiredLevelId: LevelId(LocationId.BuriedCatacombs, 4),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.SkeletonArcher, 1)));
            AddMainLevel(levels, LocationId.FrozenWastes, 2, difficulty: 14,
                Wave(Spawn(EnemyType.DraugrDefender, 2), Spawn(EnemyType.DraugrCaster, 1)));
            AddMainLevel(levels, LocationId.FrozenWastes, 3, difficulty: 15,
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrCaster, 2)),
                Wave(Spawn(EnemyType.SkeletonWarrior, 2), Spawn(EnemyType.SkeletonArcher, 2)));
            AddMainLevel(levels, LocationId.FrozenWastes, 4, difficulty: 16,
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrDefender, 1)),
                Wave(Spawn(EnemyType.DraugrCaster, 2), Spawn(EnemyType.SkeletonArcher, 1)));
            // Ответвление: третий заход на мини-босса (Генерал Драугров) с усиленной охраной.
            AddBranchLevel(levels, LocationId.FrozenWastes, "frozen_wastes_branch_1", difficulty: 15,
                requiredLevelId: LevelId(LocationId.FrozenWastes, 1),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrDefender, 1)),
                Wave(Spawn(EnemyType.GeneralOfDraugr)));

            // --- Локация 5: Логово Некроманта ---
            // Финальные тяжёлые смешанные волны и бой с главным боссом.
            AddMainLevel(levels, LocationId.NecromancerLair, 1, difficulty: 17, requiredLevelId: LevelId(LocationId.FrozenWastes, 4),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrCaster, 2)));
            AddMainLevel(levels, LocationId.NecromancerLair, 2, difficulty: 18,
                Wave(Spawn(EnemyType.Arhiskeleton)),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrDefender, 1)));
            AddMainLevel(levels, LocationId.NecromancerLair, 3, difficulty: 20,
                Wave(Spawn(EnemyType.Necromancer)));

            return levels;
        }

        /// <summary>
        /// Возвращает ключ локализации отображаемого названия локации (см. Data/Languages/translations.csv).
        /// </summary>
        public static string GetLocationNameKey(LocationId location)
        {
            return location switch
            {
                LocationId.VillageRuins => "LOCATION_VILLAGE_RUINS",
                LocationId.DarkForest => "LOCATION_DARK_FOREST",
                LocationId.BuriedCatacombs => "LOCATION_BURIED_CATACOMBS",
                LocationId.FrozenWastes => "LOCATION_FROZEN_WASTES",
                LocationId.NecromancerLair => "LOCATION_NECROMANCER_LAIR",
                _ => "LOCATION_VILLAGE_RUINS"
            };
        }

        /// <summary>
        /// Возвращает уровень кампании по его идентификатору, либо <c>null</c>, если такого уровня нет.
        /// </summary>
        public static LevelDefinition GetLevel(string levelId)
        {
            foreach (LevelDefinition level in Levels)
            {
                if (level.Id == levelId)
                {
                    return level;
                }
            }

            return null;
        }

        /// <summary>
        /// Возвращает уровень, идущий следующим за указанным на основной линии в пределах его локации,
        /// либо первый уровень следующей локации, если это был последний уровень текущей.
        /// Ответвления в основную последовательность не входят. Возвращает <c>null</c> после
        /// финального уровня кампании.
        /// </summary>
        public static LevelDefinition GetNextMainLevel(string completedLevelId)
        {
            LevelDefinition completed = GetLevel(completedLevelId);
            if (completed is null || completed.IsBranch)
            {
                return null;
            }

            LevelDefinition best = null;
            foreach (LevelDefinition level in Levels)
            {
                if (level.IsBranch || level.Location != completed.Location || level.OrderInLocation <= completed.OrderInLocation)
                {
                    continue;
                }

                if (best is null || level.OrderInLocation < best.OrderInLocation)
                {
                    best = level;
                }
            }

            if (best is not null)
            {
                return best;
            }

            int nextLocation = (int)completed.Location + 1;
            if (nextLocation > (int)LocationId.NecromancerLair)
            {
                return null;
            }

            return GetFirstLevelOfLocation((LocationId)nextLocation);
        }

        private static LevelDefinition GetFirstLevelOfLocation(LocationId location)
        {
            LevelDefinition first = null;
            foreach (LevelDefinition level in Levels)
            {
                if (level.IsBranch || level.Location != location)
                {
                    continue;
                }

                if (first is null || level.OrderInLocation < first.OrderInLocation)
                {
                    first = level;
                }
            }

            return first;
        }

        /// <summary>
        /// Формирует стандартный идентификатор уровня основной линии по локации и порядковому номеру.
        /// </summary>
        private static string LevelId(LocationId location, int orderInLocation)
        {
            return $"{ToSnakeCase(location)}_{orderInLocation}";
        }

        private static void AddMainLevel(List<LevelDefinition> levels, LocationId location, int orderInLocation, int difficulty, params WaveDefinition[] waves)
        {
            AddMainLevel(levels, location, orderInLocation, difficulty, null, waves);
        }

        private static void AddMainLevel(List<LevelDefinition> levels, LocationId location, int orderInLocation, int difficulty, string requiredLevelId, params WaveDefinition[] waves)
        {
            requiredLevelId ??= orderInLocation > 1 ? LevelId(location, orderInLocation - 1) : null;
            levels.Add(new LevelDefinition(LevelId(location, orderInLocation), location, orderInLocation, waves, difficulty, IsBranch: false, requiredLevelId));
        }

        private static void AddBranchLevel(List<LevelDefinition> levels, LocationId location, string id, int difficulty, string requiredLevelId, params WaveDefinition[] waves)
        {
            // Ответвления используют отрицательный порядок, чтобы не участвовать в подсчёте следующего
            // уровня основной линии (см. GetNextMainLevel/GetFirstLevelOfLocation), но оставаться
            // привязанными к своей локации для отображения на карте.
            levels.Add(new LevelDefinition(id, location, -1, waves, difficulty, IsBranch: true, requiredLevelId));
        }

        private static WaveDefinition Wave(params EnemySpawnDefinition[] enemies)
        {
            return new WaveDefinition(enemies);
        }

        private static EnemySpawnDefinition Spawn(EnemyType type, int count = 1)
        {
            return new EnemySpawnDefinition(type, count);
        }

        private static string ToSnakeCase(LocationId location)
        {
            return location switch
            {
                LocationId.VillageRuins => "village_ruins",
                LocationId.DarkForest => "dark_forest",
                LocationId.BuriedCatacombs => "buried_catacombs",
                LocationId.FrozenWastes => "frozen_wastes",
                LocationId.NecromancerLair => "necromancer_lair",
                _ => location.ToString().ToLowerInvariant()
            };
        }
    }
}
