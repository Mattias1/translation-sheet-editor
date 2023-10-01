using ClosedXML.Excel;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.Utils;

public static class ExcelUtil {
  public static void Export(TranslationData data, Uri filePath) {
    var raw = BuildTranslationDataExcelArray(data);
    ExportRawDataArray(raw, filePath);
  }

  public static RegexRow[] BuildTranslationDataExcelArray(TranslationData data) {
    var result = new List<RegexRow>(2 + 66 + 2 + 4 + 2 + 4);

    result.AddHeader("English", "", "", $"Target language ({data.Language})");
    result.AddHeader("Biblebook translations", "Biblebook", "Expression", "Biblebook", "Expression", "", "Original inputs");
    foreach (var (bookName, d) in data.BibleBooks.BibleBookData) {
      result.AddRegexRow("", bookName, d.TranslatedName ?? "", d.RegexParts);
    }

    result.AddRow();
    result.AddHeader("Visual translations");
    result.AddRow("Word for loading a bible tekst", "Loading...", "", data.LoadingStatus);
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

  private static void AddRegexRow(this List<RegexRow> result, string description, string english,
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

  private static void AddHeader(this List<RegexRow> result, params string[] values) {
    result.Add(new RegexRow(values) { IsHeader = true });
  }

  private static void AddRow(this List<RegexRow> result, params string[] values) => result.Add(new RegexRow(values));

  public static void ExportRawDataArray(RegexRow[] data, Uri filePath) {
    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add("Translation sheet");

    for (int row = 0; row < data.Length; row++) {
      for (int col = 0; col < data[row].Length; col++) {
        worksheet.Cell(row + 1, col + 1).Value = data[row][col];

        if (data[row].IsHeader) {
          worksheet.Cell(row + 1, col + 1).Style.Font.SetBold();
        }
      }
    }

    workbook.SaveAs(filePath.AbsolutePath);
  }

  public class RegexRow {
    public string[] RawData { get; }
    public bool IsHeader { get; init; }

    public RegexRow(string[] rawData) {
      RawData = rawData;
    }

    public int Length => RawData.Length;
    public string this[int col] => RawData[col];
  }
}
