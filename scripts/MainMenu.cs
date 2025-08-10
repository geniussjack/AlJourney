using Godot;
using System;

public partial class MainMenu : Node2D
{
	private Global global;
	private Camera2D camera;
	private Node2D levelsContainer;
	private Button settingsButton;
	private Button upgradesButton;
	private Vector2 dragStartPos;
	private bool isDragging;

	// Размер карты (адаптируй под спрайт художника)
	private const float MapWidth = 4096f;
	private const float MapHeight = 2048f;

	// Фиксированные позиции иконок уровней (индекс = levelId - 1 для основных, доп для ветвей)
	private Vector2[] levelPositions = new Vector2[100]; // Достаточно для 50 + ветви

	public override void _Ready()
	{
		global = GetNode<Global>("/root/Global");
		camera = GetNode<Camera2D>("Camera2D");
		levelsContainer = GetNode<Node2D>("LevelsContainer");
		settingsButton = GetNode<Button>("CanvasLayer/UI/SettingsButton");
		upgradesButton = GetNode<Button>("CanvasLayer/UI/UpgradesButton");

		InitializePositions(); // Фиксированные координаты
		GenerateLevelIcons();

		settingsButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/Settings.tscn");
		upgradesButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/Upgrades.tscn");

		// Музыка
		var music = GetNode<AudioStreamPlayer>("MusicPlayer");
		music.VolumeDb = Mathf.LinearToDb(global.MusicVolume);
		music.Play();
	}

	public override void _Input(InputEvent @event)
	{
		// Drag камеры (для мобильного)
		if (@event is InputEventScreenTouch touch && touch.Pressed)
		{
			dragStartPos = touch.Position;
			isDragging = true;
		}
		else if (@event is InputEventScreenDrag drag)
		{
			if (isDragging)
			{
				var delta = drag.Position - dragStartPos;
				camera.Position -= delta / camera.Zoom;
				dragStartPos = drag.Position;
				camera.Position = new Vector2(
					Mathf.Clamp(camera.Position.X, 0, MapWidth),
					Mathf.Clamp(camera.Position.Y, 0, MapHeight)
				);
			}
		}
		else if (@event is InputEventScreenTouch touchEnd && !touchEnd.Pressed)
		{
			isDragging = false;
		}
	}

	private void InitializePositions()
	{
		// Пример: Линейная дорога по X, ветви смещены по Y
		for (int i = 1; i <= global.MaxLevels; i++)
		{
			levelPositions[i] = new Vector2(i * 80, 1024); // Основная дорога по центру Y=1024
		}
		// Ветви: Для 5 -> 51-52 ->6
		levelPositions[51] = new Vector2(5 * 80 + 40, 1024 + 200); // Ниже
		levelPositions[52] = new Vector2(5 * 80 + 120, 1024 + 200);
		// Для 10 ->53 ->11
		levelPositions[53] = new Vector2(10 * 80 + 40, 1024 - 200); // Выше
		// Для 10 ->54-55 ->11
		levelPositions[54] = new Vector2(10 * 80 + 40, 1024 + 200);
		levelPositions[55] = new Vector2(10 * 80 + 120, 1024 + 200);
		// Добавь позиции для остальных ветвей аналогично
	}

	private void GenerateLevelIcons()
	{
		foreach (var level in global.LevelTree.Keys)
		{
			Button icon = new Button();
			icon.Text = $"{level}";
			icon.Position = levelPositions[level]; // Фиксированная позиция
			icon.Size = new Vector2(64, 64);
			icon.Theme = GD.Load<Theme>("res://pixel_theme.tres");
			if (global.UnlockedLevels.Contains(level))
			{
				icon.Disabled = false;
				icon.Pressed += () => StartLevel(level);
			}
			else
			{
				icon.Disabled = true;
				icon.Modulate = new Color(0.5f, 0.5f, 0.5f);
			}
			levelsContainer.AddChild(icon);
		}
	}

	private void StartLevel(int level)
	{
		global.CurrentLevel = level;
		GetTree().ChangeSceneToFile("res://scenes/Level.tscn");
	}
}
