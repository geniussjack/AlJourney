class_name CutsceneData
extends RefCounted
## A named, ordered sequence of slides played back to back by CutscenePlayer.

## Unique identifier for the cutscene, used for lookup and debugging.
var id: String
## The slides that make up this cutscene, in playback order.
var slides: Array[CutsceneSlide]

## Builds a cutscene from its id and ordered slides.
func _init(id: String, slides: Array[CutsceneSlide]) -> void:
	self.id = id
	self.slides = slides
