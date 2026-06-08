namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Состояния игры.
    /// </summary>
    /// <summary>
    /// Основной класс GameState.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Battle,
        Shop,
        GameOver,
        Victory
    }

    /// <summary>
    /// Типы элементов на игровом поле.
    /// </summary>
    /// <summary>
    /// Основной класс ElementType.
    /// </summary>
    public enum ElementType
    {
        None,
        Fire,      
        Heal,      
        Sword,     
        Shield     
    }

    /// <summary>
    /// Классы игровых персонажей.
    /// </summary>
    /// <summary>
    /// Основной класс CharacterClass.
    /// </summary>
    public enum CharacterClass
    {
        Mage,      
        Warrior    
    }

    /// <summary>
    /// Типы врагов.
    /// </summary>
    /// <summary>
    /// Основной класс EnemyType.
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
    /// Типы атак.
    /// </summary>
    /// <summary>
    /// Основной класс AttackType.
    /// </summary>
    public enum AttackType
    {
        Physical,
        Magical
    }

    /// <summary>
    /// Статусные эффекты.
    /// </summary>
    /// <summary>
    /// Основной класс StatusEffect.
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
        Weakened           
    }

    /// <summary>
    /// Фазы хода в битве.
    /// </summary>
    /// <summary>
    /// Основной класс BattlePhase.
    /// </summary>
    public enum BattlePhase
    {
        PlayerSwap,        
        PlayerCombo,       
        EnemyTurn,         
        WaveTransition     
    }

    /// <summary>
    /// Слоты для экипировки.
    /// </summary>
    /// <summary>
    /// UI-компонент EquipmentSlot. Отвечает за отображение пользовательского интерфейса.
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
    /// Уровни редкости экипировки.
    /// </summary>
    /// <summary>
    /// UI-компонент EquipmentRarity. Отвечает за отображение пользовательского интерфейса.
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
    /// Типы способностей персонажей.
    /// </summary>
    /// <summary>
    /// Основной класс AbilityType.
    /// </summary>
    public enum AbilityType
    {
        Attack,            
        Support            
    }

    /// <summary>
    /// Элементы, связанные со способностями.
    /// </summary>
    /// <summary>
    /// Основной класс AbilityElement.
    /// </summary>
    public enum AbilityElement
    {
        Fire,              
        Heal,              
        Sword,             
        Shield             
    }
}
