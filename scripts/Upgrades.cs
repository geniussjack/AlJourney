using Godot;

public partial class Upgrades : Control
{
	private Global global;
	private VBoxContainer spellsContainer;
	private VBoxContainer equipContainer;
	private Label charInfo;
	private Button backButton;

	public override void _Ready()
	{
		global = GetNode<Global>("/root/Global");
		spellsContainer = GetNode<VBoxContainer>("TabContainer/Чары/VBoxContainer");
		equipContainer = GetNode<VBoxContainer>("TabContainer/Снаряжение/VBoxContainer");
		charInfo = GetNode<Label>("TabContainer/Персонажи/Label");
		backButton = GetNode<Button>("BackButton");

		UpdateUI();
		backButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}

	private void UpdateUI()
	{
		// Чары
		spellsContainer.QueueFreeChildren();
		foreach (var spell in global.UnlockedSpells)
		{
			Button btn = new Button { Text = spell };
			btn.Pressed += () => { global.EquippedSpell = spell; GD.Print($"Equipped {spell}"); global.SaveProgress(); };
			spellsContainer.AddChild(btn);
		}

		// Снаряжение
		equipContainer.QueueFreeChildren();
		foreach (var equip in global.UnlockedEquipment)
		{
			Button btn = new Button { Text = equip };
			btn.Pressed += () => { global.EquippedEquipment = equip; GD.Print($"Equipped {equip}"); global.SaveProgress(); };
			equipContainer.AddChild(btn);
		}

		// Персонажи
		charInfo.Text = $"Уровень: {global.PlayerLevel}\nОпыт: {global.Experience}/{global.ExpToNextLevel}";
	}
}
