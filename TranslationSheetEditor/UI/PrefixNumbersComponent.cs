using Avalonia.Markup.Declarative;
using Avalonia.Media;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.UI;

public sealed class PrefixNumbersComponent : CanvasComponentBase {
  private const int LABEL_WIDTH = 110;

  private TranslationData? _data;
  private TranslationData Data => _data ??= SettingsFiles.Get.GetSettings<TranslationData>();

  private ExpandingTextBoxes _tbFirst = null!;
  private ExpandingTextBoxes _tbSecond = null!;
  private ExpandingTextBoxes _tbThird = null!;

  protected override void InitializeControls() {
    var lbl1 = AddTextBlockHeader("Prefix numbers").TopLeftInPanel().StretchFractionRightInPanel(1, 4);
    var lbl2 = AddTextBlock($"(language: {Data.Language})").FontWeight(FontWeight.Bold).FontStyle(FontStyle.Italic)
        .XRightOf(lbl1).YAlignBottom(lbl1).StretchFractionRightInPanel(1, 3);
    var lbl3 = AddTextBlock(".").RightOf(lbl2);
    AddTextBlock("Below you can enter the numbers prefixing some biblebooks (like Kings, Corinthians and John).")
        .Below(lbl1);

    _tbFirst = ExpandingTextBoxes.Add(this, tb => tb.Below(),
        tb => tb.StretchRightTo(lbl2), "1 (First)", "(e.g. '1', 'I',\n '1st', 'First')", LABEL_WIDTH);
    _tbSecond = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbFirst.FirstTextBox),
        tb => tb.StretchRightTo(lbl3), "2 (Second)", "(e.g. '2', 'II',\n '2nd', 'Second')", LABEL_WIDTH);
    _tbThird = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbSecond.FirstTextBox),
        tb => tb.StretchFractionRightInPanel(1, 2), "3 (Third)", "(e.g. '3', 'III',\n '3rd', 'Third')", LABEL_WIDTH);

    NavigationControls.Add(this, SaveData);
  }

  protected override void OnSwitchingToComponent() {
    _data = null;
    LoadData();
  }

  private void LoadData() {
    _tbFirst.Data = Data.PrefixNumberOptionsForFirst;
    _tbSecond.Data = Data.PrefixNumberOptionsForSecond;
    _tbThird.Data = Data.PrefixNumberOptionsForThird;
  }

  private void SaveData() {
    Data.PrefixNumberOptionsForFirst = _tbFirst.Data;
    Data.PrefixNumberOptionsForSecond = _tbSecond.Data;
    Data.PrefixNumberOptionsForThird = _tbThird.Data;

    SettingsFiles.Get.SaveSettings();
  }
}
