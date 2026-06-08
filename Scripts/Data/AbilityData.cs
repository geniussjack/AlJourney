using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Структура данных, представляющая способность персонажа.
    /// Хранит идентификатор, название, тип, элемент, путь к иконке, описание, стоимость разблокировки и эффекты способности.
    /// </summary>
    public record AbilityData(
        string Id,
        string Name,
        AbilityType Type,
        AbilityElement Element,
        string IconPath,
        string Description,
        int UnlockCost,
        Dictionary<string, int> Effects
    )
    {
        /// <summary>
        /// Возвращает цвет, соответствующий элементу данной способности (Огонь, Лечение, Меч, Щит).
        /// Используется для цветового кодирования в пользовательском интерфейсе.
        /// </summary>
        /// <returns>Цвет элемента (Color) для отображения.</returns>
        public Color GetElementColor()
        {
            return Element switch
            {
                AbilityElement.Fire => Colors.Orange,
                AbilityElement.Heal => Colors.Green,
                AbilityElement.Sword => Colors.Red,
                AbilityElement.Shield => Colors.Blue,
                _ => Colors.White
            };
        }

        /// <summary>
        /// Указывает, является ли данная способность атакующей (наносящей урон).
        /// </summary>
        public bool IsAttackAbility => Type == AbilityType.Attack;

        /// <summary>
        /// Указывает, является ли данная способность поддерживающей (например, лечением или защитой).
        /// </summary>
        public bool IsSupportAbility => Type == AbilityType.Support;

        /// <summary>
        /// Возвращает значение первого эффекта из словаря эффектов способности.
        /// Удобно использовать для способностей, имеющих только один числовой показатель (например, базовый урон или лечение).
        /// </summary>
        /// <returns>Значение основного эффекта или 0, если эффектов нет.</returns>
        public int GetPrimaryEffect()
        {
            return Effects.Values.FirstOrDefault();
        }

        /// <summary>
        /// Возвращает значение конкретного эффекта по его названию.
        /// </summary>
        /// <param name="effectName">Название эффекта (например, "damage", "healing").</param>
        /// <returns>Числовое значение эффекта, если он найден, иначе 0.</returns>
        public int GetEffect(string effectName)
        {
            return Effects.TryGetValue(effectName, out int value) ? value : 0;
        }

        /// <summary>
        /// Возвращает строковое представление способности, включающее её название, тип и элемент.
        /// </summary>
        /// <returns>Строка в формате "Название (Тип - Элемент)".</returns>
        public override string ToString()
        {
            return $"{Name} ({Type} - {Element})";
        }
    }
}
