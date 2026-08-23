class_name CutsceneSlide
extends RefCounted
## A single slide within a cutscene: a translated line of text and an
## optional illustration.

## Localization key for the slide's text (see translations.csv).
var text_key: String
## Optional path to an illustration shown above the text. Left empty while
## no cutscene art exists yet — CutscenePlayer simply skips the image area
## when it's empty or missing.
var image_path: String

## Builds a slide from its text key and optional illustration path.
func _init(text_key: String, image_path: String = "") -> void:
	self.text_key = text_key
	self.image_path = image_path
