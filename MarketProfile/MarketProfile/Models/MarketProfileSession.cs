using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo;

public class MarketProfileSession
{
    public SessionRange Range { get; set; }
    public MarketProfileModel Model { get; set; }
    public List<ChartObject> Profile { get; set; }
    public List<ChartObject> ValueArea { get; set; }
    public List<ChartObject> ValueAreaRays { get; set; }
    public List<ChartObject> MedianRays { get; set; }
    public List<ChartObject> KeyValues { get; set; }
    public List<ChartObject> TpoCounts { get; set; }
    public List<ChartObject> SinglePrints { get; set; }
    public ChartObject ProminentLine { get; set; }
    public ChartRectangle Rectangle { get; set; }

    public void ClearObjects()
    {
        Profile?.Clear();
        ValueArea?.Clear();
        ValueAreaRays?.Clear();
        MedianRays?.Clear();
        KeyValues?.Clear();
        TpoCounts?.Clear();
        SinglePrints?.Clear();
        Model.DevelopingPoC?.Clear();
        Model.DevelopingAreaHigh?.Clear();
        Model.DevelopingAreaLow?.Clear();
        ProminentLine = null;
    }
}