using AlJourney.Scripts.Data;

namespace AlJourneyTests.Scripts.Data
{
    public class CutsceneDatabaseTests
    {
        private static IEnumerable<CutsceneData> AllCutscenes =>
            [CutsceneDatabase.NewGameIntro, CutsceneDatabase.NecromancerDefeat];

        [Fact]
        public void Cutscenes_AllHaveUniqueIds()
        {
            List<string> ids = [.. AllCutscenes.Select(cutscene => cutscene.Id)];

            Assert.Equal(ids.Count, ids.Distinct().Count());
        }

        [Fact]
        public void Cutscenes_AllHaveAtLeastOneSlide()
        {
            Assert.All(AllCutscenes, cutscene => Assert.NotEmpty(cutscene.Slides));
        }

        [Fact]
        public void Slides_AllHaveANonEmptyTextKey()
        {
            foreach (CutsceneData cutscene in AllCutscenes)
            {
                Assert.All(cutscene.Slides, slide => Assert.False(string.IsNullOrWhiteSpace(slide.TextKey)));
            }
        }

        [Fact]
        public void Slides_TextKeysAreUniqueWithinACutscene()
        {
            foreach (CutsceneData cutscene in AllCutscenes)
            {
                List<string> textKeys = [.. cutscene.Slides.Select(slide => slide.TextKey)];

                Assert.Equal(textKeys.Count, textKeys.Distinct().Count());
            }
        }

        [Fact]
        public void Slides_ImagePathIsNullByDefault()
        {
            // No cutscene art exists yet (see CutsceneDatabase remarks) - every slide should currently
            // fall back to text-only display in CutscenePlayer.
            foreach (CutsceneData cutscene in AllCutscenes)
            {
                Assert.All(cutscene.Slides, slide => Assert.Null(slide.ImagePath));
            }
        }

        [Fact]
        public void NewGameIntro_HasExpectedId()
        {
            Assert.Equal("new_game_intro", CutsceneDatabase.NewGameIntro.Id);
        }

        [Fact]
        public void NecromancerDefeat_HasExpectedId()
        {
            Assert.Equal("necromancer_defeat", CutsceneDatabase.NecromancerDefeat.Id);
        }
    }
}
