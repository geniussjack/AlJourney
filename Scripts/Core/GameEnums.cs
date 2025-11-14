namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Represents the current state of the game flow.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        CharacterSelect,
        Battle,
        Shop,
        GameOver,
        Victory
    }

    /// <summary>
    /// Types of match-3 grid elements.
    /// </summary>
    public enum ElementType
    {
        None,
        Fire,      // Fireball - damage
        Heal,      // Healing sphere
        Sword,     // Axe for warrior, sword mechanic
        Shield     // Shield - defense
    }

    /// <summary>
    /// Player character classes.
    /// </summary>
    public enum CharacterClass
    {
        Mage,      // Eltarion - AoE + Support
        Warrior    // Eldric - Single target + Defense
    }

    /// <summary>
    /// Enemy types in the necromancer's undead army.
    /// </summary>
    public enum EnemyType
    {
        // Basic enemies
        SkeletonWarrior,
        SkeletonArcher,
        Zombie,
        DraugrWarrior,
        DraugrDefender,
        DraugrCaster,

        // Minibosses
        GeneralOfDraugr,
        Arhiskeleton,

        // Boss
        Necromancer
    }

    /// <summary>
    /// Attack type for damage calculation.
    /// </summary>
    public enum AttackType
    {
        Physical,
        Magical
    }

    /// <summary>
    /// Status effects that can be applied to characters.
    /// </summary>
    public enum StatusEffect
    {
        None,
        Burning,           // Fire DoT
        Bleeding,          // Physical DoT
        Regeneration,      // Healing over time
        ShieldReflect,     // Reflects damage
        Immunity,          // Immune to effects
        Stunned,           // Cannot act
        Weakened           // Reduced damage/defense
    }

    /// <summary>
    /// Current phase of battle turn.
    /// </summary>
    public enum BattlePhase
    {
        PlayerSwap,        // Player making match-3 moves
        PlayerCombo,       // Combo effects being applied
        EnemyTurn,         // Enemies attacking
        WaveTransition     // Moving to next wave
    }
}