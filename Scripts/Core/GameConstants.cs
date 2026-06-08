namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Игровые константы.
    /// </summary>
    /// <summary>
    /// Основной класс GameConstants.
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Элемент GRID_SIZE.
        /// </summary>
        public const int GRID_SIZE = 5;
        /// <summary>
        /// Воспроизводит ER_SWAPS_PER_TURN.
        /// </summary>
        public const int PLAYER_SWAPS_PER_TURN = 5;             
        /// <summary>
        /// Элемент MATCH_MIN_LENGTH.
        /// </summary>
        public const int MATCH_MIN_LENGTH = 3;


        /// <summary>
        /// Элемент FIRE_3_DAMAGE.
        /// </summary>
        public const int FIRE_3_DAMAGE = 10;
        /// <summary>
        /// Элемент FIRE_4_DAMAGE.
        /// </summary>
        public const int FIRE_4_DAMAGE = 15;
        /// <summary>
        /// Элемент FIRE_5_DAMAGE.
        /// </summary>
        public const int FIRE_5_DAMAGE = 25;
        /// <summary>
        /// Элемент FIRE_4_BURN_DAMAGE.
        /// </summary>
        public const int FIRE_4_BURN_DAMAGE = 3;
        /// <summary>
        /// Элемент FIRE_4_BURN_DURATION.
        /// </summary>
        public const int FIRE_4_BURN_DURATION = 2;
        /// <summary>
        /// Элемент FIRE_5_BURN_DAMAGE.
        /// </summary>
        public const int FIRE_5_BURN_DAMAGE = 5;
        /// <summary>
        /// Элемент FIRE_5_BURN_DURATION.
        /// </summary>
        public const int FIRE_5_BURN_DURATION = 3;

        /// <summary>
        /// Элемент SWORD_3_DAMAGE.
        /// </summary>
        public const int SWORD_3_DAMAGE = 10;
        /// <summary>
        /// Элемент SWORD_4_DAMAGE.
        /// </summary>
        public const int SWORD_4_DAMAGE = 20;
        /// <summary>
        /// Элемент SWORD_5_DAMAGE.
        /// </summary>
        public const int SWORD_5_DAMAGE = 35;
        /// <summary>
        /// Элемент SWORD_4_BLEED_DAMAGE.
        /// </summary>
        public const int SWORD_4_BLEED_DAMAGE = 4;
        /// <summary>
        /// Элемент SWORD_4_BLEED_DURATION.
        /// </summary>
        public const int SWORD_4_BLEED_DURATION = 2;

        /// <summary>
        /// Элемент HEAL_3_AMOUNT.
        /// </summary>
        public const int HEAL_3_AMOUNT = 15;
        /// <summary>
        /// Элемент HEAL_4_AMOUNT.
        /// </summary>
        public const int HEAL_4_AMOUNT = 25;
        /// <summary>
        /// Элемент HEAL_5_AMOUNT.
        /// </summary>
        public const int HEAL_5_AMOUNT = 40;
        /// <summary>
        /// Элемент HEAL_5_REGEN_AMOUNT.
        /// </summary>
        public const int HEAL_5_REGEN_AMOUNT = 5;
        /// <summary>
        /// Элемент HEAL_5_REGEN_DURATION.
        /// </summary>
        public const int HEAL_5_REGEN_DURATION = 3;

        /// <summary>
        /// Элемент SHIELD_3_AMOUNT.
        /// </summary>
        public const int SHIELD_3_AMOUNT = 10;
        /// <summary>
        /// Элемент SHIELD_4_AMOUNT.
        /// </summary>
        public const int SHIELD_4_AMOUNT = 20;
        /// <summary>
        /// Элемент SHIELD_5_AMOUNT.
        /// </summary>
        public const int SHIELD_5_AMOUNT = 35;
        /// <summary>
        /// Элемент SHIELD_4_REFLECT_PERCENT.
        /// </summary>
        public const float SHIELD_4_REFLECT_PERCENT = 0.2f;

        /// <summary>
        /// Элемент ENEMY_HP_SCALE_PER_WAVE.
        /// </summary>
        public const float ENEMY_HP_SCALE_PER_WAVE = 0.10f;     
        /// <summary>
        /// Элемент ENEMY_DAMAGE_SCALE_PER_WAVE.
        /// </summary>
        public const float ENEMY_DAMAGE_SCALE_PER_WAVE = 0.06f; 
        /// <summary>
        /// Элемент MAX_ENEMIES_PER_WAVE.
        /// </summary>
        public const int MAX_ENEMIES_PER_WAVE = 5;

        /// <summary>
        /// Элемент ENEMY_COUNT_BASE.
        /// </summary>
        public const int ENEMY_COUNT_BASE = 1;
        /// <summary>
        /// Элемент ENEMY_COUNT_INCREASE_EVERY.
        /// </summary>
        public const int ENEMY_COUNT_INCREASE_EVERY = 2; 

        /// <summary>
        /// Элемент SKELETON_UNLOCK_WAVE.
        /// </summary>
        public const int SKELETON_UNLOCK_WAVE = 21;

        /// <summary>
        /// Элемент MINIBOSS_WAVE_INTERVAL.
        /// </summary>
        public const int MINIBOSS_WAVE_INTERVAL = 999;
        /// <summary>
        /// Элемент BOSS_WAVE_INTERVAL.
        /// </summary>
        public const int BOSS_WAVE_INTERVAL = 999;


        /// <summary>
        /// Элемент MAGE_BASE_HP.
        /// </summary>
        public const int MAGE_BASE_HP = 80;
        /// <summary>
        /// Элемент MAGE_BASE_DAMAGE.
        /// </summary>
        public const int MAGE_BASE_DAMAGE = 8;
        /// <summary>
        /// Элемент MAGE_BASE_DEFENSE.
        /// </summary>
        public const int MAGE_BASE_DEFENSE = 5;

        /// <summary>
        /// Элемент WARRIOR_BASE_HP.
        /// </summary>
        public const int WARRIOR_BASE_HP = 120;
        /// <summary>
        /// Элемент WARRIOR_BASE_DAMAGE.
        /// </summary>
        public const int WARRIOR_BASE_DAMAGE = 12;
        /// <summary>
        /// Элемент WARRIOR_BASE_DEFENSE.
        /// </summary>
        public const int WARRIOR_BASE_DEFENSE = 8;


        /// <summary>
        /// Элемент SKELETON_WARRIOR_HP.
        /// </summary>
        public const int SKELETON_WARRIOR_HP = 30;
        /// <summary>
        /// Элемент SKELETON_WARRIOR_DAMAGE.
        /// </summary>
        public const int SKELETON_WARRIOR_DAMAGE = 12;
        /// <summary>
        /// Элемент SKELETON_WARRIOR_DEFENSE.
        /// </summary>
        public const int SKELETON_WARRIOR_DEFENSE = 2;

        /// <summary>
        /// Элемент SKELETON_ARCHER_HP.
        /// </summary>
        public const int SKELETON_ARCHER_HP = 25;
        /// <summary>
        /// Элемент SKELETON_ARCHER_DAMAGE.
        /// </summary>
        public const int SKELETON_ARCHER_DAMAGE = 8;
        /// <summary>
        /// Элемент SKELETON_ARCHER_DEFENSE.
        /// </summary>
        public const int SKELETON_ARCHER_DEFENSE = 1;

        /// <summary>
        /// Элемент ZOMBIE_HP.
        /// </summary>
        public const int ZOMBIE_HP = 60;
        /// <summary>
        /// Элемент ZOMBIE_DAMAGE.
        /// </summary>
        public const int ZOMBIE_DAMAGE = 6;
        /// <summary>
        /// Элемент ZOMBIE_DEFENSE.
        /// </summary>
        public const int ZOMBIE_DEFENSE = 3;

        /// <summary>
        /// Элемент SLIME_HP.
        /// </summary>
        public const int SLIME_HP = 20;
        /// <summary>
        /// Элемент SLIME_DAMAGE.
        /// </summary>
        public const int SLIME_DAMAGE = 4;
        /// <summary>
        /// Элемент SLIME_DEFENSE.
        /// </summary>
        public const int SLIME_DEFENSE = 0;

        /// <summary>
        /// Элемент DRAUGR_WARRIOR_HP.
        /// </summary>
        public const int DRAUGR_WARRIOR_HP = 45;
        /// <summary>
        /// Элемент DRAUGR_WARRIOR_DAMAGE.
        /// </summary>
        public const int DRAUGR_WARRIOR_DAMAGE = 10;
        /// <summary>
        /// Элемент DRAUGR_WARRIOR_DEFENSE.
        /// </summary>
        public const int DRAUGR_WARRIOR_DEFENSE = 4;

        /// <summary>
        /// Элемент DRAUGR_DEFENDER_HP.
        /// </summary>
        public const int DRAUGR_DEFENDER_HP = 70;
        /// <summary>
        /// Элемент DRAUGR_DEFENDER_DAMAGE.
        /// </summary>
        public const int DRAUGR_DEFENDER_DAMAGE = 7;
        /// <summary>
        /// Элемент DRAUGR_DEFENDER_DEFENSE.
        /// </summary>
        public const int DRAUGR_DEFENDER_DEFENSE = 8;

        /// <summary>
        /// Элемент DRAUGR_CASTER_HP.
        /// </summary>
        public const int DRAUGR_CASTER_HP = 40;
        /// <summary>
        /// Элемент DRAUGR_CASTER_DAMAGE.
        /// </summary>
        public const int DRAUGR_CASTER_DAMAGE = 11;
        /// <summary>
        /// Элемент DRAUGR_CASTER_DEFENSE.
        /// </summary>
        public const int DRAUGR_CASTER_DEFENSE = 3;

        /// <summary>
        /// Элемент GENERAL_DRAUGR_HP.
        /// </summary>
        public const int GENERAL_DRAUGR_HP = 150;
        /// <summary>
        /// Элемент GENERAL_DRAUGR_DAMAGE.
        /// </summary>
        public const int GENERAL_DRAUGR_DAMAGE = 18;
        /// <summary>
        /// Элемент GENERAL_DRAUGR_DEFENSE.
        /// </summary>
        public const int GENERAL_DRAUGR_DEFENSE = 10;

        /// <summary>
        /// Элемент ARHISKELETON_HP.
        /// </summary>
        public const int ARHISKELETON_HP = 120;
        /// <summary>
        /// Элемент ARHISKELETON_DAMAGE.
        /// </summary>
        public const int ARHISKELETON_DAMAGE = 15;
        /// <summary>
        /// Элемент ARHISKELETON_DEFENSE.
        /// </summary>
        public const int ARHISKELETON_DEFENSE = 5;
        /// <summary>
        /// Элемент ARHISKELETON_ARROWS_PER_TURN.
        /// </summary>
        public const int ARHISKELETON_ARROWS_PER_TURN = 3;

        /// <summary>
        /// Элемент NECROMANCER_HP.
        /// </summary>
        public const int NECROMANCER_HP = 300;
        /// <summary>
        /// Элемент NECROMANCER_DAMAGE.
        /// </summary>
        public const int NECROMANCER_DAMAGE = 20;
        /// <summary>
        /// Элемент NECROMANCER_DEFENSE.
        /// </summary>
        public const int NECROMANCER_DEFENSE = 12;

        /// <summary>
        /// Элемент SHOP_UPGRADE_HP_MIN.
        /// </summary>
        public const int SHOP_UPGRADE_HP_MIN = 25;
        /// <summary>
        /// Элемент SHOP_UPGRADE_HP_MAX.
        /// </summary>
        public const int SHOP_UPGRADE_HP_MAX = 60;
        /// <summary>
        /// Элемент SHOP_UPGRADE_DAMAGE_MIN.
        /// </summary>
        public const int SHOP_UPGRADE_DAMAGE_MIN = 2;
        /// <summary>
        /// Элемент SHOP_UPGRADE_DAMAGE_MAX.
        /// </summary>
        public const int SHOP_UPGRADE_DAMAGE_MAX = 5;
        /// <summary>
        /// Элемент SHOP_UPGRADE_DEFENSE_MIN.
        /// </summary>
        public const int SHOP_UPGRADE_DEFENSE_MIN = 1;
        /// <summary>
        /// Элемент SHOP_UPGRADE_DEFENSE_MAX.
        /// </summary>
        public const int SHOP_UPGRADE_DEFENSE_MAX = 4;
        /// <summary>
        /// Элемент SHOP_WAVE_SCALE_FACTOR.
        /// </summary>
        public const float SHOP_WAVE_SCALE_FACTOR = 1.8f;       

        /// <summary>
        /// Элемент COINS_PER_BASIC_ENEMY.
        /// </summary>
        public const int COINS_PER_BASIC_ENEMY = 8;             
        /// <summary>
        /// Элемент COINS_PER_MINIBOSS.
        /// </summary>
        public const int COINS_PER_MINIBOSS = 40;               
        /// <summary>
        /// Элемент COINS_PER_BOSS.
        /// </summary>
        public const int COINS_PER_BOSS = 150;                  

        /// <summary>
        /// Сохраняет _FILE_NAME.
        /// </summary>
        public const string SAVE_FILE_NAME = "save_data.json";
        /// <summary>
        /// Сохраняет _DIRECTORY.
        /// </summary>
        public const string SAVE_DIRECTORY = "user://SaveData/";
    }
}
