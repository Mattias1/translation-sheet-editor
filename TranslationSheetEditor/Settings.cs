namespace TranslationSheetEditor;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class Settings {
  public List<string> Languages { get; set; }
  public string? SelectedLanguage { get; set; }
  public string PreviewText { get; set; }

  public void FirstTimeInit() {
    Languages ??= new List<string>();
    PreviewText ??= string.Empty;
  }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
