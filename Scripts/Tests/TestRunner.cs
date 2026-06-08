using System.Reflection;
using Godot;
using Chickensoft.GoDotTest;

namespace AlJourney.Tests
{
    public partial class TestRunner : Node
    {
        public override void _Ready()
        {
            _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this);
            GetTree().Quit();
        }
    }
}
