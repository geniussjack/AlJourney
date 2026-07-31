namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Storage for all game constants: base hero stats, enemy stats, wave scaling, shop pricing and save data settings.
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Number of tile swaps available to the player per turn.
        /// </summary>
        public const int PLAYER_SWAPS_PER_TURN = 1;
        /// <summary>
        /// Minimum number of identical tiles in a row required for a successful match.
        /// </summary>
        public const int MATCH_MIN_LENGTH = 3;

        /// <summary>
        /// Coefficient by which enemy health increases with each new wave.
        /// </summary>
        public const float ENEMY_HP_SCALE_PER_WAVE = 0.10f;
        /// <summary>
        /// Coefficient by which enemy damage increases with each new wave.
        /// </summary>
        public const float ENEMY_DAMAGE_SCALE_PER_WAVE = 0.06f;
        /// <summary>
        /// Maximum number of enemies allowed in a single wave (used, for example, as the cap on the
        /// number of creatures the necromancer can summon — see EnemyAIController).
        /// </summary>
        public const int MAX_ENEMIES_PER_WAVE = 5;


        /// <summary>
        /// Base maximum health of the Mage.
        /// </summary>
        public const int MAGE_BASE_HP = 80;
        /// <summary>
        /// Base damage of the Mage, before equipment.
        /// </summary>
        public const int MAGE_BASE_DAMAGE = 8;
        /// <summary>
        /// Base defense of the Mage, before equipment.
        /// </summary>
        public const int MAGE_BASE_DEFENSE = 2;

        /// <summary>
        /// Base maximum health of the Warrior.
        /// </summary>
        public const int WARRIOR_BASE_HP = 120;
        /// <summary>
        /// Base damage of the Warrior, before equipment.
        /// </summary>
        public const int WARRIOR_BASE_DAMAGE = 12;
        /// <summary>
        /// Base defense of the Warrior, before equipment.
        /// </summary>
        public const int WARRIOR_BASE_DEFENSE = 4;


        /// <summary>
        /// Health of the Skeleton Warrior.
        /// </summary>
        public const int SKELETON_WARRIOR_HP = 30;
        /// <summary>
        /// Damage of the Skeleton Warrior.
        /// </summary>
        public const int SKELETON_WARRIOR_DAMAGE = 25;
        /// <summary>
        /// Defense of the Skeleton Warrior.
        /// </summary>
        public const int SKELETON_WARRIOR_DEFENSE = 2;

        /// <summary>
        /// Health of the Skeleton Archer.
        /// </summary>
        public const int SKELETON_ARCHER_HP = 25;
        /// <summary>
        /// Damage of the Skeleton Archer.
        /// </summary>
        public const int SKELETON_ARCHER_DAMAGE = 18;
        /// <summary>
        /// Defense of the Skeleton Archer.
        /// </summary>
        public const int SKELETON_ARCHER_DEFENSE = 1;

        /// <summary>
        /// Health of the Zombie.
        /// </summary>
        public const int ZOMBIE_HP = 60;
        /// <summary>
        /// Damage of the Zombie.
        /// </summary>
        public const int ZOMBIE_DAMAGE = 20;
        /// <summary>
        /// Defense of the Zombie.
        /// </summary>
        public const int ZOMBIE_DEFENSE = 3;

        /// <summary>
        /// Health of the Slime.
        /// </summary>
        public const int SLIME_HP = 20;
        /// <summary>
        /// Damage of the Slime.
        /// </summary>
        public const int SLIME_DAMAGE = 12;
        /// <summary>
        /// Defense of the Slime.
        /// </summary>
        public const int SLIME_DEFENSE = 0;

        /// <summary>
        /// Health of the Draugr Warrior.
        /// </summary>
        public const int DRAUGR_WARRIOR_HP = 45;
        /// <summary>
        /// Damage of the Draugr Warrior.
        /// </summary>
        public const int DRAUGR_WARRIOR_DAMAGE = 22;
        /// <summary>
        /// Defense of the Draugr Warrior.
        /// </summary>
        public const int DRAUGR_WARRIOR_DEFENSE = 4;

        /// <summary>
        /// Health of the Draugr Defender.
        /// </summary>
        public const int DRAUGR_DEFENDER_HP = 70;
        /// <summary>
        /// Damage of the Draugr Defender.
        /// </summary>
        public const int DRAUGR_DEFENDER_DAMAGE = 15;
        /// <summary>
        /// Defense of the Draugr Defender.
        /// </summary>
        public const int DRAUGR_DEFENDER_DEFENSE = 8;

        /// <summary>
        /// Health of the Draugr Caster.
        /// </summary>
        public const int DRAUGR_CASTER_HP = 40;
        /// <summary>
        /// Damage of the Draugr Caster.
        /// </summary>
        public const int DRAUGR_CASTER_DAMAGE = 25;
        /// <summary>
        /// Defense of the Draugr Caster.
        /// </summary>
        public const int DRAUGR_CASTER_DEFENSE = 3;

        /// <summary>
        /// Health of the General of Draugr.
        /// </summary>
        public const int GENERAL_DRAUGR_HP = 150;
        /// <summary>
        /// Damage of the General of Draugr.
        /// </summary>
        public const int GENERAL_DRAUGR_DAMAGE = 35;
        /// <summary>
        /// Defense of the General of Draugr.
        /// </summary>
        public const int GENERAL_DRAUGR_DEFENSE = 10;

        /// <summary>
        /// Health of the Archskeleton.
        /// </summary>
        public const int ARHISKELETON_HP = 120;
        /// <summary>
        /// Damage of the Archskeleton.
        /// </summary>
        public const int ARHISKELETON_DAMAGE = 25;
        /// <summary>
        /// Defense of the Archskeleton.
        /// </summary>
        public const int ARHISKELETON_DEFENSE = 5;
        /// <summary>
        /// Number of arrow shots the Archskeleton fires per turn.
        /// </summary>
        public const int ARHISKELETON_ARROWS_PER_TURN = 3;

        /// <summary>
        /// Health of the Necromancer.
        /// </summary>
        public const int NECROMANCER_HP = 300;
        /// <summary>
        /// Damage of the Necromancer.
        /// </summary>
        public const int NECROMANCER_DAMAGE = 45;
        /// <summary>
        /// Defense of the Necromancer.
        /// </summary>
        public const int NECROMANCER_DEFENSE = 12;

        /// <summary>
        /// Minimum health increase granted by a shop upgrade purchase.
        /// </summary>
        public const int SHOP_UPGRADE_HP_MIN = 25;
        /// <summary>
        /// Maximum health increase granted by a shop upgrade purchase.
        /// </summary>
        public const int SHOP_UPGRADE_HP_MAX = 60;
        /// <summary>
        /// Minimum damage increase granted by a shop upgrade purchase.
        /// </summary>
        public const int SHOP_UPGRADE_DAMAGE_MIN = 2;
        /// <summary>
        /// Maximum damage increase granted by a shop upgrade purchase.
        /// </summary>
        public const int SHOP_UPGRADE_DAMAGE_MAX = 5;
        /// <summary>
        /// Minimum defense increase granted by a shop upgrade purchase.
        /// </summary>
        public const int SHOP_UPGRADE_DEFENSE_MIN = 1;
        /// <summary>
        /// Maximum defense increase granted by a shop upgrade purchase.
        /// </summary>
        public const int SHOP_UPGRADE_DEFENSE_MAX = 4;
        /// <summary>
        /// Coefficient scaling shop prices as the wave number increases.
        /// </summary>
        public const float SHOP_WAVE_SCALE_FACTOR = 0.7f;

        /// <summary>
        /// Coins dropped for defeating a basic enemy.
        /// </summary>
        public const int COINS_PER_BASIC_ENEMY = 8;
        /// <summary>
        /// Coins dropped for defeating a miniboss.
        /// </summary>
        public const int COINS_PER_MINIBOSS = 40;
        /// <summary>
        /// Coins dropped for defeating a main boss.
        /// </summary>
        public const int COINS_PER_BOSS = 150;

        /// <summary>
        /// File name used to store the player's save data.
        /// </summary>
        public const string SAVE_FILE_NAME = "save_data.json";
        /// <summary>
        /// Directory in which the save file is stored.
        /// </summary>
        public const string SAVE_DIRECTORY = "user://SaveData/";
    }
}
