class_name TextureGenerator
extends RefCounted
## Utility class for generating textures programmatically. Used to create
## simple graphic primitives, avoiding the need to import external images
## for prototyping.

## Creates a solid square texture of the given color and size.
static func create_color_square(color: Color, size: int = 64) -> Texture2D:
	var image: Image = Image.create_empty(size, size, false, Image.FORMAT_RGBA8)
	image.fill(color)
	return ImageTexture.create_from_image(image)

## Creates a square texture with a fill color and a border.
static func create_color_square_with_border(fill_color: Color, border_color: Color, size: int = 64, border_width: int = 4) -> Texture2D:
	var image: Image = Image.create_empty(size, size, false, Image.FORMAT_RGBA8)
	image.fill(fill_color)

	for i: int in range(border_width):
		_draw_rect_outline(image, i, border_color)

	return ImageTexture.create_from_image(image)

## Draws a single-pixel-wide square outline at the given inset offset.
static func _draw_rect_outline(image: Image, offset: int, color: Color) -> void:
	var width: int = image.get_width()
	var height: int = image.get_height()

	for x: int in range(offset, width - offset):
		image.set_pixel(x, offset, color)
		image.set_pixel(x, height - 1 - offset, color)

	for y: int in range(offset, height - offset):
		image.set_pixel(offset, y, color)
		image.set_pixel(width - 1 - offset, y, color)
