using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Описывает один спавн внутри волны: тип врага и размер стека (см. <see cref="Characters.Enemy.Create"/>).
    /// </summary>
    /// <param name="Type">Тип врага.</param>
    /// <param name="Count">Количество существ в стеке.</param>
    public record EnemySpawnDefinition(EnemyType Type, int Count = 1);

    /// <summary>
    /// Описывает один заход волны внутри уровня — набор спавнов, появляющихся одновременно.
    /// </summary>
    /// <param name="Enemies">Список спавнов данной волны.</param>
    public record WaveDefinition(IReadOnlyList<EnemySpawnDefinition> Enemies);

    /// <summary>
    /// Описывает один уровень карты кампании: локацию, позицию внутри неё, условие разблокировки
    /// и курируемую (заранее определённую) последовательность волн, которые идут подряд без выхода
    /// из боя в рамках одной попытки прохождения уровня.
    /// </summary>
    /// <param name="Id">Уникальный идентификатор уровня.</param>
    /// <param name="Location">Локация, к которой относится уровень.</param>
    /// <param name="OrderInLocation">Порядковый номер уровня внутри локации (для отображения на карте).</param>
    /// <param name="Waves">Курируемая последовательность волн уровня.</param>
    /// <param name="DifficultyRating">
    /// Числовая сложность уровня, используемая вместо номера волны как вход для
    /// <see cref="Core.ScalingSystem"/> (масштабирование характеристик врагов, наград и цен).
    /// </param>
    /// <param name="IsBranch">
    /// Истина, если уровень — необязательное ответвление от основной линии (источник ресурсов
    /// и, в будущем, катализаторов редкости), а не часть обязательной линейной цепочки к некроманту.
    /// </param>
    /// <param name="RequiredLevelId">
    /// Id уровня, который должен быть пройден для разблокировки этого уровня, либо <c>null</c>
    /// для самого первого уровня кампании.
    /// </param>
    public record LevelDefinition(
        string Id,
        LocationId Location,
        int OrderInLocation,
        IReadOnlyList<WaveDefinition> Waves,
        int DifficultyRating,
        bool IsBranch = false,
        string RequiredLevelId = null
    );
}
