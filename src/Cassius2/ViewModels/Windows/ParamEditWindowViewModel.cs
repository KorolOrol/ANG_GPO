using System;
using BaseClasses.Interface;
using BaseClasses.Services;
using Cassius2.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cassius2.ViewModels.Windows;

public partial class ParamEditWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _paramNamespace = "Base";

    [ObservableProperty]
    private string _paramName = "";

    [ObservableProperty]
    private string _paramValueType = "string";

    [ObservableProperty]
    private string _paramValueJson = "null";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isErrorVisible;

    [ObservableProperty]
    private bool _isEditing;

    public object? ParsedValue { get; private set; }

    public Action<bool>? CloseAction { get; set; }

    public void SetupForEdit(IParamKey key, string valueJson)
    {
        ParamNamespace = key.Namespace;
        ParamName = key.Name;
        ParamValueType = TypeNameHelper.GetTypeName(key.ValueType);
        ParamValueJson = valueJson;
        IsEditing = true;
    }

    public void SetupForAdd()
    {
        IsEditing = false;
    }

    [RelayCommand]
    private void Ok()
    {
        try
        {
            IsErrorVisible = false;
            var type = TypeNameHelper.ResolveType(ParamValueType);
            ParsedValue = ValueParser.Parse(ParamValueJson, type);
            CloseAction?.Invoke(true);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsErrorVisible = true;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseAction?.Invoke(false);
    }
}
