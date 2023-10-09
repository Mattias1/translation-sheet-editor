using ClosedXML.Excel;
using System.Text.RegularExpressions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.Utils;

public static class ExcelUtil {
  public const int EXPECTED_NR_OF_ROWS = 2 + 66 + 2 + 4 + 2 + 4;

  public static void Export(TranslationData data, Uri filePath) {
    var raw = BuildTranslationDataExcelArray(data);
    ExportRawDataArray(raw, filePath);
  }

  public static ExcelRow[] BuildTranslationDataExcelArray(TranslationData data) {
    var result = new List<ExcelRow>(EXPECTED_NR_OF_ROWS);

    result.AddHeader("English", "", "", $"Target language ({data.Language})");
    result.AddHeader("Biblebook translations", "Biblebook", "Expression", "Biblebook", "Expression", "", "Original inputs");
    foreach (var (bookName, d) in data.BibleBooks.BibleBookData) {
      result.AddRegexRow("", bookName, d.TranslatedName ?? "", d.RegexParts);
    }

    result.AddRow();
    result.AddHeader("Visual translations");
    result.AddRow("Word for loading a bible text", "Loading...", "", data.LoadingStatus);
    result.AddRow("No result", "No result", "", data.NoResultStatus);
    result.AddRow("Error code", "Error code", "", data.ErrorCodeStatus);
    result.AddRow("Read more (Read further)", "Read more", "", data.ReadMoreStatus);

    result.AddRow();
    result.AddHeader("Detection specific expressions");
    result.AddRegexRow("Words for bibleverse", "verse", null, data.WordsForVerse);
    result.AddRegexRow("Words to indicate selection verses", "to|till|until", null, data.VerseSelectionWords);
    result.AddRegexRow("Optional: chapter:verse separators", ":", null, data.ChapterVerseSeparator);
    result.AddRegexRow("Optional: verse-verse separators", "-", null, data.VerseVerseSeparator);

    return result.ToArray();
  }

  public static void AddRegexRow(this List<ExcelRow> result, string description, string english, string? name,
      params string[] regexParts) => AddRegexRow(result, description, english, name, regexParts.ToList());
  public static void AddRegexRow(this List<ExcelRow> result, string description, string english,
      string? name, List<string> regexParts) {
    var row = new List<string> { description, english, "" };
    if (name is not null) {
      row.Add(name);
    }
    row.AddRange(new[] { regexParts.ToRegex(), "" });
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

  public static TranslationData Import(Uri filePath) {
    var raw = ImportRawDataArrays(filePath);
    return ParseRawData(raw);
  }

  public static ExcelRow[] ImportRawDataArrays(Uri filePath) {
    using var workbook = new XLWorkbook(filePath.AbsolutePath);
    var worksheet = workbook.Worksheets.First();

    var result = new List<ExcelRow>(EXPECTED_NR_OF_ROWS);
    for (int row = 1; row <= EXPECTED_NR_OF_ROWS; row++) { // Excel sheets are 1-based
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

  public static TranslationData ParseRawData(ExcelRow[] raw) {
    var result = new TranslationData();
    result.FirstTimeInit();

    int headerIndex = LoopTillHeader("English", raw, 0);
    if (headerIndex >= 0) {
      var excelRow = raw[headerIndex];
      if (string.IsNullOrWhiteSpace(excelRow.At(3, ""))) {
        result.Language = excelRow.At(1, "");
      } else {
        var regex = new Regex("Target language \\((.*)\\)", RegexOptions.IgnoreCase);
        var match = regex.Match(excelRow.At(3, ""));

        result.Language = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
      }
    }

    headerIndex = LoopTillHeader("Biblebook translations", raw, headerIndex);
    if (headerIndex >= 0) {
      bool originalInputsEnabled = raw[headerIndex].At(6, "").Contains("Original inputs");
      for (int row = headerIndex + 1; row < headerIndex + BibleBooks.ALL_BOOKS.Length + 1; row++) {
        var excelRow = raw[row];
        if (excelRow.Length < 2) {
          continue;
        }
        var englishName = excelRow.At(1, "");
        if (result.BibleBooks.ContainsKey(englishName)) {
          result.BibleBooks[englishName].TranslatedName = excelRow.At(3, "");

          result.BibleBooks[englishName].RegexParts = originalInputsEnabled && excelRow.Length > 6
              ? excelRow.RawData.Skip(6).ToList()
              : excelRow.At(4, "").ToPartsFromRegex();
        }
      }
    }

    headerIndex = LoopTillHeader("Visual translations", raw, headerIndex + BibleBooks.ALL_BOOKS.Length);
    if (headerIndex >= 0) {
      result.LoadingStatus = raw[headerIndex + 1].At(3, "");
      result.NoResultStatus = raw[headerIndex + 2].At(3, "");
      result.ErrorCodeStatus = raw[headerIndex + 3].At(3, "");
      result.ReadMoreStatus = raw[headerIndex + 4].At(3, "");
    }

    headerIndex = LoopTillHeader("Detection specific expressions", raw, headerIndex);
    if (headerIndex >= 0) {
      result.WordsForVerse = raw[headerIndex + 1].At(3, "").ToPartsFromRegex();
      result.VerseSelectionWords = raw[headerIndex + 2].At(3, "").ToPartsFromRegex();
      result.ChapterVerseSeparator = raw[headerIndex + 3].At(3, "").ToPartsFromRegex();
      result.VerseVerseSeparator = raw[headerIndex + 4].At(3, "").ToPartsFromRegex();
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
}
