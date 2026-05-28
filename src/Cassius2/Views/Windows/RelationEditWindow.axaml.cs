using System.Collections.Generic;
using Avalonia.Controls;
using BaseClasses.Interface;
using Cassius2.ViewModels.Windows;

namespace Cassius2.Views.Windows;

public partial class RelationEditWindow : Window
{
    public RelationEditWindowViewModel ViewModel { get; }

    public IElement? SelectedTarget => ViewModel.SelectedTarget;
    public string ParamNamespace => ViewModel.ParamNamespace;
    public string ParamName => ViewModel.ParamName;
    public string ParamValueType => ViewModel.ParamValueType;
    public object? ParsedValue => ViewModel.ParsedValue;

    public RelationEditWindow()
    {
        InitializeComponent();
        ViewModel = new RelationEditWindowViewModel();
        DataContext = ViewModel;
        ViewModel.CloseAction = (result) => Close(result);
    }
    
    public void SetupForEdit(IElement target, IParamKey key, string valueJson)
    {
        ViewModel.SetupForEdit(target, key, valueJson);
    }
    
    public void SetupForAdd(IEnumerable<IElement> availableTargets)
    {
        ViewModel.SetupForAdd(availableTargets);
    }
}
