namespace EngineerDashboard.App.Helpers;

public static class TimeHelper
{
    public static string FormatMsToLapTimeString(uint ms)
    {
        TimeSpan ts = TimeSpan.FromMilliseconds(ms);
        
        string formatted = string.Format("{0}:{1:D2}.{2:D3}",
            (int)ts.TotalMinutes,
            ts.Seconds,
            ts.Milliseconds);

        return formatted;
    }
    
    public static string FormatMsToSectorString(ushort ms)
    {
        TimeSpan ts = TimeSpan.FromMilliseconds(ms);
        
        string formatted = string.Format("{0}.{1:D3}",
            (int)ts.TotalSeconds,
            ts.Milliseconds);

        return formatted;
    }

    public static string FormatMsToDeltaString(ushort ms)
    {
        TimeSpan ts = TimeSpan.FromMilliseconds(ms);
        
        string formatted = string.Format("+{0}.{1:D3}",
            (int)ts.TotalSeconds,
            ts.Milliseconds);
        
        return formatted;
    }
}