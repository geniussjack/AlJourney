using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Ability browser UI for the mage and warrior.
    /// </summary>
    public partial class AbilitiesUI : Control
    {
        private Button _closeButton;
        private VBoxContainer _mageAbilitiesContainer;
        private VBoxContainer _warriorAbilitiesContainer;

        public override void _Ready()
        {
            _closeButton = GetNode<Button>("MarginContainer/VBoxContainer/Header/CloseButton");
            _mageAbilitiesContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ContentHBox/MageSection/ScrollContainer/MageAbilitiesContainer");
            _warriorAbilitiesContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ContentHBox/WarriorSection/ScrollContainer/WarriorAbilitiesContainer");

            _closeButton.Pressed += OnClosePressed;

            RefreshUI();

            GD.Print("[AbilitiesUI] Initialized");
        }

        private void RefreshUI()
        {
            if (AbilitySystem.Instance == null)
            {
                return;
            }

            PopulateAbilitiesContainer(_mageAbilitiesContainer, CharacterClass.Mage);
            PopulateAbilitiesContainer(_warriorAbilitiesContainer, CharacterClass.Warrior);
        }

        private void PopulateAbilitiesContainer(VBoxContainer container, CharacterClass heroClass)
        {
            foreach (Node child in container.GetChildren())
            {
                child.QueueFree();
            }

            List<AbilityData> abilities = AbilitySystem.Instance.GetAvailableAbilities(heroClass);
            foreach (AbilityData ability in abilities)
            {
                Button btn = new()
                {
                    Text = $"{ability.Name} (Cost: {ability.UnlockCost})",
                    CustomMinimumSize = new Vector2(0, 50)
                };

                AbilityData currentAbility = ability;
                btn.Pressed += () => OnAbilityPressed(currentAbility, heroClass);
                container.AddChild(btn);
            }
        }

        private static void OnAbilityPressed(AbilityData ability, CharacterClass _)
        {
            GD.Print($"[AbilitiesUI] Ability pressed: {ability.Name}");
            // Placeholder for unlock/equip logic.
        }

        private void OnClosePressed()
        {
            GD.Print("[AbilitiesUI] Closing abilities menu");
            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));
            QueueFree();
        }
    }
}
