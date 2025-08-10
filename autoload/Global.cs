using Godot;
using System.Collections.Generic;
using System.IO;

public partial class Global : Node
{
	// Прогресс
	public int PlayerLevel { get; set; } = 1; // Общий уровень (повышается опытом)
	public int Experience { get; set; } = 0; // Текущий опыт
	public int ExpToNextLevel { get; set; } = 100; // Пример: опыт для следующего уровня (увеличивай по формуле)
	
	// Уровни: Дерево с ветвями (ключ — ID уровня, значение — список следующих уровней)
	public Dictionary<int, List<int>> LevelTree { get; } = new Dictionary<int, List<int>>
	{
		{1, new List<int>{2, 3}}, // Уровень 1 ведёт к 2 и 3
		{2, new List<int>{4}},    // 2 ведёт к 4
		{3, new List<int>{4, 5}}, // 3 ведёт к 4 и 5
		// ... Добавь до 300 уровней с ветвями (генерируй procedural или вручную)
		// Пример: Для 300 уровней сделай ветви каждые 10 уровней
	};
	public List<int> UnlockedLevels { get; } = new List<int>(); // Разблокированные
	public List<int> CompletedLevels { get; } = new List<int>(); // Пройденные
	public int CurrentLevel { get; set; } = 1; // Текущий выбранный

	// Прокачка: Списки unlocked вещей (падают с боссов)
	public List<string> UnlockedSpells { get; } = new List<string>(); // Чары для мага (e.g., "Fireball", "Heal")
	public List<string> UnlockedEquipment { get; } = new List<string>(); // Снаряжение для воина (e.g., "Sword", "Shield")
	public string EquippedSpell { get; set; } = ""; // Выбранная чара
	public string EquippedEquipment { get; set; } = ""; // Выбранное снаряжение

	// Настройки (звук, музыка)
	public float MusicVolume { get; set; } = 1.0f;
	public float SoundVolume { get; set; } = 1.0f;

	public override void _Ready()
	{
		LoadProgress(); // Загрузка при старте
		if (UnlockedLevels.Count == 0)
		{
			UnlockedLevels.Add(1); // Первый уровень всегда unlocked
		}
	}

	public void AddExperience(int amount)
	{
		Experience += amount;
		while (Experience >= ExpToNextLevel)
		{
			PlayerLevel++;
			Experience -= ExpToNextLevel;
			ExpToNextLevel = (int)(ExpToNextLevel * 1.2f); // Пример роста
			// Улучшения от уровня (например, базовый дамаг +)
		}
		SaveProgress();
	}

	public void CompleteLevel(int level)
	{
		if (!CompletedLevels.Contains(level))
		{
			CompletedLevels.Add(level);
			if (LevelTree.ContainsKey(level))
			{
				foreach (var next in LevelTree[level])
				{
					if (!UnlockedLevels.Contains(next))
					{
						UnlockedLevels.Add(next);
					}
				}
			}
			// Пример: С босса падает вещь
			if (level % 10 == 0) // Каждый 10-й — босс, чара для мага
			{
				UnlockedSpells.Add($"Spell_{level}");
			}
			else if (level % 5 == 0) // Минибосс, снаряжение
			{
				UnlockedEquipment.Add($"Equip_{level}");
			}
			SaveProgress();
		}
	}

	public int MaxLevels { get; } = 50; // Уменьшили до 50

	// Фиксированное дерево уровней с ветвями
	public Dictionary<int, List<int>> LevelTree { get; } = new Dictionary<int, List<int>>
	{
		{1, new List<int>{2}},
		{2, new List<int>{3}},
		{3, new List<int>{4}},
		{4, new List<int>{5}},
		{5, new List<int>{6, 51}}, // Ветвь: 51 - альтернативный путь (допустим, 51-52-6)
		{6, new List<int>{7}},
		// Ветвь от 5
		{51, new List<int>{52}},
		{52, new List<int>{6}}, // Сходится обратно в 6
		{7, new List<int>{8}},
		{8, new List<int>{9}},
		{9, new List<int>{10}},
		{10, new List<int>{11, 53, 54}}, // Ветвь на 2 пути: 53-11, 54-55-11
		{11, new List<int>{12}},
		// Ветви от 10
		{53, new List<int>{11}},
		{54, new List<int>{55}},
		{55, new List<int>{11}},
		// Продолжай так до 50, добавляя ветви каждые 5-10 уровней
		// Пример для конца:
		{49, new List<int>{50}},
		{50, new List<int>{}} // Конец
	};

	public void UnlockItem(string item, bool isSpell)
	{
		if (isSpell && !UnlockedSpells.Contains(item)) UnlockedSpells.Add(item);
		else if (!UnlockedEquipment.Contains(item)) UnlockedEquipment.Add(item);
		SaveProgress();
	}

	// Сохранение/Загрузка (локально в JSON)
	private void SaveProgress()
	{
		var data = new Godot.Collections.Dictionary
		{
			{"PlayerLevel", PlayerLevel},
			{"Experience", Experience},
			{"ExpToNextLevel", ExpToNextLevel},
			{"UnlockedLevels", new Godot.Collections.Array(UnlockedLevels)},
			{"CompletedLevels", new Godot.Collections.Array(CompletedLevels)},
			{"UnlockedSpells", new Godot.Collections.Array(UnlockedSpells)},
			{"UnlockedEquipment", new Godot.Collections.Array(UnlockedEquipment)},
			{"EquippedSpell", EquippedSpell},
			{"EquippedEquipment", EquippedEquipment},
			{"MusicVolume", MusicVolume},
			{"SoundVolume", SoundVolume}
		};
		var json = Json.Stringify(data);
		FileAccess file = FileAccess.Open("user://save.json", FileAccess.ModeFlags.Write);
		file.StoreString(json);
		file.Close();

		// Для Google Play: Интегрируй Godot Google Play Services плагин (скачай с AssetLib).
		// Затем используй Cloud Save: GetNode<GooglePlayServices>("/root/GPS").SaveGame("save.json", json);
		// Но сначала настрой плагин в проекте и Android export.
	}

	private void LoadProgress()
	{
		if (FileAccess.FileExists("user://save.json"))
		{
			FileAccess file = FileAccess.Open("user://save.json", FileAccess.ModeFlags.Read);
			var json = file.GetAsText();
			file.Close();
			var data = Json.ParseString(json).AsGodotDictionary();
			PlayerLevel = data["PlayerLevel"].AsInt32();
			Experience = data["Experience"].AsInt32();
			ExpToNextLevel = data["ExpToNextLevel"].AsInt32();
			UnlockedLevels.Clear(); UnlockedLevels.AddRange(data["UnlockedLevels"].AsGodotArray().ToIntList());
			CompletedLevels.Clear(); CompletedLevels.AddRange(data["CompletedLevels"].AsGodotArray().ToIntList());
			UnlockedSpells.Clear(); UnlockedSpells.AddRange(data["UnlockedSpells"].AsGodotArray().ToStringList());
			UnlockedEquipment.Clear(); UnlockedEquipment.AddRange(data["UnlockedEquipment"].AsGodotArray().ToStringList());
			EquippedSpell = data["EquippedSpell"].AsString();
			EquippedEquipment = data["EquippedEquipment"].AsString();
			MusicVolume = data["MusicVolume"].AsSingle();
			SoundVolume = data["SoundVolume"].AsSingle();
		}
		// Для Google Play: Загружай из облака аналогично.
	}
}

// Расширение для конвертации
public static class Extensions
{
	public static List<int> ToIntList(this Godot.Collections.Array array)
	{
		var list = new List<int>();
		foreach (var item in array) list.Add(item.AsInt32());
		return list;
	}
	public static List<string> ToStringList(this Godot.Collections.Array array)
	{
		var list = new List<string>();
		foreach (var item in array) list.Add(item.AsString());
		return list;
	}
}
