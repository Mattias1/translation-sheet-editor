using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.Utils;

public static class ValidationUtil {
  public static string ValidateTranslationData(TranslationData data) {
    string errorText = "";
    if (data.BibleBooks.BibleBookData.Any(kv => string.IsNullOrWhiteSpace(kv.Value.TranslatedName))) {
      errorText += "\n- Missing values at 'Initial book names'.";
    }

    if (string.IsNullOrWhiteSpace(data.LoadingStatus)
        || string.IsNullOrWhiteSpace(data.NoResultStatus)
        || string.IsNullOrWhiteSpace(data.ErrorCodeStatus)
        || string.IsNullOrWhiteSpace(data.ReadMoreStatus)
        || data.WordsForVerse.All(string.IsNullOrWhiteSpace)
        || data.VerseSelectionWords.All(string.IsNullOrWhiteSpace)
        || data.ChapterVerseSeparator.All(string.IsNullOrWhiteSpace)
        || data.VerseVerseSeparator.All(string.IsNullOrWhiteSpace)) {
      errorText += "\n- Missing values at 'Other translations'.";
    }

    if (data.PrefixNumberOptionsForFirst.All(string.IsNullOrWhiteSpace)
        || data.PrefixNumberOptionsForSecond.All(string.IsNullOrWhiteSpace)
        || data.PrefixNumberOptionsForThird.All(string.IsNullOrWhiteSpace)) {
      errorText += "\n- Missing values at 'Prefix numbers'.";
    }

    var regexPartSet = new Dictionary<string, string>();
    var regexPartSetButNotJohn = new HashSet<string>(); // Gotta love edge cases :)
    foreach (var (name, bookData) in data.BibleBooks.BibleBookData) {
      if (bookData.RegexParts.All(string.IsNullOrWhiteSpace)) {
        errorText += $"\n- Missing values for book '{name}'.";
      }
      foreach (string regexPart in bookData.RegexParts) {
        if (!regexPartSet.TryAdd(regexPart, name)
            && (name != BibleBooks.JOHN_LETTER || regexPartSetButNotJohn.Contains(regexPart))) {
          errorText += $"\n- Book alternative '{regexPart}' is used multiple times; {regexPartSet[regexPart]}, {name}.";
        }
        if (name != BibleBooks.JOHN) {
          regexPartSetButNotJohn.Add(regexPart);
        }
      }
    }

    return errorText;
  }
}
