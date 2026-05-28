using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BaseClasses.Interface;
using Cassius2.Views.Windows;

namespace Cassius2.Views.Controls;

public partial class EditableElementListItem : UserControl
{
    public static readonly StyledProperty<IElement?> ElementProperty =
        AvaloniaProperty.Register<EditableElementListItem, IElement?>(nameof(Element));

    public IElement? Element
    {
        get => GetValue(ElementProperty);
        set => SetValue(ElementProperty, value);
    }
    
    public EditableElementListItem()
    {
        InitializeComponent();
        UpdateData();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ElementProperty)
        {
            UpdateData();
        }
    }

    private void UpdateData()
    {
        ParamValue.Text = Element?.Name ?? string.Empty;
    }
    
    private async void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (Element is null) return;

            Cassius2.Models.AppState.NotifyEditElementRequested(Element);
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            await exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(this) as Window)!);
        }
    }
}