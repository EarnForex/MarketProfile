using System;
using cAlgo.API;

namespace cAlgo;

public interface ISessionChangeManagerResources
{
    LocalStorage LocalStorage { get; }
    SessionState SessionState { get; set; }
    SessionPeriod InputSession { get; }
    bool InputSeamlessScrollingMode { get; }
    Chart Chart { get; }
    TimeFrame TimeFrame { get; }
    void Print(object message);
}

public class SessionChangeManager : ISessionChangeManagerResources
{
    private readonly ISessionChangeManagerResources _resources;

    public SessionChangeManager(ISessionChangeManagerResources resources)
    {
        _resources = resources;
    }

    public void UpdateSessionStateTo(SessionPeriod sessionPeriod)
    {
        SessionState.LastSessionState = sessionPeriod;
        SessionState.LastTransition = Transitions.ChangedByHotkey;
        LocalStorage.SetObject("SessionState", SessionState);
        LocalStorage.Flush(LocalStorageScope.Instance);
    }

    public void LoadSessionState()
    {
        var sessionState = LocalStorage.GetObject<SessionState>("SessionState", LocalStorageScope.Instance);

        //- There's no storage for this instance
        //  - The indicator is initialized with the values from the parameter
        if (sessionState == null)
        {
            Print($"No storage for this instance. Initializing with the values from the parameter.");

            SessionState = new SessionState
            {
                LastSessionState = InputSession,
                LastSessionStateByParameter = InputSession,
                LastTransition = Transitions.Initialized
            };

            return;
        }

        SessionState = new SessionState();

        //  - The input-parameter has changed from last run, SessionState needs to be updated
        //    with the current input-parameter, this change takes priority over the hotkey-state change
        if (InputSession != sessionState.LastSessionStateByParameter)
        {
            Print($"The input-parameter has changed from last run, SessionState will be changed from {sessionState.LastSessionStateByParameter} to {InputSession}.");

            SessionState.LastSessionState = InputSession;
            SessionState.LastSessionStateByParameter = InputSession;
            SessionState.LastTransition = Transitions.ChangedByParameter;
        }
        //The input-parameter has not changed from last run
        else
        {
            Print($"The input-parameter has not changed from last run");

            //if a hotkey was used, SessionState needs to be updated with the last hotkey-state used
            //since I'm loading from the file, no need to change anything
            if (sessionState.LastTransition == Transitions.ChangedByHotkey)
            {
                Print($"A Hotkey was used, Session State will be changed to {SessionState.LastSessionState}.");

                SessionState = new SessionState
                {
                    LastSessionState = sessionState.LastSessionState,
                    LastSessionStateByParameter = sessionState.LastSessionStateByParameter,
                    LastTransition = Transitions.ChangedByHotkey
                };
            }
            //Also nothing to change here
            //if a hotkey was not used, SessionState and input-parameter should be the same,
            //no need to update anything
            else
            {
                Print($"No hotkey was used, SessionState will be assigned {sessionState.LastSessionState}.");

                SessionState = new SessionState
                {
                    LastSessionState = sessionState.LastSessionState,
                    LastSessionStateByParameter = sessionState.LastSessionStateByParameter,
                    LastTransition = sessionState.LastTransition
                };
            }
        }
    }
    
    /// <summary>
    /// Returns true if it can draw the session
    /// But also handles if the TimeFrame needs to be changed 
    /// </summary>
    /// <param name="sessionPeriod"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public bool CheckSessions(SessionPeriod sessionPeriod) =>
        sessionPeriod switch
        {
            SessionPeriod.Daily => CheckSession(sessionPeriod, TimeFrame.Minute5, TimeFrame.Minute30),
            SessionPeriod.Weekly => CheckSession(sessionPeriod, TimeFrame.Minute30, TimeFrame.Hour4),
            SessionPeriod.Monthly => CheckSession(sessionPeriod, TimeFrame.Hour, TimeFrame.Daily),
            SessionPeriod.Quarterly => CheckSession(sessionPeriod, TimeFrame.Hour4, TimeFrame.Daily),
            SessionPeriod.Semiannual => CheckSession(sessionPeriod, TimeFrame.Hour4, TimeFrame.Weekly),
            SessionPeriod.Annual => CheckSession(sessionPeriod, TimeFrame.Hour4, TimeFrame.Weekly),
            SessionPeriod.Intraday => CheckSession(sessionPeriod),
            SessionPeriod.Rectangle => CheckSession(sessionPeriod),
            _ => true
        };

    private bool CheckSession(SessionPeriod sessionPeriod)
    {
        if (TimeFrame <= TimeFrame.Minute30) 
            return true;
        
        var result = MessageBox.Show($"Timeframe should not be higher than M30 for an {sessionPeriod} sessions, do you want to change it?", "Alert", MessageBoxButton.YesNo);
    
        if (result == MessageBoxResult.Yes)
        {
            UpdateSessionStateTo(sessionPeriod);
            Chart.TryChangeTimeFrame(TimeFrame.Minute30);
        }
        
        return false;
    }

    public bool CheckSession(SessionPeriod sessionPeriod,TimeFrame lowerTf, TimeFrame upperTf)
    {
        if (TimeFrame >= lowerTf && TimeFrame <= upperTf) 
            return true;
        
        var result = MessageBox.Show($"Timeframe should be between {lowerTf} and {upperTf} for a {sessionPeriod} session, do you want to change it?", "Alert", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            UpdateSessionStateTo(sessionPeriod);
            Chart.TryChangeTimeFrame(TimeFrame < lowerTf ? lowerTf : upperTf);
        }
        
        return false;
    }

    public LocalStorage LocalStorage => _resources.LocalStorage;
    public SessionState SessionState
    {
        get => _resources.SessionState;
        set => _resources.SessionState = value;
    }

    public TimeFrame TimeFrame => _resources.TimeFrame;

    public void Print(object message) => _resources.Print(message);
    public SessionPeriod InputSession => _resources.InputSession;

    public bool InputSeamlessScrollingMode => _resources.InputSeamlessScrollingMode;

    public Chart Chart => _resources.Chart;
}