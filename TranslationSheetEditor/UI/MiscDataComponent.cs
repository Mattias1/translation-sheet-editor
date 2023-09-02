using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.UI;

public sealed class MiscDataComponent : CanvasComponentBase {
  private const int LABEL_WIDTH = 130;

  private TranslationData? _data;
  private TranslationData Data => _data ??= GetSettings<TranslationData>();

  protected override void InitializeControls() {
    AddTextBlockHeader("TODO").TopLeftInPanel();

    // TODO

    AddButton("Next", OnNextClick).BottomRightInPanel();
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    base.OnLoaded(e);
    LoadData();
    // _bibleBookTbs[BibleBooks.GENESIS].Focus();
  }

  private void OnNextClick(RoutedEventArgs e) {
    SaveData();
  }

  private void LoadData() {
    // TODO
  }

  private void SaveData() {
    // TODO
  }
}
