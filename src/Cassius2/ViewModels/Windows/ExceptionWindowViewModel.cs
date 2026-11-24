using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cassius2.ViewModels.Windows;

public partial class ExceptionWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _exceptionMessage;
    
    public Action<bool>? CloseAction { get; set; }

    public void LoadException(Exception exception)
    {
        ExceptionMessage = exception.Message;
    }
    
    [RelayCommand]
    private void Close()
    {
        CloseAction?.Invoke(false);
    }
}