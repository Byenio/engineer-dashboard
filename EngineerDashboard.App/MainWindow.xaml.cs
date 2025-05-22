using System.ComponentModel;
using System.Windows;
using EngineerDashboard.App.Services;
using EngineerDashboard.App.ViewModels;
using EngineerDashboard.App.Views;

namespace EngineerDashboard.App;

public partial class MainWindow : Window
{
    public MainWindow(SessionInfoView sessionInfoView, DriversTableView driversTableView)
    {
        InitializeComponent();
        
        SessionInfoControl.Content = sessionInfoView;
        DriversTableView.Content = driversTableView;
    }
}