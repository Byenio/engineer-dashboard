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
        if (tyreWear <= 10) return Brushes.Green;
        if (tyreWear <= 20) return Brushes.ForestGreen;
        if (tyreWear <= 30) return Brushes.GreenYellow;
        if (tyreWear <= 40) return Brushes.Yellow;
        if (tyreWear <= 50) return Brushes.Orange;
        if (tyreWear <= 60) return Brushes.DarkOrange;
        if (tyreWear <= 70) return Brushes.OrangeRed;
        if (tyreWear <= 80) return Brushes.Red;
        
        return Brushes.DarkRed;
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