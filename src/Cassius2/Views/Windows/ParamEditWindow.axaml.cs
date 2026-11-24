using Avalonia.Controls;
using BaseClasses.Interface;
using Cassius2.ViewModels.Windows;

namespace Cassius2.Views.Windows;

public partial class ParamEditWindow : Window
{
    public ParamEditWindowViewModel ViewModel { get; }

    public string ParamNamespace => ViewModel.ParamNamespace;
    public string ParamName => ViewModel.ParamName;
    public string ParamValueType => ViewModel.ParamValueType;
    public object? ParsedValue => ViewModel.ParsedValue;

    public ParamEditWindow()
    {
        InitializeComponent();
        ViewModel = new ParamEditWindowViewModel();
        DataContext = ViewModel;
        ViewModel.CloseAction = (result) => Close(result);
    }
    
    public void SetupForEdit(IParamKey key, string valueJson)
    {
        ViewModel.SetupForEdit(key, valueJson);
    }
    
    public void SetupForAdd()
    {
        ViewModel.SetupForAdd();
    }
}
