using System.Windows.Threading;
using EngineerDashboard.Telemetry;
using EngineerDashboard.Telemetry.Packets;

namespace EngineerDashboard.App.Services;

public class TelemetryService : IDisposable
{
   public TelemetryClient TelemetryClient { get; } = new TelemetryClient(20777);

   public void Dispose()
   {
      TelemetryClient.Stop();
   }
}