using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Manages both player characters (Mage and Warrior) simultaneously.
    /// </summary>
    public partial class DualHeroSystem : Node
    {
        [Signal]
        public delegate void HeroHealthChangedEventHandler(CharacterClass heroClass, int currentHealth, int maxHealth);

        [Signal]
        public delegate void HeroShieldChangedEventHandler(CharacterClass heroClass, int shieldAmount);

        [Signal]
        public delegate void HeroDiedEventHandler(CharacterClass heroClass);

        [Signal]
        public delegate void BothHeroesDiedEventHandler();

        /// <summary>
        /// The Mage character (Eltarion).
        /// </summary>
        public PlayerCharacter Mage { get; private set; }

        /// <summary>
        /// The Warrior character (Eldric).
        /// </summary>
        public PlayerCharacter Warrior { get; private set; }

        /// <summary>
        /// Are both heroes still alive.
        /// </summary>
        public bool AreBothAlive => Mage.IsAlive && Warrior.IsAlive;

        /// <summary>
        /// Is at least one hero alive.
        /// </summary>
        public bool IsAnyAlive => Mage.IsAlive || Warrior.IsAlive;

        public override void _Ready()
        {
            // Create both heroes
            Mage = PlayerCharacter.Create(CharacterClass.Mage);
            Warrior = PlayerCharacter.Create(CharacterClass.Warrior);

            AddChild(Mage);
            AddChild(Warrior);

            // Connect signals
            ConnectHeroSignals(Mage, CharacterClass.Mage);
            ConnectHeroSignals(Warrior, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Both heroes initialized");
        }

        /// <summary>
        /// Connects hero signals to relay them with class identification.
        /// </summary>
        private void ConnectHeroSignals(PlayerCharacter hero, CharacterClass heroClass)
        {
            hero.HealthChanged += (current, max) =>
            {
                _ = EmitSignal(SignalName.HeroHealthChanged, (int)heroClass, current, max);
                CheckBothDead();
            };

            hero.ShieldChanged += (shield) =>
                EmitSignal(SignalName.HeroShieldChanged, (int)heroClass, shield);

            hero.CharacterDied += () =>
            {
                _ = EmitSignal(SignalName.HeroDied, (int)heroClass);
                CheckBothDead();
            };
        }

        /// <summary>
        /// Checks if both heroes are dead and emits signal.
        /// </summary>
        private void CheckBothDead()
        {
            if (!Mage.IsAlive && !Warrior.IsAlive)
            {
                _ = EmitSignal(SignalName.BothHeroesDied);
                GD.Print("[DualHeroSystem] Both heroes have died - Game Over!");
            }
        }

        /// <summary>
        /// Gets hero by element type.
        /// Fire/Heal → Mage, Sword/Shield → Warrior
        /// </summary>
        public PlayerCharacter GetHeroForElement(ElementType elementType)
        {
            return elementType switch
            {
                ElementType.Fire => Mage,
                ElementType.Heal => Mage,
                ElementType.Sword => Warrior,
                ElementType.Shield => Warrior,
                _ => null
            };
        }

        /// <summary>
        /// Initializes heroes from save data.
        /// </summary>
        public void LoadFromSave(int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                                 int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense)
        {
            Mage.InitializeFromSave("Eltarion", mageMaxHealth, mageHealth, mageDamage, mageDefense, CharacterClass.Mage);
            Warrior.InitializeFromSave("Eldric", warriorMaxHealth, warriorHealth, warriorDamage, warriorDefense, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Heroes loaded from save");
        }

        /// <summary>
        /// Gets combined stats for both heroes for saving.
        /// IMPORTANT: Return order is (mageHealth, mageMaxHealth, mageDamage, mageDefense,
        ///                              warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense)
        /// Note: GetStats() returns (maxHealth, currentHealth, damage, defense),
        ///       but this method reorders to (currentHealth, maxHealth, damage, defense) for consistency.
        /// </summary>
        /// <returns>
        /// Named tuple with all hero stats:
        /// - mageHealth: Mage's current HP
        /// - mageMaxHealth: Mage's maximum HP
        /// - mageDamage: Mage's damage stat
        /// - mageDefense: Mage's defense stat
        /// - warriorHealth: Warrior's current HP
        /// - warriorMaxHealth: Warrior's maximum HP
        /// - warriorDamage: Warrior's damage stat
        /// - warriorDefense: Warrior's defense stat
        /// </returns>
        public (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) GetCombinedStats()
        {
            // Get Mage stats (returns: maxHealth, currentHealth, damage, defense)
            (int maxHealth, int currentHealth, int damage, int defense) = Mage.GetStats();
            
            // Get Warrior stats (returns: maxHealth, currentHealth, damage, defense)
            (int maxHealth, int currentHealth, int damage, int defense) warriorStats = Warrior.GetStats();

            // Return in documented order: (currentHealth, maxHealth, damage, defense) for each hero
            return (
                currentHealth, maxHealth, damage, defense,
                warriorStats.currentHealth, warriorStats.maxHealth, warriorStats.damage, warriorStats.defense
            );
        }

        /// <summary>
        /// Processes status effects for both heroes.
        /// </summary>
        public void ProcessStatusEffects()
        {
            Mage.ProcessStatusEffects();
            Warrior.ProcessStatusEffects();
        }
    }
}
