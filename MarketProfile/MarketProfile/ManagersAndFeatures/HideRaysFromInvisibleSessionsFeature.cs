using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo;

public interface IHideRaysFromInvisibleSessionsFeatureResources
{
    Chart Chart { get; }
    Bars Bars { get; }
    public List<MarketProfileSession> Sessions { get; }
}

public class HideRaysFromInvisibleSessionsFeature : IHideRaysFromInvisibleSessionsFeatureResources
{
    private readonly IHideRaysFromInvisibleSessionsFeatureResources _resources;

    public HideRaysFromInvisibleSessionsFeature(IHideRaysFromInvisibleSessionsFeatureResources resources)
    {
        _resources = resources;
    }

    public void Start()
    {
        ChangeRaysVisibility();
        Chart.ScrollChanged += ChartScrollChanged;
        Chart.ZoomChanged += ChartZoomChanged;
    }

    private void ChartZoomChanged(ChartZoomEventArgs obj)
    {
        ChangeRaysVisibility();
    }

    private void ChartScrollChanged(ChartScrollEventArgs obj)
    {
        ChangeRaysVisibility();
    }

    public void ChangeRaysVisibility()
    {
        foreach (var session in Sessions) 
            SetVisibility(session, session.Model.EndTime < Bars.OpenTimes[Chart.FirstVisibleBarIndex]);
    }

    private static void SetVisibility(MarketProfileSession session, bool isHidden)
    {
        foreach (var ray in session.MedianRays)
            ray.IsHidden = isHidden;

        foreach (var ray in session.ValueAreaRays)
            ray.IsHidden = isHidden;
    }

    public Chart Chart => _resources.Chart;
    public Bars Bars => _resources.Bars;

    public List<MarketProfileSession> Sessions => _resources.Sessions;
}