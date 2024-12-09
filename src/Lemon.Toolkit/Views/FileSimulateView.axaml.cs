using Avalonia.Controls;
using Lemon.ModuleNavigation.Abstracts;

namespace Lemon.Toolkit.Views;

public partial class FileSimulateView : UserControl,IView
{
    public FileSimulateView()
    {
        InitializeComponent();
    }

    public void SetDataContext(IViewModel viewModel)
    {
        DataContext = viewModel;
    }
}