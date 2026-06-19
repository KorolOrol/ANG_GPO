using System;
using Avalonia.Controls;
using Cassius2.ViewModels.Windows;

namespace Cassius2.Views.Windows;

public partial class ExceptionWindow : Window
{
    public ExceptionWindowViewModel ViewModel { get; }
    
    public ExceptionWindow()
    {
        InitializeComponent();
        ViewModel = new ExceptionWindowViewModel();
        DataContext = ViewModel;
        ViewModel.CloseAction = (result) => Close(result);
    }

    public void LoadException(Exception exception)
    {
        ViewModel.LoadException(exception);
    }
}