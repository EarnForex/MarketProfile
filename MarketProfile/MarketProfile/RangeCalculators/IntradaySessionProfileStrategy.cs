using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using cAlgo.API;

namespace cAlgo;

public class IntradaySessionProfileStrategy : ISessionProfileStrategy, IRenderingModesResources
{
    private readonly List<IntradaySessionDefinition> _sessions;
    private readonly IIndicatorResources _resources;
    private readonly IRenderingModesResources _renderingModesResources;

    public IntradaySessionProfileStrategy(List<IntradaySessionDefinition> sessions, IIndicatorResources resources, IRenderingModesResources renderingModesResources)
    {
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _resources = resources;
        _renderingModesResources = renderingModesResources;
    }

    public IEnumerable<SessionRange> GetSessionRanges(Bars bars, int sessionsToCount, Color startColor, Color endColor, DateTime? endAt = null)
    {
        foreach (var session in _sessions)
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

            // Handle different weekend solutions
            switch (InputSaturdaySunday)
            {
                case SatSunSolution.IgnoreSaturdaySunday:
                    filteredBars = filteredBars.Where(b => b.OpenTime.DayOfWeek != DayOfWeek.Saturday && b.OpenTime.DayOfWeek != DayOfWeek.Sunday);
                    break;
                case SatSunSolution.AppendSaturdaySunday:
                    // Group Sat/Sun bars with the following Monday
                    var customGroups = filteredBars.GroupBy(b => {
                        var date = b.OpenTime.Date;
                        if (b.OpenTime.DayOfWeek == DayOfWeek.Saturday || b.OpenTime.DayOfWeek == DayOfWeek.Sunday)
                        {
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
                        if (!group.Any()) continue;
                        var sessionSpan = session.End > session.Start
                            ? session.End - session.Start
                            : (session.End + TimeSpan.FromDays(1)) - session.Start;
                        var date = group.Key;
                        var start = date.Add(session.Start);
                        var end = start.Add(sessionSpan);
                        var sessionBars = bars.Where(b => b.OpenTime >= start && b.OpenTime <= end).ToList();
                        if (!sessionBars.Any()) continue;
                        yield return new SessionRange
                        {
                            Start = start,
                            End = end,
                            StartColor = session.StartColor,
                            EndColor = session.EndColor,
                            Bars = sessionBars
                        };
                    }
                    continue;
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
                if (!group.Any()) continue;
                var sessionSpan = session.End > session.Start
                    ? session.End - session.Start
                    : (session.End + TimeSpan.FromDays(1)) - session.Start;
                var date = group.Key;
                var start = date.Add(session.Start);
                var end = start.Add(sessionSpan);
                var sessionBars = bars.Where(b => b.OpenTime >= start && b.OpenTime <= end).ToList();
                if (!sessionBars.Any()) continue;
                yield return new SessionRange
                {
                    Start = start, 
                    End = end,
                    StartColor = session.StartColor,
                    EndColor = session.EndColor,
                    Bars = sessionBars
                };
            }
        }
    }

    public IEnumerable<SessionRange> GetSessionRanges(Bars bars, Color startColor, Color endColor, DateTime startFrom, DateTime endAt)
    {
        foreach (var session in _sessions)
        {
            var filteredBars = bars.Where(b => b.OpenTime >= startFrom && b.OpenTime <= endAt);
            var grouped = filteredBars
                .Where(b => b.OpenTime.DayOfWeek != DayOfWeek.Saturday)
                .GroupBy(b => b.OpenTime.Date)
                .OrderBy(g => g.Key);
            
            foreach (var group in grouped)
            {
                var sessionSpan = session.End > session.Start
                    ? session.End - session.Start
                    : (session.End + TimeSpan.FromDays(1)) - session.Start;
                var date = group.Key;
                var start = date.Add(session.Start);
                var end = start.Add(sessionSpan);

                yield return new SessionRange
                {
                    Start = start, 
                    End = end,
                    StartColor = session.StartColor,
                    EndColor = session.EndColor,
                    Bars = bars.Where(b => b.OpenTime >= start && b.OpenTime <= end)
                };
            }
        }
    }

    public string InputStartFromDate => _renderingModesResources.InputStartFromDate;
    public bool InputStartFromCurrentSession => _renderingModesResources.InputStartFromCurrentSession;
    public bool InputSeamlessScrollingMode => _renderingModesResources.InputSeamlessScrollingMode;
    public SatSunSolution InputSaturdaySunday => _renderingModesResources.InputSaturdaySunday;
}

