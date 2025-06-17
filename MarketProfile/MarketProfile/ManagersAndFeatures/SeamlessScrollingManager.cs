using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo;

public interface ISeamlessScrollingManagerResources
{
    Bars Bars { get; }
    Chart Chart { get; }
    bool SetSessionsAndAddMarketProfiles(int sessionsToCount, DateTime? endAt = null);
    MarketProfileRenderer Renderer { get; }
    List<MarketProfileSession> Sessions { get; }
    SessionState SessionState { get; }
    int InputSessionsToCount { get; }
    void Print(object message);
    void Sleep(TimeSpan timeSpan);
}

public class SeamlessScrollingManager : ISeamlessScrollingManagerResources
{
    //--to do seamless scrolling I need to:
    //Check we're not in Rectangle Mode (just in case)
    //Check if the last MarketProfileSession time is not visible in the current screen
    //If it is not visible
    //I delete everything
    //Render them again
    //But the bars used for rendering have changed, the latest bar is the one visible
    
    private readonly ISeamlessScrollingManagerResources _resources;

    public SeamlessScrollingManager(ISeamlessScrollingManagerResources resources)
    {
        _resources = resources;
        
        // var button = new Button
        // {
        //     Text = "Seamless Scrolling Mode",
        //     HorizontalAlignment = HorizontalAlignment.Center,
        //     VerticalAlignment = VerticalAlignment.Bottom,
        //     Width = 200,
        //     Height = 50,
        //     FontSize = 12,
        // };
        //
        // button.Click += _ => ResetSessions();
        //
        // Chart.AddControl(button);
        //
        // var justDeleteButton = new Button
        // {
        //     Text = "Delete All",
        //     HorizontalAlignment = HorizontalAlignment.Right,
        //     VerticalAlignment = VerticalAlignment.Bottom,
        //     Width = 200,
        //     Height = 50,
        //     FontSize = 12,
        // };
        //
        // justDeleteButton.Click += _ =>
        // {
        //     DeleteAllSessions();
        //     Sessions.Clear();
        // };
        //
        // Chart.AddControl(justDeleteButton);
    }

    public void Start()
    {
        Chart.ScrollChanged += _ => ResetSessions();
        //Chart.ZoomChanged += _ => ResetSessions();
        //Chart.SizeChanged += _ => ResetSessions();
    }

    private void ResetSessions()
    {
        if (SessionState.LastSessionState == SessionPeriod.Rectangle)
            return;
        
        //Sleeping so that it doesn't redraw immediately and slows down the process by doing this too often
        Sleep(TimeSpan.FromSeconds(1));
        
        var lastSessionStartTime = Sessions.LastOrDefault()?.Range.Start;

        //In other words, if the date of the last session is visible, don't delete it
        if (!lastSessionStartTime.HasValue)
            return;

        // if (lastSessionStartTime.Value >= Bars.OpenTimes[Chart.LastVisibleBarIndex] ||
        //     lastSessionStartTime.Value < Bars.OpenTimes[Chart.FirstVisibleBarIndex])
        // {
        //
        // }
        
        Renderer.DeleteAllSessions(Sessions);
        Sessions.Clear();
        SetSessionsAndAddMarketProfiles(InputSessionsToCount, Bars.OpenTimes[Chart.LastVisibleBarIndex]);
    }

    public Bars Bars => _resources.Bars;

    public Chart Chart => _resources.Chart;
    public bool SetSessionsAndAddMarketProfiles(int sessionsToCount, DateTime? endAt = null) => 
        _resources.SetSessionsAndAddMarketProfiles(sessionsToCount,endAt);
    public MarketProfileRenderer Renderer => _resources.Renderer;

    public List<MarketProfileSession> Sessions => _resources.Sessions;
    public SessionState SessionState => _resources.SessionState;
    public int InputSessionsToCount => _resources.InputSessionsToCount;
    public void Print(object message) => _resources.Print(message);
    public void Sleep(TimeSpan timeSpan)
    {
        _resources.Sleep(timeSpan);
    }
}