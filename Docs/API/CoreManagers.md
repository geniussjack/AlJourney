# API и Интерфейсы Систем

В проекте используется набор интерфейсов для гарантии слабого связывания. Ниже описаны контракты основных систем.

## IGameStateManager
Отвечает за глобальное состояние сессии.
* `void StartNewGame()` — инициализирует новую игру и сбрасывает волну.
* `void LoadGame(SaveData)` — загружает прогресс из файла.
* `void ChangeState(GameState)` — переключает состояние игры (Меню, Бой, и т.д.).
* `bool SpendCoins(int)` — позволяет безопасно списывать валюту.

## IGridManager
Интерфейс для поля Match-3.
* `void InitializeGrid()` — генерирует безопасное поле 5x5 без изначальных совпадений.
* `bool TrySwap(int x1, int y1, int x2, int y2)` — пытается поменять местами два элемента.
* `List<MatchResult> FindAllMatches()` — возвращает список всех текущих совпадений на поле.

## IInventoryManager
* `void AddItems(List<EquipmentData>)` — добавляет предметы в инвентарь игрока.
* `bool EquipItem(CharacterClass, EquipmentData)` — надевает предмет на выбранного героя.

## ILootSystem
* `EquipmentData GenerateNormalLoot(int waveNumber)` — генерирует лут для обычного врага с учетом текущей волны.
* `List<EquipmentData> GenerateBossLoot(int waveNumber)` — генерирует гарантированный ценный лут после убийства босса.

## ISaveSystem
* `bool SaveGame()` — сохраняет текущий `SaveData` в JSON/бинарный файл.
* `SaveData LoadGame()` — десериализует файл сохранения и возвращает объект данных.
