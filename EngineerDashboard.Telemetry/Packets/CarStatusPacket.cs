using System.Runtime.InteropServices;
using EngineerDashboard.Telemetry.Data;

namespace EngineerDashboard.Telemetry.Packets;

/// <summary>
/// This packet details car statuses for all the cars in the race.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CarStatusPacket
{
    /// <summary>
    /// Packet header
    /// </summary>
    public PacketHeader header;
    /// <summary>
    /// List of car statuses
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
    public CarStatusData[] carStatusData;
}