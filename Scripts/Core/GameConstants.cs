namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Хранилище всех игровых констант, включая настройки сетки, базовые характеристики, формулы урона и параметры врагов.
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Размер игрового поля в клетках.
        /// </summary>
        public const int GRID_SIZE = 5;
        /// <summary>
        /// Количество перемещений фишек, доступных игроку за один ход.
        /// </summary>
        public const int PLAYER_SWAPS_PER_TURN = 1;
        /// <summary>
        /// Минимальное количество одинаковых фишек в ряд для успешного совпадения.
        /// </summary>
        public const int MATCH_MIN_LENGTH = 3;


        /// <summary>
        /// Урон от совпадения 3 огненных элементов.
        /// </summary>
        public const int FIRE_3_DAMAGE = 10;
        /// <summary>
        /// Урон от совпадения 4 огненных элементов.
        /// </summary>
        public const int FIRE_4_DAMAGE = 15;
        /// <summary>
        /// Урон от совпадения 5 огненных элементов.
        /// </summary>
        public const int FIRE_5_DAMAGE = 25;
        /// <summary>
        /// Урон от статуса горения при совпадении 4 огненных элементов.
        /// </summary>
        public const int FIRE_4_BURN_DAMAGE = 3;
        /// <summary>
        /// Длительность статуса горения при совпадении 4 огненных элементов.
        /// </summary>
        public const int FIRE_4_BURN_DURATION = 2;
        /// <summary>
        /// Урон от статуса горения при совпадении 5 огненных элементов.
        /// </summary>
        public const int FIRE_5_BURN_DAMAGE = 5;
        /// <summary>
        /// Длительность статуса горения при совпадении 5 огненных элементов.
        /// </summary>
        public const int FIRE_5_BURN_DURATION = 3;

        /// <summary>
        /// Урон от совпадения 3 элементов меча.
        /// </summary>
        public const int SWORD_3_DAMAGE = 10;
        /// <summary>
        /// Урон от совпадения 4 элементов меча.
        /// </summary>
        public const int SWORD_4_DAMAGE = 20;
        /// <summary>
        /// Урон от совпадения 5 элементов меча.
        /// </summary>
        public const int SWORD_5_DAMAGE = 35;
        /// <summary>
        /// Урон от кровотечения при совпадении 4 элементов меча.
        /// </summary>
        public const int SWORD_4_BLEED_DAMAGE = 4;
        /// <summary>
        /// Длительность кровотечения при совпадении 4 элементов меча.
        /// </summary>
        public const int SWORD_4_BLEED_DURATION = 2;

        /// <summary>
        /// Количество восстанавливаемого здоровья при совпадении 3 элементов лечения.
        /// </summary>
        public const int HEAL_3_AMOUNT = 15;
        /// <summary>
        /// Количество восстанавливаемого здоровья при совпадении 4 элементов лечения.
        /// </summary>
        public const int HEAL_4_AMOUNT = 25;
        /// <summary>
        /// Количество восстанавливаемого здоровья при совпадении 5 элементов лечения.
        /// </summary>
        public const int HEAL_5_AMOUNT = 40;
        /// <summary>
        /// Объем регенерации здоровья за ход при совпадении 5 элементов лечения.
        /// </summary>
        public const int HEAL_5_REGEN_AMOUNT = 5;
        /// <summary>
        /// Длительность эффекта регенерации при совпадении 5 элементов лечения.
        /// </summary>
        public const int HEAL_5_REGEN_DURATION = 3;

        /// <summary>
        /// Прочность щита при совпадении 3 элементов щита.
        /// </summary>
        public const int SHIELD_3_AMOUNT = 10;
        /// <summary>
        /// Прочность щита при совпадении 4 элементов щита.
        /// </summary>
        public const int SHIELD_4_AMOUNT = 20;
        /// <summary>
        /// Прочность щита при совпадении 5 элементов щита.
        /// </summary>
        public const int SHIELD_5_AMOUNT = 35;
        /// <summary>
        /// Процент отражаемого урона при совпадении 4 элементов щита.
        /// </summary>
        public const float SHIELD_4_REFLECT_PERCENT = 0.2f;

        /// <summary>
        /// Коэффициент увеличения здоровья врагов с каждой новой волной.
        /// </summary>
        public const float ENEMY_HP_SCALE_PER_WAVE = 0.10f;
        /// <summary>
        /// Коэффициент увеличения урона врагов с каждой новой волной.
        /// </summary>
        public const float ENEMY_DAMAGE_SCALE_PER_WAVE = 0.06f;
        /// <summary>
        /// Максимально допустимое количество врагов в одной волне.
        /// </summary>
        public const int MAX_ENEMIES_PER_WAVE = 5;

        /// <summary>
        /// Базовое количество врагов на начальных волнах.
        /// </summary>
        public const int ENEMY_COUNT_BASE = 1;
        /// <summary>
        /// Интервал, через который количество врагов увеличивается на 1.
        /// </summary>
        public const int ENEMY_COUNT_INCREASE_EVERY = 2;

        /// <summary>
        /// Номер волны, начиная с которой могут появляться враги-скелеты.
        /// </summary>
        public const int SKELETON_UNLOCK_WAVE = 21;

        /// <summary>
        /// Интервал между появлениями мини-боссов.
        /// </summary>
        public const int MINIBOSS_WAVE_INTERVAL = 999;
        /// <summary>
        /// Интервал между появлениями главных боссов.
        /// </summary>
        public const int BOSS_WAVE_INTERVAL = 999;


        /// <summary>
        /// Базовое максимальное здоровье Мага.
        /// </summary>
        public const int MAGE_BASE_HP = 80;
        /// <summary>
        /// Базовый урон Мага без учета экипировки.
        /// </summary>
        public const int MAGE_BASE_DAMAGE = 8;
        /// <summary>
        /// Базовая защита Мага без учета экипировки.
        /// </summary>
        public const int MAGE_BASE_DEFENSE = 2;

        /// <summary>
        /// Базовое максимальное здоровье Воина.
        /// </summary>
        public const int WARRIOR_BASE_HP = 120;
        /// <summary>
        /// Базовый урон Воина без учета экипировки.
        /// </summary>
        public const int WARRIOR_BASE_DAMAGE = 12;
        /// <summary>
        /// Базовая защита Воина без учета экипировки.
        /// </summary>
        public const int WARRIOR_BASE_DEFENSE = 4;


        /// <summary>
        /// Здоровье Скелета-воина.
        /// </summary>
        public const int SKELETON_WARRIOR_HP = 30;
        /// <summary>
        /// Урон Скелета-воина.
        /// </summary>
        public const int SKELETON_WARRIOR_DAMAGE = 25;
        /// <summary>
        /// Защита Скелета-воина.
        /// </summary>
        public const int SKELETON_WARRIOR_DEFENSE = 2;

        /// <summary>
        /// Здоровье Скелета-лучника.
        /// </summary>
        public const int SKELETON_ARCHER_HP = 25;
        /// <summary>
        /// Урон Скелета-лучника.
        /// </summary>
        public const int SKELETON_ARCHER_DAMAGE = 18;
        /// <summary>
        /// Защита Скелета-лучника.
        /// </summary>
        public const int SKELETON_ARCHER_DEFENSE = 1;

        /// <summary>
        /// Здоровье Зомби.
        /// </summary>
        public const int ZOMBIE_HP = 60;
        /// <summary>
        /// Урон Зомби.
        /// </summary>
        public const int ZOMBIE_DAMAGE = 20;
        /// <summary>
        /// Защита Зомби.
        /// </summary>
        public const int ZOMBIE_DEFENSE = 3;

        /// <summary>
        /// Здоровье Слайма.
        /// </summary>
        public const int SLIME_HP = 20;
        /// <summary>
        /// Урон Слайма.
        /// </summary>
        public const int SLIME_DAMAGE = 12;
        /// <summary>
        /// Защита Слайма.
        /// </summary>
        public const int SLIME_DEFENSE = 0;

        /// <summary>
        /// Здоровье Драугра-воина.
        /// </summary>
        public const int DRAUGR_WARRIOR_HP = 45;
        /// <summary>
        /// Урон Драугра-воина.
        /// </summary>
        public const int DRAUGR_WARRIOR_DAMAGE = 22;
        /// <summary>
        /// Защита Драугра-воина.
        /// </summary>
        public const int DRAUGR_WARRIOR_DEFENSE = 4;

        /// <summary>
        /// Здоровье Драугра-защитника.
        /// </summary>
        public const int DRAUGR_DEFENDER_HP = 70;
        /// <summary>
        /// Урон Драугра-защитника.
        /// </summary>
        public const int DRAUGR_DEFENDER_DAMAGE = 15;
        /// <summary>
        /// Защита Драугра-защитника.
        /// </summary>
        public const int DRAUGR_DEFENDER_DEFENSE = 8;

        /// <summary>
        /// Здоровье Драугра-кастера.
        /// </summary>
        public const int DRAUGR_CASTER_HP = 40;
        /// <summary>
        /// Урон Драугра-кастера.
        /// </summary>
        public const int DRAUGR_CASTER_DAMAGE = 25;
        /// <summary>
        /// Защита Драугра-кастера.
        /// </summary>
        public const int DRAUGR_CASTER_DEFENSE = 3;

        /// <summary>
        /// Здоровье Генерала Драугров.
        /// </summary>
        public const int GENERAL_DRAUGR_HP = 150;
        /// <summary>
        /// Урон Генерала Драугров.
        /// </summary>
        public const int GENERAL_DRAUGR_DAMAGE = 35;
        /// <summary>
        /// Защита Генерала Драугров.
        /// </summary>
        public const int GENERAL_DRAUGR_DEFENSE = 10;

        /// <summary>
        /// Здоровье Архискелета.
        /// </summary>
        public const int ARHISKELETON_HP = 120;
        /// <summary>
        /// Урон Архискелета.
        /// </summary>
        public const int ARHISKELETON_DAMAGE = 25;
        /// <summary>
        /// Защита Архискелета.
        /// </summary>
        public const int ARHISKELETON_DEFENSE = 5;
        /// <summary>
        /// Количество выстрелов стрелами за один ход у Архискелета.
        /// </summary>
        public const int ARHISKELETON_ARROWS_PER_TURN = 3;

        /// <summary>
        /// Здоровье Некроманта.
        /// </summary>
        public const int NECROMANCER_HP = 300;
        /// <summary>
        /// Урон Некроманта.
        /// </summary>
        public const int NECROMANCER_DAMAGE = 45;
        /// <summary>
        /// Защита Некроманта.
        /// </summary>
        public const int NECROMANCER_DEFENSE = 12;

        /// <summary>
        /// Минимальный прирост здоровья при покупке улучшения в магазине.
        /// </summary>
        public const int SHOP_UPGRADE_HP_MIN = 25;
        /// <summary>
        /// Максимальный прирост здоровья при покупке улучшения в магазине.
        /// </summary>
        public const int SHOP_UPGRADE_HP_MAX = 60;
        /// <summary>
        /// Минимальный прирост урона при покупке улучшения в магазине.
        /// </summary>
        public const int SHOP_UPGRADE_DAMAGE_MIN = 2;
        /// <summary>
        /// Максимальный прирост урона при покупке улучшения в магазине.
        /// </summary>
        public const int SHOP_UPGRADE_DAMAGE_MAX = 5;
        /// <summary>
        /// Минимальный прирост защиты при покупке улучшения в магазине.
        /// </summary>
        public const int SHOP_UPGRADE_DEFENSE_MIN = 1;
        /// <summary>
        /// Максимальный прирост защиты при покупке улучшения в магазине.
        /// </summary>
        public const int SHOP_UPGRADE_DEFENSE_MAX = 4;
        /// <summary>
        /// Коэффициент масштабирования цен в магазине с увеличением номера волны.
        /// </summary>
        public const float SHOP_WAVE_SCALE_FACTOR = 0.7f;

        /// <summary>
        /// Количество монет, выпадающих за победу над обычным врагом.
        /// </summary>
        public const int COINS_PER_BASIC_ENEMY = 8;
        /// <summary>
        /// Количество монет, выпадающих за победу над мини-боссом.
        /// </summary>
        public const int COINS_PER_MINIBOSS = 40;
        /// <summary>
        /// Количество монет, выпадающих за победу над главным боссом.
        /// </summary>
        public const int COINS_PER_BOSS = 150;

        /// <summary>
        /// Имя файла, используемое для сохранения прогресса игрока.
        /// </summary>
        public const string SAVE_FILE_NAME = "save_data.json";
        /// <summary>
        /// Директория, в которой сохраняется файл прогресса.
        /// </summary>
        public const string SAVE_DIRECTORY = "user://SaveData/";
    }
}
