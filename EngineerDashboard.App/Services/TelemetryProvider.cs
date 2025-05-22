using System.Reactive.Subjects;
using System.Windows.Threading;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Data;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.Services;

public class TelemetryProvider : IDisposable
{
   private readonly TelemetryClient _telemetryClient = new(20777);

   private readonly Subject<CarDamagePacket> _carDamageSubject = new();
   private readonly Subject<CarSetupPacket> _carSetupSubject = new();
   private readonly Subject<CarTelemetryPacket> _carTelemetrySubject = new();
   private readonly Subject<EventPacket> _eventSubject = new();
   private readonly Subject<FinalClassificationPacket> _finalClassificationSubject = new();
   private readonly Subject<LapDataPacket> _lapDataSubject = new();
   private readonly Subject<LobbyInfoPacket> _lobbyInfoSubject = new();
   private readonly Subject<MotionExPacket> _motionExSubject = new();
   private readonly Subject<MotionPacket> _motionSubject = new();
   private readonly Subject<ParticipantsPacket> _participantsSubject = new();
   private readonly Subject<SessionHistoryPacket> _sessionHistorySubject = new();
   private readonly Subject<SessionPacket> _sessionSubject = new();
   private readonly Subject<TyreSetPacket> _tyreSetSubject = new();
   
   public IObservable<CarDamagePacket> CarDamageStream => _carDamageSubject;
   public IObservable<CarSetupPacket> CarSetupStream => _carSetupSubject;
   public IObservable<CarTelemetryPacket> CarTelemetryStream => _carTelemetrySubject;
   public IObservable<EventPacket> EventStream => _eventSubject;
   public IObservable<FinalClassificationPacket> FinalClassificationStream => _finalClassificationSubject;
   public IObservable<LapDataPacket> LapDataStream => _lapDataSubject;
   public IObservable<LobbyInfoPacket> LobbyInfoStream => _lobbyInfoSubject;
   public IObservable<MotionExPacket> MotionExStream => _motionExSubject;
   public IObservable<MotionPacket> MotionStream => _motionSubject;
   public IObservable<ParticipantsPacket> ParticipantsStream => _participantsSubject;
   public IObservable<SessionHistoryPacket> SessionHistoryStream => _sessionHistorySubject;
   public IObservable<SessionPacket> SessionStream => _sessionSubject;
   public IObservable<TyreSetPacket> TyreSetStream => _tyreSetSubject;

   public TelemetryProvider()
   {
      _telemetryClient.OnCarDamageDataReceive += packet => _carDamageSubject.OnNext(packet);
      _telemetryClient.OnCarSetupDataReceive += packet => _carSetupSubject.OnNext(packet);
      _telemetryClient.OnCarTelemetryDataReceive += packet => _carTelemetrySubject.OnNext(packet);
      _telemetryClient.OnEventDetailsReceive += packet => _eventSubject.OnNext(packet);
      _telemetryClient.OnFinalClassificationDataReceive += packet => _finalClassificationSubject.OnNext(packet);
      _telemetryClient.OnLapDataReceive += packet => _lapDataSubject.OnNext(packet);
      _telemetryClient.OnLobbyInfoDataReceive += packet => _lobbyInfoSubject.OnNext(packet);
      _telemetryClient.OnMotionExDataReceive += packet => _motionExSubject.OnNext(packet);
      _telemetryClient.OnMotionDataReceive += packet => _motionSubject.OnNext(packet);
      _telemetryClient.OnParticipantsDataReceive += packet => _participantsSubject.OnNext(packet);
      _telemetryClient.OnSessionHistoryDataReceive += packet => _sessionHistorySubject.OnNext(packet);
      _telemetryClient.OnSessionDataReceive += packet => _sessionSubject.OnNext(packet);
      _telemetryClient.OnTyreSetDataReceive += packet => _tyreSetSubject.OnNext(packet);
   }

   public void Dispose()
   {
      _telemetryClient.Stop();
      
      _carDamageSubject.OnCompleted();
      _carDamageSubject.Dispose();
      
      _carSetupSubject.OnCompleted();
      _carSetupSubject.Dispose();
      
      _carTelemetrySubject.OnCompleted();
      _carTelemetrySubject.Dispose();
      
      _eventSubject.OnCompleted();
      _eventSubject.Dispose();
      
      _finalClassificationSubject.OnCompleted();
      _finalClassificationSubject.Dispose();
      
      _lapDataSubject.OnCompleted();
      _lapDataSubject.Dispose();
      
      _lobbyInfoSubject.OnCompleted();
      _lobbyInfoSubject.Dispose();
      
      _motionExSubject.OnCompleted();
      _motionExSubject.Dispose();
      
      _motionSubject.OnCompleted();
      _motionSubject.Dispose();
      
      _participantsSubject.OnCompleted();
      _participantsSubject.Dispose();
      
      _sessionHistorySubject.OnCompleted();
      _sessionHistorySubject.Dispose();

      _sessionSubject.OnCompleted();
      _sessionSubject.Dispose();
      
      _tyreSetSubject.OnCompleted();
      _tyreSetSubject.Dispose();
   }
}