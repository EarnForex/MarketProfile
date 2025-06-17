using System;
using System.Collections.Generic;

namespace cAlgo;

public class MarketProfileModel
{
    //I'm not sure if I should keep the original matrix or just stick with the piled one
    public MatrixPoint[,] Matrix { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public double ValueAreaHigh { get; set; }
    public Dictionary<DateTime, double> DevelopingAreaHigh { get; } = new();
    public double ValueAreaLow { get; set; }
    public Dictionary<DateTime, double> DevelopingAreaLow { get; } = new();
    public double Median { get; set; }
    public double PointOfControl { get; set; }
    public Dictionary<DateTime, double> DevelopingPoC { get; } = new();
    
    /// <summary>
    /// TPO stands for Time Price Opportunity
    /// </summary>
    public int TpoCountAbove { get; set; }
    
    /// <summary>
    /// TPO stands for Time Price Opportunity
    /// </summary>
    public int TpoCountBelow { get; set; }

    public List<SinglePrint> SinglePrints { get; } = new();
    
    /// <summary>
    /// A Prominent line is when the Median TPO is > X% of the total amount of TPOs
    /// </summary>
    public bool IsProminentLine { get; set; }

    // public MarketProfileModel(MatrixPoint[,] matrix, DateTime startTime, DateTime endTime)
    // {
    //     Matrix = matrix;
    //     StartTime = startTime;
    //     EndTime = endTime;
    // }
}

public class SinglePrint
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public int TopTpoIndex { get; set; }
    public int BottomTpoIndex { get; set; }

    public SinglePrint(DateTime startTime, DateTime endTime, double high, double low, int topTpoIndex, int bottomTpoIndex)
    {
        StartTime = startTime;
        EndTime = endTime;
        High = high;
        Low = low;
        TopTpoIndex = topTpoIndex;
        BottomTpoIndex = bottomTpoIndex;
    }
}