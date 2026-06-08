using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Менеджер DualHeroSystem. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class DualHeroSystem : Node
    {
        [Signal]
        /// <summary>
        /// Элемент HeroHealthChangedEventHandler.
        /// </summary>
        public delegate void HeroHealthChangedEventHandler(CharacterClass heroClass, int currentHealth, int maxHealth);

        [Signal]
        /// <summary>
        /// Элемент HeroShieldChangedEventHandler.
        /// </summary>
        public delegate void HeroShieldChangedEventHandler(CharacterClass heroClass, int shieldAmount);

        [Signal]
        /// <summary>
        /// Элемент HeroDiedEventHandler.
        /// </summary>
        public delegate void HeroDiedEventHandler(CharacterClass heroClass);

        [Signal]
        /// <summary>
        /// Элемент BothHeroesDiedEventHandler.
        /// </summary>
        public delegate void BothHeroesDiedEventHandler();

        public PlayerCharacter Mage { get; private set; }

        public PlayerCharacter Warrior { get; private set; }

        /// <summary>
        /// Элемент AreBothAlive.
        /// </summary>
        public bool AreBothAlive => Mage.IsAlive && Warrior.IsAlive;

        /// <summary>
        /// Проверяет, является ли AnyAlive.
        /// </summary>
        public bool IsAnyAlive => Mage.IsAlive || Warrior.IsAlive;

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            Mage = PlayerCharacter.Create(CharacterClass.Mage);
            Warrior = PlayerCharacter.Create(CharacterClass.Warrior);

            AddChild(Mage);
            AddChild(Warrior);

            ConnectHeroSignals(Mage, CharacterClass.Mage);
            ConnectHeroSignals(Warrior, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Both heroes initialized");
        }

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

        private void CheckBothDead()
        {
            if (!Mage.IsAlive && !Warrior.IsAlive)
            {
                _ = EmitSignal(SignalName.BothHeroesDied);
                GD.Print("[DualHeroSystem] Both heroes have died - Game Over!");
            }
        }

        /// <summary>
        /// Возвращает HeroForElement.
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
        /// Загружает FromSave.
        /// </summary>
        public void LoadFromSave(int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                                 int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense)
        {
            Mage.InitializeFromSave("Altarion", mageMaxHealth, mageHealth, mageDamage, mageDefense, CharacterClass.Mage);
            Warrior.InitializeFromSave("Aldric", warriorMaxHealth, warriorHealth, warriorDamage, warriorDefense, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Heroes loaded from save");
        }

        public (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) GetCombinedStats()
        {
            (int maxHealth, int currentHealth, int damage, int defense) = Mage.GetStats();
            
            (int maxHealth, int currentHealth, int damage, int defense) warriorStats = Warrior.GetStats();

            return (
                currentHealth, maxHealth, damage, defense,
                warriorStats.currentHealth, warriorStats.maxHealth, warriorStats.damage, warriorStats.defense
            );
        }

        /// <summary>
        /// Обрабатывает StatusEffects.
        /// </summary>
        public void ProcessStatusEffects()
        {
            Mage.ProcessStatusEffects();
            Warrior.ProcessStatusEffects();
        }
    }
}
