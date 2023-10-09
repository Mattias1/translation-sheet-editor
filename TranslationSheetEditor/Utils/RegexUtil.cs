namespace TranslationSheetEditor.Utils;

public static class RegexUtil {
  public static string ToRegex(this IEnumerable<string> parts) {
    return string.Join('|', parts.Where(s => !string.IsNullOrWhiteSpace(s)));
  }

  public static List<string> ToPartsFromRegex(this string s) {
    return s.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
  }
}
