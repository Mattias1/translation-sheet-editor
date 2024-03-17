using ClosedXML.Excel;
using System.Text.RegularExpressions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.Utils;

public static class ExcelUtil {
  public const int NR_OF_BIBLEBOOKS = 66;
  public const int EXPECTED_NR_OF_ROWS = 2 + NR_OF_BIBLEBOOKS + 2 + 5 + 2 + 6 + 2 + 3;

  public static void Export(TranslationData data, Uri filePath) {
    var raw = BuildTranslationDataExcelArray(data);
    ExportRawDataArray(raw, filePath);
  }

  public static ExcelRow[] BuildTranslationDataExcelArray(TranslationData data) {
    var result = new List<ExcelRow>(EXPECTED_NR_OF_ROWS);

    result.AddHeader("English", "", "", $"Target language ({data.Language})");
    result.AddHeader("Biblebook translations", "Biblebook", "Expression", "Biblebook", "Expression", "", "Original inputs");
    foreach (var (bookName, d) in data.BibleBooks.BibleBookData) {
      int numberOfBooks = d.NumberOfBooks();
      if (numberOfBooks > 1) {
        for (int i = 1; i <= numberOfBooks; i++) {
          result.AddRegexRow("", $"{i} {bookName}", d.TranslatedName, d.RegexParts, data.GetPrefixNumberOptions(i));
        }
      } else {
        result.AddRegexRow("", bookName, d.TranslatedName ?? "", d.RegexParts);
      }
    }

    result.AddRow();
    result.AddHeader("Visual translations");
    result.AddRow("Word for loading a bible text", "Loading...", "", data.LoadingStatus);
    result.AddRow("No result", "No result", "", data.NoResultStatus);
    result.AddRow("Error code", "Error code", "", data.ErrorCodeStatus);
    result.AddRow("Read more (Read further)", "Read more", "", data.ReadMoreStatus);
    result.AddRow("Not found", "Not found", "", data.NotFoundStatus);

    result.AddRow();
    result.AddHeader("Detection specific expressions");
    result.AddRegexRow("Words for bibleverse", "verse", null, data.WordsForVerse);
    result.AddRegexRow("Words to indicate selection verses", "to|till|until", null, data.VerseSelectionWords);
    result.AddRegexRow("Optional: chapter:verse separators", ":", null, data.ChapterVerseSeparator);
    var unicodeDashes = new [] { "–", "—" };
    result.AddRegexRow("Optional: verse-verse separators", "-", null, data.VerseVerseSeparator, null, unicodeDashes);
    result.AddRegexRow("Words for chapter", "chapter", null, data.WordsForChapter);
    result.AddRegexRow("Words/characters for listing references", "and|or|as well as", null, data.WordsOrCharactersForListingReferences);

    result.AddRow();
    result.AddHeader("Prefix numbers");
    result.AddRegexRow("1 (First)", "1|I|1st|First", null, data.PrefixNumberOptionsForFirst);
    result.AddRegexRow("2 (Second)", "2|II|2nd|Second", null, data.PrefixNumberOptionsForSecond);
    result.AddRegexRow("3 (Third)", "3|III|3rd|Third", null, data.PrefixNumberOptionsForThird);

    return result.ToArray();
  }

  public static void AddRegexRow(this List<ExcelRow> result, string description, string english, string? name,
      params string[] regexParts) => AddRegexRow(result, description, english, name, regexParts.ToList());
  public static void AddRegexRow(this List<ExcelRow> result, string description, string english,
      string? name, List<string> regexParts) => AddRegexRow(result, description, english, name, regexParts, null);
  public static void AddRegexRow(this List<ExcelRow> result, string description, string english,
      string? name, List<string> regexParts, List<string>? prefixNumbers, string[]? sneakilyAddToRegex = null) {
    var row = new List<string> { description, english, "" };
    if (name is not null) {
      row.Add(prefixNumbers is null ? name : $"{prefixNumbers.FirstOrDefault()} {name}");
    }
    var expandedRegexParts = regexParts;
    if (sneakilyAddToRegex is not null) {
      var temp = new HashSet<string>(regexParts);
      temp.AddAll(sneakilyAddToRegex);
      expandedRegexParts = temp.ToList();
    }
    row.Add(prefixNumbers is null
        ? expandedRegexParts.ToRegex()
        : expandedRegexParts.Select(p => $"({prefixNumbers.ToRegex()}) ?{p}").ToRegex());
    row.Add("");
    if (name is null) {
      row.Add("");
    }
    row.AddRange(regexParts);
    result.AddRow(row.ToArray());
  }

  public static void AddHeader(this List<ExcelRow> result, params string[] values) {
    result.Add(new ExcelRow(values, isHeader: true));
  }

  public static void AddRow(this List<ExcelRow> result, params string[] values) => result.Add(new ExcelRow(values));

  public static void ExportRawDataArray(ExcelRow[] data, Uri filePath) {
    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add("Translation sheet");

    for (int row = 0; row < data.Length; row++) {
      for (int col = 0; col < data[row].Length; col++) {
        worksheet.Cell(row + 1, col + 1).Value = data[row][col]; // Excel sheets are 1-based

        if (data[row].IsHeader) {
          worksheet.Cell(row + 1, col + 1).Style.Font.SetBold(); // Excel sheets are 1-based
        }
      }
    }

    workbook.SaveAs(filePath.AbsolutePath);
  }

  public static TranslationData Import(Uri filePath, out string? errors) {
    var raw = ImportRawDataArrays(filePath);
    return ParseRawData(raw, out errors);
  }

  public static ExcelRow[] ImportRawDataArrays(Uri filePath) {
    using var workbook = new XLWorkbook(filePath.AbsolutePath);
    var worksheet = workbook.Worksheets.First();

    var result = new List<ExcelRow>(EXPECTED_NR_OF_ROWS);
    for (int row = 1; row <= EXPECTED_NR_OF_ROWS * 10; row++) { // Excel sheets are 1-based (x10 just because there might be more rows)
      var worksheetRow = worksheet.Row(row);
      var rowValues = new string[worksheetRow.LastCellUsed()?.Address.ColumnNumber ?? 0];
      for (int i = 0; i < rowValues.Length; i++) {
        rowValues[i] = worksheetRow.Cell(i + 1).Value.ToString() ?? ""; // Excel sheets are 1-based
      }
      var isHeader = worksheetRow.FirstCellUsed()?.Style.Font.Bold ?? false;
      result.Add(new ExcelRow(rowValues, isHeader));
    }
    return result.ToArray();
  }

  public static TranslationData ParseRawData(ExcelRow[] raw, out string? errors) {
    var result = new TranslationData();
    result.FirstTimeInit();
    var prefixNumberParts = new PrefixNumberParts();
    errors = null;

    int headerIndex = LoopTillHeader("English", raw, 0);
    if (headerIndex >= 0) {
      var excelRow = raw[headerIndex];
      if (excelRow.At(3, "").StartsWith("Target language (")) {
        var regex = new Regex("Target language \\((.*)\\)", RegexOptions.IgnoreCase);
        var match = regex.Match(excelRow.At(3, ""));

        result.Language = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
      } else {
        result.Language = excelRow.At(3, "");
      }
    }

    bool originalInputsEnabled = false;
    headerIndex = LoopTillHeader("Biblebook translations", raw, headerIndex);
    if (headerIndex >= 0) {
      originalInputsEnabled = raw[headerIndex].At(6, "").Contains("Original inputs");
      for (int row = headerIndex + 1; row < headerIndex + NR_OF_BIBLEBOOKS + 1; row++) {
        var excelRow = raw[row];
        if (excelRow.Length < 2) {
          continue;
        }
        var englishName = excelRow.At(1, "");
        if (englishName.StartsWith("2 ") || englishName.StartsWith("3 ")) {
          // Skip these, already parsed at 1
        } else {
          bool isPrefixBook = englishName.StartsWith("1 ");
          if (isPrefixBook) {
            englishName = englishName[2..];
            if (englishName == BibleBooks.JOHN) {
              englishName = BibleBooks.JOHN_LETTER;
            }
          }
          if (result.BibleBooks.ContainsKey(englishName)) {
            string translatedName = excelRow.At(3, "");
            if (translatedName.StartsWith("1 ")) {
              translatedName = translatedName[2..];
            }
            result.BibleBooks[englishName].TranslatedName = translatedName;

            var excelRow3 = result.BibleBooks[englishName].NumberOfBooks() == 3 ? raw[row + 2] : null;

            RegexUtil.Parts? partsForNumbers = null;
            result.BibleBooks[englishName].RegexParts = isPrefixBook && row < raw.Length - 2
                ? FromOriginalInputsOrRegexAtForPrefixBooks(excelRow, raw[row + 1], excelRow3, originalInputsEnabled, 4, out partsForNumbers)
                : FromOriginalInputsOrRegexAt(excelRow, originalInputsEnabled, 4);

            if (partsForNumbers is not null) {
              prefixNumberParts.AddAll(partsForNumbers.NumberOneParts, partsForNumbers.NumberTwoParts,
                  partsForNumbers.NumberThreeParts);
            }
          }
        }
      }
    }

    headerIndex = LoopTillHeader("Visual translations", raw, headerIndex + BibleBooks.ALL_BOOKS.Length);
    if (headerIndex >= 0) {
      result.LoadingStatus = raw[headerIndex + 1].At(3, "");
      result.NoResultStatus = raw[headerIndex + 2].At(3, "");
      result.ErrorCodeStatus = raw[headerIndex + 3].At(3, "");
      result.ReadMoreStatus = raw[headerIndex + 4].At(3, "");
      if (raw[headerIndex + 5].Length > 0) {
        result.NotFoundStatus = raw[headerIndex + 5].At(3, "");
      }
    }

    headerIndex = LoopTillHeader("Detection specific expressions", raw, headerIndex);
    if (headerIndex >= 0) {
      result.WordsForVerse = FromOriginalInputsOrRegexAt(raw[headerIndex + 1], originalInputsEnabled, 3);
      result.VerseSelectionWords = FromOriginalInputsOrRegexAt(raw[headerIndex + 2], originalInputsEnabled, 3);
      result.ChapterVerseSeparator = FromOriginalInputsOrRegexAt(raw[headerIndex + 3], originalInputsEnabled, 3);
      result.VerseVerseSeparator = FromOriginalInputsOrRegexAt(raw[headerIndex + 4], originalInputsEnabled, 3);
      if (raw.Length > headerIndex + 5 && raw[headerIndex + 5].Length > 0) {
        result.WordsForChapter = FromOriginalInputsOrRegexAt(raw[headerIndex + 5], originalInputsEnabled, 3);
        result.WordsOrCharactersForListingReferences = FromOriginalInputsOrRegexAt(raw[headerIndex + 6], originalInputsEnabled, 3);
      }
    }

    headerIndex = LoopTillHeader("Prefix numbers", raw, headerIndex);
    if (headerIndex >= 0) {
      prefixNumberParts.AddAll(
          FromOriginalInputsOrRegexAt(raw[headerIndex + 1], originalInputsEnabled, 3),
          FromOriginalInputsOrRegexAt(raw[headerIndex + 2], originalInputsEnabled, 3),
          FromOriginalInputsOrRegexAt(raw[headerIndex + 3], originalInputsEnabled, 3)
        );
    }
    result.PrefixNumberOptionsForFirst = prefixNumberParts.NumberOneParts.ToList();
    result.PrefixNumberOptionsForSecond = prefixNumberParts.NumberTwoParts.ToList();
    result.PrefixNumberOptionsForThird = prefixNumberParts.NumberThreeParts.ToList();

    if (!originalInputsEnabled) {
      errors = "You've imported a manually created\nexcel file. Books with prefix numbers are"
        + "\nprobably not be loaded correctly,\nplease check them for errors.";
    }
    return result;
  }

  private static int LoopTillHeader(string needle, ExcelRow[] haystack, int currentIndex) {
    for (int i = Math.Max(currentIndex, 0); i < haystack.Length; i++) {
      if (haystack[i].Length > 0 && haystack[i][0].Contains(needle)) {
        return i;
      }
    }
    return -1;
  }

  private static List<string> FromOriginalInputsOrRegexAtForPrefixBooks(ExcelRow excelRow1, ExcelRow excelRow2,
      ExcelRow? excelRow3, bool originalsEnabled, int regexColumn, out RegexUtil.Parts? parts) {
    var result = new HashSet<string>();
    parts = null;
    if (originalsEnabled && excelRow1.Length > 6) {
      result.AddAll(excelRow1.RawData.Skip(6));
      if (excelRow2.Length > 6) {
        result.AddAll(excelRow2.RawData.Skip(6));
      }
      if (excelRow3 is not null && excelRow3.Length > 6) {
        result.AddAll(excelRow3.RawData.Skip(6));
      }
    } else {
      parts = RegexUtil.ToPartsFromMultiplePrefixBookRegexes(excelRow1.At(regexColumn, ""),
          excelRow2.At(regexColumn, ""), excelRow3?.At(regexColumn, ""));
      result.AddAll(parts.BookParts);
    }
    return result.ToList();
  }

  private static List<string> FromOriginalInputsOrRegexAt(ExcelRow excelRow, bool originalsEnabled, int regexColumn) {
    return originalsEnabled && excelRow.Length > 6
        ? excelRow.RawData.Skip(6).ToList()
        : excelRow.At(regexColumn, "").ToPartsFromRegex();
  }

  public class ExcelRow {
    public string[] RawData { get; }
    public bool IsHeader { get; }

    public ExcelRow(string[] rawData, bool isHeader = false) {
      RawData = rawData;
      IsHeader = isHeader;
    }

    public int Length => RawData.Length;
    public string this[int col] => RawData[col];

    public string At(int col, string defaultValue) => Length > col ? RawData[col] : defaultValue;

    public override string ToString() => string.Join(", ", RawData);
  }

  private class PrefixNumberParts {
    public HashSet<string> NumberOneParts { get; } = new();
    public HashSet<string> NumberTwoParts { get; } = new();
    public HashSet<string> NumberThreeParts { get; } = new();

    public void AddAll(IEnumerable<string> parts1, IEnumerable<string> parts2, IEnumerable<string> parts3) {
      NumberOneParts.AddAll(parts1);
      NumberTwoParts.AddAll(parts2);
      NumberThreeParts.AddAll(parts3);
    }
  }
}
