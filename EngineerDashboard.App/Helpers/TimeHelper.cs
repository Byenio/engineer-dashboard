namespace EngineerDashboard.App.Helpers;

public static class TimeHelper
{
    public static string FormatMsToString(uint ms)
    {
        TimeSpan ts = TimeSpan.FromMilliseconds(ms);
        
        string formatted = string.Format("{0}:{1:D2}.{2:D3}",
            (int)ts.TotalMinutes,
            ts.Seconds,
            ts.Milliseconds);

        return formatted;
    }
}