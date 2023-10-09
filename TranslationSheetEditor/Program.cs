using Avalonia;
using Avalonia.Markup.Declarative;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.UI;

AvaloniaExtensionsApp.Init()
    .WithSettingsFile<Settings>("./translation-sheet-editor-settings.json")
    .StartDesktopApp(() => ExtendedWindow.Init<InitialComponent>("Translation sheet editor")
        .AddLazyComponent<BookListComponent>()
        .AddLazyComponent<MiscDataComponent>()
        .AddLazyComponent<RegexComponent>()
        .AddLazyComponent<ExportComponent>()
        .WithSize(new Size(1300, 740), new Size(1240, 740)) // Laptop screens are 1377x768
        .Icon(AssetExtensions.LoadWindowIcon("Assets/BibleLink.ico")));
