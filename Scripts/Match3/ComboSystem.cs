using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Представляет эффект, возникающий при сборе комбинации элементов.
    /// Хранит информацию о типе элемента, уровне комбо и результирующих значениях урона, лечения, защиты,
    /// а также о накладываемых статусных эффектах и области действия.
    /// </summary>
    public class ComboEffect(ElementType elementType, int comboLevel)
    {
        /// <summary>
        /// Тип элемента, из которого было собрано комбо.
        /// </summary>
        public ElementType ElementType { get; set; } = elementType;

        /// <summary>
        /// Уровень комбо, который зависит от количества собранных элементов.
        /// </summary>
        public int ComboLevel { get; set; } = comboLevel;

        /// <summary>
        /// Количество урона, которое нанесет данное комбо противнику.
        /// </summary>
        public int Damage { get; set; }

        /// <summary>
        /// Количество очков здоровья, которое восстановит данное комбо союзникам.
        /// </summary>
        public int Healing { get; set; }

        /// <summary>
        /// Количество очков щита, которое данное комбо наложит на союзников.
        /// </summary>
        public int Shield { get; set; }

        /// <summary>
        /// Указывает, применяется ли эффект данного комбо по площади.
        /// </summary>
        public bool IsAoE { get; set; }

        /// <summary>
        /// Данные о дополнительном статусном эффекте,
        /// который накладывается в результате этого комбо.
        /// </summary>
        public StatusEffectData StatusEffect { get; set; }
    }

    /// <summary>
    /// Система управления комбинациями элементов.
    /// Отвечает за преобразование собранных на игровом поле линий в игровые эффекты,
    /// а также за отслеживание и начисление бонусов за каскадные совпадения.
    /// </summary>
    public partial class ComboSystem : Node
    {
        [Signal]
        /// <summary>
        /// Событие, которое вызывается после завершения обработки комбо-эффектов.
        /// Передает общее количество успешно обработанных комбинаций.
        /// </summary>
        public delegate void CombosProcessedEventHandler(int comboCount);

        [Signal]
        /// <summary>
        /// Событие, которое вызывается при обнаружении каскадного совпадения.
        /// Передает текущий уровень каскада.
        /// </summary>
        public delegate void CascadeDetectedEventHandler(int cascadeLevel);

        private List<ComboEffect> _lastProcessedEffects = [];
        private int _currentCascadeLevel;

        /// <summary>
        /// Возвращает список эффектов, полученных после последней обработки комбинаций элементов.
        /// Используется для передачи данных об эффектах в боевую систему.
        /// </summary>
        public List<ComboEffect> GetLastProcessedEffects()
        {
            return _lastProcessedEffects;
        }

        /// <summary>
        /// Обрабатывает список собранных комбинаций и превращает их в список боевых эффектов.
        /// При наличии каскада увеличивает текущий уровень каскада и применяет бонусы к эффектам.
        /// </summary>
        /// <param name="matches">Список результатов совпадений, собранных на поле.</param>
        /// <param name="isCascade">Указывает, является ли текущая обработка частью цепной реакции.</param>
        /// <returns>Список сгенерированных комбо-эффектов со всеми примененными бонусами.</returns>
        public List<ComboEffect> ProcessMatches(List<MatchResult> matches, bool isCascade = false)
        {
            if (isCascade)
            {
                _currentCascadeLevel++;
                _ = EmitSignal(SignalName.CascadeDetected, _currentCascadeLevel);
                GD.Print($"[ComboSystem] Cascade detected! Level: {_currentCascadeLevel}");
            }
            else
            {
                _currentCascadeLevel = 0;
            }

            List<ComboEffect> comboEffects = [];

            foreach (MatchResult match in matches)
            {
                ComboEffect effect = CreateComboEffect(match);
                if (effect != null)
                {
                    if (_currentCascadeLevel > 0)
                    {
                        float cascadeBonus = 1.0f + (_currentCascadeLevel * 0.2f);
                        effect.Damage = Mathf.CeilToInt(effect.Damage * cascadeBonus);
                        effect.Healing = Mathf.CeilToInt(effect.Healing * cascadeBonus);
                        effect.Shield = Mathf.CeilToInt(effect.Shield * cascadeBonus);

                        GD.Print($"[ComboSystem] Cascade bonus applied: x{cascadeBonus:F1}");
                    }

                    comboEffects.Add(effect);
                }
            }

            if (comboEffects.Count > 0)
            {
                _lastProcessedEffects = comboEffects;
                _ = EmitSignal(SignalName.CombosProcessed, comboEffects.Count);
                GD.Print($"[ComboSystem] Processed {comboEffects.Count} combo effects");
            }

            return comboEffects;
        }

        /// <summary>
        /// Возвращает текущий уровень каскадных совпадений.
        /// Чем выше уровень, тем больше бонусный множитель применяется к эффектам.
        /// </summary>
        public int GetCascadeLevel()
        {
            return _currentCascadeLevel;
        }

        /// <summary>
        /// Сбрасывает текущий уровень каскада до нуля.
        /// Вызывается перед началом нового хода игрока, чтобы обнулить бонусный множитель.
        /// </summary>
        public void ResetCascade()
        {
            _currentCascadeLevel = 0;
        }

        private static ComboEffect CreateComboEffect(MatchResult match)
        {
            int comboLevel = match.GetComboLevel();
            if (comboLevel == 0)
            {
                return null;
            }

            ComboEffect effect = new(match.ElementType, comboLevel);

            switch (match.ElementType)
            {
                case ElementType.Fire:
                    ProcessFireCombo(effect, comboLevel);
                    break;

                case ElementType.Sword:
                    ProcessSwordCombo(effect, comboLevel);
                    break;

                case ElementType.Heal:
                    ProcessHealCombo(effect, comboLevel);
                    break;

                case ElementType.Shield:
                    ProcessShieldCombo(effect, comboLevel);
                    break;
            }

            return effect;
        }

        private static void ProcessFireCombo(ComboEffect effect, int level)
        {
            AlJourney.Scripts.Data.EquipmentData weapon = AlJourney.Scripts.Managers.InventoryManager.Instance?.GetEquippedItem(CharacterClass.Mage, EquipmentSlot.Weapon);
            string weaponId = weapon?.Id ?? "fireball";

            StatusEffect effectType = StatusEffect.Burning;
            if (weaponId == "iceball") effectType = StatusEffect.Freeze;
            else if (weaponId == "electroball") effectType = StatusEffect.Shock;

            switch (level)
            {
                case 1:
                    effect.Damage = GameConstants.FIRE_3_DAMAGE;
                    effect.IsAoE = false;
                    break;

                case 2:
                    effect.Damage = GameConstants.FIRE_4_DAMAGE;
                    effect.IsAoE = false;
                    effect.StatusEffect = new Data.StatusEffectData(
                        effectType,
                        GameConstants.FIRE_4_BURN_DURATION,
                        effectType == StatusEffect.Burning ? GameConstants.FIRE_4_BURN_DAMAGE : 0
                    );
                    break;

                case 3:
                    effect.Damage = GameConstants.FIRE_5_DAMAGE;
                    effect.IsAoE = true;
                    effect.StatusEffect = new Data.StatusEffectData(
                        effectType,
                        GameConstants.FIRE_5_BURN_DURATION,
                        effectType == StatusEffect.Burning ? GameConstants.FIRE_5_BURN_DAMAGE : 0
                    );
                    break;
            }

            GD.Print($"[ComboSystem] Mage combo level {level}: {effect.Damage} damage" +
                     (effect.IsAoE ? " (AoE)" : "") +
                     (effect.StatusEffect != null ? $" + {effectType}" : ""));
        }

        private static void ProcessSwordCombo(ComboEffect effect, int level)
        {
            AlJourney.Scripts.Data.EquipmentData weapon = AlJourney.Scripts.Managers.InventoryManager.Instance?.GetEquippedItem(CharacterClass.Warrior, EquipmentSlot.Weapon);
            string weaponId = weapon?.Id ?? "sword";

            StatusEffect effectType = StatusEffect.Stunned; // default level 3 effect
            if (weaponId == "axe") effectType = StatusEffect.Bleeding;
            else if (weaponId == "spear") effectType = StatusEffect.Vulnerable;

            switch (level)
            {
                case 1:
                    effect.Damage = GameConstants.SWORD_3_DAMAGE;
                    effect.IsAoE = false;
                    break;

                case 2:
                    effect.Damage = GameConstants.SWORD_4_DAMAGE;
                    effect.IsAoE = false;
                    effect.StatusEffect = new Data.StatusEffectData(
                        StatusEffect.Bleeding,
                        GameConstants.SWORD_4_BLEED_DURATION,
                        GameConstants.SWORD_4_BLEED_DAMAGE
                    );
                    break;

                case 3:
                    effect.Damage = GameConstants.SWORD_5_DAMAGE;
                    effect.IsAoE = false;
                    effect.StatusEffect = new Data.StatusEffectData(
                        effectType,
                        weaponId == "axe" ? 3 : 2,
                        weaponId == "axe" ? 5 : 0
                    );
                    break;
            }

            GD.Print($"[ComboSystem] Warrior combo level {level}: {effect.Damage} damage" +
                     (effect.StatusEffect != null ? $" + {effect.StatusEffect.Type}" : ""));
        }

        private static void ProcessHealCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1:
                    effect.Healing = GameConstants.HEAL_3_AMOUNT;
                    break;

                case 2:
                    effect.Healing = GameConstants.HEAL_4_AMOUNT;
                    break;

                case 3:
                    effect.Healing = GameConstants.HEAL_5_AMOUNT;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Regeneration,
                        GameConstants.HEAL_5_REGEN_DURATION,
                        GameConstants.HEAL_5_REGEN_AMOUNT
                    );
                    break;
            }

            GD.Print($"[ComboSystem] Heal combo level {level}: {effect.Healing} HP" +
                     (level == 2 ? " + Cleanse" : "") +
                     (effect.StatusEffect != null ? " + Regeneration" : ""));
        }

        private static void ProcessShieldCombo(ComboEffect effect, int level)
        {
            switch (level)
            {
                case 1:
                    effect.Shield = GameConstants.SHIELD_3_AMOUNT;
                    break;

                case 2:
                    effect.Shield = GameConstants.SHIELD_4_AMOUNT;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.ShieldReflect,
                        1,
                        0,
                        GameConstants.SHIELD_4_REFLECT_PERCENT
                    );
                    break;

                case 3:
                    effect.Shield = GameConstants.SHIELD_5_AMOUNT;
                    effect.StatusEffect = new StatusEffectData(
                        StatusEffect.Immunity,
                        1,
                        0
                    );
                    break;
            }

            GD.Print($"[ComboSystem] Shield combo level {level}: {effect.Shield} shield" +
                     (effect.StatusEffect?.Type == StatusEffect.ShieldReflect ? " + Reflect" : "") +
                     (effect.StatusEffect?.Type == StatusEffect.Immunity ? " + Immunity" : ""));
        }
    }
}
