using AlJourney.Scripts.Match3;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Visual representation of a match-3 grid element.
    /// Handles animations and user interactions.
    /// </summary>
    public partial class ElementSprite : Control
    {
        [Signal]
        public delegate void ElementClickedEventHandler(ElementSprite element);

        private TextureRect _sprite;
        private Panel _highlightPanel;
        private ElementData _data;

        private Vector2 _targetPosition;
        private bool _isAnimating;
        private float _animationSpeed = 10.0f;

        /// <summary>
        /// Associated element data.
        /// </summary>
        public ElementData Data => _data;

        /// <summary>
        /// Grid X position.
        /// </summary>
        public int GridX => _data?.X ?? -1;

        /// <summary>
        /// Grid Y position.
        /// </summary>
        public int GridY => _data?.Y ?? -1;

        /// <summary>
        /// Is element currently animating.
        /// </summary>
        public bool IsAnimating => _isAnimating;

        public override void _Ready()
        {
            // Create sprite
            _sprite = new TextureRect
            {
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(64, 64)
            };
            AddChild(_sprite);

            // Create highlight panel
            _highlightPanel = new Panel
            {
                Visible = false
            };
            _highlightPanel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(_highlightPanel);

            // Setup interaction
            MouseFilter = MouseFilterEnum.Stop;
            GuiInput += OnGuiInput;
        }

        /// <summary>
        /// Initializes element with data and texture.
        /// </summary>
        public void Initialize(ElementData data, Texture2D texture)
        {
            _data = data;
            _sprite.Texture = texture;
            _targetPosition = Position;

            CustomMinimumSize = new Vector2(64, 64);
            Size = new Vector2(64, 64);
        }

        /// <summary>
        /// Updates element data (for refills/swaps).
        /// </summary>
        public void UpdateData(ElementData newData)
        {
            _data = newData;
        }

        /// <summary>
        /// Sets element texture based on type.
        /// </summary>
        public void SetTexture(Texture2D texture)
        {
            _sprite.Texture = texture;
        }

        /// <summary>
        /// Shows/hides selection highlight.
        /// </summary>
        public void SetHighlight(bool enabled)
        {
            _highlightPanel.Visible = enabled;

            if (enabled)
            {
                _highlightPanel.Modulate = new Color(1, 1, 0, 0.5f); // Yellow highlight
            }
        }

        /// <summary>
        /// Animates element to target position.
        /// </summary>
        public void AnimateToPosition(Vector2 targetPos)
        {
            _targetPosition = targetPos;
            _isAnimating = true;
        }

        /// <summary>
        /// Instantly sets position without animation.
        /// </summary>
        public void SetGridPosition(Vector2 pos)
        {
            Position = pos;
            _targetPosition = pos;
            _isAnimating = false;
        }

        /// <summary>
        /// Plays match animation and destroys element.
        /// </summary>
        public void PlayMatchAnimation()
        {
            // Scale down animation
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(this, "scale", Vector2.Zero, 0.3f).SetEase(Tween.EaseType.In);
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.3f);
            tween.Chain().TweenCallback(Callable.From(() => QueueFree()));
        }

        /// <summary>
        /// Plays swap animation.
        /// </summary>
        public void PlaySwapAnimation(Vector2 targetPos, float duration = 0.2f)
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "position", targetPos, duration).SetEase(Tween.EaseType.InOut);
        }

        public override void _Process(double delta)
        {
            if (_isAnimating)
            {
                // Smooth movement to target position
                Position = Position.Lerp(_targetPosition, _animationSpeed * (float)delta);

                // Check if reached target
                if (Position.DistanceTo(_targetPosition) < 1.0f)
                {
                    Position = _targetPosition;
                    _isAnimating = false;
                }
            }
        }

        /// <summary>
        /// Handles mouse input for element selection.
        /// </summary>
        private void OnGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
                {
                    EmitSignal(SignalName.ElementClicked, this);
                }
            }
        }
    }
}