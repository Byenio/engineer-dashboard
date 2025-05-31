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

    [ObservableProperty] private object _currentPageView;

    private enum Page
    {
        None,
        Drivers,
        CarSetup,
        Charts
    }

    private Page _currentPage = Page.None;

    public MainWindowViewModel(
        TelemetryProvider telemetryProvider,
        SessionInfoView sessionInfoView,
        DriversTableView driversTableView,
        TyreCardView tyreCardView,
        LapTimeChartView lapTimeChartView,
        TyreWearChartView tyreWearChartView)
    {
        SessionInfoView = sessionInfoView;

        _driversTableView = driversTableView;
        _tyreCardView = tyreCardView;
        _lapTimeChartView = lapTimeChartView;
        _tyreWearChartView = tyreWearChartView;

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
        CurrentPageView = _tyreCardView;
    }

    [RelayCommand]
    private void ShowChartsPage()
    {
        if (_currentPage == Page.Charts)
            return;

        _currentPage = Page.Charts;

        RemoveFromParent(_lapTimeChartView);
        RemoveFromParent(_tyreWearChartView);

        var chartGrid = new Grid();
        chartGrid.ColumnDefinitions.Add(new ColumnDefinition());
        chartGrid.ColumnDefinitions.Add(new ColumnDefinition());

        _lapTimeChartView.SetValue(Grid.ColumnProperty, 0);
        _tyreWearChartView.SetValue(Grid.ColumnProperty, 1);

        chartGrid.Children.Add(_lapTimeChartView);
        chartGrid.Children.Add(_tyreWearChartView);

        CurrentPageView = chartGrid;
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
