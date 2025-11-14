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

        private PlayerCharacter _mage;
        private PlayerCharacter _warrior;

        /// <summary>
        /// The Mage character (Eltarion).
        /// </summary>
        public PlayerCharacter Mage => _mage;

        /// <summary>
        /// The Warrior character (Eldric).
        /// </summary>
        public PlayerCharacter Warrior => _warrior;

        /// <summary>
        /// Are both heroes still alive.
        /// </summary>
        public bool AreBothAlive => _mage.IsAlive && _warrior.IsAlive;

        /// <summary>
        /// Is at least one hero alive.
        /// </summary>
        public bool IsAnyAlive => _mage.IsAlive || _warrior.IsAlive;

        public override void _Ready()
        {
            // Create both heroes
            _mage = PlayerCharacter.Create(CharacterClass.Mage);
            _warrior = PlayerCharacter.Create(CharacterClass.Warrior);

            AddChild(_mage);
            AddChild(_warrior);

            // Connect signals
            ConnectHeroSignals(_mage, CharacterClass.Mage);
            ConnectHeroSignals(_warrior, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Both heroes initialized");
        }

        /// <summary>
        /// Connects hero signals to relay them with class identification.
        /// </summary>
        private void ConnectHeroSignals(PlayerCharacter hero, CharacterClass heroClass)
        {
            hero.HealthChanged += (current, max) =>
            {
                EmitSignal(SignalName.HeroHealthChanged, (int)heroClass, current, max);
                CheckBothDead();
            };

            hero.ShieldChanged += (shield) =>
                EmitSignal(SignalName.HeroShieldChanged, (int)heroClass, shield);

            hero.CharacterDied += () =>
            {
                EmitSignal(SignalName.HeroDied, (int)heroClass);
                CheckBothDead();
            };
        }

        /// <summary>
        /// Checks if both heroes are dead and emits signal.
        /// </summary>
        private void CheckBothDead()
        {
            if (!_mage.IsAlive && !_warrior.IsAlive)
            {
                EmitSignal(SignalName.BothHeroesDied);
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
                ElementType.Fire => _mage,
                ElementType.Heal => _mage,
                ElementType.Sword => _warrior,
                ElementType.Shield => _warrior,
                _ => null
            };
        }

        /// <summary>
        /// Initializes heroes from save data.
        /// </summary>
        public void LoadFromSave(int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                                 int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense)
        {
            _mage.InitializeFromSave("Eltarion", mageMaxHealth, mageHealth, mageDamage, mageDefense, CharacterClass.Mage);
            _warrior.InitializeFromSave("Eldric", warriorMaxHealth, warriorHealth, warriorDamage, warriorDefense, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Heroes loaded from save");
        }

        /// <summary>
        /// Gets combined stats for saving.
        /// </summary>
        public (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) GetCombinedStats()
        {
            var (maxHealth, currentHealth, damage, defense) = _mage.GetStats();
            var warriorStats = _warrior.GetStats();

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
            _mage.ProcessStatusEffects();
            _warrior.ProcessStatusEffects();
        }
    }
}