using System;

namespace cAlgo;

public class CalculateEventArgs : EventArgs
{
    public bool IsNewBar { get; set; }
    public bool IsLastBar { get; set; }
    public int Index { get; set; }
}