using System.Runtime.InteropServices;
using EngineerDashboard.Telemetry.Data;

namespace EngineerDashboard.Telemetry.Packets;

/// <summary>
/// This packet details car damage parameters for all the cars in the race.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CarDamagePacket
{
    /// <summary>
    /// Packet header
    /// </summary>
    public PacketHeader header;
    /// <summary>
    /// List of damages of all cars
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
    public CarDamageData[] carDamageData;
}