namespace TranslationSheetEditor.Utils;

public static class RegexUtil {
  public static string ToRegex(this List<string> parts) {
    return string.Join('|', parts.Where(s => !string.IsNullOrWhiteSpace(s)));
  }
}
