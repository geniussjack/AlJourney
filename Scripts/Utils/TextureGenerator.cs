using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Утилитарный статический класс для программной генерации текстур.
    /// Используется для создания простых графических примитивов, 
    /// что позволяет избежать необходимости импортировать внешние изображения для прототипирования.
    /// </summary>
    public static class TextureGenerator
    {
        /// <summary>
        /// Создает сплошную квадратную текстуру заданного цвета и размера.
        /// </summary>
        /// <param name="color">Цвет текстуры.</param>
        /// <param name="size">Ширина и высота квадрата в пикселях.</param>
        /// <returns>Сгенерированная текстура, готовая к использованию в UI или спрайтах.</returns>
        public static Texture2D CreateColorSquare(Color color, int size = 64)
        {
            Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
            image.Fill(color);
            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Создает квадратную текстуру с заливкой и рамкой по краям.
        /// </summary>
        /// <param name="fillColor">Основной цвет заливки.</param>
        /// <param name="borderColor">Цвет рамки.</param>
        /// <param name="size">Ширина и высота квадрата в пикселях.</param>
        /// <param name="borderWidth">Толщина рамки в пикселях.</param>
        /// <returns>Сгенерированная текстура с рамкой.</returns>
        public static Texture2D CreateColorSquareWithBorder(Color fillColor, Color borderColor, int size = 64, int borderWidth = 4)
        {
            Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
            image.Fill(fillColor);

            for (int i = 0; i < borderWidth; i++)
            {
                DrawRectOutline(image, i, borderColor);
            }

            return ImageTexture.CreateFromImage(image);
        }

        private static void DrawRectOutline(Image image, int offset, Color color)
        {
            int width = image.GetWidth();
            int height = image.GetHeight();

            for (int x = offset; x < width - offset; x++)
            {
                image.SetPixel(x, offset, color);
                image.SetPixel(x, height - 1 - offset, color);
            }

            for (int y = offset; y < height - offset; y++)
            {
                image.SetPixel(offset, y, color);
                image.SetPixel(width - 1 - offset, y, color);
            }
        }
    }
}
