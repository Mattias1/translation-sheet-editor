namespace TranslationSheetEditor.Utils;

public static class ICollectionExtensions {
  public static void AddAll<T>(this ICollection<T> source, IEnumerable<T> items) {
    foreach (var item in items) {
      source.Add(item);
    }
  }
}
