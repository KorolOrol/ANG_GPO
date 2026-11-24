using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Interface;
using BaseClasses.Services;
using Cassius2.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cassius2.ViewModels.Windows;

public partial class RelationEditWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private IEnumerable<IElement> _availableTargets = [];

    [ObservableProperty]
    private IElement? _selectedTarget;

    [ObservableProperty]
    private string _paramNamespace = "Relations";

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
    private bool _isTargetSelectionEnabled = true;

    [ObservableProperty]
    private bool _isEditing;

    public object? ParsedValue { get; private set; }

    public Action<bool>? CloseAction { get; set; }

    public void SetupForEdit(IElement target, IParamKey key, string valueJson)
    {
        AvailableTargets = [target];
        SelectedTarget = target;
        IsTargetSelectionEnabled = false;

        ParamNamespace = key.Namespace;
        ParamName = key.Name;
        ParamValueType = TypeNameHelper.GetTypeName(key.ValueType);
        ParamValueJson = valueJson;
        IsEditing = true;
    }

    public void SetupForAdd(IEnumerable<IElement> availableTargets)
    {
        AvailableTargets = availableTargets.ToList();
        SelectedTarget = AvailableTargets.FirstOrDefault();
        IsTargetSelectionEnabled = true;

        ParamNamespace = "Relations";
        ParamName = "";
        ParamValueType = "string";
        ParamValueJson = "null";
        IsEditing = false;
    }

    [RelayCommand]
    private void Ok()
    {
        try
        {
            if (SelectedTarget == null)
                throw new Exception("Please select a target.");

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
