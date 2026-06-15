using Godot;
using System.Xml;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Автозагружаемый синглтон для управления локализацией через XML-файлы.
    /// Парсит файлы Data/Languages/*/strings.xml и добавляет переводы в TranslationServer.
    /// </summary>
    public partial class LocalizationManager : Node
    {
        public static LocalizationManager Instance { get; private set; }

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;

            LoadTranslations();
            
            GD.Print("[LocalizationManager] Initialized");
        }

        private void LoadTranslations()
        {
            // Поддерживаемые языки
            string[] languages = { "EN", "RU" };

            foreach (string lang in languages)
            {
                string path = $"res://Data/Languages/{lang}/strings.xml";
                LoadTranslationFile(path, lang.ToLower());
            }
        }

        private void LoadTranslationFile(string resPath, string locale)
        {
            if (!FileAccess.FileExists(resPath))
            {
                GD.PrintErr($"[LocalizationManager] File not found: {resPath}");
                return;
            }

            using var file = FileAccess.Open(resPath, FileAccess.ModeFlags.Read);
            string xmlContent = file.GetAsText();

            XmlDocument xmlDoc = new();
            try
            {
                xmlDoc.LoadXml(xmlContent);

                Translation translation = new()
                {
                    Locale = locale
                };

                XmlNodeList strings = xmlDoc.SelectNodes("//string");
                if (strings != null)
                {
                    foreach (XmlNode node in strings)
                    {
                        if (node.Attributes?["id"] != null && node.Attributes?["text"] != null)
                        {
                            string id = node.Attributes["id"].Value;
                            string text = node.Attributes["text"].Value;
                            translation.AddMessage(id, text);
                        }
                    }
                }

                TranslationServer.AddTranslation(translation);
                GD.Print($"[LocalizationManager] Loaded {strings?.Count ?? 0} translations for '{locale}'.");
            }
            catch (XmlException e)
            {
                GD.PrintErr($"[LocalizationManager] Failed to parse XML at {resPath}: {e.Message}");
            }
        }
    }
}
