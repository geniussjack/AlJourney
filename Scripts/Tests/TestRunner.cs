using Chickensoft.GoDotTest;
using Godot;
using System.Reflection;

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
