import os
import re

managers_dir = r"c:\Users\DNS\OneDrive\Рабочий стол\Godot\aljourney\Scripts\Managers"
for filename in os.listdir(managers_dir):
    if filename.endswith(".cs"):
        filepath = os.path.join(managers_dir, filename)
        with open(filepath, "r", encoding="utf-8") as f:
            content = f.read()
        
        name = filename[:-3]
        
        if "using AlJourney.Scripts.Interfaces;" not in content:
            content = content.replace("using Godot;", "using Godot;\nusing AlJourney.Scripts.Interfaces;")
            
        pattern = r"public partial class " + name + r" : Node(?!\s*,)"
        content = re.sub(pattern, f"public partial class {name} : Node, I{name}", content)
        
        with open(filepath, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"Updated {name}")

for file_info in [
    (r"c:\Users\DNS\OneDrive\Рабочий стол\Godot\aljourney\Scripts\Match3\GridManager.cs", "GridManager"),
    (r"c:\Users\DNS\OneDrive\Рабочий стол\Godot\aljourney\Scripts\Battle\BattleManager.cs", "BattleManager")
]:
    filepath, name = file_info
    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()
    
    if "using AlJourney.Scripts.Interfaces;" not in content:
        content = content.replace("using Godot;", "using Godot;\nusing AlJourney.Scripts.Interfaces;")
        
    pattern = r"public partial class " + name + r" : Node(?!\s*,)"
    content = re.sub(pattern, f"public partial class {name} : Node, I{name}", content)
    
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Updated {name}")
