namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Возможные состояния игры, определяющие текущий экран и логику поведения.
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
    /// Типы элементов на игровом поле. Каждый элемент соответствует определенной механике.
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
    /// Классы игровых персонажей, определяющие их роли в бою.
    /// </summary>
    public enum CharacterClass
    {
        Mage,
        Warrior
    }

    /// <summary>
    /// Различные типы врагов, с которыми игрок может столкнуться во время прохождения.
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
    /// Типы наносимого урона.
    /// </summary>
    public enum AttackType
    {
        Physical,
        Magical
    }

    /// <summary>
    /// Статусные эффекты, которые могут быть наложены на персонажей или врагов во время боя.
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
    /// Фазы хода в боевой системе. Определяют порядок действий: перемещение фишек, расчет комбо, ход врагов и переход к следующей волне.
    /// </summary>
    public enum BattlePhase
    {
        PlayerSwap,
        PlayerCombo,
        EnemyTurn,
        WaveTransition
    }

    /// <summary>
    /// Слоты для экипировки персонажей. Определяют, в какую ячейку можно надеть предмет.
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
    /// Уровни редкости предметов, влияющие на их характеристики, ценность и вероятность выпадения.
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
    /// Типы способностей, доступных персонажам.
    /// </summary>
    public enum AbilityType
    {
        Attack,
        Support
    }

    /// <summary>
    /// Элементы, связанные с активными способностями персонажей.
    /// </summary>
    public enum AbilityElement
    {
        Fire,
        Heal,
        Sword,
        Shield
    }
}
