using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// The system managing the player's party. Historically named "DualHeroSystem" (after the two main
    /// heroes), but as of Stage 1 of the redesign it represents a three-slot party: the Mage and Warrior
    /// (Altarion and Aldric, always present) and an optional third mercenary slot that will become
    /// available at the village-restoration stage (see REDESIGN_NOTES.md, sections 4 and 7). Responsible
    /// for initializing party members, tracking their state, and routing signals.
    /// </summary>
    public partial class DualHeroSystem : Node
    {
        /// <summary>
        /// Raised when one of the heroes' health changes. Passes the hero's class and their current and maximum health.
        /// </summary>
        [Signal]
        public delegate void HeroHealthChangedEventHandler(CharacterClass heroClass, int currentHealth, int maxHealth);

        /// <summary>
        /// Raised when one of the heroes' shield strength changes. Passes the hero's class and their current shield value.
        /// </summary>
        [Signal]
        public delegate void HeroShieldChangedEventHandler(CharacterClass heroClass, int shieldAmount);

        /// <summary>
        /// Raised when one of the heroes dies. Passes the class of the fallen hero.
        /// </summary>
        [Signal]
        public delegate void HeroDiedEventHandler(CharacterClass heroClass);

        /// <summary>
        /// Raised when the entire party is defeated. This event typically leads to the game ending.
        /// </summary>
        [Signal]
        public delegate void PartyDefeatedEventHandler();

        /// <summary>
        /// Reference to the Mage character (Altarion). Read-only from outside.
        /// </summary>
        public PlayerCharacter Mage { get; private set; }

        /// <summary>
        /// Reference to the Warrior character (Aldric). Read-only from outside.
        /// </summary>
        public PlayerCharacter Warrior { get; private set; }

        /// <summary>
        /// The party's third slot — a mercenary hired from the settlement. Always empty in Stage 1:
        /// hiring becomes available at the village-restoration stage. Included ahead of time so the
        /// party structure doesn't need to be reworked later.
        /// </summary>
        public PlayerCharacter Companion { get; private set; }

        /// <summary>
        /// Godot lifecycle method, called when the node is added to the scene.
        /// Initializes the Mage and Warrior, adds them as child nodes, and subscribes to their signals.
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
                CheckPartyDefeated();
            };

            hero.ShieldChanged += (shield) =>
                EmitSignal(SignalName.HeroShieldChanged, (int)heroClass, shield);

            hero.CharacterDied += () =>
            {
                _ = EmitSignal(SignalName.HeroDied, (int)heroClass);
                CheckPartyDefeated();
            };
        }

        private void CheckPartyDefeated()
        {
            if (GetAliveMembers().Count == 0)
            {
                _ = EmitSignal(SignalName.PartyDefeated);
                GD.Print("[DualHeroSystem] Entire party has fallen - Game Over!");
            }
        }

        /// <summary>
        /// Returns every member of the party: the two heroes and the mercenary, if one is assigned.
        /// </summary>
        /// <returns>The list of party members in a fixed order (Mage, Warrior, Companion).</returns>
        public IReadOnlyList<PlayerCharacter> GetPartyMembers()
        {
            return Companion is null ? [Mage, Warrior] : [Mage, Warrior, Companion];
        }

        /// <summary>
        /// Returns only the party members who are currently alive.
        /// </summary>
        /// <returns>The list of living party members.</returns>
        public IReadOnlyList<PlayerCharacter> GetAliveMembers()
        {
            return [.. GetPartyMembers().Where(member => member.IsAlive)];
        }

        /// <summary>
        /// Loads both heroes' state from save data.
        /// </summary>
        public void LoadFromSave(int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                                 int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense)
        {
            Mage.InitializeFromSave("Altarion", mageMaxHealth, mageHealth, mageDamage, mageDefense, CharacterClass.Mage);
            Warrior.InitializeFromSave("Aldric", warriorMaxHealth, warriorHealth, warriorDamage, warriorDefense, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Heroes loaded from save");
        }

        /// <summary>
        /// Returns both heroes' combined stats as a single tuple.
        /// </summary>
        /// <returns>A tuple containing the current health, maximum health, damage and defense for both heroes.</returns>
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
        /// Processes every active status effect for every party member.
        /// </summary>
        public void ProcessStatusEffects()
        {
            foreach (PlayerCharacter member in GetPartyMembers())
            {
                member.ProcessStatusEffects();
            }
        }
    }
}
