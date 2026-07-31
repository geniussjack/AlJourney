namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Static catalog of the game's cutscenes. Slide text currently points at placeholder copy (see the
    /// <c>CUTSCENE_*</c> keys in translations.csv) so the real narrative can be written and dropped in
    /// later purely as a translation change, without touching this file or <see cref="UI.CutscenePlayer"/>.
    /// </summary>
    public static class CutsceneDatabase
    {
        /// <summary>
        /// Played once when a new game begins, before the player reaches the campaign map for the
        /// first time.
        /// </summary>
        public static readonly CutsceneData NewGameIntro = new(
            "new_game_intro",
            [
                new CutsceneSlide("CUTSCENE_NEW_GAME_INTRO_1"),
                new CutsceneSlide("CUTSCENE_NEW_GAME_INTRO_2"),
                new CutsceneSlide("CUTSCENE_NEW_GAME_INTRO_3")
            ]);

        /// <summary>
        /// Played after the Necromancer is defeated, before returning to the campaign map.
        /// </summary>
        public static readonly CutsceneData NecromancerDefeat = new(
            "necromancer_defeat",
            [
                new CutsceneSlide("CUTSCENE_NECROMANCER_DEFEAT_1"),
                new CutsceneSlide("CUTSCENE_NECROMANCER_DEFEAT_2"),
                new CutsceneSlide("CUTSCENE_NECROMANCER_DEFEAT_3")
            ]);
    }
}
