using System.ComponentModel;
using System.Runtime.CompilerServices;
using EngineerDashboard.App.Helpers;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Data;

namespace EngineerDashboard.App.ViewModels;

public class TyreSetDataViewModel : INotifyPropertyChanged
{
    private readonly TyreSetData _tyreSetData;

    public TyreSetDataViewModel(TyreSetData tyreSetData)
    {
        _tyreSetData = tyreSetData;
    }

    public bool Available => _tyreSetData.available != 0;
    public VisualTyreCompound VisualTyreCompound => _tyreSetData.visualTyreCompound;
    public TyreCompound ActualTyreCompound => _tyreSetData.actualTyreCompound;
    public byte Wear => _tyreSetData.wear;
    public byte RecommendedSession => _tyreSetData.recommendedSession;
    public byte LifeSpan => _tyreSetData.lifeSpan;
    public byte UsableLife => _tyreSetData.usableLife;
    public short LapDeltaTime => _tyreSetData.lapDeltaTime;
    public bool Fitted => _tyreSetData.fitted != 0;
    
    public string TyreInfo => $"Wear: {Wear}%\nLife Span: {LifeSpan} laps\nUsable Life: {UsableLife} laps\nLap Delta Time: {Formatter.FormatMsToDeltaString(LapDeltaTime)}ms";

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}