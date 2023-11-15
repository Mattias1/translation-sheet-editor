using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using AvaloniaExtensions;

namespace TranslationSheetEditor.UI;

public sealed class ExpandingTextBoxes {
  private CanvasComponentBase _parent;
  private double _textBoxWidth;
  private List<TextBox> _textBoxes;
  private Action<TextBox>? _stretchFunc;
  private Func<string, string>? _validationFunc;

  public Label Label { get; private set; } = null!;
  public Label? Description { get; private set; }
  public TextBlock Validation { get; private set; }

  public IReadOnlyList<TextBox> TextBoxes => _textBoxes.AsReadOnly();
  public TextBox FirstTextBox => _textBoxes[0];
  public TextBox LastTextBox => _textBoxes[^1];
  public TextBox ButOneLastTextBox => _textBoxes[^2];

  public List<string> Data {
    get => _textBoxes.Where(tb => !string.IsNullOrWhiteSpace(tb.Text)).Select(tb => tb.Text ?? "").ToList();
    set {
      for (int i = 0; i < value.Count; i++) {
        if (_textBoxes.Count <= i) {
          AddTextBoxBelow();
        }
        _textBoxes[i].Text = value[i];
      }
    }
  }

  private ExpandingTextBoxes(CanvasComponentBase parent, double textBoxWidth, Action<TextBox>? stretchFunc) {
    _parent = parent;
    _textBoxes = new List<TextBox>();
    _textBoxWidth = textBoxWidth;
    _stretchFunc = stretchFunc;
  }

  public ExpandingTextBoxes IsVisible(bool isVisible) {
    _textBoxes.ForEach(tb => tb.IsVisible(isVisible));
    Label.IsVisible(isVisible);
    Description?.IsVisible(isVisible);
    Validation.IsVisible(isVisible);
    return this;
  }

  /// <summary>
  /// Run validation (return error message, empty if ok) on text change of any of the textboxes
  /// </summary>
  /// <param name="validationFunc"></param>
  public void ValidateOnChange(Func<string, string> validationFunc) {
    _validationFunc = validationFunc;
    _textBoxes.ForEach(tb => SetupTextBoxValidation(tb));
  }

  private TextBox SetupTextBoxValidation(TextBox tb) {
    return tb.OnTextChanged(e => {
      var text = (e.Source as TextBox)?.Text;
      if (text is not null && _validationFunc is not null) {
        SetError(_validationFunc(text));
      }
    });
  }

  public void SetError(string errorMessage) {
    Validation.Text = errorMessage;
    Validation.Foreground = Brushes.Red;
  }

  private void OnTextChanged(TextChangedEventArgs e) {
    if (ReferenceEquals(e.Source, LastTextBox) && !string.IsNullOrEmpty(LastTextBox.Text)) {
      AddTextBoxBelow();
    }
  }

  private TextBox AddTextBoxBelow() {
    var textBox = AddTextBox().Below(ButOneLastTextBox);
    _stretchFunc?.Invoke(textBox);
    _parent.RepositionControls();
    return textBox;
  }

  private TextBox AddTextBox() {
    var textBox = _parent.AddTextBox().OnTextChanged(OnTextChanged);
    if (!double.IsNaN(_textBoxWidth)) {
      textBox.MinWidth(_textBoxWidth).Width(_textBoxWidth).MaxWidth(_textBoxWidth);
    }
    if (_textBoxes.Count > 0) {
      textBox.IsVisible(FirstTextBox.IsVisible);
    }
    _textBoxes.Add(textBox);
    if (_validationFunc is not null) {
      SetupTextBoxValidation(textBox);
    }
    return textBox;
  }

  private void AddLabels(string text, string? description, double labelWidth) {
    Label = _parent.InsertLabelLeftOf(text, FirstTextBox, labelWidth);
    if (description is not null) {
      Description = _parent.AddLabel(description, FirstTextBox).Below(Label);
    }
    Validation = _parent.AddTextBlock("").Below(Description ?? Label);
  }

  public override string ToString() => $"ExpandingTexBoxes-{Label.Content}";

  public static ExpandingTextBoxes Add(CanvasComponentBase parent, Func<TextBox, TextBox> initialPositionFunc,
      string labelText, string? description = null, double textBoxWidth = double.NaN, double labelWidth = double.NaN) {
    var expandingTextboxes = new ExpandingTextBoxes(parent, textBoxWidth, null);

    initialPositionFunc(expandingTextboxes.AddTextBox());
    expandingTextboxes.AddLabels(labelText, description, labelWidth);
    expandingTextboxes.AddTextBoxBelow();

    return expandingTextboxes;
  }

  public static ExpandingTextBoxes Add(CanvasComponentBase parent, Func<TextBox, TextBox> initialPositionFunc,
      Action<TextBox> stretchFunc, string labelText, string? description = null, double labelWidth = double.NaN) {
    var expandingTextboxes = new ExpandingTextBoxes(parent, double.NaN, stretchFunc);

    initialPositionFunc(expandingTextboxes.AddTextBox());
    expandingTextboxes.AddLabels(labelText, description, labelWidth);
    stretchFunc(expandingTextboxes.FirstTextBox); // Manually stretch the first text box after labels are inserted
    expandingTextboxes.AddTextBoxBelow();

    return expandingTextboxes;
  }
}
