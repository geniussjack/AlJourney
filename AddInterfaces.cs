using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string[] files = Directory.GetFiles(@"c:\Users\DNS\OneDrive\Рабочий стол\Godot\aljourney\Scripts\Managers", "*.cs");
        
        foreach (var file in files)
        {
            string content = File.ReadAllText(file);
            string name = Path.GetFileNameWithoutExtension(file);
            
            if (!content.Contains("using AlJourney.Scripts.Interfaces;"))
            {
                content = content.Replace("using Godot;", "using Godot;\nusing AlJourney.Scripts.Interfaces;");
            }

            string pattern = $@"public partial class {name} : Node(?!\s*,)";
            if (Regex.IsMatch(content, pattern))
            {
                content = Regex.Replace(content, pattern, $"public partial class {name} : Node, I{name}");
                File.WriteAllText(file, content);
                Console.WriteLine($"Updated {name}");
            }
        }
        
        string[] match3Files = Directory.GetFiles(@"c:\Users\DNS\OneDrive\Рабочий стол\Godot\aljourney\Scripts\Match3", "GridManager.cs");
        foreach(var file in match3Files)
        {
            string content = File.ReadAllText(file);
            if (!content.Contains("using AlJourney.Scripts.Interfaces;"))
            {
                content = content.Replace("using Godot;", "using Godot;\nusing AlJourney.Scripts.Interfaces;");
            }
            content = Regex.Replace(content, @"public partial class GridManager : Node(?!\s*,)", "public partial class GridManager : Node, IGridManager");
            File.WriteAllText(file, content);
        }
        
        string[] battleFiles = Directory.GetFiles(@"c:\Users\DNS\OneDrive\Рабочий стол\Godot\aljourney\Scripts\Battle", "BattleManager.cs");
        foreach(var file in battleFiles)
        {
            string content = File.ReadAllText(file);
            if (!content.Contains("using AlJourney.Scripts.Interfaces;"))
            {
                content = content.Replace("using Godot;", "using Godot;\nusing AlJourney.Scripts.Interfaces;");
            }
            content = Regex.Replace(content, @"public partial class BattleManager : Node(?!\s*,)", "public partial class BattleManager : Node, IBattleManager");
            File.WriteAllText(file, content);
        }
    }
}
