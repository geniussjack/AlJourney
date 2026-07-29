using AlJourney.Scripts.Battle;
using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Панель выбора действия игрока в пошаговом бою. Реализует шаги "выбери бойца → выбери способность"
    /// (наведение и подтверждение цели выполняется кликом по портрету союзника/полоске здоровья врага,
    /// см. <see cref="BattleHUD"/>). Появляется только во время фазы <see cref="BattlePhase.PlayerTurn"/>.
    /// </summary>
    public partial class TurnActionPanel : Control
    {
        private BattleManager _battleManager;

        private Label _promptLabel;
        private HBoxContainer _actorRow;
        private HBoxContainer _abilityRow;

        /// <summary>
        /// Создаёт визуальную структуру панели. Сама панель добавляется в сцену вызывающим кодом (BattleScene).
        /// </summary>
        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            SetAnchorsPreset(LayoutPreset.FullRect);

            VBoxContainer root = new()
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                MouseFilter = MouseFilterEnum.Ignore
            };
            root.SetAnchorsPreset(LayoutPreset.CenterBottom);
            root.GrowVertical = GrowDirection.Begin;
            root.GrowHorizontal = GrowDirection.Both;
            root.Position -= new Vector2(0, 130);
            root.AddThemeConstantOverride("separation", 8);
            AddChild(root);

            _promptLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
            root.AddChild(_promptLabel);

            _actorRow = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            _actorRow.AddThemeConstantOverride("separation", 12);
            root.AddChild(_actorRow);

            _abilityRow = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            _abilityRow.AddThemeConstantOverride("separation", 12);
            root.AddChild(_abilityRow);
        }

        /// <summary>
        /// Связывает панель с менеджером боя и подписывается на изменения состояния хода.
        /// </summary>
        /// <param name="battleManager">Менеджер пошагового боя.</param>
        public void Initialize(BattleManager battleManager)
        {
            _battleManager = battleManager;

            _battleManager.TurnStateChanged += Refresh;
            _battleManager.PhaseChanged += OnPhaseChanged;

            Refresh();
        }

        private void OnPhaseChanged(BattlePhase newPhase)
        {
            Refresh();
        }

        private void Refresh()
        {
            ClearChildren(_actorRow);
            ClearChildren(_abilityRow);

            if (_battleManager.CurrentPhase != BattlePhase.PlayerTurn)
            {
                _promptLabel.Text = "";
                Visible = false;
                return;
            }

            Visible = true;

            if (_battleManager.SelectedActor is null)
            {
                _promptLabel.Text = Tr("UI_BATTLE_CHOOSE_ACTOR");
                foreach (PlayerCharacter actor in _battleManager.PendingActors)
                {
                    Button actorButton = new() { Text = actor.CharacterName };
                    actorButton.Pressed += () => _battleManager.SelectActor(actor);
                    _actorRow.AddChild(actorButton);
                }
                return;
            }

            if (_battleManager.SelectedAbility is null)
            {
                _promptLabel.Text = Tr("UI_BATTLE_CHOOSE_ABILITY");

                (AbilityData attack, AbilityData support) = AbilityDatabase.GetHeroAbilities(_battleManager.SelectedActor.CharacterClass);

                Button attackButton = new() { Text = Tr(attack.Name) };
                attackButton.Pressed += () => _battleManager.SelectAbility(attack);
                _abilityRow.AddChild(attackButton);

                Button supportButton = new() { Text = Tr(support.Name) };
                supportButton.Pressed += () => _battleManager.SelectAbility(support);
                _abilityRow.AddChild(supportButton);
                return;
            }

            _promptLabel.Text = Tr("UI_BATTLE_CHOOSE_TARGET");
        }

        private static void ClearChildren(Node container)
        {
            foreach (Node child in container.GetChildren())
            {
                child.QueueFree();
            }
        }

        /// <summary>
        /// Отписывается от событий менеджера боя при удалении узла из дерева.
        /// </summary>
        public override void _ExitTree()
        {
            if (_battleManager != null)
            {
                _battleManager.TurnStateChanged -= Refresh;
                _battleManager.PhaseChanged -= OnPhaseChanged;
            }
        }
    }
}
