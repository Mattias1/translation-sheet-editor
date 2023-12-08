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
    var initial = SplitButNotBraces(s);
    var result = new List<string>(initial.Count);
    foreach (var part in initial) {
      AddQuestionMarkedPartsRecursively(result, part);
    }
    return result;
  }

  private static List<string> SplitButNotBraces(string s) {
    var initial = new List<string>();
    int lastIndex = 0;
    for (int i = 0; i < s.Length; i++) {
      if (s[i] == '(') {
        int teleportLocation = FindMatchingClosingBrace(s, i);
        if (teleportLocation != -1) {
          i = teleportLocation;
        }
      }
      if (s[i] == '|') {
        string part = s.Substring(lastIndex, i - lastIndex).Trim();
        if (!string.IsNullOrWhiteSpace(part)) {
          initial.Add(part);
        }
        lastIndex = i + 1;
      }
    }
    string lastPart = s.Substring(lastIndex).Trim();
    if (!string.IsNullOrWhiteSpace(lastPart)) {
      initial.Add(lastPart);
    }
    return initial;
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

  public static Parts ToPartsFromMultiplePrefixBookRegexes(string first, string second, string? potentialThird) {
    var parts1 = first.ToPartsFromRegex();
    var parts2 = second.ToPartsFromRegex();
    var result = new HashSet<string>();
    var numParts1 = new HashSet<string>();
    var numParts2 = new HashSet<string>();
    var numParts3 = new HashSet<string>();

    for (int i = 0; i < Math.Min(parts1.Count, parts2.Count); i++) {
      var (equalPart, nums1, nums2) = EqualPartFromTheRight(parts1[i], parts2[i]);
      equalPart = equalPart.Trim(' ', '?');
      if (!string.IsNullOrWhiteSpace(equalPart)) {
        result.Add(equalPart);
        numParts1.AddAll(nums1.ToPartsFromRegex());
        numParts2.AddAll(nums2.ToPartsFromRegex());
      }
    }
    if (potentialThird is not null) {
      var parts3 = potentialThird.ToPartsFromRegex();
      for (int i = 0; i < Math.Min(parts1.Count, parts2.Count); i++) {
        var (equalPart, _, nums3) = EqualPartFromTheRight(parts1[i], parts3[i]);
        if (!string.IsNullOrWhiteSpace(equalPart.Trim(' ', '?'))) {
          numParts3.AddAll(nums3.ToPartsFromRegex());
        }
      }
    }

    return new Parts(result, numParts1, numParts2, numParts3);
  }

  private static (string, string, string) EqualPartFromTheRight(string first, string second) {
    // Special case: '(1|I) Bookname', '(2|II) ?Bookname'
    if (first[0] == '(') {
      int closingBraceIndex = FindMatchingClosingBrace(first, 0);
      if (closingBraceIndex != -1
          && first.Length > closingBraceIndex
          && second[0] == '(') {
        int closingBrace2Index = FindMatchingClosingBrace(second, 0);
        if (closingBrace2Index != -1) {
          return (first.Substring(closingBraceIndex + 1),
              first.Substring(1, closingBraceIndex - 1),
              second.Substring(1, closingBrace2Index - 1));
        }
      }
    }
    // Special case: 'Eerste Boek', 'Tweede Boek' (note: almost covers the first case - except that removes the braces)
    var splitFirst = first.Split(' ');
    var splitSecond = second.Split(' ');
    if (splitFirst.Length > 1 && splitFirst[^1] == splitSecond[^1]) {
      for (int i = 1; i <= Math.Min(splitFirst.Length, splitSecond.Length); i++) {
        if (splitFirst[^i] != splitSecond[^i]) {
          return (string.Join(' ', splitFirst.Skip(splitFirst.Length - i + 1)),
              string.Join(' ', splitFirst.Take(splitFirst.Length - i + 1)),
              string.Join(' ', splitSecond.Take(splitSecond.Length - i + 1)));
        }
      }
    }
    // Common case: any series of characters
    for (int i = 1; i <= Math.Min(first.Length, second.Length); i++) {
      if (first[^i] != second[^i]) {
        return (first.Substring(first.Length - i + 1),
            first.Substring(0, first.Length - i + 1),
            second.Substring(0, second.Length - i + 1));
      }
    }
    return (first, "", "");
  }

  private static int FindMatchingClosingBrace(string s, int openingBraceIndex) {
    int braceCount = 0;
    for (int i = openingBraceIndex; i < s.Length; i++) {
      if (s[i] == '(') {
        braceCount++;
      } else if (s[i] == ')') {
        braceCount--;
        if (braceCount == 0) {
          return i;
        }
      }
    }
    return -1;
  }

  public record Parts(
      IEnumerable<string> BookParts,
      IEnumerable<string> NumberOneParts,
      IEnumerable<string> NumberTwoParts,
      IEnumerable<string> NumberThreeParts);
}
