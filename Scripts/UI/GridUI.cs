using AlJourney.Scripts.Core;
using AlJourney.Scripts.Match3;
using AlJourney.Scripts.Utils;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Visual controller for the match-3 grid.
    /// Handles rendering, animations, and user input.
    /// </summary>
    public partial class GridUI : Control
    {
        [Signal]
        public delegate void SwapAttemptedEventHandler(int x1, int y1, int x2, int y2);

        private const int CELL_SIZE = 80;
        private const int CELL_SPACING = 10;

        private GridContainer _gridContainer;
        private ElementSprite[,] _visualGrid;
        private ElementSprite _selectedElement;

        // Element textures
        private Dictionary<ElementType, Texture2D> _elementTextures;

        private GridManager _gridManager;
        private ComboSystem _comboSystem;
        private int _gridSize;

        public override void _Ready()
        {
            _gridManager = GetNode<GridManager>("/root/GridManager");
            _comboSystem = GetNode<ComboSystem>("/root/ComboSystem");
            _gridSize = GameConstants.GRID_SIZE;

            // Create grid container
            _gridContainer = new GridContainer
            {
                Columns = _gridSize
            };
            _gridContainer.AddThemeConstantOverride("h_separation", CELL_SPACING);
            _gridContainer.AddThemeConstantOverride("v_separation", CELL_SPACING);
            AddChild(_gridContainer);

            // Initialize visual grid
            _visualGrid = new ElementSprite[_gridSize, _gridSize];

            // Load element textures
            LoadElementTextures();

            // Connect grid manager signals
            _gridManager.GridInitialized += OnGridInitialized;
            _gridManager.SwapCompleted += OnSwapCompleted;
            _gridManager.GridRefillCompleted += OnGridRefilled;

            CustomMinimumSize = new Vector2(
                (_gridSize * CELL_SIZE) + ((_gridSize - 1) * CELL_SPACING),
                (_gridSize * CELL_SIZE) + ((_gridSize - 1) * CELL_SPACING)
            );

            GD.Print("[GridUI] Initialized");
        }

        /// <summary>
        /// Loads element textures (placeholders for now).
        /// </summary>
        private void LoadElementTextures()
        {
            _elementTextures = new Dictionary<ElementType, Texture2D>
            {
                // Try to load textures, fallback to colored squares if not found
                [ElementType.Fire] = LoadOrCreateTexture("res://Resources/Sprites/Elements/fire_icon.png", Colors.Red),
                [ElementType.Heal] = LoadOrCreateTexture("res://Resources/Sprites/Elements/heal_icon.png", Colors.Green),
                [ElementType.Sword] = LoadOrCreateTexture("res://Resources/Sprites/Elements/sword_icon.png", Colors.Orange),
                [ElementType.Shield] = LoadOrCreateTexture("res://Resources/Sprites/Elements/shield_icon.png", Colors.Blue)
            };

            GD.Print("[GridUI] Element textures loaded");
        }

        /// <summary>
        /// Loads texture or creates colored placeholder.
        /// </summary>
        private static Texture2D LoadOrCreateTexture(string path, Color fallbackColor)
        {
            // Try to load texture
            if (ResourceLoader.Exists(path))
            {
                return GD.Load<Texture2D>(path);
            }

            // Create colored square placeholder
            Image image = Image.CreateEmpty(64, 64, false, Image.Format.Rgba8);
            image.Fill(fallbackColor);
            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Called when grid is initialized.
        /// </summary>
        private void OnGridInitialized()
        {
            CreateVisualGrid();
            GD.Print("[GridUI] Visual grid created");
        }

        /// <summary>
        /// Creates visual representation of the grid.
        /// </summary>
        private void CreateVisualGrid()
        {
            // Clear existing elements
            foreach (Node child in _gridContainer.GetChildren())
            {
                child.QueueFree();
            }

            ElementData[,] logicalGrid = _gridManager.GetGrid();

            // Create visual elements
            for (int y = 0; y < _gridSize; y++)
            {
                for (int x = 0; x < _gridSize; x++)
                {
                    ElementData data = logicalGrid[x, y];

                    // Create visual element
                    ElementSprite elementSprite = new();
                    _gridContainer.AddChild(elementSprite);

                    // Initialize with data and texture
                    Texture2D texture = _elementTextures[data.Type];
                    elementSprite.Initialize(data, texture);
                    elementSprite.ElementClicked += OnElementClicked;

                    _visualGrid[x, y] = elementSprite;
                }
            }
        }

        /// <summary>
        /// Called when an element is clicked.
        /// </summary>
        private void OnElementClicked(ElementSprite clickedElement)
        {
            if (_selectedElement == null)
            {
                // First selection
                _selectedElement = clickedElement;
                _selectedElement.SetHighlight(true);
                GD.Print($"[GridUI] Selected element at ({clickedElement.GridX}, {clickedElement.GridY})");
            }
            else
            {
                // Second selection - attempt swap
                if (clickedElement == _selectedElement)
                {
                    // Deselect if clicking same element
                    _selectedElement.SetHighlight(false);
                    _selectedElement = null;
                    return;
                }

                // Check if elements are adjacent
                int deltaX = Mathf.Abs(clickedElement.GridX - _selectedElement.GridX);
                int deltaY = Mathf.Abs(clickedElement.GridY - _selectedElement.GridY);

                if ((deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1))
                {
                    // Valid adjacent swap
                    GD.Print($"[GridUI] Attempting swap: ({_selectedElement.GridX},{_selectedElement.GridY}) <-> ({clickedElement.GridX},{clickedElement.GridY})");

                    // Try swap
                    bool swapSuccessful = _gridManager.TrySwap(
                        _selectedElement.GridX, _selectedElement.GridY,
                        clickedElement.GridX, clickedElement.GridY
                    );

                    if (swapSuccessful)
                    {
                        // Animate swap
                        AnimateSwap(_selectedElement, clickedElement);
                    }
                    else
                    {
                        // Invalid swap - shake animation
                        PlayInvalidSwapAnimation(_selectedElement);
                        PlayInvalidSwapAnimation(clickedElement);
                    }
                }
                else
                {
                    // Not adjacent - select new element
                    _selectedElement.SetHighlight(false);
                    _selectedElement = clickedElement;
                    _selectedElement.SetHighlight(true);
                }

                // Deselect
                _selectedElement?.SetHighlight(false);
                _selectedElement = null;
            }
        }

        /// <summary>
        /// Animates element swap.
        /// </summary>
        private void AnimateSwap(ElementSprite element1, ElementSprite element2)
        {
            Vector2 pos1 = element1.Position;
            Vector2 pos2 = element2.Position;

            element1.PlaySwapAnimation(pos2);
            element2.PlaySwapAnimation(pos1);

            // Swap visual references
            (_visualGrid[element1.GridX, element1.GridY], _visualGrid[element2.GridX, element2.GridY]) =
                (_visualGrid[element2.GridX, element2.GridY], _visualGrid[element1.GridX, element1.GridY]);
        }

        /// <summary>
        /// Plays invalid swap shake animation.
        /// </summary>
        private void PlayInvalidSwapAnimation(ElementSprite element)
        {
            Vector2 originalPos = element.Position;
            Tween tween = CreateTween();
            _ = tween.TweenProperty(element, "position", originalPos + new Vector2(10, 0), 0.05f);
            _ = tween.TweenProperty(element, "position", originalPos - new Vector2(10, 0), 0.05f);
            _ = tween.TweenProperty(element, "position", originalPos, 0.05f);
        }

        /// <summary>
        /// Called when swap is completed in grid manager.
        /// </summary>
        private void OnSwapCompleted(bool wasValid)
        {
            if (wasValid)
            {
                // Process matches
                ProcessMatches();
            }
        }

        /// <summary>
        /// Processes and animates matches.
        /// </summary>
        private void ProcessMatches()
        {
            List<MatchResult> matches = _gridManager.FindAllMatches();

            if (matches.Count > 0)
            {
                // Get combo effects
                List<ComboEffect> effects = _comboSystem.ProcessMatches(matches);

                // Visualize combo effects
                VisualizeComboEffects(effects, matches);

                // Animate matched elements
                foreach (MatchResult match in matches)
                {
                    foreach ((int x, int y) in match.MatchedPositions)
                    {
                        _visualGrid[x, y]?.PlayMatchAnimation();
                        _visualGrid[x, y] = null;
                    }
                }

                // Process matches in grid manager (with delay)
                GetTree().CreateTimer(0.4f).Timeout += () =>
                {
                    _gridManager.ProcessMatches(matches);
                };
            }
        }

        /// <summary>
        /// Visualizes combo effects on the grid.
        /// </summary>
        private void VisualizeComboEffects(List<ComboEffect> effects, List<MatchResult> matches)
        {
            for (int i = 0; i < effects.Count && i < matches.Count; i++)
            {
                ComboEffect effect = effects[i];
                MatchResult match = matches[i];

                // Calculate center position of match
                Vector2 centerPos = CalculateMatchCenter(match);

                // Spawn particles at match location
                ComboParticles.SpawnComboEffect(this, centerPos, effect.ElementType, effect.ComboLevel);

                // Flash cells
                FlashMatchedCells(match, effect.ElementType);

                // Spawn effect text
                string effectText = GetEffectText(effect);
                Color effectColor = GetEffectColor(effect.ElementType);
                ComboParticles.SpawnFloatingText(this, centerPos - new Vector2(0, 40), effectText, effectColor);
            }
        }

        /// <summary>
        /// Calculates center position of a match.
        /// </summary>
        private Vector2 CalculateMatchCenter(MatchResult match)
        {
            if (match.MatchedPositions.Count == 0)
            {
                return Vector2.Zero;
            }

            float sumX = 0;
            float sumY = 0;

            foreach ((int x, int y) in match.MatchedPositions)
            {
                Vector2 pos = GetElementPosition(x, y);
                sumX += pos.X;
                sumY += pos.Y;
            }

            return new Vector2(
                sumX / match.MatchedPositions.Count + CELL_SIZE / 2,
                sumY / match.MatchedPositions.Count + CELL_SIZE / 2
            );
        }

        /// <summary>
        /// Flashes matched cells with color.
        /// </summary>
        private void FlashMatchedCells(MatchResult match, ElementType elementType)
        {
            Color flashColor = GetEffectColor(elementType);

            foreach ((int x, int y) in match.MatchedPositions)
            {
                ElementSprite sprite = _visualGrid[x, y];
                if (sprite != null)
                {
                    // Quick flash animation
                    Tween tween = CreateTween();
                    _ = tween.TweenProperty(sprite, "modulate", flashColor * 1.5f, 0.1f);
                    _ = tween.TweenProperty(sprite, "modulate", Colors.White, 0.1f);
                }
            }
        }

        /// <summary>
        /// Gets effect text for combo.
        /// </summary>
        private static string GetEffectText(ComboEffect effect)
        {
            return effect.ElementType switch
            {
                ElementType.Fire => $"🔥 {effect.Damage} DMG!",
                ElementType.Heal => $"💚 +{effect.Healing} HP!",
                ElementType.Sword => $"⚔️ {effect.Damage} DMG!",
                ElementType.Shield => $"🛡️ +{effect.Shield} Shield!",
                _ => "Combo!"
            };
        }

        /// <summary>
        /// Gets effect color for element type.
        /// </summary>
        private static Color GetEffectColor(ElementType elementType)
        {
            return elementType switch
            {
                ElementType.Fire => new Color(1.0f, 0.5f, 0.0f),
                ElementType.Heal => new Color(0.0f, 1.0f, 0.5f),
                ElementType.Sword => new Color(1.0f, 0.7f, 0.0f),
                ElementType.Shield => new Color(0.3f, 0.6f, 1.0f),
                _ => Colors.White
            };
        }

        /// <summary>
        /// Called when grid is refilled after matches.
        /// </summary>
        private void OnGridRefilled()
        {
            // Update visual grid with new elements
            ElementData[,] logicalGrid = _gridManager.GetGrid();

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_visualGrid[x, y] == null)
                    {
                        // Create new element
                        ElementData data = logicalGrid[x, y];
                        ElementSprite elementSprite = new();
                        _gridContainer.AddChild(elementSprite);

                        Texture2D texture = _elementTextures[data.Type];
                        elementSprite.Initialize(data, texture);
                        elementSprite.ElementClicked += OnElementClicked;

                        // Position above grid for fall animation
                        Vector2 targetPos = GetElementPosition(x, y);
                        elementSprite.SetGridPosition(targetPos - new Vector2(0, 400));
                        elementSprite.AnimateToPosition(targetPos);

                        _visualGrid[x, y] = elementSprite;
                    }
                }
            }

            // Check for cascade matches after animation
            GetTree().CreateTimer(0.5f).Timeout += ProcessCascadeMatches;
        }

        /// <summary>
        /// Processes cascade matches with bonus.
        /// </summary>
        private void ProcessCascadeMatches()
        {
            List<MatchResult> matches = _gridManager.FindAllMatches();

            if (matches.Count > 0)
            {
                // Get combo effects with cascade bonus
                List<ComboEffect> effects = _comboSystem.ProcessMatches(matches, isCascade: true);

                // Visualize combo effects with cascade indicator
                VisualizeComboEffects(effects, matches);

                // Show cascade level indicator
                int cascadeLevel = _comboSystem.GetCascadeLevel();
                if (cascadeLevel > 0)
                {
                    ShowCascadeIndicator(cascadeLevel);
                }

                // Animate matched elements
                foreach (MatchResult match in matches)
                {
                    foreach ((int x, int y) in match.MatchedPositions)
                    {
                        _visualGrid[x, y]?.PlayMatchAnimation();
                        _visualGrid[x, y] = null;
                    }
                }

                // Process matches in grid manager (with delay)
                GetTree().CreateTimer(0.4f).Timeout += () =>
                {
                    _gridManager.ProcessMatches(matches);
                };
            }
            else
            {
                // No more cascades - reset counter
                _comboSystem.ResetCascade();
            }
        }

        /// <summary>
        /// Shows cascade level indicator.
        /// </summary>
        private void ShowCascadeIndicator(int cascadeLevel)
        {
            Vector2 centerPos = new Vector2(
                (_gridSize * (CELL_SIZE + CELL_SPACING)) / 2,
                (_gridSize * (CELL_SIZE + CELL_SPACING)) / 2
            );

            string cascadeText = $"⚡ CASCADE x{cascadeLevel}! ⚡";
            Color cascadeColor = new Color(1.0f, 0.8f, 0.0f); // Golden yellow

            ComboParticles.SpawnFloatingText(this, centerPos, cascadeText, cascadeColor);
            GD.Print($"[GridUI] Cascade x{cascadeLevel} displayed!");
        }

        /// <summary>
        /// Gets screen position for grid coordinates.
        /// </summary>
        private static Vector2 GetElementPosition(int gridX, int gridY)
        {
            return new Vector2(
                gridX * (CELL_SIZE + CELL_SPACING),
                gridY * (CELL_SIZE + CELL_SPACING)
            );
        }

        public override void _ExitTree()
        {
            if (_gridManager != null)
            {
                _gridManager.GridInitialized -= OnGridInitialized;
                _gridManager.SwapCompleted -= OnSwapCompleted;
                _gridManager.GridRefillCompleted -= OnGridRefilled;
            }
        }
    }
}
