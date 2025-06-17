using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo;

public class SemiannualSessionProfileStrategy : ISessionProfileStrategy, IIndicatorResources, IRenderingModesResources
{
    private readonly IIndicatorResources _resources;
    private readonly IRenderingModesResources _renderingModesResources;

    public SemiannualSessionProfileStrategy(IIndicatorResources resources, IRenderingModesResources renderingModesResources)
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
        // Group by semiannual (year, half)
        IEnumerable<IGrouping<DateTime, Bar>> grouped;
        if (useStartFromDate)
        {
            grouped = filteredBars
                .GroupBy(b => GetSemiannualStart(b.OpenTime))
                .OrderBy(g => g.Key)
                .Where(b => b.Key >= startFrom)
                .Take(sessionsToCount);
        }
        else
        {
            grouped = filteredBars
                .GroupBy(b => GetSemiannualStart(b.OpenTime))
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
        var grouped = filteredBars.GroupBy(b => GetSemiannualStart(b.OpenTime))
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

    private static DateTime GetSemiannualStart(DateTime date)
    {
        int half = (date.Month - 1) / 6;
        int startMonth = half * 6 + 1;
        return new DateTime(date.Year, startMonth, 1);
    }

    public TimeFrame TimeFrame => _resources.TimeFrame;
    public string InputStartFromDate => _renderingModesResources.InputStartFromDate;
    public bool InputStartFromCurrentSession => _renderingModesResources.InputStartFromCurrentSession;
    public bool InputSeamlessScrollingMode => _renderingModesResources.InputSeamlessScrollingMode;
    public SatSunSolution InputSaturdaySunday => _renderingModesResources.InputSaturdaySunday;
}

