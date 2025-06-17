using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo;

public class SessionRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Color StartColor { get; set; }
    public Color EndColor { get; set; }
    public IEnumerable<Bar> Bars { get; set; }

    public override string ToString()
    {
        return $"Start: {Start} - End: {End} | Bars: {Bars.Count()}";
    }
}