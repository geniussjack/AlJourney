using Godot;
public partial class TestScript : Node {
    public override void _Ready() {
        int x = GD.RandRange(0, 10);
        GD.Print(x);
    }
}
