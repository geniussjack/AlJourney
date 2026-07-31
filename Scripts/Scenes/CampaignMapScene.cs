using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Scenes
{
    /// <summary>
    /// The campaign map screen — the hub between levels. Shows locations in playthrough order (from
    /// the village ruins to the necromancer's lair), the main line's sequentially unlocking levels and
    /// their branches, and provides access to the settlement shop (see REDESIGN_NOTES.md, Stage 3).
    /// Built entirely in code, similar to <see cref="UI.BattleHUD"/>/<see cref="UI.TurnActionPanel"/> —
    /// doesn't use any editor-authored child nodes besides the root Control.
    /// </summary>
    public partial class CampaignMapScene : Control
    {
        /// <summary>
        /// Initializes the map screen: builds the list of locations and levels based on the current
        /// save progress (<see cref="GameStateManager.CompletedLevelIds"/>).
        /// </summary>
        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);

            VBoxContainer root = new()
            {
                MouseFilter = MouseFilterEnum.Ignore
            };
            root.SetAnchorsPreset(LayoutPreset.FullRect);
            root.AddThemeConstantOverride("separation", 12);
            AddChild(root);

            root.AddChild(BuildTopBar());

            ScrollContainer scroll = new()
            {
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            root.AddChild(scroll);

            VBoxContainer locationsContainer = new();
            locationsContainer.AddThemeConstantOverride("separation", 16);
            scroll.AddChild(locationsContainer);

            IReadOnlyCollection<string> completedLevelIds = GameStateManager.Instance.CompletedLevelIds;

            foreach (LocationId location in System.Enum.GetValues<LocationId>())
            {
                locationsContainer.AddChild(BuildLocationSection(location, completedLevelIds));
            }

            GD.Print("[CampaignMapScene] Initialized");
        }

        private HBoxContainer BuildTopBar()
        {
            HBoxContainer topBar = new();
            topBar.AddThemeConstantOverride("separation", 12);

            Label title = new() { Text = Tr("UI_MAP_TITLE"), SizeFlagsHorizontal = SizeFlags.ExpandFill };
            topBar.AddChild(title);

            Button shopButton = new() { Text = Tr("UI_MAP_SHOP") };
            shopButton.Pressed += SceneManager.GoToShop;
            topBar.AddChild(shopButton);

            Button mainMenuButton = new() { Text = Tr("UI_MAP_MAIN_MENU") };
            mainMenuButton.Pressed += SceneManager.GoToMainMenu;
            topBar.AddChild(mainMenuButton);

            return topBar;
        }

        private VBoxContainer BuildLocationSection(LocationId location, IReadOnlyCollection<string> completedLevelIds)
        {
            VBoxContainer section = new();
            section.AddThemeConstantOverride("separation", 6);

            Label header = new() { Text = Tr(CampaignDatabase.GetLocationNameKey(location)) };
            section.AddChild(header);

            HBoxContainer levelsRow = new();
            levelsRow.AddThemeConstantOverride("separation", 8);
            section.AddChild(levelsRow);

            IEnumerable<LevelDefinition> levelsInLocation = CampaignDatabase.Levels
                .Where(level => level.Location == location)
                .OrderBy(level => level.IsBranch)
                .ThenBy(level => level.OrderInLocation);

            foreach (LevelDefinition level in levelsInLocation)
            {
                levelsRow.AddChild(BuildLevelButton(level, completedLevelIds));
            }

            return section;
        }

        private Button BuildLevelButton(LevelDefinition level, IReadOnlyCollection<string> completedLevelIds)
        {
            bool isUnlocked = level.RequiredLevelId is null || completedLevelIds.Contains(level.RequiredLevelId);
            bool isCompleted = completedLevelIds.Contains(level.Id);

            string label = level.IsBranch ? Tr("UI_MAP_BRANCH") : $"{Tr("UI_MAP_LEVEL")} {level.OrderInLocation}";
            if (isCompleted)
            {
                label += $" ({Tr("UI_MAP_COMPLETED")})";
            }
            else if (!isUnlocked)
            {
                label += $" ({Tr("UI_MAP_LOCKED")})";
            }

            Button button = new()
            {
                Text = label,
                Disabled = !isUnlocked,
                Modulate = isUnlocked ? Colors.White : new Color(1, 1, 1, 0.45f)
            };

            if (isUnlocked)
            {
                button.Pressed += () => OnLevelSelected(level);
            }

            return button;
        }

        private static void OnLevelSelected(LevelDefinition level)
        {
            GD.Print($"[CampaignMapScene] Selected level {level.Id}");
            GameStateManager.Instance.SelectLevel(level.Id);
            SceneManager.Instance.LoadScene(GameState.Battle);
        }
    }
}
