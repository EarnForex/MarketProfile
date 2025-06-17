using System;
using cAlgo.API;

namespace cAlgo;

public class MatrixPoint
{
    public Direction Direction { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public double Top { get; init; }
    public double Bottom { get; init; }
    public double Middle => (Top + Bottom) / 2.0;
    public Color Color { get; init; }
}