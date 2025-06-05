using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EngineerDashboard.App.Services;
using EngineerDashboard.App.Views;

namespace EngineerDashboard.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public object SessionInfoView { get; }

    private readonly DriversTableView _driversTableView;
    private readonly TyreCardView _tyreCardView;
    private readonly LapTimeChartView _lapTimeChartView;
    private readonly TyreWearChartView _tyreWearChartView;
    private readonly FuelChartView _fuelChartView;
    private readonly BatteryChartView _batteryChartView;
    private readonly InputsChartView _inputsChartView;
    private readonly TelemetryChartView _telemetryChartView;
    private readonly CarInfoCardView _carInfoCardView;
    private readonly DamageCardView _damageCardView;

    [ObservableProperty] private object _currentPageView;

    private enum Page
    {
        None,
        Drivers,
        CarSetup,
        Charts,
        InputsAndTelemetry,
        Database
    }

    private Page _currentPage = Page.None;

    public MainWindowViewModel(
        TelemetryProvider telemetryProvider,
        SessionInfoView sessionInfoView,
        DriversTableView driversTableView,
        TyreCardView tyreCardView,
        LapTimeChartView lapTimeChartView,
        TyreWearChartView tyreWearChartView,
        FuelChartView fuelChartView,
        BatteryChartView batteryChartView,
        InputsChartView inputsChartView,
        TelemetryChartView telemetryChartView,
        CarInfoCardView carInfoCardView,
        DamageCardView damageCardView)
    {
        SessionInfoView = sessionInfoView;

        _driversTableView = driversTableView;
        _tyreCardView = tyreCardView;
        _lapTimeChartView = lapTimeChartView;
        _tyreWearChartView = tyreWearChartView;
        _fuelChartView = fuelChartView;
        _batteryChartView = batteryChartView;
        _inputsChartView = inputsChartView;
        _telemetryChartView = telemetryChartView;
        _carInfoCardView = carInfoCardView;
        _damageCardView = damageCardView;

        ShowDriversPage();
    }

    [RelayCommand]
    private void ShowDriversPage()
    {
        if (_currentPage == Page.Drivers)
            return;

        _currentPage = Page.Drivers;
        CurrentPageView = _driversTableView;
    }

    [RelayCommand]
    private void ShowCarSetup()
    {
        if (_currentPage == Page.CarSetup)
            return;

        _currentPage = Page.CarSetup;
        
        RemoveFromParent(_tyreCardView);
        RemoveFromParent(_carInfoCardView);
        RemoveFromParent(_damageCardView);

        var carInfoGrid = new Grid();
        carInfoGrid.RowDefinitions.Add(new RowDefinition());
        carInfoGrid.RowDefinitions.Add(new RowDefinition());
        carInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        carInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });

        carInfoGrid.SetValue(Grid.MarginProperty, new Thickness(30, 10, 30, 10));
        
        _carInfoCardView.SetValue(Grid.RowProperty, 0);
        _carInfoCardView.SetValue(Grid.ColumnProperty, 0);
        
        _tyreCardView.SetValue(Grid.RowProperty, 1);
        _tyreCardView.SetValue(Grid.ColumnProperty, 0);
        
        _damageCardView.SetValue(Grid.RowProperty, 0);
        _damageCardView.SetValue(Grid.ColumnProperty, 1);
        _damageCardView.SetValue(Grid.RowSpanProperty, 2);
        
        carInfoGrid.Children.Add(_tyreCardView);
        carInfoGrid.Children.Add(_carInfoCardView);
        carInfoGrid.Children.Add(_damageCardView);
       
        CurrentPageView = carInfoGrid;
    }

    [RelayCommand]
    private void ShowChartsPage()
    {
        if (_currentPage == Page.Charts)
            return;

        _currentPage = Page.Charts;

        RemoveFromParent(_lapTimeChartView);
        RemoveFromParent(_tyreWearChartView);
        RemoveFromParent(_fuelChartView);
        RemoveFromParent(_batteryChartView);

        var chartGrid = new Grid();
        chartGrid.ColumnDefinitions.Add(new ColumnDefinition());
        chartGrid.ColumnDefinitions.Add(new ColumnDefinition());
        chartGrid.RowDefinitions.Add(new RowDefinition());
        chartGrid.RowDefinitions.Add(new RowDefinition());
        
        chartGrid.SetValue(Grid.MarginProperty, new Thickness(30, 0, 30, 0));

        _lapTimeChartView.SetValue(Grid.ColumnProperty, 0);
        _lapTimeChartView.SetValue(Grid.RowProperty, 0);
        
        _tyreWearChartView.SetValue(Grid.ColumnProperty, 1);
        _tyreWearChartView.SetValue(Grid.RowProperty, 0);
        
        _fuelChartView.SetValue(Grid.ColumnProperty, 0);
        _fuelChartView.SetValue(Grid.RowProperty, 1);
        
        _batteryChartView.SetValue(Grid.ColumnProperty, 1);
        _batteryChartView.SetValue(Grid.RowProperty, 1);

        chartGrid.Children.Add(_lapTimeChartView);
        chartGrid.Children.Add(_tyreWearChartView);
        chartGrid.Children.Add(_fuelChartView);
        chartGrid.Children.Add(_batteryChartView);

        CurrentPageView = chartGrid;
    }
    
    [RelayCommand]
    private void ShowInputsAndTelemetryPage()
    {
        if (_currentPage == Page.InputsAndTelemetry)
            return;
        
        _currentPage = Page.InputsAndTelemetry;
        
        RemoveFromParent(_inputsChartView);
        RemoveFromParent(_telemetryChartView);
        
        var telemetryGrid = new Grid();
        telemetryGrid.RowDefinitions.Add(new RowDefinition());
        telemetryGrid.RowDefinitions.Add(new RowDefinition());
        
        telemetryGrid.SetValue(Grid.MarginProperty, new Thickness(30, 0, 30, 0));
        
        _inputsChartView.SetValue(Grid.RowProperty, 0);
        _telemetryChartView.SetValue(Grid.RowProperty, 1);
        
        telemetryGrid.Children.Add(_inputsChartView);
        telemetryGrid.Children.Add(_telemetryChartView);
        
        CurrentPageView = telemetryGrid;
    }

    private void RemoveFromParent(UIElement element)
    {
        if (element == null)
            return;

        var parent = LogicalTreeHelper.GetParent(element);

        switch (parent)
        {
            case Panel panel:
                panel.Children.Remove(element);
                break;
            case ContentControl contentControl:
                contentControl.Content = null;
                break;
        }
    }
}
