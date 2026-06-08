using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Система управления двумя героями (Маг и Воин). Отвечает за их инициализацию, отслеживание их состояний (здоровье, щит, смерть) и маршрутизацию сигналов.
    /// </summary>
    public partial class DualHeroSystem : Node
    {
        /// <summary>
        /// Сигнал, вызываемый при изменении здоровья одного из героев. Передает класс героя, его текущее и максимальное количество здоровья.
        /// </summary>
        [Signal]
        public delegate void HeroHealthChangedEventHandler(CharacterClass heroClass, int currentHealth, int maxHealth);

        /// <summary>
        /// Сигнал, вызываемый при изменении прочности щита одного из героев. Передает класс героя и текущее значение его щита.
        /// </summary>
        [Signal]
        public delegate void HeroShieldChangedEventHandler(CharacterClass heroClass, int shieldAmount);

        /// <summary>
        /// Сигнал, вызываемый в случае гибели одного из героев. Передает класс павшего героя.
        /// </summary>
        [Signal]
        public delegate void HeroDiedEventHandler(CharacterClass heroClass);

        /// <summary>
        /// Сигнал, вызываемый, когда оба героя (Маг и Воин) погибают. Это событие обычно приводит к окончанию игры.
        /// </summary>
        [Signal]
        public delegate void BothHeroesDiedEventHandler();

        /// <summary>
        /// Ссылка на персонажа-Мага. Доступна только для чтения извне.
        /// </summary>
        public PlayerCharacter Mage { get; private set; }

        /// <summary>
        /// Ссылка на персонажа-Воина. Доступна только для чтения извне.
        /// </summary>
        public PlayerCharacter Warrior { get; private set; }

        /// <summary>
        /// Возвращает истину, если оба героя (Маг и Воин) в данный момент живы.
        /// </summary>
        public bool AreBothAlive => Mage.IsAlive && Warrior.IsAlive;

        /// <summary>
        /// Возвращает истину, если хотя бы один из героев (Маг или Воин) в данный момент жив.
        /// </summary>
        public bool IsAnyAlive => Mage.IsAlive || Warrior.IsAlive;

        /// <summary>
        /// Метод жизненного цикла Godot, вызываемый при добавлении узла в сцену.
        /// Инициализирует Мага и Воина, добавляет их как дочерние узлы и подписывается на их сигналы.
        /// </summary>
        public override void _Ready()
        {
            Mage = PlayerCharacter.Create(CharacterClass.Mage);
            Warrior = PlayerCharacter.Create(CharacterClass.Warrior);

            AddChild(Mage);
            AddChild(Warrior);

            ConnectHeroSignals(Mage, CharacterClass.Mage);
            ConnectHeroSignals(Warrior, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Both heroes initialized");
        }

        private void ConnectHeroSignals(PlayerCharacter hero, CharacterClass heroClass)
        {
            hero.HealthChanged += (current, max) =>
            {
                _ = EmitSignal(SignalName.HeroHealthChanged, (int)heroClass, current, max);
                CheckBothDead();
            };

            hero.ShieldChanged += (shield) =>
                EmitSignal(SignalName.HeroShieldChanged, (int)heroClass, shield);

            hero.CharacterDied += () =>
            {
                _ = EmitSignal(SignalName.HeroDied, (int)heroClass);
                CheckBothDead();
            };
        }

        private void CheckBothDead()
        {
            if (!Mage.IsAlive && !Warrior.IsAlive)
            {
                _ = EmitSignal(SignalName.BothHeroesDied);
                GD.Print("[DualHeroSystem] Both heroes have died - Game Over!");
            }
        }

        /// <summary>
        /// Возвращает соответствующего героя в зависимости от типа элемента (магические элементы для Мага, физические — для Воина).
        /// </summary>
        /// <param name="elementType">Тип элемента (огонь, лечение, меч, щит).</param>
        /// <returns>Персонаж-игрок, соответствующий данному элементу, или null при неизвестном элементе.</returns>
        public PlayerCharacter GetHeroForElement(ElementType elementType)
        {
            return elementType switch
            {
                ElementType.Fire => Mage,
                ElementType.Heal => Mage,
                ElementType.Sword => Warrior,
                ElementType.Shield => Warrior,
                _ => null
            };
        }

        /// <summary>
        /// Загружает состояние обоих героев (здоровье, урон и защиту) из данных сохранения.
        /// </summary>
        public void LoadFromSave(int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                                 int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense)
        {
            Mage.InitializeFromSave("Altarion", mageMaxHealth, mageHealth, mageDamage, mageDefense, CharacterClass.Mage);
            Warrior.InitializeFromSave("Aldric", warriorMaxHealth, warriorHealth, warriorDamage, warriorDefense, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Heroes loaded from save");
        }

        /// <summary>
        /// Возвращает объединенные характеристики обоих героев (Мага и Воина) в виде единого кортежа.
        /// </summary>
        /// <returns>Кортеж, содержащий текущее здоровье, максимальное здоровье, урон и защиту для обоих героев.</returns>
        public (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) GetCombinedStats()
        {
            (int maxHealth, int currentHealth, int damage, int defense) = Mage.GetStats();
            
            (int maxHealth, int currentHealth, int damage, int defense) warriorStats = Warrior.GetStats();

            return (
                currentHealth, maxHealth, damage, defense,
                warriorStats.currentHealth, warriorStats.maxHealth, warriorStats.damage, warriorStats.defense
            );
        }

        /// <summary>
        /// Обрабатывает все активные статусные эффекты (например, горение или регенерацию) для обоих героев.
        /// </summary>
        public void ProcessStatusEffects()
        {
            Mage.ProcessStatusEffects();
            Warrior.ProcessStatusEffects();
        }
    }
}
