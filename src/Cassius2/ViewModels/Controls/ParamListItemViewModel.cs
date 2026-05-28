using System;
using CommunityToolkit.Mvvm.ComponentModel;
using BaseClasses.Interface;

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
