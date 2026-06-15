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
        /// Возвращает цвет, соответствующий элементу данной способности.
        /// Используется для цветового кодирования в пользовательском интерфейсе.
        /// </summary>
        /// <returns>Цвет элемента для отображения.</returns>
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
        /// Указывает, является ли данная способность атакующей.
        /// </summary>
        public bool IsAttackAbility => Type == AbilityType.Attack;

        /// <summary>
        /// Указывает, является ли данная способность поддерживающей.
        /// </summary>
        public bool IsSupportAbility => Type == AbilityType.Support;

        /// <summary>
        /// Возвращает значение первого эффекта из словаря эффектов способности.
        /// Удобно использовать для способностей, имеющих только один числовой показатель.
        /// </summary>
        /// <returns>Значение основного эффекта или 0, если эффектов нет.</returns>
        public int GetPrimaryEffect()
        {
            return Effects.Values.FirstOrDefault();
        }

        /// <summary>
        /// Возвращает значение конкретного эффекта по его названию.
        /// </summary>
        /// <param name="effectName">Название эффекта.</param>
        /// <returns>Числовое значение эффекта, если он найден, иначе 0.</returns>
        public int GetEffect(string effectName)
        {
            return Effects.TryGetValue(effectName, out int value) ? value : 0;
        }

        /// <summary>
        /// Возвращает строковое представление способности, включающее её название, тип и элемент.
        /// </summary>
        /// <returns>Строка в формате "Название".</returns>
        public override string ToString()
        {
            return $"{Name} ({Type} - {Element})";
        }
    }
}
