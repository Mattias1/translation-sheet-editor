using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TranslationSheetEditor.Utils;

public static class RegexUtil {
  public static string ToRegex(this IEnumerable<string> regexParts) {
    var sb = new StringBuilder();
    var parts = regexParts.ToArray();

    for (int i = 0; i < parts.Length; i++) {
      string p = parts[i];
      if (string.IsNullOrWhiteSpace(p)) {
        continue;
      }

      if (i < parts.Length - 1 && parts[i + 1].Length == p.Length - 1) {
        if (TryMerge(p, parts[i + 1], out string? mergedParts)) {
          sb.Append(mergedParts).Append('|');
          i++;
          continue;
        }
      }

      sb.Append(p).Append('|');
    }

    if (sb.Length > 0) {
      sb.Length--; // Remove the final '|'
    }
    return sb.ToString();
  }

  private static bool TryMerge(string longer, string smaller, [NotNullWhen(returnValue: true)] out string? result) {
      int diffIndex = FirstDifferentChar(longer, smaller);
      if (longer.Substring(diffIndex + 1) == smaller.Substring(diffIndex)) {
        result = string.Concat(longer.AsSpan(0, diffIndex + 1), "?", longer.AsSpan(diffIndex + 1));
        return true;
      }
      result = null;
      return false;
  }

  private static int FirstDifferentChar(string longer, string smaller) {
    for (int i = 0; i < smaller.Length; i++) {
      if (longer[i] != smaller[i]) {
        return i;
      }
    }
    return smaller.Length;
  }

  public static List<string> ToPartsFromRegex(this string s) {
    var initial = s.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    var result = new List<string>(initial.Count);
    foreach (var part in initial) {
      AddQuestionMarkedPartsRecursively(result, part);
    }
    return result;
  }

  private static void AddQuestionMarkedPartsRecursively(List<string> result, string part) {
    int i = part.IndexOf('?');
    if (i < 0) {
      if (!string.IsNullOrWhiteSpace(part)) {
        result.Add(part);
      }
      return;
    }
    AddQuestionMarkedPartsRecursively(result, part[..i] + part[(i + 1)..]);
    AddQuestionMarkedPartsRecursively(result, part[..(i - 1)] + part[(i + 1)..]);
  }
}
