using System;
using CommunityToolkit.Mvvm.Input;

namespace Cassius2.ViewModels.Windows;

public partial class ConfirmWindowViewModel : ViewModelBase
{
    public Action<bool>? CloseAction { get; set; }

    [RelayCommand]
    private void Discard()
    {
        CloseAction?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseAction?.Invoke(false);
    }
}
