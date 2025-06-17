using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo;

public interface ISessionProfileStrategy
{
    // Keep existing method for backward compatibility
    IEnumerable<SessionRange> GetSessionRanges(Bars bars, int sessionsToCount, Color startColor, Color endColor, DateTime? startFrom = null);
    
    // Add new method for date range filtering without sessionsToCount
    IEnumerable<SessionRange> GetSessionRanges(Bars bars, Color startColor, Color endColor, DateTime startFrom, DateTime endAt);
}