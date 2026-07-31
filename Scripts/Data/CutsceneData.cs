using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// A single slide within a cutscene: a translated line of text and an optional illustration.
    /// </summary>
    /// <param name="TextKey">Localization key for the slide's text (see translations.csv).</param>
    /// <param name="ImagePath">
    /// Optional path to an illustration shown above the text. Left null while no cutscene art exists yet
    /// — <see cref="UI.CutscenePlayer"/> simply skips the image area when it's null or missing.
    /// </param>
    public sealed record CutsceneSlide(string TextKey, string ImagePath = null);

    /// <summary>
    /// A named, ordered sequence of slides played back to back by <see cref="UI.CutscenePlayer"/>.
    /// </summary>
    /// <param name="Id">Unique identifier for the cutscene, used for lookup and debugging.</param>
    /// <param name="Slides">The slides that make up this cutscene, in playback order.</param>
    public sealed record CutsceneData(string Id, IReadOnlyList<CutsceneSlide> Slides);
}
