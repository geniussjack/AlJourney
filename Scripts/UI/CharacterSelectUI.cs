using AlJourney.Scripts.Core;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Character selection screen UI controller.
    /// Allows player to choose between Mage and Warrior.
    /// </summary>
    public partial class CharacterSelectUI : Control
    {
        private Button _mageButton;
        private Button _warriorButton;
        private Button _backButton;

        private Label _mageStatsLabel;
        private Label _warriorStatsLabel;

        public override void _Ready()
        {
            // Get buttons
            _mageButton = GetNode<Button>("CenterContainer/VBoxContainer/CharactersContainer/MagePanel/MageButton");
            _warriorButton = GetNode<Button>("CenterContainer/VBoxContainer/CharactersContainer/WarriorPanel/WarriorButton");
            _backButton = GetNode<Button>("CenterContainer/VBoxContainer/BackButton");

            // Get stat labels
            _mageStatsLabel = GetNode<Label>("CenterContainer/VBoxContainer/CharactersContainer/MagePanel/StatsLabel");
            _warriorStatsLabel = GetNode<Label>("CenterContainer/VBoxContainer/CharactersContainer/WarriorPanel/StatsLabel");

            // Connect signals
            _mageButton.Pressed += OnMageSelected;
            _warriorButton.Pressed += OnWarriorSelected;
            _backButton.Pressed += OnBackPressed;

            // Setup stat displays
            SetupStatDisplays();

            GD.Print("[CharacterSelectUI] Initialized");
        }

        /// <summary>
        /// Sets up character stat displays.
        /// </summary>
        private void SetupStatDisplays()
        {
            // Mage stats
            _mageStatsLabel.Text = $@"Eltarion - The Mage

HP: {GameConstants.MAGE_BASE_HP}
Damage: {GameConstants.MAGE_BASE_DAMAGE}
Defense: {GameConstants.MAGE_BASE_DEFENSE}

Type: Magical
Specialty: AoE + Support";

            // Warrior stats
            _warriorStatsLabel.Text = $@"Eldric - The Warrior

HP: {GameConstants.WARRIOR_BASE_HP}
Damage: {GameConstants.WARRIOR_BASE_DAMAGE}
Defense: {GameConstants.WARRIOR_BASE_DEFENSE}

Type: Physical
Specialty: Single Target + Defense";
        }

        /// <summary>
        /// Called when Mage is selected.
        /// </summary>
        private void OnMageSelected()
        {
            GD.Print("[CharacterSelectUI] Mage selected");
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Start new game with Mage
            GameStateManager.Instance.StartNewGame(CharacterClass.Mage);
        }

        /// <summary>
        /// Called when Warrior is selected.
        /// </summary>
        private void OnWarriorSelected()
        {
            GD.Print("[CharacterSelectUI] Warrior selected");
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Start new game with Warrior
            GameStateManager.Instance.StartNewGame(CharacterClass.Warrior);
        }

        /// <summary>
        /// Called when Back button is pressed.
        /// </summary>
        private void OnBackPressed()
        {
            GD.Print("[CharacterSelectUI] Back pressed");
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            SceneManager.GoToMainMenu();
        }
    }
}