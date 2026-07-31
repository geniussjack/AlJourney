namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Possible game states, determining the current screen and behavior.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Map,
        Battle,
        Shop,
        GameOver,
        Victory
    }

    /// <summary>
    /// Character classes, defining their role in combat.
    /// </summary>
    public enum CharacterClass
    {
        Mage,
        Warrior
    }

    /// <summary>
    /// The different enemy types the player can encounter while playing.
    /// </summary>
    public enum EnemyType
    {
        SkeletonWarrior,
        SkeletonArcher,
        Zombie,
        Slime,
        DraugrWarrior,
        DraugrDefender,
        DraugrCaster,

        GeneralOfDraugr,
        Arhiskeleton,

        Necromancer
    }

    /// <summary>
    /// Types of damage dealt.
    /// </summary>
    public enum AttackType
    {
        Physical,
        Magical
    }

    /// <summary>
    /// Status effects that can be applied to characters or enemies during combat.
    /// </summary>
    public enum StatusEffect
    {
        None,
        Burning,
        Bleeding,
        Regeneration,
        ShieldReflect,
        Immunity,
        Stunned,
        Weakened,
        Freeze,
        Shock,
        Vulnerable
    }

    /// <summary>
    /// Turn phases of the turn-based combat system. Determine whose turn it currently is and whether
    /// a wave transition is in progress.
    /// </summary>
    public enum BattlePhase
    {
        PlayerTurn,
        EnemyTurn,
        WaveTransition
    }

    /// <summary>
    /// Equipment slots for characters. Determine which slot an item can be equipped into.
    /// </summary>
    public enum EquipmentSlot
    {
        Weapon,
        Head,
        Body,
        Legs,
        Necklace,
        Ring,
        Earring
    }

    /// <summary>
    /// Item rarity tiers, affecting their stats, value and drop chance.
    /// </summary>
    public enum EquipmentRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Types of abilities available to characters.
    /// </summary>
    public enum AbilityType
    {
        Attack,
        Support
    }

    /// <summary>
    /// Elements associated with characters' active abilities.
    /// </summary>
    public enum AbilityElement
    {
        Fire,
        Heal,
        Sword,
        Shield
    }

    /// <summary>
    /// Defines who an ability can be targeted at: an enemy, or the caster/an ally.
    /// </summary>
    public enum AbilityTargetType
    {
        Enemy,
        AllyOrSelf
    }

    /// <summary>
    /// Campaign map locations, arranged in order of distance from the village to the necromancer's lair.
    /// Declaration order matches playthrough order.
    /// </summary>
    public enum LocationId
    {
        VillageRuins,
        DarkForest,
        BuriedCatacombs,
        FrozenWastes,
        NecromancerLair
    }
}
