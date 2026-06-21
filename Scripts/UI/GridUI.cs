using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Match3;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Utils;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Visual layer for the Match-3 board.
    /// </summary>
    public partial class GridUI : Control
    {
        [Signal]
        public delegate void SwapAttemptedEventHandler(int x1, int y1, int x2, int y2);

        private const int CELL_SIZE = 80;
        private const int CELL_SPACING = 4;
        private const int GRID_TOP_OFFSET = 40;

        private Vector2 _elementSize = new(64, 64);
        private Vector2 _gridOffset = new(0, 0);

        public bool CanInteract { get; set; } = true;

        private Control _gridContainer;
        private ElementSprite[,] _visualGrid;
        private ElementSprite _selectedElement;

        private Dictionary<ElementType, Texture2D> _elementTextures;
        private AlJourney.Scripts.Characters.DualHeroSystem _heroSystem;
        private readonly Dictionary<ElementData, ElementSprite> _spriteMap = [];

        private GridManager _gridManager;
        private ComboSystem _comboSystem;
        private int _gridSize;

        public override void _Ready()
        {
            _gridManager = GetNode<GridManager>("/root/GridManager");
            _comboSystem = GetNode<ComboSystem>("/root/ComboSystem");
            _gridSize = GameConstants.GRID_SIZE;

            _gridContainer = new Control
            {
                Position = new Vector2(0, GRID_TOP_OFFSET)
            };
            AddChild(_gridContainer);

            _visualGrid = new ElementSprite[_gridSize, _gridSize];
            LoadElementTextures();

            _gridManager.GridInitialized += OnGridInitialized;
            _gridManager.SwapCompleted += OnSwapCompleted;
            _gridManager.GridRefillCompleted += OnGridRefilled;

            CustomMinimumSize = new Vector2(
                (_gridSize * (CELL_SIZE + CELL_SPACING)) - CELL_SPACING,
                GRID_TOP_OFFSET + (_gridSize * (CELL_SIZE + CELL_SPACING)) - CELL_SPACING
            );

            GD.Print("[GridUI] Initialized");
        }

        public void Initialize(AlJourney.Scripts.Characters.DualHeroSystem heroSystem)
        {
            _heroSystem = heroSystem;
            _heroSystem.HeroDied += OnHeroDied;
        }

        private void LoadElementTextures()
        {
            _elementTextures = new Dictionary<ElementType, Texture2D>
            {
                [ElementType.Heal] = LoadOrCreateTexture("res://Resources/Sprites/Elements/heal_icon.png", Colors.Green),
                [ElementType.Shield] = LoadOrCreateTexture("res://Resources/Sprites/Elements/shield_icon.png", Colors.Blue)
            };

            // Load Mage weapon (Fire element)
            AlJourney.Scripts.Data.EquipmentData mageWeapon = InventoryManager.Instance?.GetEquippedItem(CharacterClass.Mage, EquipmentSlot.Weapon);
            string mageSprite = mageWeapon?.Id != null ? $"res://Resources/Sprites/Elements/{mageWeapon.Id}_sprite.png" : "res://Resources/Sprites/Elements/fireball_sprite.png";
            if (mageWeapon?.Id == "fireball") mageSprite = "res://Resources/Sprites/Elements/fireball_sprite.png"; // Fallback to ensure
            if (!ResourceLoader.Exists(mageSprite)) mageSprite = "res://Resources/Sprites/Elements/fire_icon.png";
            _elementTextures[ElementType.Fire] = LoadOrCreateTexture(mageSprite, Colors.Red);

            // Load Warrior weapon (Sword element)
            AlJourney.Scripts.Data.EquipmentData warriorWeapon = InventoryManager.Instance?.GetEquippedItem(CharacterClass.Warrior, EquipmentSlot.Weapon);
            string warriorSprite = warriorWeapon?.Id != null ? $"res://Resources/Sprites/Elements/{warriorWeapon.Id}_sprite.png" : "res://Resources/Sprites/Elements/sword_icon.png";
            if (warriorWeapon?.Id == "sword") warriorSprite = "res://Resources/Sprites/Elements/sword_icon.png";
            if (!ResourceLoader.Exists(warriorSprite)) warriorSprite = "res://Resources/Sprites/Elements/sword_icon.png";
            _elementTextures[ElementType.Sword] = LoadOrCreateTexture(warriorSprite, Colors.Orange);
        }

        public void RefreshTextures()
        {
            LoadElementTextures();
            foreach (var kvp in _spriteMap)
            {
                kvp.Value.SetTexture(_elementTextures[kvp.Key.Type]);
            }
        }

        private static Texture2D LoadOrCreateTexture(string path, Color fallbackColor)
        {
            if (ResourceLoader.Exists(path))
            {
                return GD.Load<Texture2D>(path);
            }

            Image image = Image.CreateEmpty(64, 64, false, Image.Format.Rgba8);
            image.Fill(fallbackColor);
            return ImageTexture.CreateFromImage(image);
        }

        private void OnGridInitialized()
        {
            CreateVisualGrid();
        }

        private void CreateVisualGrid()
        {
            SyncVisualGridFromLogicalGrid(animateElements: false);
        }

        private void OnElementClicked(ElementSprite clickedElement)
        {
            if (!CanInteract)
            {
                return;
            }

            PlayerCharacter hero = _heroSystem.GetHeroForElement(clickedElement.Data.Type);

            if (_selectedElement == null)
            {
                SelectElement(clickedElement);
                return;
            }

            if (clickedElement == _selectedElement)
            {
                DeselectCurrentElement();
                return;
            }

            HandleElementSwap(clickedElement);
        }

        private void SelectElement(ElementSprite element)
        {
            _selectedElement = element;
            _selectedElement.SetHighlight(true);
        }

        private void DeselectCurrentElement()
        {
            _selectedElement?.SetHighlight(false);
            _selectedElement = null;
        }

        private void HandleElementSwap(ElementSprite clickedElement)
        {
            int deltaX = Mathf.Abs(clickedElement.GridX - _selectedElement.GridX);
            int deltaY = Mathf.Abs(clickedElement.GridY - _selectedElement.GridY);

            if ((deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1))
            {
                ProcessSwapAction(clickedElement);
                return;
            }

            DeselectCurrentElement();
            SelectElement(clickedElement);
        }

        private void ProcessSwapAction(ElementSprite clickedElement)
        {
            int fromX = _selectedElement.GridX;
            int fromY = _selectedElement.GridY;
            int toX = clickedElement.GridX;
            int toY = clickedElement.GridY;

            bool swapSuccessful = _gridManager.TrySwap(fromX, fromY, toX, toY);
            if (swapSuccessful)
            {
                AnimateSwap(_selectedElement, clickedElement, fromX, fromY, toX, toY);
            }
            else
            {
                PlayInvalidSwapAnimation(_selectedElement);
                PlayInvalidSwapAnimation(clickedElement);
            }

            DeselectCurrentElement();
        }

        private void AnimateSwap(ElementSprite element1, ElementSprite element2, int fromX, int fromY, int toX, int toY)
        {
            Vector2 pos1 = element1.Position;
            Vector2 pos2 = element2.Position;

            element1.PlaySwapAnimation(pos2);
            element2.PlaySwapAnimation(pos1);

            (_visualGrid[fromX, fromY], _visualGrid[toX, toY]) =
                (_visualGrid[toX, toY], _visualGrid[fromX, fromY]);
        }

        private void PlayInvalidSwapAnimation(ElementSprite element)
        {
            Vector2 originalPos = element.Position;
            Tween tween = CreateTween();
            _ = tween.TweenProperty(element, "position", originalPos + new Vector2(10, 0), 0.05f);
            _ = tween.TweenProperty(element, "position", originalPos - new Vector2(10, 0), 0.05f);
            _ = tween.TweenProperty(element, "position", originalPos, 0.05f);
        }

        private void OnSwapCompleted(bool wasValid)
        {
            if (!wasValid)
            {
                return;
            }

            GD.Print("[GridUI] Swap completed, waiting for BattleManager match processing");
        }

        public void VisualizeMatchesAndEffects(List<MatchResult> matches, List<ComboEffect> effects)
        {
            if (matches == null || effects == null || matches.Count == 0)
            {
                return;
            }

            VisualizeComboEffects(effects, matches);

            int cascadeLevel = _comboSystem.GetCascadeLevel();
            if (cascadeLevel > 0)
            {
                ShowCascadeIndicator(cascadeLevel);
            }

            foreach (MatchResult match in matches)
            {
                foreach ((int x, int y) in match.MatchedPositions)
                {
                    _visualGrid[x, y]?.PlayMatchAnimation();
                    if (_visualGrid[x, y] != null)
                    {
                        _ = _spriteMap.Remove(_visualGrid[x, y].Data);
                    }
                    _visualGrid[x, y] = null;
                }
            }
        }

        private void VisualizeComboEffects(List<ComboEffect> effects, List<MatchResult> matches)
        {
            for (int i = 0; i < effects.Count && i < matches.Count; i++)
            {
                ComboEffect effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                MatchResult match = matches[i];
                Vector2 centerPos = CalculateMatchCenter(match);

                ComboParticles.SpawnComboEffect(this, centerPos, effect.ElementType, effect.ComboLevel);
                FlashMatchedCells(match, effect.ElementType);

                string effectText = GetEffectText(effect);
                Color effectColor = GetEffectColor(effect.ElementType);
                ComboParticles.SpawnFloatingText(this, centerPos - new Vector2(0, 40), effectText, effectColor);
            }
        }

        private static Vector2 CalculateMatchCenter(MatchResult match)
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
                (sumX / match.MatchedPositions.Count) + (CELL_SIZE / 2),
                (sumY / match.MatchedPositions.Count) + (CELL_SIZE / 2)
            );
        }

        private void FlashMatchedCells(MatchResult match, ElementType elementType)
        {
            Color flashColor = GetEffectColor(elementType);

            foreach ((int x, int y) in match.MatchedPositions)
            {
                ElementSprite sprite = _visualGrid[x, y];
                if (sprite == null)
                {
                    continue;
                }

                Tween tween = CreateTween();
                _ = tween.TweenProperty(sprite, "modulate", flashColor * 1.5f, 0.1f);
                _ = tween.TweenProperty(sprite, "modulate", Colors.White, 0.1f);
            }
        }

        private string GetEffectText(ComboEffect effect)
        {
            return effect.ElementType switch
            {
                ElementType.Fire => $"🔥 {effect.Damage} {Tr("UI_DMG_SHORT")}!",
                ElementType.Heal => $"💚 +{effect.Healing} {Tr("UI_HP_SHORT")}!",
                ElementType.Sword => $"⚔️ {effect.Damage} {Tr("UI_DMG_SHORT")}!",
                ElementType.Shield => $"🛡️ +{effect.Shield} {Tr("UI_SHIELD_SHORT")}!",
                _ => Tr("UI_COMBO")
            };
        }

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

        private void OnGridRefilled()
        {
            SyncVisualGridFromLogicalGrid(animateElements: true);
        }

        private void SyncVisualGridFromLogicalGrid(bool animateElements)
        {
            ElementData[,] logicalGrid = _gridManager.GetGrid();
            ElementSprite[,] syncedGrid = new ElementSprite[_gridSize, _gridSize];
            HashSet<ElementSprite> activeSprites = [];

            for (int y = 0; y < _gridSize; y++)
            {
                for (int x = 0; x < _gridSize; x++)
                {
                    ElementData data = logicalGrid[x, y];
                    if (data == null)
                    {
                        continue;
                    }

                    ElementSprite sprite = ProcessGridElement(data, x, y, animateElements);
                    syncedGrid[x, y] = sprite;
                    _ = activeSprites.Add(sprite);
                }
            }

            CleanupOldSprites(activeSprites);
            _visualGrid = syncedGrid;
            UpdateElementVisuals();
        }

        private ElementSprite ProcessGridElement(ElementData data, int x, int y, bool animateElements)
        {
            Vector2 targetPos = GetElementPosition(x, y);

            if (!_spriteMap.TryGetValue(data, out ElementSprite sprite))
            {
                sprite = CreateElementSprite(data);
                _spriteMap[data] = sprite;

                if (animateElements)
                {
                    sprite.SetGridPosition(targetPos - new Vector2(0, 400));
                    sprite.AnimateToPosition(targetPos);
                }
                else
                {
                    sprite.SetGridPosition(targetPos);
                }
            }
            else
            {
                sprite.UpdateData(data);
                sprite.SetTexture(_elementTextures[data.Type]);

                if (animateElements)
                {
                    sprite.AnimateToPosition(targetPos);
                }
                else
                {
                    sprite.SetGridPosition(targetPos);
                }
            }

            return sprite;
        }

        private void CleanupOldSprites(HashSet<ElementSprite> activeSprites)
        {
            List<ElementData> keysToRemove = [];

            foreach (KeyValuePair<ElementData, ElementSprite> kvp in _spriteMap)
            {
                if (!activeSprites.Contains(kvp.Value))
                {
                    kvp.Value.QueueFree();
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (ElementData key in keysToRemove)
            {
                _ = _spriteMap.Remove(key);
            }

            if (_selectedElement != null && !activeSprites.Contains(_selectedElement))
            {
                _selectedElement = null;
            }
        }

        private ElementSprite CreateElementSprite(ElementData data)
        {
            ElementSprite elementSprite = new();
            _gridContainer.AddChild(elementSprite);

            Texture2D texture = _elementTextures[data.Type];
            elementSprite.Initialize(data, texture, CELL_SIZE);
            elementSprite.ElementClicked += OnElementClicked;
            return elementSprite;
        }

        private void ShowCascadeIndicator(int cascadeLevel)
        {
            Vector2 centerPos = new(
                _gridSize * (CELL_SIZE + CELL_SPACING) / 2,
                _gridSize * (CELL_SIZE + CELL_SPACING) / 2
            );

            string cascadeText = $"⚡ {Tr("UI_CASCADE")} x{cascadeLevel}! ⚡";
            Color cascadeColor = new(1.0f, 0.8f, 0.0f);
            ComboParticles.SpawnFloatingText(this, centerPos, cascadeText, cascadeColor);
        }

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

            _heroSystem?.HeroDied -= OnHeroDied;
        }

        private void OnHeroDied(AlJourney.Scripts.Core.CharacterClass heroClass)
        {
            UpdateElementVisuals();
        }

        private void UpdateElementVisuals()
        {
            foreach (ElementSprite sprite in _spriteMap.Values)
            {
                ElementData data = sprite.Data;
                if (data == null)
                {
                    continue;
                }

                PlayerCharacter hero = _heroSystem.GetHeroForElement(data.Type);
                sprite.Modulate = hero?.IsAlive == false ? Colors.Gray : Colors.White;
            }
        }
    }
}
