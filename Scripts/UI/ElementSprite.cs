using AlJourney.Scripts.Match3;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI-компонент, представляющий отдельный элемент на игровом поле "три в ряд". Управляет отображением текстуры, анимациями перемещения и выделением.
    /// </summary>
    public partial class ElementSprite : Control
    {
        [Signal]
        /// <summary>
        /// Делегат для события клика по элементу.
        /// </summary>
        public delegate void ElementClickedEventHandler(ElementSprite element);

        private TextureRect _sprite;
        private Panel _highlightPanel;
        private Vector2 _targetPosition;
        private float _animationSpeed = 10.0f;

        public ElementData Data { get; private set; }

        /// <summary>
        /// Возвращает текущую позицию элемента по оси X в сетке, либо -1, если данные отсутствуют.
        /// </summary>
        public int GridX => Data?.X ?? -1;

        /// <summary>
        /// Возвращает текущую позицию элемента по оси Y в сетке, либо -1, если данные отсутствуют.
        /// </summary>
        public int GridY => Data?.Y ?? -1;

        public bool IsAnimating { get; private set; }

        /// <summary>
        /// Вызывается при готовности узла. Создает и настраивает визуальные компоненты (текстуру и панель выделения), а также подписывается на события ввода.
        /// </summary>
        public override void _Ready()
        {
            _sprite = new TextureRect
            {
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(128, 128)
            };
            AddChild(_sprite);

            _highlightPanel = new Panel
            {
                Visible = false
            };
            _highlightPanel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(_highlightPanel);

            MouseFilter = MouseFilterEnum.Stop;
            GuiInput += OnGuiInput;
        }

        /// <summary>
        /// Инициализирует спрайт элемента заданными данными и текстурой, устанавливая начальные размеры и позицию.
        /// </summary>
        /// <param name="data">Данные элемента (тип, позиция в сетке).</param>
        /// <param name="texture">Текстура для визуального отображения.</param>
        public void Initialize(ElementData data, Texture2D texture)
        {
            Data = data;
            _sprite.Texture = texture;
            _targetPosition = Position;

            CustomMinimumSize = new Vector2(128, 128);
            Size = new Vector2(128, 128);
        }

        /// <summary>
        /// Обновляет логические данные элемента (ElementData), связанные с этим визуальным компонентом.
        /// </summary>
        /// <param name="newData">Новые данные элемента.</param>
        public void UpdateData(ElementData newData)
        {
            Data = newData;
        }

        /// <summary>
        /// Устанавливает новую текстуру для отображения данного элемента.
        /// </summary>
        /// <param name="texture">Новая текстура.</param>
        public void SetTexture(Texture2D texture)
        {
            _sprite.Texture = texture;
        }

        /// <summary>
        /// Включает или отключает визуальное выделение элемента (например, при выборе для обмена).
        /// </summary>
        /// <param name="enabled">True, если элемент должен быть выделен, иначе False.</param>
        public void SetHighlight(bool enabled)
        {
            _highlightPanel.Visible = enabled;

            if (enabled)
            {
                _highlightPanel.Modulate = new Color(1, 1, 0, 0.5f); 
            }
        }

        /// <summary>
        /// Запускает плавную анимацию перемещения элемента к указанной целевой позиции.
        /// </summary>
        /// <param name="targetPos">Целевая позиция на экране.</param>
        public void AnimateToPosition(Vector2 targetPos)
        {
            _targetPosition = targetPos;
            IsAnimating = true;
        }

        /// <summary>
        /// Мгновенно устанавливает позицию элемента на экране без анимации и обновляет целевую позицию.
        /// </summary>
        /// <param name="pos">Новая позиция.</param>
        public void SetGridPosition(Vector2 pos)
        {
            Position = pos;
            _targetPosition = pos;
            IsAnimating = false;
        }

        /// <summary>
        /// Воспроизводит анимацию исчезновения элемента при успешном совпадении (матче) "три в ряд", после чего удаляет узел.
        /// </summary>
        public void PlayMatchAnimation()
        {
            Tween tween = CreateTween();
            _ = tween.SetParallel(true);
            _ = tween.TweenProperty(this, "scale", Vector2.Zero, 0.3f).SetEase(Tween.EaseType.In);
            _ = tween.TweenProperty(this, "modulate:a", 0.0f, 0.3f);
            _ = tween.Chain().TweenCallback(Callable.From(QueueFree));
        }

        /// <summary>
        /// Воспроизводит анимацию обмена местами с другим элементом, перемещая данный спрайт в указанную позицию за заданное время.
        /// </summary>
        /// <param name="targetPos">Позиция, в которую нужно переместиться.</param>
        /// <param name="duration">Продолжительность анимации в секундах (по умолчанию 0.2f).</param>
        public void PlaySwapAnimation(Vector2 targetPos, float duration = 0.2f)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(this, "position", targetPos, duration).SetEase(Tween.EaseType.InOut);
        }

        /// <summary>
        /// Вызывается каждый кадр. Обрабатывает логику плавной анимации перемещения элемента к целевой позиции.
        /// </summary>
        /// <param name="delta">Время, прошедшее с предыдущего кадра.</param>
        public override void _Process(double delta)
        {
            if (IsAnimating)
            {
                Position = Position.Lerp(_targetPosition, _animationSpeed * (float)delta);

                if (Position.DistanceTo(_targetPosition) < 1.0f)
                {
                    Position = _targetPosition;
                    IsAnimating = false;
                }
            }
        }

        private void OnGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
                {
                    _ = EmitSignal(SignalName.ElementClicked, this);
                }
            }
        }
    }
}
