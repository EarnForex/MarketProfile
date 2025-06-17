using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo;

public interface IHotkeyStateManagerResources
{
    Chart Chart { get; }
    void Print(object message);
    TimeFrame TimeFrame { get; }
    SessionState SessionState { get; set; }
    SessionChangeManager SessionChangeManager { get; }
    List<MarketProfileSession> Sessions { get; }
    INotifications Notifications { get; }
    MarketProfileRenderer Renderer { get; }
    
    bool InputSeamlessScrollingMode { get; }
    int InputSessionsToCount { get; }
    bool InputDisableAlertsOnWrongTimeframes { get; }
    SoundType InputAlertSoundType { get; }
    
    void CheckZeroIntradaySessions();
    bool SetSessionsAndAddMarketProfiles(int sessionsToCount, DateTime? endAt = null);
    
    //--
    string InputHotkeyDaily { get; }
    string InputHotkeyWeekly { get; }
    string InputHotkeyMonthly { get; }
    string InputHotkeyQuarterly { get; }
    string InputHotkeySemiannual { get; }
    string InputHotkeyAnnual { get; }
    string InputHotkeyIntraday { get; }
    string InputHotkeyRectangle { get; }
}

public class HotkeyStateManager : IHotkeyStateManagerResources
{
    private readonly IHotkeyStateManagerResources _resources;

    public HotkeyStateManager(IHotkeyStateManagerResources resources)
    {
        _resources = resources;
    }

    public void AddHotkeys()
    {
        Chart.AddHotkey(() => SwitchSessionTo(SessionPeriod.Daily), InputHotkeyDaily);
        Chart.AddHotkey(() => SwitchSessionTo(SessionPeriod.Weekly), InputHotkeyWeekly);
        Chart.AddHotkey(() => SwitchSessionTo(SessionPeriod.Monthly), InputHotkeyMonthly);
        Chart.AddHotkey(() => SwitchSessionTo(SessionPeriod.Quarterly), InputHotkeyQuarterly);
        Chart.AddHotkey(() => SwitchSessionTo(SessionPeriod.Semiannual), InputHotkeySemiannual);
        Chart.AddHotkey(() => SwitchSessionTo(SessionPeriod.Annual), InputHotkeyAnnual);
        Chart.AddHotkey(() => SwitchSessionTo(SessionPeriod.Intraday), InputHotkeyIntraday);
        Chart.AddHotkey(() => SwitchSessionTo(SessionPeriod.Rectangle), InputHotkeyRectangle);   
    }
    
    private void SwitchSessionTo(SessionPeriod sessionPeriod)
    {
        if (sessionPeriod == SessionPeriod.Rectangle && InputSeamlessScrollingMode)
            throw new ArgumentException("Seamless scrolling mode doesn't work with Rectangle sessions.");
        
        if (sessionPeriod == SessionState.LastSessionState)
        {
            Alert($"Session is already set to {sessionPeriod}.");
            return;
        }
        
        Print($"Switching session to {sessionPeriod}.");
        
        if (!SessionChangeManager.CheckSessions(sessionPeriod))
            return;

        SessionChangeManager.UpdateSessionStateTo(sessionPeriod);
        
        foreach (var session in Sessions)
            Renderer.DeleteAllFromSession(session);

        SetSessionsAndAddMarketProfiles(InputSessionsToCount);
    }

    public void Alert(string alertText)
    {
        if (InputDisableAlertsOnWrongTimeframes)
        {
            Print($"Initialization failed: {alertText}");
        }
        else
        {
            Notifications.PlaySound(InputAlertSoundType);
            MessageBox.Show(alertText, "Alert", MessageBoxButton.OK);   
        }
    }
    
    #region MyRegion

    public Chart Chart => _resources.Chart;
    public SessionState SessionState
    {
        get => _resources.SessionState;
        set => _resources.SessionState = value;
    }
    public void Print(object message) => _resources.Print(message);
    public TimeFrame TimeFrame => _resources.TimeFrame;
    public SessionChangeManager SessionChangeManager => _resources.SessionChangeManager;
    public List<MarketProfileSession> Sessions => _resources.Sessions;
    public INotifications Notifications => _resources.Notifications;
    public MarketProfileRenderer Renderer => _resources.Renderer;
    public bool InputSeamlessScrollingMode => _resources.InputSeamlessScrollingMode;
    public int InputSessionsToCount => _resources.InputSessionsToCount;
    public bool InputDisableAlertsOnWrongTimeframes => _resources.InputDisableAlertsOnWrongTimeframes;
    public SoundType InputAlertSoundType => _resources.InputAlertSoundType;
    public void CheckZeroIntradaySessions() => 
        _resources.CheckZeroIntradaySessions();
    public bool SetSessionsAndAddMarketProfiles(int sessionsToCount, DateTime? endAt = null) => 
        _resources.SetSessionsAndAddMarketProfiles(sessionsToCount, endAt);
    public string InputHotkeyDaily => _resources.InputHotkeyDaily;
    public string InputHotkeyWeekly => _resources.InputHotkeyWeekly;
    public string InputHotkeyMonthly => _resources.InputHotkeyMonthly;
    public string InputHotkeyQuarterly => _resources.InputHotkeyQuarterly;
    public string InputHotkeySemiannual => _resources.InputHotkeySemiannual;
    public string InputHotkeyAnnual => _resources.InputHotkeyAnnual;
    public string InputHotkeyIntraday => _resources.InputHotkeyIntraday;
    public string InputHotkeyRectangle => _resources.InputHotkeyRectangle;

    #endregion
}