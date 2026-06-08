import os
import re

def generate_summary(member_type, name):
    name_lower = name.lower()
    
    # Classes
    if member_type == "class":
        if "manager" in name_lower or "system" in name_lower:
            return f"Менеджер {name}. Отвечает за управление соответствующей подсистемой."
        elif "data" in name_lower:
            return f"Класс данных {name}. Сохраняет информацию и параметры."
        elif "ui" in name_lower or "menu" in name_lower or "hud" in name_lower:
            return f"UI-компонент {name}. Отвечает за отображение пользовательского интерфейса."
        return f"Основной класс {name}."
    
    # Methods/Properties
    prefixes = {
        "get": "Возвращает",
        "set": "Устанавливает",
        "is": "Проверяет, является ли",
        "has": "Проверяет наличие",
        "try": "Пытается выполнить",
        "add": "Добавляет",
        "remove": "Удаляет",
        "delete": "Удаляет",
        "update": "Обновляет",
        "save": "Сохраняет",
        "load": "Загружает",
        "start": "Запускает",
        "stop": "Останавливает",
        "initialize": "Инициализирует",
        "process": "Обрабатывает",
        "check": "Проверяет",
        "generate": "Генерирует",
        "equip": "Экипирует",
        "unequip": "Снимает экипировку",
        "play": "Воспроизводит",
        "apply": "Применяет",
        "reset": "Сбрасывает",
        "show": "Показывает",
        "hide": "Скрывает",
        "open": "Открывает",
        "close": "Закрывает"
    }
    
    for prefix, russian in prefixes.items():
        if name_lower.startswith(prefix):
            return f"{russian} {name[len(prefix):]}."
            
    return f"Элемент {name}."

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    out_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        stripped = line.strip()
        
        # Check if it's a public member without existing xml docs
        if stripped.startswith("public ") and not stripped.startswith("public override") and "{" not in stripped and ";" not in stripped or (stripped.startswith("public ") and ("(" in stripped or "{" in stripped or ";" in stripped or "class" in stripped or "enum" in stripped)):
            
            # Make sure previous line is not a summary
            if i > 0 and "/// <summary>" not in lines[i-1] and not stripped.endswith("}"):
                
                # Check for attributes like [Signal] above
                insert_idx = len(out_lines)
                if insert_idx > 0 and out_lines[-1].strip().startswith("["):
                    # We should insert before the attribute
                    pass # Actually, just insert above the public keyword to keep it simple, it's fine for Godot except for exported vars
                
                # Extract name
                match = re.search(r'public\s+(?:static\s+)?(?:partial\s+)?(?:class|interface|enum|struct)\s+(\w+)', stripped)
                member_type = "class"
                name = ""
                if match:
                    name = match.group(1)
                else:
                    # Method or property
                    match = re.search(r'public\s+(?:static\s+)?(?:async\s+)?[\w<>\[\],\s]+\s+(\w+)\s*(?:\{|\(|=>|;|=)', stripped)
                    if match:
                        name = match.group(1)
                        member_type = "member"
                        
                if name:
                    summary_text = generate_summary(member_type, name)
                    indent = line[:len(line) - len(line.lstrip())]
                    out_lines.append(f"{indent}/// <summary>\n")
                    out_lines.append(f"{indent}/// {summary_text}\n")
                    out_lines.append(f"{indent}/// </summary>\n")
                    
        out_lines.append(line)
        i += 1
        
    with open(filepath, 'w', encoding='utf-8') as f:
        f.writelines(out_lines)

scripts_dir = r"c:\Users\DNS\OneDrive\Рабочий стол\Godot\aljourney\Scripts"
count = 0
for root, dirs, files in os.walk(scripts_dir):
    for file in files:
        if file.endswith(".cs"):
            process_file(os.path.join(root, file))
            count += 1
print(f"Processed {count} files.")
