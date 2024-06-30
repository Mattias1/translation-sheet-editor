using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.UI;

public static class NavigationControls {
  private const int NAV_COMBOBOX_WIDTH = 195;
  private const int NAV_INDEX_OF_FIRST_BOOK = 5;

  private static int _currentIndex = 0;

  public static void Add(CanvasComponentBase componentBase, Action saveData) {
    var cbNavigation = AddNavigationCombobox(componentBase, saveData);

    var nextOrQuitButton = componentBase is ExportComponent
        ? AddQuitButton(componentBase).BottomRightInPanel()
        : AddNextButton(componentBase, cbNavigation).BottomRightInPanel();

    cbNavigation.LeftOf(nextOrQuitButton);

    if (componentBase is not BookListComponent) {
      AddPreviousButton(componentBase, cbNavigation).LeftOf(cbNavigation);
    }

    SetupOnLoaded(componentBase, cbNavigation);
  }

  private static Button AddQuitButton(CanvasComponentBase componentBase) {
    void OnQuitClick(RoutedEventArgs e) {
      componentBase.Quit();
    }

    return componentBase.AddButton("Quit", OnQuitClick);
  }

  private static Button AddNextButton(CanvasComponentBase componentBase, ComboBox cbNavigation) {
    void OnNextClick(RoutedEventArgs e) {
      cbNavigation.SelectedIndex += 1;
    }

    return componentBase.AddButton("Next", OnNextClick);
  }

  private static Button AddPreviousButton(CanvasComponentBase componentBase, ComboBox cbNavigation) {
    void OnPreviousClick(RoutedEventArgs e) {
      cbNavigation.SelectedIndex -= 1;
    }

    return componentBase.AddButton("Previous", OnPreviousClick);
  }

  private static ComboBox AddNavigationCombobox(CanvasComponentBase componentBase, Action saveData) {
    var navItems = new List<string> {
        "Initial book names", "Other translations", "Other tran… (part 2)", "Prefix numbers", "-----------------------"
    };
    navItems.AddRange(BibleBooks.ALL_BOOKS);
    navItems.AddRange(new []{ "----------------------", "Export result" }); // This has one less dash

    void OnSelectedItemChanged(SelectedItemChangedEventArgs<string> e) {
      saveData();

      int i = navItems.IndexOf(e.SelectedItem);
      if (i == _currentIndex) {
        return;
      }
      if (e.SelectedItem.StartsWith("---")) {
        i += i > _currentIndex ? 1 : -1;
      }

      if (i == 0) {
        componentBase.SwitchToComponent<BookListComponent>();
      } else if (i == 1) {
        componentBase.SwitchToComponent<MiscDataComponent>();
      } else if (i == 2) {
        componentBase.SwitchToComponent<MiscDataComponentPartTwo>();
      } else if (i == 3) {
        componentBase.SwitchToComponent<PrefixNumbersComponent>();
      } else if (i >= BibleBooks.ALL_BOOKS.Length + NAV_INDEX_OF_FIRST_BOOK) {
        componentBase.SwitchToComponent<ExportComponent>();
      } else {
        i = Math.Max(i, NAV_INDEX_OF_FIRST_BOOK);
        if (componentBase is RegexComponent currentRegexComponent) {
          currentRegexComponent.ChangeBookIndex(i - NAV_INDEX_OF_FIRST_BOOK);
        } else {
          var targetRegexComponent = componentBase.SwitchToComponent<RegexComponent>();
          targetRegexComponent.ChangeBookIndex(i - NAV_INDEX_OF_FIRST_BOOK);
        }
      }

      _currentIndex = i;
    }

    return componentBase.AddComboBox(navItems, OnSelectedItemChanged).Width(NAV_COMBOBOX_WIDTH);
  }

  private static void SetupOnLoaded(CanvasComponentBase componentBase, ComboBox cbNavigation) {
    componentBase.OnLoaded(_ => {
      cbNavigation.SelectedIndex = _currentIndex;
    });
  }
}
