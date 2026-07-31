using Godot;

namespace AlJourney.Scripts.Utils
{
    /// <summary>
    /// Utility static class for generating textures programmatically.
    /// Used to create simple graphic primitives,
    /// avoiding the need to import external images for prototyping.
    /// </summary>
    public static class TextureGenerator
    {
        /// <summary>
        /// Creates a solid square texture of the given color and size.
        /// </summary>
        /// <param name="color">The texture color.</param>
        /// <param name="size">The width and height of the square, in pixels.</param>
        /// <returns>The generated texture, ready to use in UI or sprites.</returns>
        public static Texture2D CreateColorSquare(Color color, int size = 64)
        {
            Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
            image.Fill(color);
            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Creates a square texture with a fill color and a border.
        /// </summary>
        /// <param name="fillColor">The main fill color.</param>
        /// <param name="borderColor">The border color.</param>
        /// <param name="size">The width and height of the square, in pixels.</param>
        /// <param name="borderWidth">The border thickness, in pixels.</param>
        /// <returns>The generated texture with a border.</returns>
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
