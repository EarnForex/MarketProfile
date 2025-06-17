using System;
using cAlgo.API;

namespace cAlgo;

public class NewHighLowEventArgs : EventArgs
{
    public double Value { get; set; }
}

public interface INewHighLowManagerResources
{
    event EventHandler<CalculateEventArgs> CalculateEvent;
    Bars Bars { get; }
}

/// <summary>
/// Has events to handle when the last bar is making a new high/low
/// </summary>
public class NewHighLowManager : INewHighLowManagerResources
{
    private readonly INewHighLowManagerResources _resources;
    
    private double _lastHigh = double.NaN;
    private double _lastLow = double.NaN;
    
    public event EventHandler<NewHighLowEventArgs> NewHighOnLastBar;
    public event EventHandler<NewHighLowEventArgs> NewLowOnLastBar;

    public NewHighLowManager(INewHighLowManagerResources resources)
    {
        _resources = resources;
        
        CalculateEvent += NewHighLowFeature_CalculateEvent;
    }

    private void NewHighLowFeature_CalculateEvent(object sender, CalculateEventArgs e)
    {
        if (!e.IsLastBar)
            return;
        
        var index = e.Index;

        if (e.IsNewBar)
        {
            _lastHigh = High[index];
            _lastLow = Low[index];
        }
        else
        {
            if (High[index] > _lastHigh)
            {
                _lastHigh = High[index];
                NewHighOnLastBar?.Invoke(this, new NewHighLowEventArgs { Value = High[index] });
            }

            if (Low[index] < _lastLow)
            {
                _lastLow = Low[index];
                NewLowOnLastBar?.Invoke(this, new NewHighLowEventArgs { Value = Low[index] });
            }   
        }
    }

    public event EventHandler<CalculateEventArgs> CalculateEvent
    {
        add => _resources.CalculateEvent += value;
        remove => _resources.CalculateEvent -= value;
    }

    public Bars Bars => _resources.Bars;
    public DataSeries Open => Bars.OpenPrices;
    public DataSeries High => Bars.HighPrices;
    public DataSeries Low => Bars.LowPrices;
    public DataSeries Close => Bars.ClosePrices;
    public TimeSeries Times => Bars.OpenTimes;
    public int Index => Bars.Count - 1; 
}