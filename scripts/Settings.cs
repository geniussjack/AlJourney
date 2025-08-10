using Godot;

public partial class Settings : Control
{
	private Global global;
	private HSlider musicSlider;
	private HSlider soundSlider;
	private Button backButton;

	public override void _Ready()
	{
		global = GetNode<Global>("/root/Global");
		musicSlider = GetNode<HSlider>("VBoxContainer/MusicSlider");
		soundSlider = GetNode<HSlider>("VBoxContainer/SoundSlider");
		backButton = GetNode<Button>("VBoxContainer/BackButton");

		musicSlider.Value = global.MusicVolume;
		soundSlider.Value = global.SoundVolume;

		musicSlider.ValueChanged += (value) => { global.MusicVolume = (float)value; global.SaveProgress(); };
		soundSlider.ValueChanged += (value) => { global.SoundVolume = (float)value; global.SaveProgress(); };
		backButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}
}
