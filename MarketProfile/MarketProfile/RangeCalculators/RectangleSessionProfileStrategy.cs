using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo;

public class RectangleSessionProfileStrategy : ISessionProfileStrategy
{
    private readonly IIndicatorResources _resources;
    private readonly DateTime _startTime;
    private readonly DateTime _endTime;

    public RectangleSessionProfileStrategy(IIndicatorResources resources, DateTime startTime, DateTime endTime)
    {
        _resources = resources;
        _startTime = startTime;
        _endTime = endTime;
    }

    public IEnumerable<SessionRange> GetSessionRanges(Bars bars, int sessionsToCount, Color startColor, Color endColor, DateTime? endAt = null)
    {
        var start = _startTime;
        var end = endAt ?? _endTime;
        yield return new SessionRange
        {
            Start = start,
            End = end,
            StartColor = startColor,
            EndColor = endColor,
            Bars = bars.Where(b => b.OpenTime >= start && b.OpenTime <= end)
        };
    }

    public IEnumerable<SessionRange> GetSessionRanges(Bars bars, Color startColor, Color endColor, DateTime startFrom, DateTime endAt)
    {
        throw new NotSupportedException("GetSessionRanges with date range is not supported for RectangleSessionProfileStrategy.");
    }
}

