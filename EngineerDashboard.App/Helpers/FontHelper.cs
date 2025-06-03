using System.Diagnostics;
using SkiaSharp;

namespace EngineerDashboard.App.Helpers;

public static class FontHelper
{
    public static SKTypeface LoadCustomFont()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Assets/Fonts/JetBrainsMono-Regular.ttf");
            var info = System.Windows.Application.GetResourceStream(uri);
            if (info != null)
            {
                return SKTypeface.FromStream(info.Stream);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load custom font: {ex.Message}");
        }
        
        return SKTypeface.FromFamilyName("Consolas") ?? SKTypeface.Default;
    }
    
    public static SKTypeface LoadCustomFont(string fontFileName)
    {
        try
        {
            var uri = new Uri($"pack://application:,,,/Assets/Fonts/{fontFileName}");
            var info = System.Windows.Application.GetResourceStream(uri);
            if (info != null)
            {
                return SKTypeface.FromStream(info.Stream);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load custom font: {ex.Message}");
        }
        
        return SKTypeface.FromFamilyName("Consolas") ?? SKTypeface.Default;
    }
}