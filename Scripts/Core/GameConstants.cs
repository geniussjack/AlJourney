namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Central storage for all game balance constants and configuration values.
    /// </summary>
    public static class GameConstants
    {
        // === MATCH-3 GRID ===
        public const int GRID_SIZE = 5;
        public const int PLAYER_SWAPS_PER_TURN = 5;             // Increased from 3 - more dynamic gameplay
        public const int MATCH_MIN_LENGTH = 3;

        // === COMBO DAMAGE/EFFECTS ===

        // Fire (Fireball)
        public const int FIRE_3_DAMAGE = 10;
        public const int FIRE_4_DAMAGE = 15;
        public const int FIRE_5_DAMAGE = 25;
        public const int FIRE_4_BURN_DAMAGE = 3;
        public const int FIRE_4_BURN_DURATION = 2;
        public const int FIRE_5_BURN_DAMAGE = 5;
        public const int FIRE_5_BURN_DURATION = 3;

        // Sword (Axe visual)
        public const int SWORD_3_DAMAGE = 10;
        public const int SWORD_4_DAMAGE = 20;
        public const int SWORD_5_DAMAGE = 35;
        public const int SWORD_4_BLEED_DAMAGE = 4;
        public const int SWORD_4_BLEED_DURATION = 2;

        // Heal
        public const int HEAL_3_AMOUNT = 15;
        public const int HEAL_4_AMOUNT = 25;
        public const int HEAL_5_AMOUNT = 40;
        public const int HEAL_5_REGEN_AMOUNT = 5;
        public const int HEAL_5_REGEN_DURATION = 3;

        // Shield
        public const int SHIELD_3_AMOUNT = 10;
        public const int SHIELD_4_AMOUNT = 20;
        public const int SHIELD_5_AMOUNT = 35;
        public const float SHIELD_4_REFLECT_PERCENT = 0.2f;

        // === DIFFICULTY SCALING ===
        public const float ENEMY_HP_SCALE_PER_WAVE = 0.08f;     // Reduced from 0.15f - more gradual scaling
        public const float ENEMY_DAMAGE_SCALE_PER_WAVE = 0.06f; // Reduced from 0.10f - more gradual scaling
        public const int ENEMY_COUNT_INCREASE_EVERY = 5;
        public const int MAX_ENEMIES_PER_WAVE = 5;
        public const int MINIBOSS_WAVE_INTERVAL = 5;
        public const int BOSS_WAVE_INTERVAL = 10;

        // === CHARACTER BASE STATS ===

        // Mage (Eltarion)
        public const int MAGE_BASE_HP = 80;
        public const int MAGE_BASE_DAMAGE = 8;
        public const int MAGE_BASE_DEFENSE = 5;

        // Warrior (Eldric)
        public const int WARRIOR_BASE_HP = 120;
        public const int WARRIOR_BASE_DAMAGE = 12;
        public const int WARRIOR_BASE_DEFENSE = 8;

        // === ENEMY BASE STATS ===

        // Skeleton Warrior
        public const int SKELETON_WARRIOR_HP = 30;
        public const int SKELETON_WARRIOR_DAMAGE = 12;
        public const int SKELETON_WARRIOR_DEFENSE = 2;

        // Skeleton Archer
        public const int SKELETON_ARCHER_HP = 25;
        public const int SKELETON_ARCHER_DAMAGE = 8;
        public const int SKELETON_ARCHER_DEFENSE = 1;

        // Zombie
        public const int ZOMBIE_HP = 60;
        public const int ZOMBIE_DAMAGE = 6;
        public const int ZOMBIE_DEFENSE = 3;

        // Draugr Warrior
        public const int DRAUGR_WARRIOR_HP = 45;
        public const int DRAUGR_WARRIOR_DAMAGE = 10;
        public const int DRAUGR_WARRIOR_DEFENSE = 4;

        // Draugr Defender
        public const int DRAUGR_DEFENDER_HP = 70;
        public const int DRAUGR_DEFENDER_DAMAGE = 7;
        public const int DRAUGR_DEFENDER_DEFENSE = 8;

        // Draugr Caster
        public const int DRAUGR_CASTER_HP = 40;
        public const int DRAUGR_CASTER_DAMAGE = 11;
        public const int DRAUGR_CASTER_DEFENSE = 3;

        // General of Draugr (Miniboss)
        public const int GENERAL_DRAUGR_HP = 150;
        public const int GENERAL_DRAUGR_DAMAGE = 18;
        public const int GENERAL_DRAUGR_DEFENSE = 10;

        // Arhiskeleton (Miniboss)
        public const int ARHISKELETON_HP = 120;
        public const int ARHISKELETON_DAMAGE = 15;
        public const int ARHISKELETON_DEFENSE = 5;
        public const int ARHISKELETON_ARROWS_PER_TURN = 3;

        // Necromancer (Boss)
        public const int NECROMANCER_HP = 300;
        public const int NECROMANCER_DAMAGE = 20;
        public const int NECROMANCER_DEFENSE = 12;

        // === SHOP & ECONOMY ===
        public const int SHOP_UPGRADE_HP_MIN = 25;
        public const int SHOP_UPGRADE_HP_MAX = 60;
        public const int SHOP_UPGRADE_DAMAGE_MIN = 2;
        public const int SHOP_UPGRADE_DAMAGE_MAX = 5;
        public const int SHOP_UPGRADE_DEFENSE_MIN = 1;
        public const int SHOP_UPGRADE_DEFENSE_MAX = 4;
        public const float SHOP_WAVE_SCALE_FACTOR = 1.8f;       // Reduced from 2.5f - more affordable

        // Coin rewards (increased for better economy)
        public const int COINS_PER_BASIC_ENEMY = 8;             // Increased from 5
        public const int COINS_PER_MINIBOSS = 40;               // Increased from 25
        public const int COINS_PER_BOSS = 150;                  // Increased from 100

        // === SAVE SYSTEM ===
        public const string SAVE_FILE_NAME = "save_data.json";
        public const string SAVE_DIRECTORY = "user://SaveData/";
    }
}