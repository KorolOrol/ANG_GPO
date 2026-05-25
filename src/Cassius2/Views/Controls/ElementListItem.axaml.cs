using Avalonia;
using Avalonia.Controls;

namespace Cassius2.Views.Controls;

public partial class ElementListItem : UserControl
{
    public static readonly StyledProperty<string> ItemTextProperty =
        AvaloniaProperty.Register<ElementListItem, string>(nameof(ItemText), "Label");

    public static readonly StyledProperty<Avalonia.Svg.Skia.Svg?> ItemIconProperty =
        AvaloniaProperty.Register<ElementListItem, Avalonia.Svg.Skia.Svg?>(nameof(ItemIcon));

    public string ItemText
    {
        get => GetValue(ItemTextProperty);
        set => SetValue(ItemTextProperty, value);
    }

    public Avalonia.Svg.Skia.Svg? ItemIcon
    {
        get => GetValue(ItemIconProperty);
        set => SetValue(ItemIconProperty, value);
    }

    public ElementListItem()
    {
        InitializeComponent();
    }
}