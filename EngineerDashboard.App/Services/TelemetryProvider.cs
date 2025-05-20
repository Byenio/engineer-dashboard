using System.Reactive.Subjects;
using System.Windows.Threading;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.Services;

public class TelemetryProvider : IDisposable
{
   private readonly TelemetryClient _telemetryClient = new(20777);

   private readonly Subject<CarTelemetryPacket> _carTelemetrySubject = new();
   public IObservable<CarTelemetryPacket> CarTelemetryStream => _carTelemetrySubject;

   public TelemetryProvider()
   {
      _telemetryClient.OnCarTelemetryDataReceive += packet => _carTelemetrySubject.OnNext(packet);
   }

   public void Dispose()
   {
      _telemetryClient.Stop();
      
      _carTelemetrySubject.OnCompleted();
      _carTelemetrySubject.Dispose();
   }
}