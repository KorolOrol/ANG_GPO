using System;
using Avalonia;
using Avalonia.Controls;
using BaseClasses.Interface;

namespace Cassius2.Views.Controls;

public partial class ElementListItem : UserControl
{
    public static readonly StyledProperty<IElement?> ElementProperty =
        AvaloniaProperty.Register<ElementListItem, IElement?>(nameof(Element));

    public IElement? Element
    {
        get => GetValue(ElementProperty);
        set => SetValue(ElementProperty, value);
    }

    public ElementListItem()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ElementProperty)
        {
            UpdateData();
        }
    }

    /// <summary>
    /// Метод обновления UI. Имеет сигнатуру EventHandler для удобной подписки на внешние события
    /// </summary>
    public void UpdateData(object? sender = null, EventArgs? e = null)
    {
        if (Element is { } element)
        {
            ElementName.Text = element.Name;
            ElementIcon.Path = $"/Assets/Icons/ElemType{element.Type}Icon.svg";
        }
        else
        {
            ElementName.Text = string.Empty;
            ElementIcon.Path = string.Empty;
        }
    }
}