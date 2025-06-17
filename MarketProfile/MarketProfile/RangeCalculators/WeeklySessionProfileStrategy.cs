using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo;

public class WeeklySessionProfileStrategy : ISessionProfileStrategy, IIndicatorResources, IRenderingModesResources
{
    private readonly IIndicatorResources _resources;
    private readonly IRenderingModesResources _renderingModesResources;

    public WeeklySessionProfileStrategy(IIndicatorResources resources, IRenderingModesResources renderingModesResources)
    {
        _resources = resources;
        _renderingModesResources = renderingModesResources;
    }

    public IEnumerable<SessionRange> GetSessionRanges(Bars bars, int sessionsToCount, Color startColor, Color endColor, DateTime? endAt = null)
    {
        var useStartFromDate = false;
        var startFrom = DateTime.MinValue;
        
        if (!InputStartFromCurrentSession)
        {
            if (DateTime.TryParse(InputStartFromDate, out startFrom) && !InputSeamlessScrollingMode)
            {
                useStartFromDate = true;
            }
        }
        // Filter bars up to endAt if provided
        IEnumerable<Bar> filteredBars = bars;
        
        var result = new List<SessionRange>();
        // Handle different weekend solutions
        switch (InputSaturdaySunday)
        {
            case SatSunSolution.IgnoreSaturdaySunday:
                // Filter out Saturday and Sunday before grouping
                filteredBars = filteredBars.Where(b => 
                    b.OpenTime.DayOfWeek != DayOfWeek.Saturday && 
                    b.OpenTime.DayOfWeek != DayOfWeek.Sunday);
                break;
            case SatSunSolution.AppendSaturdaySunday:
            case SatSunSolution.SaturdaySundayNormalDays:
                // Weeks should start on Sunday
                break;
        }
        // Group by week starting on Sunday for both AppendSaturdaySunday and SaturdaySundayNormalDays
        Func<DateTime, DateTime> getWeekStart = (InputSaturdaySunday == SatSunSolution.AppendSaturdaySunday || InputSaturdaySunday == SatSunSolution.SaturdaySundayNormalDays)
            ? GetWeekStartSunday
            : GetWeekStartMonday;
        
        IEnumerable<IGrouping<DateTime, Bar>> grouped;
        if (useStartFromDate)
        {
            grouped = filteredBars
                .GroupBy(b => getWeekStart(b.OpenTime))
                .OrderBy(g => g.Key)
                .Where(b => b.Key >= startFrom)
                .Take(sessionsToCount);
        }
        else
        {
            grouped = filteredBars
                .GroupBy(b => getWeekStart(b.OpenTime))
                .OrderByDescending(g => g.Key)
                .Where(g => !endAt.HasValue || g.Key <= endAt.Value)
                .Take(sessionsToCount);
        }

        foreach (var group in grouped)
        {
            var first = group.First();
            var last = group.Last();
            result.Add(new SessionRange
            {
                Start = first.OpenTime,
                End = last.OpenTime.Add(Helpers.GetBarTimeSpan(TimeFrame)),
                StartColor = startColor,
                EndColor = endColor,
                Bars = group
            });
        }
        return result;
    }

    public IEnumerable<SessionRange> GetSessionRanges(Bars bars, Color startColor, Color endColor, DateTime startFrom, DateTime endAt)
    {
        var filteredBars = bars.Where(b => b.OpenTime >= startFrom && b.OpenTime <= endAt);
        var result = new List<SessionRange>();
        var grouped = filteredBars.GroupBy(b => GetWeekStartMonday(b.OpenTime))
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            var first = group.First();
            var last = group.Last();
            result.Add(new SessionRange
            {
                Start = first.OpenTime,
                End = last.OpenTime.Add(Helpers.GetBarTimeSpan(TimeFrame)),
                StartColor = startColor,
                EndColor = endColor,
                Bars = group
            });
        }
        return result;
    }

    private static DateTime GetWeekStartMonday(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static DateTime GetWeekStartSunday(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
        return date.Date.AddDays(-diff);
    }

    public TimeFrame TimeFrame => _resources.TimeFrame;
    public string InputStartFromDate => _renderingModesResources.InputStartFromDate;
    public bool InputStartFromCurrentSession => _renderingModesResources.InputStartFromCurrentSession;
    public bool InputSeamlessScrollingMode => _renderingModesResources.InputSeamlessScrollingMode;
    public SatSunSolution InputSaturdaySunday => _renderingModesResources.InputSaturdaySunday;
}

