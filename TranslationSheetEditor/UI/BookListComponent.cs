using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using AvaloniaExtensions;

namespace TranslationSheetEditor.UI;

public sealed class BookListComponent : CanvasComponentBase {
  private TextBox _tbInput = null!;

  protected override void InitializeControls() {
    AddTextBlockHeader("List of Bible Books").TopLeftInPanel();
    _tbInput = AddTextBox().Below();
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    base.OnLoaded(e);
    _tbInput.Focus();
  }

  protected override void OnGotFocus(GotFocusEventArgs e) {
    base.OnGotFocus(e);
    _tbInput.Focus();
  }
}
