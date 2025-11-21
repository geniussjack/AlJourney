using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Utility for creating placeholder textures.
    /// </summary>
    public static class TextureGenerator
    {
        /// <summary>
        /// Creates a solid color square texture.
        /// </summary>
        public static Texture2D CreateColorSquare(Color color, int size = 64)
        {
            Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
            image.Fill(color);
            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Creates a colored square with border.
        /// </summary>
        public static Texture2D CreateColorSquareWithBorder(Color fillColor, Color borderColor, int size = 64, int borderWidth = 4)
        {
            Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
            image.Fill(fillColor);

            // Draw border
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

            // Top and bottom
            for (int x = offset; x < width - offset; x++)
            {
                image.SetPixel(x, offset, color);
                image.SetPixel(x, height - 1 - offset, color);
            }

            // Left and right
            for (int y = offset; y < height - offset; y++)
            {
                image.SetPixel(offset, y, color);
                image.SetPixel(width - 1 - offset, y, color);
            }
        }
    }
}
