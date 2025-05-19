using System.Runtime.InteropServices;
using EngineerDashboard.Telemetry.Data;

namespace EngineerDashboard.Telemetry.Packets;

/// <summary>
/// The motion packet gives physical data for all the cars being driven.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MotionPacket
{
    /// <summary>
    /// Packet header
    /// </summary>
    public PacketHeader header;
    /// <summary>
    /// Motion data for all cars on track
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
    public MotionData[] carMotionData;
}