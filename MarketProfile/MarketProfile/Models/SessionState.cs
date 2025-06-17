namespace cAlgo;

//Creating a sort of "state machine" with enums, rather than the MQL4 approach
//Possible scenarios:
//- There's no storage for this instance
//  - The indicator is initialized with the values from the parameter
//- There's a storage for this instance
//  - The input-parameter has changed from last run, SessionState needs to be updated with the current input-parameter, this change takes priority over the hotkey-state change
//  - The input-parameter has not changed from last run, if a hotkey was used, SessionState needs to be updated with the last hotkey-state used
//  - The input-parameter has not changed from last run, if a hotkey was not used, SessionState and input-parameter should be the same, no need to update anything

public class SessionState
{
    // public DateTime LastHotkeyStateChanged { get; set; }
    // public DateTime LastParameterStateChanged { get; set; }
    public SessionPeriod LastSessionStateByParameter { get; set; }
    public SessionPeriod LastSessionState { get; set; }
    public Transitions LastTransition { get; set; }

    public override string ToString()
    {
        return $"LastSessionState: {LastSessionState}, LastSessionStateByParameter: {LastSessionStateByParameter}, LastTransition: {LastTransition}";
    }
}