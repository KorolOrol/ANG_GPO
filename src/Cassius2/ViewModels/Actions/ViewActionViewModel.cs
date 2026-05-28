using System.Collections.ObjectModel;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using Cassius2.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cassius2.ViewModels.Actions;

public partial class ViewActionViewModel : ViewModelBase
{
    public ObservableCollection<IElement> Elements { get; } = new();

    [ObservableProperty]
    private IElement? _selectedElement;

    public ViewActionViewModel()
    {
        RefreshElements();
        AppState.PlotChanged += RefreshElements;
    }

    public void RefreshElements()
    {
        Elements.Clear();
        foreach (var el in AppState.Plot.Elements)
        {
            Elements.Add(el);
        }
    }

    [RelayCommand]
    private void AddCharacter() => AddElement(ElemType.Character, "New Character");

    [RelayCommand]
    private void AddItem() => AddElement(ElemType.Item, "New Item");

    [RelayCommand]
    private void AddLocation() => AddElement(ElemType.Location, "New Location");

    [RelayCommand]
    private void AddEvent() => AddElement(ElemType.Event, "New Event");

    private void AddElement(ElemType type, string defaultName)
    {
        var newElement = new Element(type, defaultName);
        AppState.Plot.Add(newElement);
        RefreshElements();
        SelectedElement = newElement;
    }

    [RelayCommand]
    private void DeleteElement()
    {
        if (SelectedElement != null)
        {
            AppState.Plot.Remove(SelectedElement);
            SelectedElement = null;
            RefreshElements();
        }
    }
}
