using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Система управления отрядом игрока. Исторически называется "DualHeroSystem" (по двум главным героям),
    /// но с Этапа 1 редизайна представляет собой отряд из трёх слотов: Маг и Воин (Эльтарион и Эльдрик,
    /// всегда присутствуют) и опциональный третий слот наёмника, который появится на этапе восстановления
    /// деревни (см. REDESIGN_NOTES.md, разделы 4 и 7). Отвечает за инициализацию бойцов, отслеживание их
    /// состояний и маршрутизацию сигналов.
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
        /// Сигнал, вызываемый, когда весь отряд полностью погибает. Это событие обычно приводит к окончанию игры.
        /// </summary>
        [Signal]
        public delegate void PartyDefeatedEventHandler();

        /// <summary>
        /// Ссылка на персонажа-Мага (Эльтарион). Доступна только для чтения извне.
        /// </summary>
        public PlayerCharacter Mage { get; private set; }

        /// <summary>
        /// Ссылка на персонажа-Воина (Эльдрик). Доступна только для чтения извне.
        /// </summary>
        public PlayerCharacter Warrior { get; private set; }

        /// <summary>
        /// Третий слот отряда — наёмник из поселения. В Этапе 1 всегда пуст: наём появится на этапе
        /// восстановления деревни. Заложен заранее, чтобы не переделывать структуру отряда позже.
        /// </summary>
        public PlayerCharacter Companion { get; private set; }

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
                CheckPartyDefeated();
            };

            hero.ShieldChanged += (shield) =>
                EmitSignal(SignalName.HeroShieldChanged, (int)heroClass, shield);

            hero.CharacterDied += () =>
            {
                _ = EmitSignal(SignalName.HeroDied, (int)heroClass);
                CheckPartyDefeated();
            };
        }

        private void CheckPartyDefeated()
        {
            if (GetAliveMembers().Count == 0)
            {
                _ = EmitSignal(SignalName.PartyDefeated);
                GD.Print("[DualHeroSystem] Entire party has fallen - Game Over!");
            }
        }

        /// <summary>
        /// Возвращает всех участников отряда: двух героев и наёмника, если он назначен.
        /// </summary>
        /// <returns>Список участников отряда в фиксированном порядке (Маг, Воин, Наёмник).</returns>
        public IReadOnlyList<PlayerCharacter> GetPartyMembers()
        {
            return Companion is null ? [Mage, Warrior] : [Mage, Warrior, Companion];
        }

        /// <summary>
        /// Возвращает только тех участников отряда, которые в данный момент живы.
        /// </summary>
        /// <returns>Список живых участников отряда.</returns>
        public IReadOnlyList<PlayerCharacter> GetAliveMembers()
        {
            return [.. GetPartyMembers().Where(member => member.IsAlive)];
        }

        /// <summary>
        /// Загружает состояние обоих героев из данных сохранения.
        /// </summary>
        public void LoadFromSave(int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
                                 int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense)
        {
            Mage.InitializeFromSave("Altarion", mageMaxHealth, mageHealth, mageDamage, mageDefense, CharacterClass.Mage);
            Warrior.InitializeFromSave("Aldric", warriorMaxHealth, warriorHealth, warriorDamage, warriorDefense, CharacterClass.Warrior);

            GD.Print("[DualHeroSystem] Heroes loaded from save");
        }

        /// <summary>
        /// Возвращает объединенные характеристики обоих героев в виде единого кортежа.
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
        /// Обрабатывает все активные статусные эффекты для всех участников отряда.
        /// </summary>
        public void ProcessStatusEffects()
        {
            foreach (PlayerCharacter member in GetPartyMembers())
            {
                member.ProcessStatusEffects();
            }
        }
    }
}
