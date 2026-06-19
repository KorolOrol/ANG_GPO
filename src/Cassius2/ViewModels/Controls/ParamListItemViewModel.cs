using System;
using BaseClasses.Interface;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cassius2.ViewModels.Controls;

public partial class ParamListItemViewModel : ViewModelBase
{
    public IParamKey? Key { get; private set; }

    [ObservableProperty]
    private string _displayText = "";

    [ObservableProperty]
    private object? _value;

    public event Action<IParamKey, object?>? ParamEdited;

    public void Setup(IParamKey key, object? value)
    {
        Key = key;
        Value = value;
        DisplayText = $"{key.Namespace}.{key.Name}: {value?.ToString() ?? "null"}";
    }

    public void NotifyEdited(object? newValue)
    {
        if (Key == null) return;
        Value = newValue;
        ParamEdited?.Invoke(Key, newValue);
    }
}
