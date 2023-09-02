using Avalonia;
using Avalonia.Markup.Declarative;
using AvaloniaExtensions;
using TranslationSheetEditor;
using TranslationSheetEditor.UI;

AppBuilderExtensions.Init().StartDesktopApp(() => ExtendedWindow.Init<BookListComponent>("Translation sheet editor")
    .WithSettingsFile<Settings>("./translation-sheet-editor-settings.json")
    .WithSize(new Size(800, 500), new Size(400, 250))
    .Icon(AssetExtensions.LoadWindowIcon("Assets/BibleLink.ico")));
