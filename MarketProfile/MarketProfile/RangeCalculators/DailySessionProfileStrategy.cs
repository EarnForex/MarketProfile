using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo;

public class DailySessionProfileStrategy : ISessionProfileStrategy, IIndicatorResources, IRenderingModesResources
{
    private readonly IIndicatorResources _resources;
    private readonly IRenderingModesResources _renderingModesResources;

    public DailySessionProfileStrategy(IIndicatorResources resources, IRenderingModesResources renderingModesResources)
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
        
        // Find last N daily sessions, regardless of chart timeframe
        var result = new List<SessionRange>();
        
        // Handle different weekend solutions
        switch (InputSaturdaySunday)
        {
            case SatSunSolution.IgnoreSaturdaySunday:
                // Filter out Saturday and Sunday before grouping
                filteredBars = bars.Where(b => 
                    b.OpenTime.DayOfWeek != DayOfWeek.Saturday && 
                    b.OpenTime.DayOfWeek != DayOfWeek.Sunday);
                break;
            
            case SatSunSolution.AppendSaturdaySunday:
                // Group by custom date that adds weekend days to the following Monday
                var customGroups = bars
                    .GroupBy(b => {
                        var date = b.OpenTime.Date;
                        if (b.OpenTime.DayOfWeek == DayOfWeek.Saturday || b.OpenTime.DayOfWeek == DayOfWeek.Sunday)
                        {
                            // Calculate days until next Monday
                            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)b.OpenTime.DayOfWeek + 7) % 7;
                            return date.AddDays(daysUntilMonday);
                        }
                        return date;
                    });
                
                IEnumerable<IGrouping<DateTime, Bar>> orderedGroups;
                if (useStartFromDate)
                {
                    orderedGroups = customGroups
                        .OrderBy(g => g.Key)
                        .Where(b => b.Key >= startFrom)
                        .Take(sessionsToCount);
                }
                else
                {
                    orderedGroups = customGroups
                        .OrderByDescending(g => g.Key)
                        .Where(g => !endAt.HasValue || g.Key <= endAt.Value)
                        .Take(sessionsToCount);
                }

                foreach (var group in orderedGroups)
                {
                    var orderedBars = group.OrderBy(b => b.OpenTime).ToList();
                    var first = orderedBars.First();
                    var last = orderedBars.Last();
                    result.Add(new SessionRange
                    {
                        Start = first.OpenTime,
                        End = last.OpenTime.Add(Helpers.GetBarTimeSpan(TimeFrame)),
                        StartColor = startColor,
                        EndColor = endColor,
                        Bars = orderedBars
                    });
                }
                return result;
        }
        
        IEnumerable<IGrouping<DateTime, Bar>> grouped;
        if (useStartFromDate)
        {
            grouped = filteredBars
                .GroupBy(b => b.OpenTime.Date)
                .OrderBy(g => g.Key)
                .Where(b => b.Key >= startFrom)
                .Take(sessionsToCount);
        }
        else
        {
            grouped = filteredBars
                .GroupBy(b => b.OpenTime.Date)
                .OrderByDescending(g => g.Key)
                .Where(g => !endAt.HasValue || g.Key <= endAt.Value)
                .Take(sessionsToCount);
        }
    
        foreach (var group in grouped)
        {
            var first = group.First();
            var last = group.Last();
            //todo I think "End" should be last.OpenTime and that's it
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
        var grouped = filteredBars.GroupBy(b => b.OpenTime.Date)
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

    public TimeFrame TimeFrame => _resources.TimeFrame;
    public string InputStartFromDate => _renderingModesResources.InputStartFromDate;
    public bool InputStartFromCurrentSession => _renderingModesResources.InputStartFromCurrentSession;
    public bool InputSeamlessScrollingMode => _renderingModesResources.InputSeamlessScrollingMode;
    public SatSunSolution InputSaturdaySunday => _renderingModesResources.InputSaturdaySunday;
}

public class IntradaySessionDefinition
{
    public TimeSpan Start { get; init; }
    public TimeSpan End { get; init; }
    public string Name { get; init; }
    public Color StartColor { get; init; }
    public Color EndColor { get; init; }
}

