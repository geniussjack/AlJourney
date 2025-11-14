using AlJourney.Scripts.Core;
using AlJourney.Scripts.Match3;
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
        private int _gridSize;

        public override void _Ready()
        {
            _gridManager = GetNode<GridManager>("/root/GridManager");
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
                _gridSize * CELL_SIZE + (_gridSize - 1) * CELL_SPACING,
                _gridSize * CELL_SIZE + (_gridSize - 1) * CELL_SPACING
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
            var image = Image.CreateEmpty(64, 64, false, Image.Format.Rgba8);
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
                    var elementSprite = new ElementSprite();
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
            var tween = CreateTween();
            tween.TweenProperty(element, "position", originalPos + new Vector2(10, 0), 0.05f);
            tween.TweenProperty(element, "position", originalPos - new Vector2(10, 0), 0.05f);
            tween.TweenProperty(element, "position", originalPos, 0.05f);
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
            var matches = _gridManager.FindAllMatches();

            if (matches.Count > 0)
            {
                // Animate matched elements
                foreach (var match in matches)
                {
                    foreach (var (x, y) in match.MatchedPositions)
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
                        var elementSprite = new ElementSprite();
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
            GetTree().CreateTimer(0.5f).Timeout += ProcessMatches;
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