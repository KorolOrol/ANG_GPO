using System;
using BaseClasses.Interface;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cassius2.ViewModels.Controls;

public partial class EditableElementListItemViewModel : ViewModelBase
{
    public IElement Element { get; set; }
    
    [ObservableProperty]
    private string _displayText = "";
    
    public event Action? ElementEdited;

    public void Setup(IElement element)
    {
        Element = element;
        DisplayText = Element.ToString()!;
    }

    public void NotifyEdited()
    {
        ElementEdited?.Invoke();
    }
}