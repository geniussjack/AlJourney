using Godot;

public partial class Level : Node2D
{
	private Global global;
	private Button completeButton; // Для теста

	public override void _Ready()
	{
		global = GetNode<Global>("/root/Global");
		GetNode<Label>("LevelLabel").Text = $"Уровень {global.CurrentLevel}";

		completeButton = GetNode<Button>("CompleteButton");
		completeButton.Pressed += OnComplete;

		// Здесь механика "три в ряд" от коллеги
		// Используй global.EquippedSpell и EquippedEquipment в бою
	}

	private void OnComplete()
	{
		global.AddExperience(50); // Пример опыта
		global.CompleteLevel(global.CurrentLevel);
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}
}
