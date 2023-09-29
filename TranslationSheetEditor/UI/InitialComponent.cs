using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.UI;

public sealed class InitialComponent : CanvasComponentBase {
  private Settings? _settings;
  private Settings Settings => _settings ??= SettingsFiles.Get.GetSettings<Settings>();

  private TextBlock _lastTextBlock = null!;
  private Button? _lastAddedLanguageButton;
  private TextBox _newLanguageTextBox = null!;

  protected override void InitializeControls() {
    Settings.FirstTimeInit();

    AddTextBlockHeader("Bible-link translation sheet editor").TopLeftInPanel();
    AddTextBlock("This application will generate the bible link translation sheets for you.").Below();
    _lastTextBlock = AddTextBlock("For which language do you want to enter a translation?").Below();

    foreach (var language in Settings.Languages) {
      AddLanguageButton(language);
    }

    _newLanguageTextBox = AddTextBox().Width(400).MaxWidth(400).BottomLeftInPanel().WithInitialFocus();
    AddButton("Add", OnAddLanguageClick).RightOf();
    AddLabelAbove("Add a new language:", _newLanguageTextBox);

    Settings.SelectedLanguage = null;
  }

  private void OnAddLanguageClick(RoutedEventArgs _) {
    string? language = _newLanguageTextBox.Text;
    if (string.IsNullOrWhiteSpace(language)) {
      _newLanguageTextBox.Focus();
      return;
    }

    if (Settings.Languages is null) {
      throw new InvalidOperationException("Settings.Languages should've been initialised by now.");
    }
    Settings.Languages.Add(language);
    SettingsFiles.Get.SaveSettings();

    AddLanguageButton(language);
    _newLanguageTextBox.Text = "";
    _newLanguageTextBox.Focus();
    HandleResize();
  }

  private void AddLanguageButton(string language) {
    var previousControl = (Control?)_lastAddedLanguageButton ?? _lastTextBlock;
    _lastAddedLanguageButton = AddButton(language, e => OnNextClick(language, e))
        .YBelow(previousControl).XCenterInPanel();
  }

  private void OnNextClick(string language, RoutedEventArgs _) {
    Settings.SelectedLanguage = language;
    FindWindow().WithSettingsFile<TranslationData>($"translation-sheet-editor-data-{language}.json");
    SwitchToComponent<BookListComponent>();
  }
}
