class_name CutsceneDatabase
extends RefCounted
## Static catalog of the game's cutscenes. Slide text currently points at
## placeholder copy (see the CUTSCENE_* keys in translations.csv) so the
## real narrative can be written and dropped in later purely as a
## translation change, without touching this file or CutscenePlayer.

## Played once when a new game begins, before the player reaches the
## campaign map for the first time.
static var new_game_intro: CutsceneData = CutsceneData.new(
	"new_game_intro",
	[
		CutsceneSlide.new("CUTSCENE_NEW_GAME_INTRO_1"),
		CutsceneSlide.new("CUTSCENE_NEW_GAME_INTRO_2"),
		CutsceneSlide.new("CUTSCENE_NEW_GAME_INTRO_3"),
	] as Array[CutsceneSlide]
)

## Played after the Necromancer is defeated, before returning to the
## campaign map.
static var necromancer_defeat: CutsceneData = CutsceneData.new(
	"necromancer_defeat",
	[
		CutsceneSlide.new("CUTSCENE_NECROMANCER_DEFEAT_1"),
		CutsceneSlide.new("CUTSCENE_NECROMANCER_DEFEAT_2"),
		CutsceneSlide.new("CUTSCENE_NECROMANCER_DEFEAT_3"),
	] as Array[CutsceneSlide]
)
