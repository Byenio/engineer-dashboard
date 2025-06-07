using System.Windows.Media;
using EngineerDashboard.App.Helpers.Profiles;
using EngineerDashboard.Telemetry;

namespace EngineerDashboard.App.Helpers;

public static class ColorHelper
{
    public static Brush GetTeamBrush(Team team) => team switch
    {
        Team.MERCEDES => Brushes.Aqua,
        Team.FERRARI => Brushes.Red,
        Team.REDBULL => Brushes.RoyalBlue,
        Team.WILLIAMS => Brushes.DodgerBlue,
        Team.ASTONMARTIN => Brushes.ForestGreen,
        Team.ALPINE => Brushes.DeepSkyBlue,
        Team.ALPHATAURI => Brushes.SteelBlue,
        Team.HAAS => Brushes.LightGray,
        Team.MCLAREN => Brushes.DarkOrange,
        Team.ALFAROMEO => Brushes.DarkRed,
        Team.F1WORLD => Brushes.Goldenrod,
        _ => Brushes.Indigo
    };
    
    public static Brush GetRankBrush(string rankName) => rankName switch
    {
        "Bronze" => new SolidColorBrush(Color.FromRgb(205, 127, 50)),
        "Silver" => new SolidColorBrush(Color.FromRgb(192, 192, 192)),
        "Gold" => new SolidColorBrush(Color.FromRgb(255, 215, 0)),
        "Platinum" => new SolidColorBrush(Color.FromRgb(229, 228, 226)),
        "Master" => new SolidColorBrush(Color.FromRgb(138, 43, 226)),
        "Champion" => new SolidColorBrush(Color.FromRgb(255, 165, 0)),
        _ => Brushes.DimGray
    };
    
    public static Brush GetTyreBrush(VisualTyreCompound tyreCompound) => tyreCompound switch
    {
        VisualTyreCompound.SOFT => Brushes.Red,
        VisualTyreCompound.MEDIUM => Brushes.Gold,
        VisualTyreCompound.HARD => Brushes.White,
        VisualTyreCompound.INTER => Brushes.MediumSeaGreen,
        VisualTyreCompound.WET => Brushes.RoyalBlue,
        _ => Brushes.Black
    };
    
    public static Brush GetDrsBrush(byte drsAllowed, byte drsOpen)
    {
        if (drsOpen == 1)
        {
            return Brushes.MediumSeaGreen;
        }
        if (drsAllowed == 1)
        {
            return Brushes.DarkOliveGreen;
        }
        
        return Brushes.DimGray;
    }
    
    public static Brush GetPitBrush(byte pitLimiterStatus)
    {
        if (pitLimiterStatus == 1)
        {
            return Brushes.DarkRed;
        }
        
        return Brushes.DimGray;
    }

    public static Brush GetFlagBrush(ZoneFlag flag)
    {
        switch (flag)
        {
            case ZoneFlag.GREEN:
                return Brushes.ForestGreen;
            
            case ZoneFlag.BLUE:
                return Brushes.DodgerBlue;
            
            case ZoneFlag.YELLOW:
                return Brushes.Gold;
            
            case ZoneFlag.UNKNOWN:
            case ZoneFlag.NONE:
            default:
                return Brushes.DimGray;
        }
    }

    public static Brush GetTyreTemperatureBrush(TyreCompound actualTyreCompound, byte temperature)
    {
        var brushProfile = GripData[actualTyreCompound].TemperatureToGrip;

        if (brushProfile.TryGetValue(temperature, out Brush exactBrush))
        {
            return exactBrush;
        }

        var temperatures = brushProfile.Keys.OrderBy(t => t).ToList();

        if (temperature < temperatures.First())
        {
            return brushProfile[temperatures.First()];
        }

        if (temperature > temperatures.Last())
        {
            return brushProfile[temperatures.Last()];
        }

        var closestLowerOrEqualTemp = temperatures.Last(t => t <= temperature);
        return brushProfile[closestLowerOrEqualTemp];
    }

    public static Brush GetTyreWearBrush(float tyreWear)
    {
        const byte alpha = 179;

        return tyreWear switch
        {
            <= 10 => new SolidColorBrush(Color.FromArgb(alpha, 0, 128, 0)),
            <= 20 => new SolidColorBrush(Color.FromArgb(alpha, 34, 139, 34)),
            <= 30 => new SolidColorBrush(Color.FromArgb(alpha, 173, 255, 47)),
            <= 40 => new SolidColorBrush(Color.FromArgb(alpha, 255, 255, 0)),
            <= 50 => new SolidColorBrush(Color.FromArgb(alpha, 255, 165, 0)),
            <= 60 => new SolidColorBrush(Color.FromArgb(alpha, 255, 140, 0)),
            <= 70 => new SolidColorBrush(Color.FromArgb(alpha, 255, 69, 0)),
            <= 80 => new SolidColorBrush(Color.FromArgb(alpha, 255, 0, 0)),
            _ => new SolidColorBrush(Color.FromArgb(alpha, 139, 0, 0))
        };
    }
    
    public static Brush GetDamageColor(byte damage)
    {
        const byte alpha = 179;

        return damage switch
        {
            0 => new SolidColorBrush(Color.FromArgb(alpha, 0, 128, 0)),
            <= 10 => new SolidColorBrush(Color.FromArgb(alpha, 173, 255, 47)),
            <= 20 => new SolidColorBrush(Color.FromArgb(alpha, 255, 255, 0)),
            <= 30 => new SolidColorBrush(Color.FromArgb(alpha, 255, 165, 0)),
            <= 50 => new SolidColorBrush(Color.FromArgb(alpha, 255, 140, 0)),
            <= 70 => new SolidColorBrush(Color.FromArgb(alpha, 255, 69, 0)),
            <= 90 => new SolidColorBrush(Color.FromArgb(alpha, 255, 0, 0)),
            _ => new SolidColorBrush(Color.FromArgb(alpha, 139, 0, 0))
        };
    }
    
    private static readonly Dictionary<TyreCompound, TyreGripProfile> GripData = new()
    {
        [TyreCompound.C0] = new TyreGripProfile
        {
            TemperatureToGrip = new Dictionary<byte, Brush>
            {
                [80] = Brushes.CornflowerBlue,
                [85] = Brushes.LightSkyBlue,
                [90] = Brushes.LightSkyBlue,
                [95] = Brushes.PaleGreen,
                [100] = Brushes.MediumSeaGreen,
                [105] = Brushes.MediumSeaGreen,
                [110] = Brushes.MediumSeaGreen,
                [115] = Brushes.Yellow,
                [120] = Brushes.Orange,
                [125] = Brushes.OrangeRed,
                [130] = Brushes.Red
            }
        },
        [TyreCompound.C1] = new TyreGripProfile
        {
            TemperatureToGrip = new Dictionary<byte, Brush>
            {
                [80] = Brushes.CornflowerBlue,
                [85] = Brushes.LightSkyBlue,
                [90] = Brushes.LightSkyBlue,
                [95] = Brushes.PaleGreen,
                [100] = Brushes.MediumSeaGreen,
                [105] = Brushes.MediumSeaGreen,
                [110] = Brushes.MediumSeaGreen,
                [115] = Brushes.Yellow,
                [120] = Brushes.Orange,
                [125] = Brushes.OrangeRed,
                [130] = Brushes.Red
            }
        },
        [TyreCompound.C2] = new TyreGripProfile
        {
            TemperatureToGrip = new Dictionary<byte, Brush>
            {
                [78] = Brushes.CornflowerBlue,
                [83] = Brushes.LightSkyBlue,
                [88] = Brushes.LightSkyBlue,
                [93] = Brushes.PaleGreen,
                [98] = Brushes.MediumSeaGreen,
                [103] = Brushes.MediumSeaGreen,
                [108] = Brushes.MediumSeaGreen,
                [113] = Brushes.Yellow,
                [118] = Brushes.Orange,
                [123] = Brushes.OrangeRed,
                [128] = Brushes.Red
            }
        },
        [TyreCompound.C3] = new TyreGripProfile
        {
            TemperatureToGrip = new Dictionary<byte, Brush>
            {
                [75] = Brushes.CornflowerBlue,
                [80] = Brushes.LightSkyBlue,
                [85] = Brushes.LightSkyBlue,
                [90] = Brushes.PaleGreen,
                [95] = Brushes.MediumSeaGreen,
                [100] = Brushes.MediumSeaGreen,
                [105] = Brushes.MediumSeaGreen,
                [110] = Brushes.Yellow,
                [115] = Brushes.Orange,
                [120] = Brushes.OrangeRed,
                [125] = Brushes.Red
            }
        },
        [TyreCompound.C4] = new TyreGripProfile
        {
            TemperatureToGrip = new Dictionary<byte, Brush>
            {
                [73] = Brushes.CornflowerBlue,
                [78] = Brushes.LightSkyBlue,
                [83] = Brushes.LightSkyBlue,
                [88] = Brushes.PaleGreen,
                [93] = Brushes.MediumSeaGreen,
                [98] = Brushes.MediumSeaGreen,
                [103] = Brushes.MediumSeaGreen,
                [108] = Brushes.Yellow,
                [113] = Brushes.Orange,
                [118] = Brushes.OrangeRed,
                [123] = Brushes.Red
            }
        },
        [TyreCompound.C5] = new TyreGripProfile
        {
            TemperatureToGrip = new Dictionary<byte, Brush>
            {
                [70] = Brushes.CornflowerBlue,
                [75] = Brushes.LightSkyBlue,
                [80] = Brushes.LightSkyBlue,
                [85] = Brushes.PaleGreen,
                [90] = Brushes.MediumSeaGreen,
                [95] = Brushes.MediumSeaGreen,
                [100] = Brushes.MediumSeaGreen,
                [105] = Brushes.Yellow,
                [110] = Brushes.Orange,
                [115] = Brushes.OrangeRed,
                [120] = Brushes.Red
            }
        },
        [TyreCompound.INTER] = new TyreGripProfile
        {
            TemperatureToGrip = new Dictionary<byte, Brush>
            {
                [60] = Brushes.CornflowerBlue,
                [65] = Brushes.LightSkyBlue,
                [70] = Brushes.LightSkyBlue,
                [75] = Brushes.PaleGreen,
                [80] = Brushes.MediumSeaGreen,
                [85] = Brushes.MediumSeaGreen,
                [90] = Brushes.MediumSeaGreen,
                [95] = Brushes.Yellow,
                [100] = Brushes.Orange,
                [105] = Brushes.OrangeRed,
                [110] = Brushes.Red
            }
        },
        [TyreCompound.WET] = new TyreGripProfile
        {
            TemperatureToGrip = new Dictionary<byte, Brush>
            {
                [50] = Brushes.CornflowerBlue,
                [55] = Brushes.LightSkyBlue,
                [60] = Brushes.LightSkyBlue,
                [65] = Brushes.PaleGreen,
                [70] = Brushes.MediumSeaGreen,
                [75] = Brushes.MediumSeaGreen,
                [80] = Brushes.MediumSeaGreen,
                [85] = Brushes.Yellow,
                [90] = Brushes.Orange,
                [95] = Brushes.OrangeRed,
                [100] = Brushes.Red
            }
        }
    };
}