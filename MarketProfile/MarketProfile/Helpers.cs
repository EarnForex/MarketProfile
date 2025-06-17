using System;
using System.Collections.Generic;
using System.Globalization;
using cAlgo.API;

namespace cAlgo;

public static class Helpers
{
    public static Color[] GenerateColorGradient(Color startColor, Color endColor, int numberOfBars)
    {
        if (numberOfBars <= 0)
            throw new ArgumentException("numberOfBars must be positive.");

        var colors = new Color[numberOfBars];
        for (int i = 0; i < numberOfBars; i++)
        {
            double t = numberOfBars == 1 ? 0 : (double)i / (numberOfBars - 1);
            int a = (int)Math.Round(startColor.A + (endColor.A - startColor.A) * t);
            int r = (int)Math.Round(startColor.R + (endColor.R - startColor.R) * t);
            int g = (int)Math.Round(startColor.G + (endColor.G - startColor.G) * t);
            int b = (int)Math.Round(startColor.B + (endColor.B - startColor.B) * t);
            // colors[i] = Color.FromArgb(opacity, r, g, b);
            // users can set the opacity from the parameters
            colors[i] = Color.FromArgb(a, r, g, b);
        }
        return colors;
    }
    
    public static TimeSpan GetBarTimeSpan(TimeFrame timeFrame) =>
        timeFrame.ToString() switch
        {
            "Minute" => TimeSpan.FromMinutes(1),
            "Minute2" => TimeSpan.FromMinutes(2),
            "Minute3" => TimeSpan.FromMinutes(3),
            "Minute4" => TimeSpan.FromMinutes(4),
            "Minute5" => TimeSpan.FromMinutes(5),
            "Minute6" => TimeSpan.FromMinutes(6),
            "Minute7" => TimeSpan.FromMinutes(7),
            "Minute8" => TimeSpan.FromMinutes(8),
            "Minute9" => TimeSpan.FromMinutes(9),
            "Minute10" => TimeSpan.FromMinutes(10),
            "Minute15" => TimeSpan.FromMinutes(15),
            "Minute20" => TimeSpan.FromMinutes(20),
            "Minute30" => TimeSpan.FromMinutes(30),
            "Minute45" => TimeSpan.FromMinutes(45),
            "Hour" => TimeSpan.FromHours(1),
            "Hour2" => TimeSpan.FromHours(2),
            "Hour3" => TimeSpan.FromHours(3),
            "Hour4" => TimeSpan.FromHours(4),
            "Hour6" => TimeSpan.FromHours(6),
            "Hour8" => TimeSpan.FromHours(8),
            "Hour12" => TimeSpan.FromHours(12),
            "Daily" => TimeSpan.FromDays(1),
            "Day2" => TimeSpan.FromDays(2),
            "Day3" => TimeSpan.FromDays(3),
            "Weekly" => TimeSpan.FromDays(7),
            "Monthly" => TimeSpan.FromDays(30),
            _ => throw new ArgumentOutOfRangeException(nameof(timeFrame), timeFrame, "TimeFrame not supported")
        };

    public static List<int[]> GroupAdjacent(int[] input)
    {
        var result = new List<int[]>();
        if (input == null || input.Length == 0)
            return result;

        var group = new List<int> { input[0] };

        for (int i = 1; i < input.Length; i++)
        {
            if (input[i] == input[i - 1] + 1)
            {
                group.Add(input[i]);
            }
            else
            {
                result.Add(group.ToArray());
                group = new List<int> { input[i] };
            }
        }
        result.Add(group.ToArray());
        return result;
    }
    
    public static bool IsInIntradaySession(DateTime time, IntradaySessionDefinition session)
    {
        var timeOfDay = time.TimeOfDay;
        return session.Start <= session.End 
            ? timeOfDay >= session.Start && timeOfDay < session.End
            : timeOfDay >= session.Start || timeOfDay < session.End;
    }
    
    public static bool SameSessionDay(DateTime sessionStart, DateTime barTime)
    {
        // Implement logic to check if two times belong to the same session day
        // This would need to account for market open times
        return sessionStart.Date == barTime.Date;
    }

    public static int GetWeekNumber(DateTime time)
    {
        // Get ISO week number
        return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
            time, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    public static int GetQuarter(DateTime time)
    {
        return (time.Month - 1) / 3 + 1;
    }
}