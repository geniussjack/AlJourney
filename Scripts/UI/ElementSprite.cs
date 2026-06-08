using AlJourney.Scripts.Match3;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Основной класс ElementSprite.
    /// </summary>
    public partial class ElementSprite : Control
    {
        [Signal]
        /// <summary>
        /// Элемент ElementClickedEventHandler.
        /// </summary>
        public delegate void ElementClickedEventHandler(ElementSprite element);

        private TextureRect _sprite;
        private Panel _highlightPanel;
        private Vector2 _targetPosition;
        private float _animationSpeed = 10.0f;

        public ElementData Data { get; private set; }

        /// <summary>
        /// Элемент GridX.
        /// </summary>
        public int GridX => Data?.X ?? -1;

        /// <summary>
        /// Элемент GridY.
        /// </summary>
        public int GridY => Data?.Y ?? -1;

        public bool IsAnimating { get; private set; }

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            _sprite = new TextureRect
            {
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(128, 128)
            };
            AddChild(_sprite);

            _highlightPanel = new Panel
            {
                Visible = false
            };
            _highlightPanel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(_highlightPanel);

            MouseFilter = MouseFilterEnum.Stop;
            GuiInput += OnGuiInput;
        }

        /// <summary>
        /// Инициализирует .
        /// </summary>
        public void Initialize(ElementData data, Texture2D texture)
        {
            Data = data;
            _sprite.Texture = texture;
            _targetPosition = Position;

            CustomMinimumSize = new Vector2(128, 128);
            Size = new Vector2(128, 128);
        }

        /// <summary>
        /// Обновляет Data.
        /// </summary>
        public void UpdateData(ElementData newData)
        {
            Data = newData;
        }

        /// <summary>
        /// Устанавливает Texture.
        /// </summary>
        public void SetTexture(Texture2D texture)
        {
            _sprite.Texture = texture;
        }

        /// <summary>
        /// Устанавливает Highlight.
        /// </summary>
        public void SetHighlight(bool enabled)
        {
            _highlightPanel.Visible = enabled;

            if (enabled)
            {
                _highlightPanel.Modulate = new Color(1, 1, 0, 0.5f); 
            }
        }

        /// <summary>
        /// Элемент AnimateToPosition.
        /// </summary>
        public void AnimateToPosition(Vector2 targetPos)
        {
            _targetPosition = targetPos;
            IsAnimating = true;
        }

        /// <summary>
        /// Устанавливает GridPosition.
        /// </summary>
        public void SetGridPosition(Vector2 pos)
        {
            Position = pos;
            _targetPosition = pos;
            IsAnimating = false;
        }

        /// <summary>
        /// Воспроизводит MatchAnimation.
        /// </summary>
        public void PlayMatchAnimation()
        {
            Tween tween = CreateTween();
            _ = tween.SetParallel(true);
            _ = tween.TweenProperty(this, "scale", Vector2.Zero, 0.3f).SetEase(Tween.EaseType.In);
            _ = tween.TweenProperty(this, "modulate:a", 0.0f, 0.3f);
            _ = tween.Chain().TweenCallback(Callable.From(QueueFree));
        }

        /// <summary>
        /// Воспроизводит SwapAnimation.
        /// </summary>
        public void PlaySwapAnimation(Vector2 targetPos, float duration = 0.2f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(this, "position", targetPos, duration).SetEase(Tween.EaseType.InOut);
        }

        /// <summary>
        /// Элемент _Process.
        /// </summary>
        public override void _Process(double delta)
        {
            if (IsAnimating)
            {
                Position = Position.Lerp(_targetPosition, _animationSpeed * (float)delta);

                if (Position.DistanceTo(_targetPosition) < 1.0f)
                {
                    Position = _targetPosition;
                    IsAnimating = false;
                }
            }
        }

        private void OnGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
                {
                    _ = EmitSignal(SignalName.ElementClicked, this);
                }
            }
        }
    }
}
