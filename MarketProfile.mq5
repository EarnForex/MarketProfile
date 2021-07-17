//+------------------------------------------------------------------+
//|                                                MarketProfile.mq5 |
//|                             Copyright © 2010-2021, EarnForex.com |
//|                                       https://www.earnforex.com/ |
//+------------------------------------------------------------------+
#property copyright "EarnForex.com"
#property link      "https://www.earnforex.com/metatrader-indicators/MarketProfile/"
#property version   "1.17"

#property description "Displays the Market Profile indicator for intraday, daily, weekly, or monthly trading sessions."
#property description "Daily - should be attached to M5-M30 timeframes. M30 is recommended."
#property description "Weekly - should be attached to M30-H4 timeframes. H1 is recommended."
#property description "Weeks start on Sunday."
#property description "Monthly - should be attached to H1-D1 timeframes. H4 is recommended."
#property description "Intraday - should be attached to M1-M15 timeframes. M5 is recommended.\r\n"
#property description "Designed for major currency pairs, but should work also with exotic pairs, CFDs, or commodities."
//+------------------------------------------------------------------+
// Rectangle session - a rectangle's name should start with 'MPR' and must not contain an underscore ('_').
//+------------------------------------------------------------------+
#property indicator_chart_window
// Two buffers are used for the Developing POC display because a single buffer wouldn't support an interrupting line.
#property indicator_plots 2
#property indicator_buffers 2
#property indicator_color1  clrGreen
#property indicator_color2  clrGreen
#property indicator_width1  5
#property indicator_width2  5
#property indicator_type1   DRAW_LINE
#property indicator_type2   DRAW_LINE
#property indicator_style1  STYLE_SOLID
#property indicator_style2  STYLE_SOLID
#property indicator_label1  "Developing POC"
#property indicator_label2  "Developing POC"

enum color_scheme
{
    Blue_to_Red,        // Blue to Red
    Red_to_Green,       // Red to Green
    Green_to_Blue,      // Green to Blue
    Yellow_to_Cyan,     // Yellow to Cyan
    Magenta_to_Yellow,  // Magenta to Yellow
    Cyan_to_Magenta,    // Cyan to Magenta
    Single_Color        // Single Color
};

enum session_period
{
    Daily,
    Weekly,
    Monthly,
    Intraday,
    Rectangle
};

enum sat_sun_solution
{
    Saturday_Sunday_Normal_Days,    // Normal sessions
    Ignore_Saturday_Sunday,         // Ignore Saturday and Sunday
    Append_Saturday_Sunday          // Append Saturday and Sunday
};

enum sessions_to_draw_rays
{
    None,
    Previous,
    Current,
    PreviousCurrent,    // Previous & Current
    AllPrevious,        // All Previous
    All
};

enum ways_to_stop_rays
{
    Stop_No_Rays,
    Stop_All_Rays,
    Stop_All_Rays_Except_Prev_Session,
    Stop_Only_Previous_Session,
};

// Only for dot coloring choice in PutDot() when ColorBullBear == true.
enum bar_direction
{
    Bullish,
    Bearish,
    Neutral
};

enum single_print_type
{
    No,
    Leftside,
    Rightside
};

input group "Main"
input session_period Session                 = Daily;
input datetime       StartFromDate           = __DATE__;        // StartFromDate: lower priority.
input bool           StartFromCurrentSession = true;            // StartFromCurrentSession: higher priority.
input int            SessionsToCount         = 2;               // SessionsToCount: Number of sessions to count Market Profile.
input bool           EnableDevelopingPOC     = false;           // Enable Developing POC.
input int            ValueAreaPercentage     = 70;              // ValueAreaPercentage: Percentage of TPO's inside Value Area.

input group "Colors and looks"
input color_scheme   ColorScheme              = Blue_to_Red;
input color          SingleColor              = clrBlue;        // SingleColor: if ColorScheme is set to Single_Color.
input bool           ColorBullBear            = false;          // ColorBullBear: If true, colors are from bars' direction.
input color          MedianColor              = clrWhite;
input color          ValueAreaSidesColor      = clrWhite;
input color          ValueAreaHighLowColor    = clrWhite;
input ENUM_LINE_STYLE MedianStyle             = STYLE_SOLID;
input ENUM_LINE_STYLE MedianRayStyle          = STYLE_DASH;
input ENUM_LINE_STYLE ValueAreaSidesStyle     = STYLE_SOLID;
input ENUM_LINE_STYLE ValueAreaHighLowStyle   = STYLE_SOLID;
input ENUM_LINE_STYLE ValueAreaRayHighLowStyle= STYLE_DOT;
input int            MedianWidth              = 1;
input int            MedianRayWidth           = 1;
input int            ValueAreaSidesWidth      = 1;
input int            ValueAreaHighLowWidth    = 1;
input int            ValueAreaRayHighLowWidth = 1;
input sessions_to_draw_rays ShowValueAreaRays = None;           // ShowValueAreaRays: draw previous value area high/low rays.
input sessions_to_draw_rays ShowMedianRays    = None;           // ShowMedianRays: draw previous median rays.
input ways_to_stop_rays RaysUntilIntersection = Stop_No_Rays;   // RaysUntilIntersection: which rays stop when hit another MP.
input int            TimeShiftMinutes         = 0;              // TimeShiftMinutes: shift session + to the left, - to the right.
input bool           ShowKeyValues            = true;           // ShowKeyValues: print out VAH, VAL, POC on chart.
input color          KeyValuesColor           = clrWhite;       // KeyValuesColor: color for VAH, VAL, POC printout.
input int            KeyValuesSize            = 8;              // KeyValuesSize: font size for VAH, VAL, POC printout.
input single_print_type ShowSinglePrint       = No;             // ShowSinglePrint: mark Single Print profile levels.
input bool           SinglePrintRays          = false;          // SinglePrintRays: mark Single Print edges with rays.
input color          SinglePrintColor         = clrGold;
input color          SinglePrintRayStyle      = STYLE_SOLID;
input color          SinglePrintRayWidth      = 1;
input color          ProminentMedianColor     = clrYellow;
input ENUM_LINE_STYLE ProminentMedianStyle    = STYLE_SOLID;
input int            ProminentMedianWidth     = 4;
input bool           RightToLeft              = false;          // RightToLeft: Draw histogram from right to left.

input group "Performance"
input int            PointMultiplier          = 0;      // PointMultiplier: higher value = fewer objects. 0 - adaptive.
input int            ThrottleRedraw           = 0;      // ThrottleRedraw: delay (in seconds) for updating Market Profile.
input bool           DisableHistogram         = false;  // DisableHistogram: do not draw profile, VAH, VAL, and POC still visible.

input group "Intraday settings"
input bool           EnableIntradaySession1      = true;
input string         IntradaySession1StartTime   = "00:00";
input string         IntradaySession1EndTime     = "06:00";
input color_scheme   IntradaySession1ColorScheme = Blue_to_Red;

input bool           EnableIntradaySession2      = true;
input string         IntradaySession2StartTime   = "06:00";
input string         IntradaySession2EndTime     = "12:00";
input color_scheme   IntradaySession2ColorScheme = Red_to_Green;

input bool           EnableIntradaySession3      = true;
input string         IntradaySession3StartTime   = "12:00";
input string         IntradaySession3EndTime     = "18:00";
input color_scheme   IntradaySession3ColorScheme = Green_to_Blue;

input bool           EnableIntradaySession4      = true;
input string         IntradaySession4StartTime   = "18:00";
input string         IntradaySession4EndTime     = "00:00";
input color_scheme   IntradaySession4ColorScheme = Yellow_to_Cyan;

input group "Miscellaneous"
input sat_sun_solution SaturdaySunday                 = Saturday_Sunday_Normal_Days;
input bool             DisableAlertsOnWrongTimeframes = false;  // Disable alerts on wrong timeframes.
input int              ProminentMedianPercentage      = 101;    // Percentage of Median TPOs out of total for a Prominent one.

int PointMultiplier_calculated;     // Will have to be calculated based number digits in a quote if PointMultiplier input is 0.
int DigitsM;                        // Number of digits normalized based on PointMultiplier_calculated.
bool InitFailed;                    // Used for soft INIT_FAILED. Hard INIT_FAILED resets input parameters.
datetime StartDate;                 // Will hold either StartFromDate or Time[0].
double onetick;                     // One normalized pip.
bool FirstRunDone = false;          // If true - OnCalculate() was already executed once.
string Suffix = "_";                // Will store object name suffix depending on timeframe.
color_scheme CurrentColorScheme;    // Required due to intraday sessions.
int Max_number_of_bars_in_a_session = 1;
int Timer = 0;                      // For throttling updates of market profiles in slow systems.
bool NeedToRestartDrawing = false;  // Global flag for RightToLeft redrawing;
int CleanedUpOn = 0;                // To prevent cleaning up the buffers again and again when the platform just starts.
double ValueAreaPercentage_double = 0.7; // Will be calculated based on the input parameter in OnInit().

// Used for ColorBullBear.
bar_direction CurrentBarDirection = Neutral;
bar_direction PreviousBarDirection = Neutral;
bool NeedToReviewColors = false;

// For intraday sessions' start and end times.
int IDStartHours[4];
int IDStartMinutes[4];
int IDStartTime[4]; // Stores IDStartHours x 60 + IDStartMinutes for comparison purposes.
int IDEndHours[4];
int IDEndMinutes[4];
int IDEndTime[4];   // Stores IDEndHours x 60 + IDEndMinutes for comparison purposes.
color_scheme IDColorScheme[4];
bool IntradayCheckPassed = false;
int IntradaySessionCount = 0;
int _SessionsToCount;
int IntradayCrossSessionDefined = -1; // For special case used only with Ignore_Saturday_Sunday on Monday.

// We need to know where each session starts and its price range for when RaysUntilIntersection != Stop_No_Rays.
// These are used also when RaysUntilIntersection == Stop_No_Rays for Intraday sessions counting.
double RememberSessionMax[], RememberSessionMin[];
datetime RememberSessionStart[];
string RememberSessionSuffix[];
int SessionsNumber = 0; // Different from _SessionsToCount when working with Intraday sessions and for RaysUntilIntersection != Stop_No_Rays.

// Rectangle variables:
class CRectangleMP
{
private:
    datetime prev_Time0;
    double prev_High, prev_Low;
    double prev_RectanglePriceMax, prev_RectanglePriceMin;
    int Number; // Order number of the rectangle;
public:
    double RectanglePriceMax, RectanglePriceMin;
    datetime RectangleTimeMax, RectangleTimeMin;
    datetime t1, t2; // To avoid reading object properties in Process() after sorting was done.
    string name;
    CRectangleMP(string);
    ~CRectangleMP(void) {};
    void Process(int, const double& High[], const double& Low[], const datetime& Time[], const int rates_total);
};
CRectangleMP* MPR_Array[];
int mpr_total = 0;
uint LastRecalculationTime = 0;

double DevelopingPOC_1[], DevelopingPOC_2[]; // Indicator buffers.

//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int OnInit()
{
    InitFailed = false;

    // Sessions to count for the object creation.
    _SessionsToCount = SessionsToCount;

    // Check for user Session settings.
    if (Session == Daily)
    {
        Suffix = "_D";
        if ((PeriodSeconds() < PeriodSeconds(PERIOD_M5)) || (PeriodSeconds() > PeriodSeconds(PERIOD_M30)))
        {
            string alert_text = "Timeframe should be between M5 and M30 for a Daily session.";
            if (!DisableAlertsOnWrongTimeframes) Alert(alert_text);
            else Print("Initialization failed: " + alert_text);
            InitFailed = true; // Soft INIT_FAILED.
        }
    }
    else if (Session == Weekly)
    {
        Suffix = "_W";
        if ((PeriodSeconds() < PeriodSeconds(PERIOD_M30)) || (PeriodSeconds() > PeriodSeconds(PERIOD_H4)))
        {
            string alert_text = "Timeframe should be between M30 and H4 for a Weekly session.";
            if (!DisableAlertsOnWrongTimeframes) Alert(alert_text);
            else Print("Initialization failed: " + alert_text);
            InitFailed = true; // Soft INIT_FAILED.
        }
    }
    else if (Session == Monthly)
    {
        Suffix = "_M";
        if ((PeriodSeconds() < PeriodSeconds(PERIOD_H1)) || (PeriodSeconds() > PeriodSeconds(PERIOD_D1)))
        {
            string alert_text = "Timeframe should be between H1 and D1 for a Monthly session.";
            if (!DisableAlertsOnWrongTimeframes) Alert(alert_text);
            else Print("Initialization failed: " + alert_text);
            InitFailed = true; // Soft INIT_FAILED.
        }
    }
    else if (Session == Intraday)
    {
        if (PeriodSeconds() > PeriodSeconds(PERIOD_M15))
        {
            string alert_text = "Timeframe should not be higher than M15 for an Intraday sessions.";
            if (!DisableAlertsOnWrongTimeframes) Alert(alert_text);
            else Print("Initialization failed: " + alert_text);
            InitFailed = true; // Soft INIT_FAILED.
        }

        // Check if Intraday User Settings are valid.
        IntradaySessionCount = 0;
        if (!CheckIntradaySession(EnableIntradaySession1, IntradaySession1StartTime, IntradaySession1EndTime, IntradaySession1ColorScheme)) return INIT_PARAMETERS_INCORRECT;
        if (!CheckIntradaySession(EnableIntradaySession2, IntradaySession2StartTime, IntradaySession2EndTime, IntradaySession2ColorScheme)) return INIT_PARAMETERS_INCORRECT;
        if (!CheckIntradaySession(EnableIntradaySession3, IntradaySession3StartTime, IntradaySession3EndTime, IntradaySession3ColorScheme)) return INIT_PARAMETERS_INCORRECT;
        if (!CheckIntradaySession(EnableIntradaySession4, IntradaySession4StartTime, IntradaySession4EndTime, IntradaySession4ColorScheme)) return INIT_PARAMETERS_INCORRECT;

        // Warn user about Intraday mode
        if (IntradaySessionCount == 0)
        {
            string alert_text = "Enable at least one intraday session if you want to use Intraday mode.";
            if (!DisableAlertsOnWrongTimeframes) Alert(alert_text);
            else Print("Initialization failed: " + alert_text);
            InitFailed = true; // Soft INIT_FAILED.
        }
    }

    // Indicator Name.
    IndicatorSetString(INDICATOR_SHORTNAME, "MarketProfile " + EnumToString(Session));

    // Adaptive point multiplier. Calculate based on number of digits in quote (before plus after the dot).
    if (PointMultiplier == 0)
    {
        double quote;
        bool success = SymbolInfoDouble(Symbol(), SYMBOL_ASK, quote);
        if (!success)
        {
            Print("Failed to get price data. Error #", GetLastError(), ". Using PointMultiplier = 1.");
            PointMultiplier_calculated = 1;
        }
        else
        {
            string s = DoubleToString(quote, _Digits);
            int total_digits = StringLen(s);
            // If there is a dot in a quote.
            if (StringFind(s, ".") != -1) total_digits--; // Decrease the count of digits by one.
            if (total_digits <= 5) PointMultiplier_calculated = 1;
            else PointMultiplier_calculated = (int)MathPow(10, total_digits - 5);
        }
    }
    else // Normal point multiplier.
    {
        PointMultiplier_calculated = PointMultiplier;
    }

    // Based on number of digits in PointMultiplier_calculated. -1 because if PointMultiplier_calculated < 10, it does not modify the number of digits.
    DigitsM = _Digits - (StringLen(IntegerToString(PointMultiplier_calculated)) - 1);
    onetick = NormalizeDouble(_Point * PointMultiplier_calculated, DigitsM);

    // Adjust for TickSize granularity if needed.
    double TickSize = SymbolInfoDouble(Symbol(), SYMBOL_TRADE_TICK_SIZE);
    if (onetick < TickSize)
    {
        DigitsM = _Digits - (StringLen(IntegerToString((int)MathRound(TickSize / _Point))) - 1);
        onetick = NormalizeDouble(TickSize, DigitsM);
    }

    // Get color scheme from user input.
    CurrentColorScheme = ColorScheme;

    // To clean up potential leftovers when applying a chart template.
    ObjectCleanup();

    // Check if user wants Session mode as Rectangle
    if ((Session == Rectangle) || (RightToLeft))
    {
        EventSetMillisecondTimer(1000);
    }
    
    // If user chose to display the Developing POC, apply some basic configuration to the buffer.
    if (EnableDevelopingPOC)
    {
        SetIndexBuffer(0, DevelopingPOC_1, INDICATOR_DATA);
        PlotIndexSetDouble(0, PLOT_EMPTY_VALUE, EMPTY_VALUE);
        ArraySetAsSeries(DevelopingPOC_1, true);
        SetIndexBuffer(1, DevelopingPOC_2, INDICATOR_DATA);
        PlotIndexSetDouble(1, PLOT_EMPTY_VALUE, EMPTY_VALUE);
        ArraySetAsSeries(DevelopingPOC_2, true);
    }

    ValueAreaPercentage_double = ValueAreaPercentage * 0.01;

    // Initialization successful
    return INIT_SUCCEEDED;
}

//+------------------------------------------------------------------+
//| Custom indicator deinitialization function                       |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
    if (Session == Rectangle)
    {
        for (int i = 0; i < mpr_total; i++)
        {
            ObjectCleanup(MPR_Array[i].name + "_");
            delete MPR_Array[i];
        }
    }
    else ObjectCleanup();
}

//+------------------------------------------------------------------+
//| Custom Market Profile main iteration function                    |
//+------------------------------------------------------------------+
int OnCalculate(const int rates_total,
                const int prev_calculated,
                const datetime& Time[],
                const double& open[],
                const double& High[],
                const double& Low[],
                const double& close[],
                const long& tick_volume[],
                const long& volume[],
                const int& spread[]
               )
{
    if (InitFailed)
    {
        if (!DisableAlertsOnWrongTimeframes) Print("Initialization failed. Please see the alert message for details.");
        return 0;
    }

    // New bars arrived?
    if ((EnableDevelopingPOC) && (rates_total - prev_calculated > 1) && (CleanedUpOn != rates_total))
    {
        // Initialize the DPOC buffers.
        for (int i = prev_calculated; i < rates_total; i++)
        {
            DevelopingPOC_1[i] = EMPTY_VALUE;
            DevelopingPOC_2[i] = EMPTY_VALUE;
        }
        CleanedUpOn = rates_total; // To prevent cleaning up the buffers again and again when the platform just starts.
    }

    // Use buffers as Timeseries and not ArrayNotSeries.
    ArraySetAsSeries(High, true);
    ArraySetAsSeries(Low, true);
    ArraySetAsSeries(Time, true);

    // Check if user requests current session, else a specific date.
    if (StartFromCurrentSession) StartDate = Time[0];
    else StartDate = StartFromDate;

    // Adjust date if Ignore_Saturday_Sunday is set.
    if (SaturdaySunday == Ignore_Saturday_Sunday)
    {
        // Saturday? Switch to Friday.
        if (TimeDayOfWeek(StartDate) == 6) StartDate -= 86400;
        // Sunday? Switch to Friday too.
        else if (TimeDayOfWeek(StartDate) == 0) StartDate -= 2 * 86400;
    }

    // If we calculate profiles for the past sessions, no need to run it again.
    if ((FirstRunDone) && (StartDate != Time[0])) return rates_total;

    // Delay the update of Market Profile if ThrottleRedraw is given.
    if ((ThrottleRedraw > 0) && (Timer > 0))
    {
        if ((int)TimeLocal() - Timer < ThrottleRedraw) return rates_total;
    }

    // Calculate rectangle
    if (Session == Rectangle) // Everything becomes very simple if rectangle sessions are used.
    {
        CheckRectangles(High, Low, Time, rates_total);
        Timer = (int)TimeLocal();
        return rates_total;
    }

    // Recalculate everything if there were missing bars or something like that. Or if RightToLeft is on and a new right-most session arrived.
    if ((rates_total - prev_calculated > 1) || (NeedToRestartDrawing))
    {
        FirstRunDone = false;
        ObjectCleanup();
        NeedToRestartDrawing = false;
        if (EnableDevelopingPOC)
        {
            // Initialize the DPOC buffers.
            ArrayInitialize(DevelopingPOC_1, EMPTY_VALUE); 
            ArrayInitialize(DevelopingPOC_2, EMPTY_VALUE);
        }
    }

    // Get start and end bar numbers of the given session.
    int sessionend = FindSessionEndByDate(Time, StartDate, rates_total);
    int sessionstart = FindSessionStart(Time, sessionend, rates_total);

    if (sessionstart == -1)
    {
        Print("Something went wrong! Waiting for data to load.");
        return prev_calculated;
    }

    int SessionToStart = 0;
    // If all sessions have already been counted, jump to the current one.
    if (FirstRunDone) SessionToStart = _SessionsToCount - 1;
    else
    {
        // Move back to the oldest session to count to start from it.
        for (int i = 1; i < _SessionsToCount; i++)
        {
            sessionend = sessionstart + 1;
            if (sessionend >= rates_total) return prev_calculated;
            if (SaturdaySunday == Ignore_Saturday_Sunday)
            {
                // Pass through Sunday and Saturday.
                while ((TimeDayOfWeek(Time[sessionend]) == 0) || (TimeDayOfWeek(Time[sessionend]) == 6))
                {
                    sessionend++;
                    if (sessionend >= rates_total) break;
                }
            }
            sessionstart = FindSessionStart(Time, sessionend, rates_total);
        }
    }

    // We begin from the oldest session coming to the current session or to StartFromDate.
    for (int i = SessionToStart; i < _SessionsToCount; i++)
    {
        if (Session == Intraday)
        {
            if (!ProcessIntradaySession(sessionstart, sessionend, i, High, Low, Time, rates_total)) return 0;
        }
        else
        {
            if (Session == Daily) Max_number_of_bars_in_a_session = PeriodSeconds(PERIOD_D1) / PeriodSeconds();
            else if (Session == Weekly) Max_number_of_bars_in_a_session = 604800 / PeriodSeconds();
            else if (Session == Monthly) Max_number_of_bars_in_a_session = 2678400 / PeriodSeconds();
            if (SaturdaySunday == Append_Saturday_Sunday)
            {
                // The start is on Sunday - add remaining time.
                if (TimeDayOfWeek(Time[sessionstart]) == 0) Max_number_of_bars_in_a_session += (24 * 3600 - (TimeHour(Time[sessionstart]) * 3600 + TimeMinute(Time[sessionstart]) * 60)) / PeriodSeconds();
                // The end is on Saturday. +1 because even 0:00 bar deserves a bar.
                if (TimeDayOfWeek(Time[sessionend]) == 6) Max_number_of_bars_in_a_session += ((TimeHour(Time[sessionend]) * 3600 + TimeMinute(Time[sessionend]) * 60)) / PeriodSeconds() + 1;
            }
            if (!ProcessSession(sessionstart, sessionend, i, High, Low, Time, rates_total)) return 0;
        }

        // Go to the newer session only if there is one or more left.
        if (_SessionsToCount - i > 1)
        {
            sessionstart = sessionend - 1;
            if (SaturdaySunday == Ignore_Saturday_Sunday)
            {
                // Pass through Sunday and Saturday.
                while ((TimeDayOfWeek(Time[sessionstart]) == 0) || (TimeDayOfWeek(Time[sessionstart]) == 6))
                {
                    sessionstart--;
                    if (sessionstart == 0) break;
                }
            }
            sessionend = FindSessionEndByDate(Time, Time[sessionstart], rates_total);
        }
    }

    if ((ShowValueAreaRays != None) || (ShowMedianRays != None)) CheckRays();

    FirstRunDone = true;

    Timer = (int)TimeLocal();

    return rates_total;
}

//+------------------------------------------------------------------+
//| Finds the session's starting bar number for any given bar number.|
//| n - bar number for which to find starting bar.                   |
//+------------------------------------------------------------------+
int FindSessionStart(const datetime &Time[], const int n, const int rates_total)
{
    if (Session == Daily) return FindDayStart(Time, n, rates_total);
    else if (Session == Weekly) return FindWeekStart(Time, n, rates_total);
    else if (Session == Monthly) return FindMonthStart(Time, n, rates_total);
    else if (Session == Intraday)
    {
        // A special case when Append_Saturday_Sunday is on and n is on Monday.
        if ((SaturdaySunday == Append_Saturday_Sunday) && (TimeDayOfWeek(Time[n] + TimeShiftMinutes * 60) == 1))
        {
            // One of the intraday sessions should start at 00:00 or have end < start.
            for (int intraday_i = 0; intraday_i < IntradaySessionCount; intraday_i++)
            {
                if ((IDStartTime[intraday_i] == 0) || (IDStartTime[intraday_i] > IDEndTime[intraday_i]))
                {
                    // "Monday" part of the day. Effective only for "end < start" sessions.
                    if ((TimeHour(Time[n]) * 60 + TimeMinute(Time[n]) >= IDEndTime[intraday_i]) && (IDStartTime[intraday_i] > IDEndTime[intraday_i]))
                    {
                        // Find the first bar on Monday after the borderline session.
                        int x = n;
                        while ((x < rates_total) && (TimeHour(Time[x]) * 60 + TimeMinute(Time[x]) >= IDEndTime[intraday_i]))
                        {
                            x++;
                            // If there is no Sunday session (stepped into Saturday or another non-Sunday/non-Monday day, return normal day start.
                            if (TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60) > 1) return FindDayStart(Time, n, rates_total);
                        }
                        return (x - 1);
                    }
                    else
                    {
                        // Find the first Sunday bar.
                        int x = n;
                        while ((x < rates_total) && ((TimeDayOfYear(Time[n] + TimeShiftMinutes * 60) == TimeDayOfYear(Time[x] + TimeShiftMinutes * 60)) || (TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60) == 0))) x++;
                        // Number of sessions should be increased as we "lose" one session to Sunday.
                        _SessionsToCount++;
                        return (x - 1);
                    }
                }
            }
        }

        return FindDayStart(Time, n, rates_total);
    }
    return -1;
}

//+------------------------------------------------------------------+
//| Finds the day's starting bar number for any given bar number.    |
//| n - bar number for which to find starting bar.                   |
//+------------------------------------------------------------------+
int FindDayStart(const datetime &Time[], const int n, const int rates_total)
{
    if (n >= rates_total) return -1;
    int x = n;
    int time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);
    int time_n_day_of_week = time_x_day_of_week;

    // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday or it is Friday but the bar n is on Saturday.
    while ((TimeDayOfYear(Time[n] + TimeShiftMinutes * 60) == TimeDayOfYear(Time[x] + TimeShiftMinutes * 60)) || ((SaturdaySunday == Append_Saturday_Sunday) && ((time_x_day_of_week == 0) || ((time_x_day_of_week == 5) && (time_n_day_of_week == 6)))))
    {
        x++;
        if (x >= rates_total) break;
        time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);
    }

    return (x - 1);
}

//+------------------------------------------------------------------+
//| Finds the week's starting bar number for any given bar number.   |
//| n - bar number for which to find starting bar.                   |
//+------------------------------------------------------------------+
int FindWeekStart(const datetime &Time[], const int n, const int rates_total)
{
    if (n >= rates_total) return -1;
    int x = n;
    int time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);

    // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday.
    while ((SameWeek(Time[n] + TimeShiftMinutes * 60, Time[x] + TimeShiftMinutes * 60)) || ((SaturdaySunday == Append_Saturday_Sunday) && (time_x_day_of_week == 0)))
    {
        // If Ignore_Saturday_Sunday is on and we stepped into Sunday, stop.
        if ((SaturdaySunday == Ignore_Saturday_Sunday) && (time_x_day_of_week == 0)) break;
        x++;
        if (x >= rates_total) break;
        time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);
    }

    return (x - 1);
}

//+------------------------------------------------------------------+
//| Finds the month's starting bar number for any given bar number.  |
//| n - bar number for which to find starting bar.                   |
//+------------------------------------------------------------------+
int FindMonthStart(const datetime &Time[], const int n, const int rates_total)
{
    if (n >= rates_total) return -1;
    int x = n;
    int time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);
    // These don't change:
    int time_n_day_of_week = TimeDayOfWeek(Time[n] + TimeShiftMinutes * 60);
    int time_n_day = TimeDay(Time[n] + TimeShiftMinutes * 60);
    int time_n_month = TimeMonth(Time[n] + TimeShiftMinutes * 60);

    // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday or Saturday the 1st day of month.
    while ((time_n_month == TimeMonth(Time[x] + TimeShiftMinutes * 60)) || ((SaturdaySunday == Append_Saturday_Sunday) && ((time_x_day_of_week == 0) || ((time_n_day_of_week == 6) && (time_n_day == 1)))))
    {
        // If month distance somehow becomes greater than 1, break.
        int month_distance = time_n_month - TimeMonth(Time[x] + TimeShiftMinutes * 60);
        if (month_distance < 0) month_distance = 12 - month_distance;
        if (month_distance > 1) break;
        // Check if Append_Saturday_Sunday is on and today is Saturday the 1st day of month. Despite it being current month, it should be skipped because it is appended to the previous month. Unless it is the sessionend day, which is the Saturday of the next month attached to this session.
        if (SaturdaySunday == Append_Saturday_Sunday)
        {
            if ((time_x_day_of_week == 6) && (TimeDay(Time[x] + TimeShiftMinutes * 60) == 1) && (time_n_day != TimeDay(Time[x] + TimeShiftMinutes * 60))) break;
        }
        // Check if Ignore_Saturday_Sunday is on and today is Sunday or Saturday the 2nd or the 1st day of month. Despite it being current month, it should be skipped because it is ignored.
        if (SaturdaySunday == Ignore_Saturday_Sunday)
        {
            if (((time_x_day_of_week == 0) || (time_x_day_of_week == 6)) && ((TimeDay(Time[x] + TimeShiftMinutes * 60) == 1) || (TimeDay(Time[x] + TimeShiftMinutes * 60) == 2))) break;
        }
        x++;
        if (x >= rates_total) break;
        time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);
    }

    return (x - 1);
}

//+------------------------------------------------------------------+
//| Finds the session's end bar by the session's date.               |
//+------------------------------------------------------------------+
int FindSessionEndByDate(const datetime &Time[], const datetime date, const int rates_total)
{
    if (Session == Daily) return FindDayEndByDate(Time, date, rates_total);
    else if (Session == Weekly) return FindWeekEndByDate(Time, date, rates_total);
    else if (Session == Monthly) return FindMonthEndByDate(Time, date, rates_total);
    else if (Session == Intraday)
    {
        // A special case when Append_Saturday_Sunday is on and the date is on Sunday.
        if ((SaturdaySunday == Append_Saturday_Sunday) && (TimeDayOfWeek(date + TimeShiftMinutes * 60) == 0))
        {
            // One of the intraday sessions should start at 00:00 or have end < start.
            for (int intraday_i = 0; intraday_i < IntradaySessionCount; intraday_i++)
            {
                if ((IDStartTime[intraday_i] == 0) || (IDStartTime[intraday_i] > IDEndTime[intraday_i]))
                {
                    // Find the last bar of this intraday session and return it as sessionend.
                    int x = 0;
                    int abs_day = TimeAbsoluteDay(date + TimeShiftMinutes * 60);
                    // TimeAbsoluteDay is used for cases when the given date is Dec 30 (#364) and the current date is Jan 1 (#1) for example.
                    while ((x < rates_total) && (abs_day < TimeAbsoluteDay(Time[x] + TimeShiftMinutes * 60))) // It's Sunday.
                    {
                        // On Monday.
                        if (TimeAbsoluteDay(Time[x] + TimeShiftMinutes * 60) == abs_day + 1)
                        {
                            // Inside the session.
                            if (TimeHour(Time[x]) * 60 +  TimeMinute(Time[x]) < IDEndTime[intraday_i]) break;
                            // Break out earlier (on Monday's end bar) if working with 00:00-XX:XX session.
                            if (IDStartTime[intraday_i] == 0) break;
                        }
                        x++;
                    }
                    return x;
                }
            }
        }
        return FindDayEndByDate(Time, date, rates_total);
    }

    return -1;
}

//+------------------------------------------------------------------+
//| Finds the day's end bar by the day's date.                       |
//+------------------------------------------------------------------+
int FindDayEndByDate(const datetime &Time[], const datetime date, const int rates_total)
{
    int x = 0;

    // TimeAbsoluteDay is used for cases when the given date is Dec 30 (#364) and the current date is Jan 1 (#1) for example.
    while ((x < rates_total) && (TimeAbsoluteDay(date + TimeShiftMinutes * 60) < TimeAbsoluteDay(Time[x] + TimeShiftMinutes * 60)))
    {
        // Check if Append_Saturday_Sunday is on and if the found end of the day is on Saturday and the given date is the previous Friday; or it is a Monday and the sought date is the previous Sunday.
        if (SaturdaySunday == Append_Saturday_Sunday)
        {
            if (((TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60) == 6) || (TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60) == 1)) && (TimeAbsoluteDay(Time[x] + TimeShiftMinutes * 60) - TimeAbsoluteDay(date + TimeShiftMinutes * 60) == 1)) break;
        }
        x++;
    }

    return x;
}

//+------------------------------------------------------------------+
//| Finds the week's end bar by the week's date.                     |
//+------------------------------------------------------------------+
int FindWeekEndByDate(const datetime &Time[], const datetime date, const int rates_total)
{
    int x = 0;

    int time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);

    // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday; and also if Ignore_Saturday_Sunday is on and it is Saturday or Sunday.
    while ((SameWeek(date + TimeShiftMinutes * 60, Time[x] + TimeShiftMinutes * 60) != true) || ((SaturdaySunday == Append_Saturday_Sunday) && (time_x_day_of_week == 0))  || ((SaturdaySunday == Ignore_Saturday_Sunday) && ((time_x_day_of_week == 0) || (time_x_day_of_week == 6))))
    {
        x++;
        if (x >= rates_total) break;
        time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);
    }

    return x;
}

//+------------------------------------------------------------------+
//| Finds the month's end bar by the month's date.                   |
//+------------------------------------------------------------------+
int FindMonthEndByDate(const datetime &Time[], const datetime date, const int rates_total)
{
    int x = 0;

    int time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);

    // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday; and also if Ignore_Saturday_Sunday is on and it is Saturday or Sunday.
    while ((SameMonth(date + TimeShiftMinutes * 60, Time[x] + TimeShiftMinutes * 60) != true) || ((SaturdaySunday == Append_Saturday_Sunday) && (time_x_day_of_week == 0))  || ((SaturdaySunday == Ignore_Saturday_Sunday) && ((time_x_day_of_week == 0) || (time_x_day_of_week == 6))))
    {
        // Check if Append_Saturday_Sunday is on.
        if (SaturdaySunday == Append_Saturday_Sunday)
        {
            // Today is Saturday the 1st day of the next month. Despite it being in a next month, it should be appended to the current month.
            if ((time_x_day_of_week == 6) && (TimeDay(Time[x] + TimeShiftMinutes * 60) == 1) && (TimeYear(Time[x] + TimeShiftMinutes * 60) * 12 + TimeMonth(Time[x] + TimeShiftMinutes * 60) - TimeYear(date + TimeShiftMinutes * 60) * 12 - TimeMonth(date + TimeShiftMinutes * 60) == 1)) break;
            // Given date is Sunday of a previous month. It was rejected in the previous month and should be appended to beginning of this one.
            // Works because date here can be only the end or the beginning of the month.
            if ((TimeDayOfWeek(date + TimeShiftMinutes * 60) == 0) && (TimeYear(Time[x] + TimeShiftMinutes * 60) * 12 + TimeMonth(Time[x] + TimeShiftMinutes * 60) - TimeYear(date + TimeShiftMinutes * 60) * 12 - TimeMonth(date + TimeShiftMinutes * 60) == 1)) break;
        }
        x++;
        if (x >= rates_total) break;
        time_x_day_of_week = TimeDayOfWeek(Time[x] + TimeShiftMinutes * 60);
    }

    return x;
}

//+------------------------------------------------------------------+
//| Check if two dates are in the same week.                         |
//+------------------------------------------------------------------+
int SameWeek(const datetime date1, const datetime date2)
{
    MqlDateTime dt1, dt2;

    TimeToStruct(date1, dt1);
    TimeToStruct(date2, dt2);

    int seconds_from_start = dt1.day_of_week * 24 * 3600 + dt1.hour * 3600 + dt1.min * 60 + dt1.sec;

    if (date1 == date2) return true;
    else if (date2 < date1)
    {
        if (date1 - date2 <= seconds_from_start) return true;
    }
    // 604800 - seconds in one week.
    else if (date2 - date1 < 604800 - seconds_from_start) return true;

    return false;
}

//+------------------------------------------------------------------+
//| Check if two dates are in the same month.                        |
//+------------------------------------------------------------------+
int SameMonth(const datetime date1, const datetime date2)
{
    MqlDateTime dt1, dt2;

    TimeToStruct(date1, dt1);
    TimeToStruct(date2, dt2);

    if ((dt1.mon == dt2.mon) && (dt1.year == dt2.year)) return true;
    return false;
}

//+------------------------------------------------------------------+
//| Puts a dot (rectangle) at a given position and color.            |
//| price and time are coordinates.                                  |
//| range is for the second coordinate.                              |
//| bar is to determine the color of the dot.                        |
//| Returns inverted end time only for the RightToLeft session.      |
//+------------------------------------------------------------------+
datetime PutDot(const double price, const int start_bar, const int range, const int bar, const datetime &Time[], string rectangle_prefix = "", datetime converted_time = 0)
{
    double divisor, color_shift;
    color colour = -1;

    // All dots are with the same date/time for a given origin bar, but with a different price.
    string LastNameStart = " " + TimeToString(Time[bar + start_bar]) + " ";
    string LastName = LastNameStart + DoubleToString(price, _Digits);

    if (ColorBullBear) colour = CalculateProperColor();

    // Bull/bear coloring part.
    if (NeedToReviewColors)
    {
        // Finding all dots (rectangle objects) with proper suffix and start of last name (date + time of the bar, but not price).
        // This is needed to change their color if candle changed its direction.
        int obj_total = ObjectsTotal(0);
        for (int i = obj_total - 1; i >= 0; i--)
        {
            string obj = ObjectName(0, i);
            // Not a rectangle.
            if (ObjectGetInteger(0, obj, OBJPROP_TYPE) != OBJ_RECTANGLE) continue;
            // Probably some other object.
            if (StringSubstr(obj, 0, StringLen(rectangle_prefix + "MP" + Suffix)) != rectangle_prefix + "MP" + Suffix) continue;
            // Previous bar's dot found.
            if (StringSubstr(obj, 0, StringLen(rectangle_prefix + "MP" + Suffix + LastNameStart)) != rectangle_prefix + "MP" + Suffix + LastNameStart) break;
            // Change color.
            ObjectSetInteger(0, obj, OBJPROP_COLOR, colour);
        }
    }

    if (ObjectFind(0, rectangle_prefix + "MP" + Suffix + LastName) >= 0)
    {
        if ((!RightToLeft) || (converted_time == 0)) return 0; // Normal case;
    }

    datetime time_end, time_start;
    datetime prev_time = converted_time; // For drawing, we need two times.
    if (converted_time != 0) // This is the right-to-left mode and the right-most session.
    {
        // Check if we have started a new right-most session, so the previous one should be cleaned up.
        static datetime prev_time_start_bar = 0;
        if ((Time[start_bar] != prev_time_start_bar) && (prev_time_start_bar != 0)) // New right-most session arrived - recalculate everything.
        {
            NeedToRestartDrawing = true;
        }
        prev_time_start_bar = Time[start_bar];

        // Find the time:
        int x = -1;
        for (int i = range + 1; i > 0; i--) // + 1 to get a bit "lefter" time in converted_time, and actual dot's time into prev_time.
        {
            prev_time = converted_time;
            if (converted_time == Time[0]) // First time stepped into existing candles.
            {
                x = i + 1; // Remember the position.
                converted_time = Time[1]; // Move further.
            }
            else if (converted_time < Time[0])
            {
                if (x == -1) x = iBarShift(Symbol(), Period(), converted_time) + i + 1;
                converted_time = Time[x - i]; // While inside the existing candles, use existing Time for candles.
            }
            else converted_time -= PeriodSeconds(); // While beyond the current candle, subtract fixed time periods to move left on the time scale.
        }
        time_end = converted_time;
        time_start = prev_time;
    }
    else
    {
        if (start_bar - (range + 1) < 0) time_end = Time[0] + PeriodSeconds(); // Protection from 'Array out of range' error.
        else time_end = Time[start_bar - (range + 1)];
        time_start = Time[start_bar - range];
    }

    if (ObjectFind(0, rectangle_prefix + "MP" + Suffix + LastName) >= 0) // Need to move the rectangle.
    {
        ObjectSetInteger(0, rectangle_prefix  + "MP" + Suffix + LastName, OBJPROP_TIME, 0, time_start);
        ObjectSetInteger(0, rectangle_prefix  + "MP" + Suffix + LastName, OBJPROP_TIME, 1, time_end);
    }
    else ObjectCreate(0, rectangle_prefix + "MP" + Suffix + LastName, OBJ_RECTANGLE, 0, time_start, price, time_end, price - onetick);

    if (!ColorBullBear) // Otherwise, colour is already calculated.
    {
        // Color switching depending on the distance of the bar from the session's beginning.
        int offset1, offset2;
        switch (CurrentColorScheme)
        {
        case Blue_to_Red:
            colour = 0x00FF0000; // clrBlue;
            offset1 = 0x00010000;
            offset2 = 0x00000001;
            break;
        case Red_to_Green:
            colour = 0x000000FF; // clrDarkRed;
            offset1 = 0x00000001;
            offset2 = 0x00000100;
            break;
        case Green_to_Blue:
            colour = 0x0000FF00; // clrDarkGreen;
            offset1 = 0x00000100;
            offset2 = 0x00010000;
            break;
        case Yellow_to_Cyan:
            colour = 0x0000FFFF; // clrYellow;
            offset1 = 0x00000001;
            offset2 = 0x00010000;
            break;
        case Magenta_to_Yellow:
            colour = 0x00FF00FF; // clrMagenta;
            offset1 = 0x00010000;
            offset2 = 0x00000100;
            break;
        case Cyan_to_Magenta:
            colour = 0x00FFFF00; // clrCyan;
            offset1 = 0x00000100;
            offset2 = 0x00000001;
            break;
        case Single_Color:
            colour = SingleColor;
            offset1 = 0;
            offset2 = 0;
            break;
        default:
            colour = SingleColor;
            offset1 = 0;
            offset2 = 0;
            break;
        }

        // No need to do these calculations if plain color is used.
        if (CurrentColorScheme != Single_Color)
        {
            divisor = 1.0 / 0xFF * (double)Max_number_of_bars_in_a_session;

            // bar is negative.
            color_shift = MathFloor((double)bar / divisor);

            // Prevents color overflow.
            if ((int)color_shift < -0xFF) color_shift = -0xFF;

            colour += color((int)color_shift * offset1);
            colour -= color((int)color_shift * offset2);
        }
    }

    ObjectSetInteger(0, rectangle_prefix + "MP" + Suffix + LastName, OBJPROP_COLOR, colour);
    // Fills rectangle.
    ObjectSetInteger(0, rectangle_prefix + "MP" + Suffix + LastName, OBJPROP_FILL, true);
    ObjectSetInteger(0, rectangle_prefix + "MP" + Suffix + LastName, OBJPROP_SELECTABLE, false);
    ObjectSetInteger(0, rectangle_prefix + "MP" + Suffix + LastName, OBJPROP_HIDDEN, true);

    return time_end;
}

//+------------------------------------------------------------------+
//| Deletes all chart objects created by the indicator.              |
//+------------------------------------------------------------------+
void ObjectCleanup(string rectangle_prefix = "")
{
    // Delete all rectangles with set prefix.
    ObjectsDeleteAll(0, rectangle_prefix + "MP" + Suffix, -1, OBJ_RECTANGLE);
    ObjectsDeleteAll(0, rectangle_prefix + "Median" + Suffix, -1, OBJ_TREND);
    ObjectsDeleteAll(0, rectangle_prefix + "VA_LeftSide" + Suffix, -1, OBJ_TREND);
    ObjectsDeleteAll(0, rectangle_prefix + "VA_RightSide" + Suffix, -1, OBJ_TREND);
    ObjectsDeleteAll(0, rectangle_prefix + "VA_Top" + Suffix, -1, OBJ_TREND);
    ObjectsDeleteAll(0, rectangle_prefix + "VA_Bottom" + Suffix, -1, OBJ_TREND);
    if (ShowValueAreaRays != None)
    {
        // Delete all trendlines with set prefix.
        ObjectsDeleteAll(0, rectangle_prefix + "Value Area HighRay" + Suffix, -1, OBJ_TREND);
        ObjectsDeleteAll(0, rectangle_prefix + "Value Area LowRay" + Suffix, -1, OBJ_TREND);
    }
    if (ShowMedianRays != None)
    {
        // Delete all trendlines with set prefix.
        ObjectsDeleteAll(0, rectangle_prefix + "Median Ray" + Suffix, -1, OBJ_TREND);
    }
    if (ShowKeyValues)
    {
        // Delete all text labels with set prefix.
        ObjectsDeleteAll(0, rectangle_prefix + "VAH" + Suffix, -1, OBJ_TEXT);
        ObjectsDeleteAll(0, rectangle_prefix + "VAL" + Suffix, -1, OBJ_TEXT);
        ObjectsDeleteAll(0, rectangle_prefix + "POC" + Suffix, -1, OBJ_TEXT);
    }
    if (ShowSinglePrint)
    {
        // Delete all Single Print marks.
        ObjectsDeleteAll(0, rectangle_prefix + "MPSP" + Suffix, -1, OBJ_RECTANGLE);
    }
    if (SinglePrintRays)
    {
        // Delete all Single Print rays.
        ObjectsDeleteAll(0, rectangle_prefix + "MPSPR" + Suffix, -1, OBJ_TREND);
    }
}

//+------------------------------------------------------------------+
//| Extract hours and minutes from a time string.                    |
//| Returns false in case of an error.                               |
//+------------------------------------------------------------------+
bool GetHoursAndMinutes(string time_string, int& hours, int& minutes, int& time)
{
    if (StringLen(time_string) == 4) time_string = "0" + time_string;

    if (
        // Wrong length.
        (StringLen(time_string) != 5) ||
        // Wrong separator.
        (time_string[2] != ':') ||
        // Wrong first number (only 24 hours in a day).
        ((time_string[0] < '0') || (time_string[0] > '2')) ||
        // 00 to 09 and 10 to 19.
        (((time_string[0] == '0') || (time_string[0] == '1')) && ((time_string[1] < '0') || (time_string[1] > '9'))) ||
        // 20 to 23.
        ((time_string[0] == '2') && ((time_string[1] < '0') || (time_string[1] > '3'))) ||
        // 0M to 5M.
        ((time_string[3] < '0') || (time_string[3] > '5')) ||
        // M0 to M9.
        ((time_string[4] < '0') || (time_string[4] > '9'))
    )
    {
        Print("Wrong time string: ", time_string, ". Please use HH:MM format.");
        return false;
    }

    string result[];
    int number_of_substrings = StringSplit(time_string, ':', result);
    hours = (int)StringToInteger(result[0]);
    minutes = (int)StringToInteger(result[1]);
    time = hours * 60 + minutes;

    return true;
}

//+------------------------------------------------------------------+
//| Extract hours and minutes from a time string.                    |
//| Returns false in case of an error.                               |
//+------------------------------------------------------------------+
bool CheckIntradaySession(const bool enable, const string start_time, const string end_time, const color_scheme cs)
{
    if (enable)
    {
        if (!GetHoursAndMinutes(start_time, IDStartHours[IntradaySessionCount], IDStartMinutes[IntradaySessionCount], IDStartTime[IntradaySessionCount]))
        {
            Alert("Wrong time string format: ", start_time, ".");
            return false;
        }
        if (!GetHoursAndMinutes(end_time, IDEndHours[IntradaySessionCount], IDEndMinutes[IntradaySessionCount], IDEndTime[IntradaySessionCount]))
        {
            Alert("Wrong time string format: ", end_time, ".");
            return false;
        }
        // Special case of the intraday session ending at "00:00".
        if (IDEndTime[IntradaySessionCount] == 0)
        {
            // Turn it into "24:00".
            IDEndHours[IntradaySessionCount] = 24;
            IDEndMinutes[IntradaySessionCount] = 0;
            IDEndTime[IntradaySessionCount] = 24 * 60;
        }

        IDColorScheme[IntradaySessionCount] = cs;

        // For special case used only with Ignore_Saturday_Sunday on Monday.
        if (IDEndTime[IntradaySessionCount] < IDStartTime[IntradaySessionCount]) IntradayCrossSessionDefined = IntradaySessionCount;

        IntradaySessionCount++;
    }
    return true;
}

//+------------------------------------------------------------------+
//| Main procedure to draw the Market Profile based on a session     |
//| start bar and session end bar.                                   |
//| i - session number with 0 being the oldest one.                  |
//| Returns true on success, false - on failure.                     |
//+------------------------------------------------------------------+
bool ProcessSession(const int sessionstart, const int sessionend, const int i, const double& High[], const double& Low[], const datetime& Time[], const int rates_total, CRectangleMP* rectangle = NULL)
{
    string rectangle_prefix = ""; // Only for rectangle sessions.

    if (sessionstart >= rates_total) return false; // Data not yet ready.

    double SessionMax = DBL_MIN, SessionMin = DBL_MAX;

    // Find the session's high and low.
    for (int bar = sessionstart; bar >= sessionend; bar--)
    {
        if (High[bar] > SessionMax) SessionMax = High[bar];
        if (Low[bar] < SessionMin) SessionMin = Low[bar];
    }
    SessionMax = NormalizeDouble(SessionMax, DigitsM);
    SessionMin = NormalizeDouble(SessionMin, DigitsM);

    int session_counter = i;

    if (Session == Rectangle)
    {
        rectangle_prefix = rectangle.name + "_";
        if (SessionMax > rectangle.RectanglePriceMax) SessionMax = NormalizeDouble(rectangle.RectanglePriceMax, DigitsM);
        if (SessionMin < rectangle.RectanglePriceMin) SessionMin = NormalizeDouble(rectangle.RectanglePriceMin, DigitsM);
    }
    else
    {
        // Find Time[sessionstart] among RememberSessionStart[].
        bool need_to_increment = true;

        for (int j = 0; j < SessionsNumber; j++)
        {
            if (RememberSessionStart[j] == Time[sessionstart])
            {
                need_to_increment = false;
                session_counter = j; // Real number of the session.
                break;
            }
        }
        // Raise the number of sessions and resize arrays.
        if (need_to_increment)
        {
            SessionsNumber++;
            session_counter = SessionsNumber - 1; // Newest session.
            ArrayResize(RememberSessionMax, SessionsNumber);
            ArrayResize(RememberSessionMin, SessionsNumber);
            ArrayResize(RememberSessionStart, SessionsNumber);
            ArrayResize(RememberSessionSuffix, SessionsNumber);
        }
    }

    // Adjust SessionMin, SessionMax for onetick granularity.
    SessionMax = NormalizeDouble(MathRound(SessionMax / onetick) * onetick, DigitsM);
    SessionMin = NormalizeDouble(MathRound(SessionMin / onetick) * onetick, DigitsM);

    RememberSessionMax[session_counter] = SessionMax;
    RememberSessionMin[session_counter] = SessionMin;
    RememberSessionStart[session_counter] = Time[sessionstart];
    RememberSessionSuffix[session_counter] = Suffix;

    // Used to make sure that SessionMax increments only by 'onetick' increments.
    // This is needed only when updating the latest trading session and PointMultiplier_calculated > 1.
    static double PreviousSessionMax = DBL_MIN;
    static datetime PreviousSessionStartTime = 0;
    // Reset PreviousSessionMax when a new session becomes the 'latest one'.
    if (Time[sessionstart] > PreviousSessionStartTime)
    {
        PreviousSessionMax = DBL_MIN;
        PreviousSessionStartTime = Time[sessionstart];
    }
    if ((FirstRunDone) && (i == _SessionsToCount - 1) && (PointMultiplier_calculated > 1)) // Updating the latest trading session.
    {
        if (SessionMax - PreviousSessionMax < onetick) // SessionMax increased only slightly - too small to use the new value with the current onetick.
        {
            SessionMax = PreviousSessionMax; // Do not update session max.
        }
        else
        {
            if (PreviousSessionMax != DBL_MIN)
            {
                // Calculate number of increments.
                double nc = (SessionMax - PreviousSessionMax) / onetick;
                // Adjust SessionMax.
                SessionMax = NormalizeDouble(PreviousSessionMax + MathRound(nc) * onetick, DigitsM);
            }
            PreviousSessionMax = SessionMax;
        }
    }

    int TPOperPrice[];
    // Possible price levels if multiplied to integer.
    int max = (int)MathRound((SessionMax - SessionMin) / onetick + 2); // + 2 because further we will be possibly checking array at SessionMax + 1.
    ArrayResize(TPOperPrice, max);
    ArrayInitialize(TPOperPrice, 0);

    bool SinglePrintTracking_array[]; // For SinglePrint rays.
    if (SinglePrintRays)
    {
        ArrayResize(SinglePrintTracking_array, max);
        ArrayInitialize(SinglePrintTracking_array, false);
    }
    
    int MaxRange = 0; // Maximum distance from session start to the drawn dot.
    double PriceOfMaxRange = 0; // Level of the maximum range, required to draw Median.
    double DistanceToCenter = DBL_MAX; // Closest distance to center for the Median.

    // Right to left for the final session:
    // 1. Get rightmost time.
    // 2a. If it <= Time[0] - use normal bar-walking, else:
    // 2b. To "move" to the left - subtract PeriodSeconds().
    // 3. Draw everything based on that Time.
    // 4. Redraw everything every time the rightmost time changes.
    // 5. Ray lines to the left.

    // Right-to-left depiction of the rightmost session.
    datetime converted_time = 0;
    datetime converted_end_time = 0;
    datetime min_converted_end_time = UINT_MAX;
    if ((RightToLeft) && ((sessionend == 0) || (Session == Rectangle)))
    {
        int dummy_subwindow;
        double dummy_price;
        if (Session == Rectangle) converted_time = rectangle.RectangleTimeMax;
        else ChartXYToTimePrice(0, (int)ChartGetInteger(0, CHART_WIDTH_IN_PIXELS), 0, dummy_subwindow, converted_time, dummy_price);
    }

    int TotalTPO = 0; // Total amount of dots (TPO's).

    // Going through all possible quotes from session's High to session's Low.
    for (double price = SessionMax; price >= SessionMin; price -= onetick)
    {
        price = NormalizeDouble(price, DigitsM);
        int range = 0; // Distance from first bar to the current bar.

        // Going through all bars of the session to see if the price was encountered here.
        for (int bar = sessionstart; bar >= sessionend; bar--)
        {
            // Price is encountered in the given bar.
            if ((price >= Low[bar]) && (price <= High[bar]))
            {
                // Update maximum distance from session's start to the found bar (needed for Median).
                // Using the center-most Median if there are more than one.
                if ((MaxRange < range) || ((MaxRange == range) && (MathAbs(price - (SessionMin + (SessionMax - SessionMin) / 2)) < DistanceToCenter)))
                {
                    MaxRange = range;
                    PriceOfMaxRange = price;
                    DistanceToCenter = MathAbs(price - (SessionMin + (SessionMax - SessionMin) / 2));
                }

                if (!DisableHistogram)
                {
                    if (ColorBullBear)
                    {
                        // These are needed in all cases when we color dots according to bullish/bearish bars.
                        double close = iClose(NULL, PERIOD_CURRENT, bar);
                        double open = iOpen(NULL, PERIOD_CURRENT, bar);
                        if (close == open) CurrentBarDirection = Neutral;
                        else if (close > open) CurrentBarDirection = Bullish;
                        else if (close < open) CurrentBarDirection = Bearish;

                        // This is for recoloring of the dots from the current (most-latest) bar.
                        if (bar == 0)
                        {
                            if (PreviousBarDirection == CurrentBarDirection) NeedToReviewColors = false;
                            else
                            {
                                NeedToReviewColors = true;
                                PreviousBarDirection = CurrentBarDirection;
                            }
                        }
                    }

                    // Draws rectangle.
                    if (!RightToLeft) PutDot(price, sessionstart, range, bar - sessionstart, Time, rectangle_prefix);
                    // Inverted drawing.
                    else
                    {
                        converted_end_time = PutDot(price, sessionstart, range, bar - sessionstart, Time, rectangle_prefix, converted_time);
                        if (converted_end_time < min_converted_end_time) min_converted_end_time = converted_end_time; // Find the leftmost time to use for the left border of the value area.
                    }
                }

                // Remember the number of encountered bars for this price.
                int index = (int)MathRound((price - SessionMin) / onetick);
                TPOperPrice[index]++;
                range++;
                TotalTPO++;
            }
        }
        // Single print marking is due at this price.
        if (ShowSinglePrint)
        {
            if (range == 1) PutSinglePrintMark(price, sessionstart, rectangle_prefix, Time);
            else if (range > 1) RemoveSinglePrintMark(price, sessionstart, rectangle_prefix, Time); // Remove single print max if it exists.
        }
        
        if (SinglePrintRays)
        {
            int index = (int)MathRound((price - SessionMin) / onetick);
            if (range == 1) SinglePrintTracking_array[index] = true; // Remember the single print's position relative to the price.
        }
    }

    // Single Print Rays
    // Go through all prices again, check TPOs - whether they are single and whether they aren't bordered by another single print TPOs?
    if (SinglePrintRays)
    {
        for (double price = SessionMax; price >= SessionMin; price -= onetick)
        {
            price = NormalizeDouble(price, DigitsM);
            int index = (int)MathRound((price - SessionMin) / onetick);
            if (SinglePrintTracking_array[index])
            {
                if (price == SessionMax) // Top of the session.
                {
                    PutSinglePrintRay(price, sessionstart, rectangle_prefix, Time);
                }
                else
                {
                    if (SinglePrintTracking_array[index + 1] == false) // Above is a non-single print.
                    {
                        PutSinglePrintRay(price, sessionstart, rectangle_prefix, Time);
                    }
                }
                if (price == SessionMin) // Bottom of the session.
                {
                    PutSinglePrintRay(price - onetick, sessionstart, rectangle_prefix, Time);
                }
                else
                {
                    if (SinglePrintTracking_array[index - 1] == false) // Below is a non-single print.
                    {
                        PutSinglePrintRay(price - onetick, sessionstart, rectangle_prefix, Time);
                    }
                }
            }
            else
            {
                // Attempt to remove a horizontal line above and below the "potentially no longer existing" single print mark.
                RemoveSinglePrintRay(price, sessionstart, rectangle_prefix, Time);
                RemoveSinglePrintRay(price - onetick, sessionstart, rectangle_prefix, Time);
            }
        }
    }
    
    if (EnableDevelopingPOC) CalculateDevelopingPOC(sessionstart, sessionend, High, Low, rectangle); // Developing POC if necessary.
    
    // Calculate amount of TPO's in the Value Area.
    int ValueControlTPO = (int)((double)TotalTPO * ValueAreaPercentage_double);
    // Start with the TPO's of the Median.
    int index = (int)((PriceOfMaxRange - SessionMin) / onetick);
    if (index < 0) return false; // Data not yet ready.
    int TPOcount = TPOperPrice[index];

    // Go through the price levels above and below median adding the biggest to TPO count until the 70% of TPOs are inside the Value Area.
    int up_offset = 1;
    int down_offset = 1;
    while (TPOcount < ValueControlTPO)
    {
        double abovePrice = PriceOfMaxRange + up_offset * onetick;
        double belowPrice = PriceOfMaxRange - down_offset * onetick;
        // If belowPrice is out of the session's range then we should add only abovePrice's TPO's, and vice versa.
        index = (int)MathRound((abovePrice - SessionMin) / onetick);
        int index2 = (int)MathRound((belowPrice - SessionMin) / onetick);
        if (((belowPrice < SessionMin) || (TPOperPrice[index] >= TPOperPrice[index2])) && (abovePrice <= SessionMax))
        {
            TPOcount += TPOperPrice[index];
            up_offset++;
        }
        else if (belowPrice >= SessionMin)
        {
            TPOcount += TPOperPrice[index2];
            down_offset++;
        }
        // Cannot proceed - too few data points.
        else if (TPOcount < ValueControlTPO)
        {
            break;
        }
    }
    string LastName = " " + TimeToString(Time[sessionstart], TIME_DATE);
    // Delete old Median.
    if (ObjectFind(0, rectangle_prefix + "Median" + Suffix + LastName) >= 0) ObjectDelete(0, rectangle_prefix + "Median" + Suffix + LastName);
    // Draw a new one.
    index = (int)MathMax(sessionstart - MaxRange - 1, 0);
    datetime time_start, time_end;
    if ((RightToLeft) && ((sessionend == 0) || (Session == Rectangle)))
    {
        time_end = min_converted_end_time;
        time_start = converted_time;
    }
    else
    {
        time_end = Time[index];
        time_start = Time[sessionstart];
    }
    ObjectCreate(0, rectangle_prefix + "Median" + Suffix + LastName, OBJ_TREND, 0, time_start, PriceOfMaxRange, time_end, PriceOfMaxRange);
    color mc = MedianColor;
    // Prominent Median (PPOC):
    if ((double)(sessionstart - index) / (double)Max_number_of_bars_in_a_session * 100 >= ProminentMedianPercentage)
    {
        mc = ProminentMedianColor;
        ObjectSetInteger(0, rectangle_prefix + "Median" + Suffix + LastName, OBJPROP_WIDTH, ProminentMedianWidth);
        ObjectSetInteger(0, rectangle_prefix + "Median" + Suffix + LastName, OBJPROP_STYLE, ProminentMedianStyle);
    }
    else
    {
        ObjectSetInteger(0, rectangle_prefix + "Median" + Suffix + LastName, OBJPROP_WIDTH, MedianWidth);
        ObjectSetInteger(0, rectangle_prefix + "Median" + Suffix + LastName, OBJPROP_STYLE, MedianStyle);
    }
    ObjectSetInteger(0, rectangle_prefix + "Median" + Suffix + LastName, OBJPROP_COLOR, mc);
    ObjectSetInteger(0, rectangle_prefix + "Median" + Suffix + LastName, OBJPROP_BACK, false);
    ObjectSetInteger(0, rectangle_prefix + "Median" + Suffix + LastName, OBJPROP_SELECTABLE, false);
    ObjectSetInteger(0, rectangle_prefix + "Median" + Suffix + LastName, OBJPROP_HIDDEN, true);
    ObjectSetInteger(0, rectangle_prefix + "Median" + Suffix + LastName, OBJPROP_RAY, false);

    // Delete old Value Area Sides.
    if (ObjectFind(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName) >= 0) ObjectDelete(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName);
    // Draw a new one.
    ObjectCreate(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName, OBJ_TREND, 0, time_start, PriceOfMaxRange + up_offset * onetick, time_start, PriceOfMaxRange - down_offset * onetick);
    ObjectSetInteger(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName, OBJPROP_COLOR, ValueAreaSidesColor);
    ObjectSetInteger(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName, OBJPROP_STYLE, ValueAreaSidesStyle);
    ObjectSetInteger(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName, OBJPROP_WIDTH, ValueAreaSidesWidth);
    ObjectSetInteger(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName, OBJPROP_BACK, false);
    ObjectSetInteger(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName, OBJPROP_SELECTABLE, false);
    ObjectSetInteger(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName, OBJPROP_HIDDEN, true);
    ObjectSetInteger(0, rectangle_prefix + "VA_LeftSide" + Suffix + LastName, OBJPROP_RAY, false);
    if (ObjectFind(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName) >= 0) ObjectDelete(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName);
    // Draw a new one.
    ObjectCreate(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName, OBJ_TREND, 0, time_end, PriceOfMaxRange + up_offset * onetick, time_end, PriceOfMaxRange - down_offset * onetick);
    ObjectSetInteger(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName, OBJPROP_COLOR, ValueAreaSidesColor);
    ObjectSetInteger(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName, OBJPROP_STYLE, ValueAreaSidesStyle);
    ObjectSetInteger(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName, OBJPROP_WIDTH, ValueAreaSidesWidth);
    ObjectSetInteger(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName, OBJPROP_BACK, false);
    ObjectSetInteger(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName, OBJPROP_SELECTABLE, false);
    ObjectSetInteger(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName, OBJPROP_HIDDEN, true);
    ObjectSetInteger(0, rectangle_prefix + "VA_RightSide" + Suffix + LastName, OBJPROP_RAY, false);

    // Delete old Value Area Top and Bottom.
    if (ObjectFind(0, rectangle_prefix + "VA_Top" + Suffix + LastName) >= 0) ObjectDelete(0, rectangle_prefix + "VA_Top" + Suffix + LastName);
    // Draw a new one.
    ObjectCreate(0, rectangle_prefix + "VA_Top" + Suffix + LastName, OBJ_TREND, 0, time_start, PriceOfMaxRange + up_offset * onetick, time_end, PriceOfMaxRange + up_offset * onetick);
    ObjectSetInteger(0, rectangle_prefix + "VA_Top" + Suffix + LastName, OBJPROP_COLOR, ValueAreaHighLowColor);
    ObjectSetInteger(0, rectangle_prefix + "VA_Top" + Suffix + LastName, OBJPROP_STYLE, ValueAreaHighLowStyle);
    ObjectSetInteger(0, rectangle_prefix + "VA_Top" + Suffix + LastName, OBJPROP_WIDTH, ValueAreaHighLowWidth);
    ObjectSetInteger(0, rectangle_prefix + "VA_Top" + Suffix + LastName, OBJPROP_BACK, false);
    ObjectSetInteger(0, rectangle_prefix + "VA_Top" + Suffix + LastName, OBJPROP_SELECTABLE, false);
    ObjectSetInteger(0, rectangle_prefix + "VA_Top" + Suffix + LastName, OBJPROP_HIDDEN, true);
    ObjectSetInteger(0, rectangle_prefix + "VA_Top" + Suffix + LastName, OBJPROP_RAY, false);
    if (ObjectFind(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName) >= 0) ObjectDelete(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName);
    // Draw a new one.
    ObjectCreate(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName, OBJ_TREND, 0, time_start, PriceOfMaxRange - down_offset * onetick, time_end, PriceOfMaxRange - down_offset * onetick);
    ObjectSetInteger(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName, OBJPROP_COLOR, ValueAreaHighLowColor);
    ObjectSetInteger(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName, OBJPROP_STYLE, ValueAreaHighLowStyle);
    ObjectSetInteger(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName, OBJPROP_WIDTH, ValueAreaHighLowWidth);
    ObjectSetInteger(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName, OBJPROP_BACK, false);
    ObjectSetInteger(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName, OBJPROP_SELECTABLE, false);
    ObjectSetInteger(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName, OBJPROP_HIDDEN, true);
    ObjectSetInteger(0, rectangle_prefix + "VA_Bottom" + Suffix + LastName, OBJPROP_RAY, false);

    // VAH, VAL, and POC printout.
    if (ShowKeyValues)
    {
        ENUM_ANCHOR_POINT anchor_poc = ANCHOR_RIGHT, anchor_va = ANCHOR_RIGHT;
        if ((RightToLeft) && ((sessionend == 0) || (Session == Rectangle)))
        {
            time_start = time_end; // Inverting label display position.
            // Value Area printout position.
            if (((Session != Rectangle) && ((ShowValueAreaRays == All) || (ShowValueAreaRays == Current) || (ShowValueAreaRays == PreviousCurrent))) // For non-rectangle sessions, it is already known that it is the current session, so just check if current session uses rays.
                    || ((Session == Rectangle) && ( // For rectangles, need to check which session is it and whether it has rays.
                            (((ShowValueAreaRays == AllPrevious) && (SessionsNumber - session_counter >= 2)) ||
                             (((ShowValueAreaRays == Previous) || (ShowValueAreaRays == PreviousCurrent)) && (SessionsNumber - session_counter == 2)) ||
                             (((ShowValueAreaRays == Current) || (ShowValueAreaRays == PreviousCurrent)) && (SessionsNumber - session_counter == 1)) ||
                             (ShowValueAreaRays == All)))
                       ))
            {
                anchor_va = ANCHOR_RIGHT_LOWER;
            }
            // Median printout position.
            if (((Session != Rectangle) && ((ShowMedianRays == All) || (ShowMedianRays == Current) || (ShowMedianRays == PreviousCurrent))) // For non-rectangle sessions, it is already known that it is the current session, so just check if current session uses rays.
                    || ((Session == Rectangle) && ( // For rectangles, need to check which session is it and whether it has rays.
                            (((ShowMedianRays == AllPrevious) && (SessionsNumber - session_counter >= 2)) ||
                             (((ShowMedianRays == Previous) || (ShowMedianRays == PreviousCurrent)) && (SessionsNumber - session_counter == 2)) ||
                             (((ShowMedianRays == Current) || (ShowMedianRays == PreviousCurrent)) && (SessionsNumber - session_counter == 1)) ||
                             (ShowMedianRays == All)))
                       ))
            {
                anchor_poc = ANCHOR_RIGHT_LOWER;
            }
        }
        ValuePrintOut(rectangle_prefix + "VAH" + Suffix + LastName, time_start, PriceOfMaxRange + up_offset * onetick, anchor_va);
        ValuePrintOut(rectangle_prefix + "VAL" + Suffix + LastName, time_start, PriceOfMaxRange - down_offset * onetick, anchor_va);
        ValuePrintOut(rectangle_prefix + "POC" + Suffix + LastName, time_start, PriceOfMaxRange, anchor_poc);
    }

    return true;
}

//+------------------------------------------------------------------+
//| A cycle through intraday sessions for a given day with necessary |
//| checks.                                                          |
//| Returns true on success, false - on failure.                     |
//+------------------------------------------------------------------+
bool ProcessIntradaySession(int sessionstart, int sessionend, const int i, const double& High[], const double& Low[], const datetime& Time[], const int rates_total)
{
    // 'remember_*' vars point at day start and day end throughout this function.
    int remember_sessionstart = sessionstart;
    int remember_sessionend = sessionend;

    if (remember_sessionend >= rates_total) return false;

    // Special case stuff.
    bool ContinuePreventionFlag = false;

    // Start a cycle through intraday sessions if needed.
    // For each intraday session, find its own sessionstart and sessionend.
    int IntradaySessionCount_tmp = IntradaySessionCount;
    // If Ignore_Saturday_Sunday is on, day's start is on Monday, and there is a "22:00-06:00"-style intraday session defined, increase the counter to run the "later" "22:00-06:00" session and create this temporary dummy session.
    if ((SaturdaySunday == Ignore_Saturday_Sunday) && (TimeDayOfWeek(Time[remember_sessionstart] + TimeShiftMinutes * 60) == 1) && (IntradayCrossSessionDefined > -1))
    {
        IntradaySessionCount_tmp++;
    }

    for (int intraday_i = 0; intraday_i < IntradaySessionCount_tmp; intraday_i++)
    {
        // Continue was triggered during the special case iteration.
        if (ContinuePreventionFlag) break;
        // Special case iteration.
        if (intraday_i == IntradaySessionCount)
        {
            intraday_i = IntradayCrossSessionDefined;
            ContinuePreventionFlag = true;
        }
        Suffix = "_ID" + IntegerToString(intraday_i);
        CurrentColorScheme = IDColorScheme[intraday_i];
        // Get minutes.
        Max_number_of_bars_in_a_session = IDEndTime[intraday_i] - IDStartTime[intraday_i];
        // If end is less than beginning:
        if (Max_number_of_bars_in_a_session < 0)
        {
            Max_number_of_bars_in_a_session = 24 * 60 + Max_number_of_bars_in_a_session;
            if (SaturdaySunday == Ignore_Saturday_Sunday)
            {
                // Day start is on Monday. And it is not a special additional intra-Monday session.
                if ((TimeDayOfWeek(Time[remember_sessionstart] + TimeShiftMinutes * 60) == 1) && (!ContinuePreventionFlag))
                {
                    // Cut out Sunday part.
                    Max_number_of_bars_in_a_session -= 24 * 60 - IDStartTime[intraday_i];
                }
                // Day start is on Friday.
                else if (TimeDayOfWeek(Time[remember_sessionstart] + TimeShiftMinutes * 60) == 5)
                {
                    // Cut out Saturday part.
                    Max_number_of_bars_in_a_session -= IDEndTime[intraday_i];
                }
            }
        }

        // If Append_Saturday_Sunday is on:
        if (SaturdaySunday == Append_Saturday_Sunday)
        {
            // The intraday session starts on 00:00 or otherwise captures midnight, and remember_sessionstart points to Sunday:
            if (((IDStartTime[intraday_i] == 0) || (IDStartTime[intraday_i] > IDEndTime[intraday_i])) && (TimeDayOfWeek(Time[remember_sessionstart] + TimeShiftMinutes * 60) == 0))
            {
                // Add Sunday hours.
                Max_number_of_bars_in_a_session += 24 * 60 - (TimeHour(Time[remember_sessionstart] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[remember_sessionstart] + TimeShiftMinutes * 60));
                // Remove the part of Sunday that has already been added before.
                if (IDStartTime[intraday_i] > IDEndTime[intraday_i]) Max_number_of_bars_in_a_session -= 24 * 60 - IDStartTime[intraday_i];
            }
            // The intraday session ends on 00:00 or otherwise captures midnight, and remember_sessionstart points to Friday:
            else if (((IDEndTime[intraday_i] == 24 * 60) || (IDStartTime[intraday_i] > IDEndTime[intraday_i])) && (TimeDayOfWeek(Time[remember_sessionstart] + TimeShiftMinutes * 60) == 5))
            {
                // Add Saturday hours. The thing is we don't know how many hours there will be on Saturday. So add to max.
                Max_number_of_bars_in_a_session += 24 * 60;
                // Remove the part of Saturday that has already been added before.
                if (IDStartTime[intraday_i] > IDEndTime[intraday_i]) Max_number_of_bars_in_a_session -= 24 * 60 - IDEndTime[intraday_i];
            }
        }

        Max_number_of_bars_in_a_session = Max_number_of_bars_in_a_session / (PeriodSeconds() / 60);

        // If it is the updating stage, we need to recalculate only those intraday sessions that include the current bar.
        int hour, minute, time;
        if (FirstRunDone)
        {
            //sessionstart = day_start;
            hour = TimeHour(Time[0] + TimeShiftMinutes * 60);
            minute = TimeMinute(Time[0] + TimeShiftMinutes * 60);
            time = hour * 60 + minute;

            // For example, 13:00-18:00.
            if (IDStartTime[intraday_i] < IDEndTime[intraday_i])
            {
                if (SaturdaySunday == Append_Saturday_Sunday)
                {
                    // Skip all sessions that do not absorb Sunday session:
                    if ((IDStartTime[intraday_i] != 0) && (TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 0)) continue;
                    // Skip all sessions that do not absorb Saturday session:
                    if ((IDEndTime[intraday_i] != 24 * 60) && (TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 6)) continue;
                }
                // If Append_Saturday_Sunday is on and the session starts on 00:00, and now is either Sunday or Monday before the session's end:
                if ((SaturdaySunday == Append_Saturday_Sunday) && (IDStartTime[intraday_i] == 0) && ((TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 0) || ((TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 1) && (time < IDEndTime[intraday_i]))))
                {
                    // Then we can use remember_sessionstart as the session's start.
                    sessionstart = remember_sessionstart;
                }
                else if (((time < IDEndTime[intraday_i]) && (time >= IDStartTime[intraday_i]))
                         // If Append_Saturday_Sunday is on and the session ends on 24:00, and now is Saturday, then go on in case, for example, of 18:00 Saturday time and 16:00-00:00 defined session.
                         || ((SaturdaySunday == Append_Saturday_Sunday) && (IDEndTime[intraday_i] == 24 * 60) && (TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 6)))
                {
                    sessionstart = 0;
                    int sessiontime = TimeHour(Time[sessionstart] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[sessionstart] + TimeShiftMinutes * 60);
                    while (((sessiontime > IDStartTime[intraday_i])
                            // Prevents problems when the day has partial data (e.g. Sunday) when neither appending not ignoring Saturday/Sunday. Alternatively, continue looking for the sessionstart bar if we moved from Saturday to Friday with Append_Saturday_Sunday and for XX:XX-00:00 session.
                            && ((TimeDayOfYear(Time[sessionstart] + TimeShiftMinutes * 60) == TimeDayOfYear(Time[0] + TimeShiftMinutes * 60)) || ((SaturdaySunday == Append_Saturday_Sunday) && (IDEndTime[intraday_i] == 24 * 60) && (TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 6))))
                            // If Append_Saturday_Sunday is on and the session ends on 24:00 and the session start is now going through Saturday, then go on in case, for example, of 13:00 Saturday time and 16:00-00:00 defined session.
                            || ((SaturdaySunday == Append_Saturday_Sunday) && (IDEndTime[intraday_i] == 24 * 60) && (TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 6)))
                    {
                        sessionstart++;
                        sessiontime = TimeHour(Time[sessionstart] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[sessionstart] + TimeShiftMinutes * 60);
                    }
                    // This check is necessary because sessionstart may pass to the wrong day in some cases.
                    if (sessionstart > remember_sessionstart) sessionstart = remember_sessionstart;
                }
                else continue;
            }
            // For example, 22:00-6:00.
            else if (IDStartTime[intraday_i] > IDEndTime[intraday_i])
            {
                // If Append_Saturday_Sunday is on and now is either Sunday or Monday before the session's end:
                if ((SaturdaySunday == Append_Saturday_Sunday) && ((TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 0) || ((TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 1) && (time < IDEndTime[intraday_i]))))
                {
                    // Then we can use remember_sessionstart as the session's start.
                    sessionstart = remember_sessionstart;
                }
                // If Ignore_Saturday_Sunday is on and it is Monday before the session's end:
                else if ((SaturdaySunday == Ignore_Saturday_Sunday) && (TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 1) && (time < IDEndTime[intraday_i]))
                {
                    // Then we can use remember_sessionstart as the session's start.
                    sessionstart = remember_sessionstart;
                }
                else if (((time < IDEndTime[intraday_i]) || (time >= IDStartTime[intraday_i]))
                         // If Append_Saturday_Sunday is on and now is Saturday, then go on in case, for example, of 18:00 Saturday time and 22:00-06:00 defined session.
                         || ((SaturdaySunday == Append_Saturday_Sunday) && (TimeDayOfWeek(Time[0] + TimeShiftMinutes * 60) == 6)))
                {
                    sessionstart = 0;
                    int sessiontime = TimeHour(Time[sessionstart] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[sessionstart] + TimeShiftMinutes * 60);
                    // Within 24 hours of the current time - but can be today or yesterday.
                    while (((sessiontime > IDStartTime[intraday_i]) && (Time[0] - Time[sessionstart] <= 3600 * 24))
                            // Same day only.
                            || ((sessiontime < IDEndTime[intraday_i]) && (TimeDayOfYear(Time[sessionstart] + TimeShiftMinutes * 60) == TimeDayOfYear(Time[0] + TimeShiftMinutes * 60)))
                            // If Append_Saturday_Sunday is on and the session start is now going through Saturday, then go on in case, for example, of 18:00 Saturday time and 22:00-06:00 defined session.
                            || ((SaturdaySunday == Append_Saturday_Sunday) && (TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 6)))
                    {
                        sessionstart++;
                        sessiontime = TimeHour(Time[sessionstart] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[sessionstart] + TimeShiftMinutes * 60);
                    }
                    // When the same condition in the above while cycle fails and sessionstart is one step farther than needed.
                    if (Time[0] - Time[sessionstart] > 3600 * 24) sessionstart--;
                }
                else continue;
            }
            // If start time equals end time, we can skip the session.
            else continue;

            // Because apparently, we are still inside the session.
            sessionend = 0;

            if (!ProcessSession(sessionstart, sessionend, i, High, Low, Time, rates_total)) return false;
        }
        // If it is the first run.
        else
        {
            sessionend = remember_sessionend;

            // Process the sessions that start today.
            // For example, 13:00-18:00.
            if (IDStartTime[intraday_i] < IDEndTime[intraday_i])
            {
                // If Append_Saturday_Sunday is on and the session ends on 24:00, and day's start is on Friday and day's end is on Saturday, then do not trigger 'continue' in case, for example, of 15:00 Saturday end and 16:00-00:00 defined session.
                if ((SaturdaySunday == Append_Saturday_Sunday)/* && (IDEndTime[intraday_i] == 24 * 60)*/ && (TimeDayOfWeek(Time[remember_sessionend] + TimeShiftMinutes * 60) == 6) && (TimeDayOfWeek(Time[remember_sessionstart] + TimeShiftMinutes * 60) == 5))
                {
                }
                // Intraday session starts after the today's actual session ended (for Friday/Saturday cases).
                else if (TimeHour(Time[remember_sessionend] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[remember_sessionend] + TimeShiftMinutes * 60) < IDStartTime[intraday_i]) continue;
                // If Append_Saturday_Sunday is on and the session starts on 00:00, and the session end points to Sunday or end points to Monday and start points to Sunday, then do not trigger 'continue' in case, for example, of 18:00 Sunday start and 00:00-16:00 defined session.
                if ((SaturdaySunday == Append_Saturday_Sunday) && (((IDStartTime[intraday_i] == 0) && (TimeDayOfWeek(Time[remember_sessionend] + TimeShiftMinutes * 60) == 0)) || ((TimeDayOfWeek(Time[remember_sessionend] + TimeShiftMinutes * 60) == 1) && (TimeDayOfWeek(Time[remember_sessionstart] + TimeShiftMinutes * 60) == 0))))
                {
                }
                // Intraday session ends before the today's actual session starts (for Sunday cases).
                else if (TimeHour(Time[remember_sessionstart] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[remember_sessionstart] + TimeShiftMinutes * 60) >= IDEndTime[intraday_i]) continue;
                // If Append_Saturday_Sunday is on and the session ends on 24:00, and the start points to Friday:
                if ((SaturdaySunday == Append_Saturday_Sunday) && (IDEndTime[intraday_i] == 24 * 60) && (TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 5))
                {
                    // We already have sessionend right because it is the same as remember_sessionend (end of Saturday).
                }
                // If Append_Saturday_Sunday is on and the session starts on 00:00 and the session end points to Sunday (it is current Sunday session , no Monday bars yet):
                else if ((SaturdaySunday == Append_Saturday_Sunday) && (IDStartTime[intraday_i] == 0) && (TimeDayOfWeek(Time[sessionend] + TimeShiftMinutes * 60) == 0))
                {
                    // We already have sessionend right because it is the same as remember_sessionend (current bar and it is on Sunday).
                }
                // Otherwise find the session end.
                else while ((sessionend < rates_total) && ((TimeHour(Time[sessionend] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[sessionend] + TimeShiftMinutes * 60) >= IDEndTime[intraday_i]) || ((TimeDayOfWeek(Time[sessionend] + TimeShiftMinutes * 60) == 6) && (SaturdaySunday == Append_Saturday_Sunday))))
                    {
                        sessionend++;
                    }
                if (sessionend == rates_total) sessionend--;

                // If Append_Saturday_Sunday is on and the session starts on 00:00 and the session start is now going through Sunday:
                if ((SaturdaySunday == Append_Saturday_Sunday) && (IDStartTime[intraday_i] == 0) && (TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 0))
                {
                    // We already have sessionstart right because it is the same as remember_sessionstart (start of Sunday).
                    sessionstart = remember_sessionstart;
                }
                else
                {
                    sessionstart = sessionend;
                    while ((sessionstart < rates_total) && (((TimeHour(Time[sessionstart] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[sessionstart] + TimeShiftMinutes * 60) >= IDStartTime[intraday_i])
                                                            // Same day - for cases when the day does not contain intraday session start time. Alternatively, continue looking for the sessionstart bar if we moved from Saturday to Friday with Append_Saturday_Sunday and for XX:XX-00:00 session.
                                                            && ((TimeDayOfYear(Time[sessionstart] + TimeShiftMinutes * 60) == TimeDayOfYear(Time[sessionend] + TimeShiftMinutes * 60)) || ((SaturdaySunday == Append_Saturday_Sunday) && (IDEndTime[intraday_i] == 24 * 60) && (TimeDayOfWeek(Time[sessionend] + TimeShiftMinutes * 60) == 6))))
                                                            // If Append_Saturday_Sunday is on and the session ends on 24:00, and the session start is now going through Saturday, then go on in case, for example, of 15:00 Saturday end and 16:00-00:00 defined session.
                                                            || ((SaturdaySunday == Append_Saturday_Sunday) && (IDEndTime[intraday_i] == 24 * 60) && (TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 6))
                                                           ))
                    {
                        sessionstart++;
                    }
                    sessionstart--;
                }
            }
            // For example, 22:00-6:00.
            else if (IDStartTime[intraday_i] > IDEndTime[intraday_i])
            {
                // If Append_Saturday_Sunday is on and the start points to Friday, then do not trigger 'continue' in case, for example, of 15:00 Saturday end and 22:00-06:00 defined session.
                if ((SaturdaySunday == Append_Saturday_Sunday) && (((TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 5) && (TimeDayOfWeek(Time[remember_sessionend] + TimeShiftMinutes * 60) == 6)) || ((TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 0) && (TimeDayOfWeek(Time[remember_sessionend] + TimeShiftMinutes * 60) == 1))))
                {
                }
                // Today's intraday session starts after the end of the actual session (for Friday/Saturday cases).
                else if (TimeHour(Time[remember_sessionend] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[remember_sessionend] + TimeShiftMinutes * 60) < IDStartTime[intraday_i]) continue;

                // If Append_Saturday_Sunday is on and the session start is on Sunday:
                if ((SaturdaySunday == Append_Saturday_Sunday) && (TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 0))
                {
                    // We already have sessionstart right because it is the same as remember_sessionstart (start of Sunday).
                    sessionstart = remember_sessionstart;
                }
                // If Ignore_Saturday_Sunday is on and it is Monday: (and it is not a special additional intra-Monday session.)
                else if ((SaturdaySunday == Ignore_Saturday_Sunday) && (TimeDayOfWeek(Time[remember_sessionstart] + TimeShiftMinutes * 60) == 1) && (!ContinuePreventionFlag))
                {
                    // Then we can use remember_sessionstart as the session's start.
                    sessionstart = remember_sessionstart;
                    // Monday starts on 7:00 and we have 22:00-6:00. Skip it.
                    if (TimeHour(Time[sessionstart] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[sessionstart] + TimeShiftMinutes * 60) >= IDEndTime[intraday_i]) continue;
                }
                else
                {
                    // Find starting bar.
                    sessionstart = remember_sessionend; // Start from the end.
                    while ((sessionstart < rates_total) && (((TimeHour(Time[sessionstart] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[sessionstart] + TimeShiftMinutes * 60) >= IDStartTime[intraday_i])
                                                            // Same day - for cases when the day does not contain intraday session start time.
                                                            && ((TimeDayOfYear(Time[sessionstart] + TimeShiftMinutes * 60) == TimeDayOfYear(Time[remember_sessionend] + TimeShiftMinutes * 60)) || (TimeDayOfYear(Time[sessionstart] + TimeShiftMinutes * 60) == TimeDayOfYear(Time[remember_sessionstart] + TimeShiftMinutes * 60))))
                                                            // If Append_Saturday_Sunday is on and the session start is now going through Saturday, then go on in case, for example, of 15:00 Saturday end and 22:00-06:00 defined session.
                                                            || ((SaturdaySunday == Append_Saturday_Sunday) && (TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 6))
                                                           ))
                    {
                        sessionstart++;
                    }
                    sessionstart--;
                }

                int sessionlength; // In seconds.
                // If Append_Saturday_Sunday is on and the end points to Saturday, don't go through this calculation because sessionend = remember_sessionend.
                if ((SaturdaySunday == Append_Saturday_Sunday) && (TimeDayOfWeek(Time[sessionend] + TimeShiftMinutes * 60) == 6))
                {
                    // We already have sessionend right because it is the same as remember_sessionend (end of Saturday).
                }
                // If Append_Saturday_Sunday is on and the start points to Sunday, use a simple method to find the end.
                else if ((SaturdaySunday == Append_Saturday_Sunday) && (TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 0))
                {
                    // While we are on Monday and sessionend is pointing on bar after IDEndTime.
                    while ((sessionend < rates_total) && (TimeDayOfWeek(Time[sessionend] + TimeShiftMinutes * 60) == 1) && (TimeHour(Time[sessionend] + TimeShiftMinutes * 60) * 60 + TimeMinute(Time[sessionend] + TimeShiftMinutes * 60) >= IDEndTime[intraday_i]))
                    {
                        sessionend++;
                    }
                }
                // If Ignore_Saturday_Sunday is on and the session starts on Friday:
                else if ((SaturdaySunday == Ignore_Saturday_Sunday) && (TimeDayOfWeek(Time[remember_sessionstart] + TimeShiftMinutes * 60) == 5))
                {
                    // Then it also ends on Friday.
                    sessionend = remember_sessionend;
                }
                else
                {
                    sessionend = sessionstart;
                    sessionlength = (24 * 60 - IDStartTime[intraday_i] + IDEndTime[intraday_i]) * 60;
                    // If ignoring Sundays and session start is on Monday, cut out Sunday part of the intraday session. And it is not a special additional intra-Monday session.
                    if ((SaturdaySunday == Ignore_Saturday_Sunday) && (TimeDayOfWeek(Time[sessionstart] + TimeShiftMinutes * 60) == 1) && (!ContinuePreventionFlag)) sessionlength -= (24 * 60 - IDStartTime[intraday_i]) * 60;
                    while ((sessionend >= 0) && (Time[sessionend] - Time[sessionstart] < sessionlength))
                    {
                        sessionend--;
                    }
                    sessionend++;
                }
            }
            // If start time equals end time, we can skip the session.
            else continue;

            if (sessionend == sessionstart) continue; // No need to process such an intraday session.

            if (!ProcessSession(sessionstart, sessionend, i, High, Low, Time, rates_total)) return false;
        }
    }
    Suffix = "_ID";

    return true;
}

//
int TimeHour(const datetime time)
{
    MqlDateTime dt;
    TimeToStruct(time, dt);
    return dt.hour;
}

//
int TimeMinute(const datetime time)
{
    MqlDateTime dt;
    TimeToStruct(time, dt);
    return dt.min;
}

//
int TimeDay(const datetime time)
{
    MqlDateTime dt;
    TimeToStruct(time, dt);
    return dt.day;
}

//
int TimeDayOfWeek(const datetime time)
{
    MqlDateTime dt;
    TimeToStruct(time, dt);
    return dt.day_of_week;
}

//+
int TimeDayOfYear(const datetime time)
{
    MqlDateTime dt;
    TimeToStruct(time, dt);
    return dt.day_of_year;
}

//
int TimeMonth(const datetime time)
{
    MqlDateTime dt;
    TimeToStruct(time, dt);
    return dt.mon;
}

//
int TimeYear(const datetime time)
{
    MqlDateTime dt;
    TimeToStruct(time, dt);
    return dt.year;
}

//+------------------------------------------------------------------+
//| Returns absolute day number.                                     |
//+------------------------------------------------------------------+
int TimeAbsoluteDay(const datetime time)
{
    return ((int)time / 86400);
}

//+------------------------------------------------------------------+
//| Checks whether Median/VA rays are required and whether they      |
//| should be cut.                                                   |
//+------------------------------------------------------------------+
void CheckRays()
{
    for (int i = 0; i < SessionsNumber; i++)
    {
        string last_name = " " + TimeToString(RememberSessionStart[i], TIME_DATE);
        string suffix = RememberSessionSuffix[i];
        string rec_name = "";

        if (Session == Rectangle) rec_name = MPR_Array[i].name + "_";

        // If the median rays have to be created for the given trading session:
        if (((ShowMedianRays == AllPrevious) && (SessionsNumber - i >= 2)) ||
                (((ShowMedianRays == Previous) || (ShowMedianRays == PreviousCurrent)) && (SessionsNumber - i == 2)) ||
                (((ShowMedianRays == Current) || (ShowMedianRays == PreviousCurrent)) && (SessionsNumber - i == 1)) ||
                (ShowMedianRays == All))
        {
            double median_price = ObjectGetDouble(0, rec_name + "Median" + suffix + last_name, OBJPROP_PRICE, 0);
            datetime median_time = (datetime)ObjectGetInteger(0, rec_name + "Median" + suffix + last_name, OBJPROP_TIME, 1);

            // Delete old Median Ray.
            if (ObjectFind(0, rec_name + "Median Ray" + suffix + last_name) >= 0) ObjectDelete(0, rec_name + "Median Ray" + suffix + last_name);
            // Draw a new Median Ray.
            ObjectCreate(0, rec_name + "Median Ray" + suffix + last_name, OBJ_TREND, 0, RememberSessionStart[i], median_price, median_time, median_price);
            ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_COLOR, MedianColor);
            ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_STYLE, MedianRayStyle);
            ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_WIDTH, MedianRayWidth);
            ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_BACK, false);
            ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_SELECTABLE, false);
            if ((RightToLeft) && (i == SessionsNumber - 1) && (Session != Rectangle))
            {
                ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_RAY_LEFT, true);
                ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_RAY_RIGHT, false);
            }
            else
            {
                ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_RAY_RIGHT, true);
                ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_RAY_LEFT, false);
            }
            ObjectSetInteger(0, rec_name + "Median Ray" + suffix + last_name, OBJPROP_HIDDEN, true);
        }

        // We should also delete outdated rays that no longer should be there.
        if ((((ShowMedianRays == Previous) || (ShowMedianRays == PreviousCurrent)) && (SessionsNumber - i > 2)) ||
                ((ShowMedianRays == Current) && (SessionsNumber - i > 1)))
        {
            if (ObjectFind(0, rec_name + "Median Ray" + suffix + last_name) >= 0) ObjectDelete(0, rec_name + "Median Ray" + suffix + last_name);
        }

        // If the median rays have to be created for the given trading session:
        if (((ShowValueAreaRays == AllPrevious) && (SessionsNumber - i >= 2)) ||
                (((ShowValueAreaRays == Previous) || (ShowValueAreaRays == PreviousCurrent)) && (SessionsNumber - i == 2)) ||
                (((ShowValueAreaRays == Current) || (ShowValueAreaRays == PreviousCurrent)) && (SessionsNumber - i == 1)) ||
                (ShowValueAreaRays == All))
        {
            double va_high_price = ObjectGetDouble(0, rec_name + "VA_Top" + suffix + last_name, OBJPROP_PRICE, 0);
            double va_low_price = ObjectGetDouble(0, rec_name + "VA_Bottom" + suffix + last_name, OBJPROP_PRICE, 0);
            datetime va_time = (datetime)ObjectGetInteger(0, rec_name + "VA_Top" + suffix + last_name, OBJPROP_TIME, 1);

            // Delete old Value Area Rays.
            if (ObjectFind(0, rec_name + "Value Area HighRay" + suffix + last_name) >= 0) ObjectDelete(0, rec_name + "Value Area HighRay" + suffix + last_name);
            if (ObjectFind(0, rec_name + "Value Area LowRay" + suffix + last_name) >= 0) ObjectDelete(0, rec_name + "Value Area LowRay" + suffix + last_name);

            // Draw a new Value Area High Ray.
            ObjectCreate(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJ_TREND, 0, RememberSessionStart[i], va_high_price, va_time, va_high_price);
            ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_COLOR, ValueAreaHighLowColor);
            ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_STYLE, ValueAreaRayHighLowStyle);
            ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_WIDTH, ValueAreaRayHighLowWidth);
            ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_BACK, false);
            ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_SELECTABLE, false);
            if ((RightToLeft) && (i == SessionsNumber - 1) && (Session != Rectangle))
            {
                ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_RAY_RIGHT, false);
                ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_RAY_LEFT, true);
            }
            else
            {
                ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_RAY_RIGHT, true);
                ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_RAY_LEFT, false);
            }
            ObjectSetInteger(0, rec_name + "Value Area HighRay" + suffix + last_name, OBJPROP_HIDDEN, true);
            // Draw a new Value Area Low Ray.
            ObjectCreate(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJ_TREND, 0, RememberSessionStart[i], va_low_price, va_time, va_low_price);
            ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_COLOR, ValueAreaHighLowColor);
            ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_STYLE, ValueAreaRayHighLowStyle);
            ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_WIDTH, ValueAreaRayHighLowWidth);
            ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_BACK, false);
            ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_SELECTABLE, false);
            if ((RightToLeft) && (i == SessionsNumber - 1) && (Session != Rectangle))
            {
                ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_RAY_RIGHT, false);
                ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_RAY_LEFT, true);
            }
            else
            {
                ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_RAY_RIGHT, true);
                ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_RAY_LEFT, false);
            }
            ObjectSetInteger(0, rec_name + "Value Area LowRay" + suffix + last_name, OBJPROP_HIDDEN, true);
        }

        // We should also delete outdated rays that no longer should be there.
        if ((((ShowValueAreaRays == Previous) || (ShowValueAreaRays == PreviousCurrent)) && (SessionsNumber - i > 2)) ||
                ((ShowValueAreaRays == Current) && (SessionsNumber - i > 1)))
        {
            if (ObjectFind(0, rec_name + "Value Area HighRay" + suffix + last_name) >= 0) ObjectDelete(0, rec_name + "Value Area HighRay" + suffix + last_name);
            if (ObjectFind(0, rec_name + "Value Area LowRay" + suffix + last_name) >= 0) ObjectDelete(0, rec_name + "Value Area LowRay" + suffix + last_name);
        }

        if (RaysUntilIntersection == Stop_No_Rays) continue;

        if ((((ShowMedianRays == Previous) || (ShowMedianRays == PreviousCurrent)) && (SessionsNumber - i == 2)) || (((ShowMedianRays == AllPrevious) || (ShowMedianRays == All)) && (SessionsNumber - i >= 2)))
        {
            if ((RaysUntilIntersection == Stop_All_Rays)
                    || ((RaysUntilIntersection == Stop_All_Rays_Except_Prev_Session) && (SessionsNumber - i > 2))
                    || ((RaysUntilIntersection == Stop_Only_Previous_Session) && (SessionsNumber - i == 2)))
                CheckRayIntersections(rec_name + "Median Ray" + suffix + last_name, i + 1);
        }
        if ((((ShowValueAreaRays == Previous) || (ShowValueAreaRays == PreviousCurrent)) && (SessionsNumber - i == 2)) || (((ShowValueAreaRays == AllPrevious) || (ShowValueAreaRays == All)) && (SessionsNumber - i >= 2)))
        {
            if ((RaysUntilIntersection == Stop_All_Rays)
                    || ((RaysUntilIntersection == Stop_All_Rays_Except_Prev_Session) && (SessionsNumber - i > 2))
                    || ((RaysUntilIntersection == Stop_Only_Previous_Session) && (SessionsNumber - i == 2)))
            {
                CheckRayIntersections(rec_name + "Value Area HighRay" + suffix + last_name, i + 1);
                CheckRayIntersections(rec_name + "Value Area LowRay" + suffix + last_name, i + 1);
            }
        }
    }
}

//+------------------------------------------------------------------+
//| Checks price intersection and cuts a ray for a given object.     |
//+------------------------------------------------------------------+
void CheckRayIntersections(const string object, const int start_j)
{
    if (ObjectFind(0, object) < 0) return;

    double price = ObjectGetDouble(0, object, OBJPROP_PRICE, 0);
    for (int j = start_j; j < SessionsNumber; j++) // Find the nearest intersecting session.
    {
        if ((price <= RememberSessionMax[j]) && (price >= RememberSessionMin[j]))
        {
            ObjectSetInteger(0, object, OBJPROP_RAY_RIGHT, false);
            ObjectSetInteger(0, object, OBJPROP_TIME, 1, RememberSessionStart[j]);
            break;
        }
    }
}

//+------------------------------------------------------------------+
//| Print out VAH, VAL, or POC value on the chart.                   |
//+------------------------------------------------------------------+
void ValuePrintOut(const string obj_name, const datetime time, const double price, const ENUM_ANCHOR_POINT anchor = ANCHOR_RIGHT)
{
    // Find object if it exists.
    if (ObjectFind(0, obj_name) >= 0)
    {
        // Move it.
        ObjectMove(0, obj_name, 0, time, price);
    }
    else
    {
        // Draw a new one.
        ObjectCreate(0, obj_name, OBJ_TEXT, 0, time, price);
        ObjectSetInteger(0, obj_name, OBJPROP_COLOR, KeyValuesColor);
        ObjectSetInteger(0, obj_name, OBJPROP_FONTSIZE, KeyValuesSize);
        ObjectSetInteger(0, obj_name, OBJPROP_BACK, false);
        ObjectSetInteger(0, obj_name, OBJPROP_SELECTABLE, false);
        ObjectSetInteger(0, obj_name, OBJPROP_HIDDEN, true);
        ObjectSetInteger(0, obj_name, OBJPROP_ANCHOR, anchor);
    }
    // Should be updated anyway.
    ObjectSetString(0, obj_name, OBJPROP_TEXT, DoubleToString(price, _Digits));
}

//+------------------------------------------------------------------+
//| Calculates dot color based on bar direction and color scheme.    |
//| Used only when ColorBullBear == true.                            |
//+------------------------------------------------------------------+
color CalculateProperColor()
{
    color colour = 0;
    switch (CurrentColorScheme)
    {
    case Blue_to_Red:
        if (CurrentBarDirection == Bullish)
        {
            colour = clrBlue;
        }
        else if (CurrentBarDirection == Bearish)
        {
            colour = clrDarkRed;
        }
        else if (CurrentBarDirection == Neutral)
        {
            colour = clrPink;
        }
        break;
    case Red_to_Green:
        if (CurrentBarDirection == Bullish)
        {
            colour = clrDarkRed;
        }
        else if (CurrentBarDirection == Bearish)
        {
            colour = clrDarkGreen;
        }
        else if (CurrentBarDirection == Neutral)
        {
            colour = clrBrown;
        }
        break;
    case Green_to_Blue:
        if (CurrentBarDirection == Bullish)
        {
            colour = clrDarkGreen;
        }
        else if (CurrentBarDirection == Bearish)
        {
            colour = clrBlue;
        }
        else if (CurrentBarDirection == Neutral)
        {
            colour = clrDarkGray;
        }
        break;
    case Yellow_to_Cyan:
        if (CurrentBarDirection == Bullish)
        {
            colour = clrYellow;
        }
        else if (CurrentBarDirection == Bearish)
        {
            colour = clrCyan;
        }
        else if (CurrentBarDirection == Neutral)
        {
            colour = clrGreen;
        }
        break;
    case Magenta_to_Yellow:
        if (CurrentBarDirection == Bullish)
        {
            colour = clrMagenta;
        }
        else if (CurrentBarDirection == Bearish)
        {
            colour = clrYellow;
        }
        else if (CurrentBarDirection == Neutral)
        {
            colour = clrGreen;
        }
        break;
    case Cyan_to_Magenta:
        if (CurrentBarDirection == Bullish)
        {
            colour = clrCyan;
        }
        else if (CurrentBarDirection == Bearish)
        {
            colour = clrMagenta;
        }
        else if (CurrentBarDirection == Neutral)
        {
            colour = clrGreen;
        }
        break;
    case Single_Color:
        if (CurrentBarDirection == Bullish)
        {
            colour = SingleColor;
        }
        else if (CurrentBarDirection == Bearish)
        {
            colour = color(0x00FFFFFF - SingleColor);
        }
        else if (CurrentBarDirection == Neutral)
        {
            colour = color((int)MathMax(SingleColor, 0x00FFFFFF - SingleColor) / 2);
        }
        break;
    default:
        if (CurrentBarDirection == Bullish)
        {
            colour = SingleColor;
        }
        else if (CurrentBarDirection == Bearish)
        {
            colour = color(0x00FFFFFF - SingleColor);
        }
        else if (CurrentBarDirection == Neutral)
        {
            colour = color((int)MathMax(SingleColor, 0x00FFFFFF - SingleColor) / 2);
        }
        break;
    }
    return colour;
}

//
void OnTimer()
{
    double High[], Low[];
    datetime Time[];
    ArraySetAsSeries(High, true);
    ArraySetAsSeries(Low, true);
    ArraySetAsSeries(Time, true);

    int rates_total = iBars(Symbol(), Period());

    int h = CopyHigh(Symbol(), Period(), 0, rates_total, High);
    int l = CopyLow(Symbol(), Period(), 0, rates_total, Low);
    int t = CopyTime(Symbol(), Period(), 0, rates_total, Time);

    // Data not yet ready.
    if ((h <= 0) || (l <= 0) || (t <= 0)) return;

    if (GetTickCount() - LastRecalculationTime < 500) return; // Do not recalculate on timer if less than 500 ms passed.
    if (Session == Rectangle)
    {
        CheckRectangles(High, Low, Time, rates_total);
        return; // No need to call RedrawLastSession() even if RightToLeft is on because in that case all Rectangles are all right-to-left and are redrawn as needed.
    }
    if ((!RightToLeft) || (!FirstRunDone)) return; // Need to finish normal drawing before reacting to timer.

    static datetime prev_converted_time = 0;

    datetime converted_time = 0;
    int dummy_subwindow;
    double dummy_price;
    ChartXYToTimePrice(0, (int)ChartGetInteger(0, CHART_WIDTH_IN_PIXELS), 0, dummy_subwindow, converted_time, dummy_price);

    if (converted_time == prev_converted_time) return; // Do not call RedrawLastSession() if the screen hasn't been scrolled.
    converted_time = prev_converted_time;

    // Check right-most time - did it change?
    RedrawLastSession(High, Low, Time, rates_total);
    LastRecalculationTime = GetTickCount(); // Remember last calculation time.
    ChartRedraw();
}

// Find rectangles, create objects, process rectangle sessions, delete unneeded sessions (where rectangle no longer exists).
// Make sure rectangles are added to the array in a sorted manner from oldest T1 to newest T1.
void CheckRectangles(const double& High[], const double& Low[], const datetime& Time[], const int rates_total)
{
    // Check if any existing MPR objects need to be deleted or moved:
    for (int i = mpr_total - 1; i >= 0 ; i--)
    {
        if (ObjectFind(0, MPR_Array[i].name) < 0)
        {
            ObjectCleanup(MPR_Array[i].name + "_");
            // Buffer cleanup for the Developing POC.
            if (EnableDevelopingPOC)
            {
                int sessionstart = iBarShift(Symbol(), Period(), MPR_Array[i].RectangleTimeMin, true);
                int sessionend = iBarShift(Symbol(), Period(), MPR_Array[i].RectangleTimeMax, true);
                if (sessionend < 0) sessionend = 0; // If the rectangle's rightmost side is in the future, reset it to the current bar.

                // Re-initialize all bars using old rectangle borders:
                for (int j = sessionstart; j >= sessionend; j--)
                {
                    DevelopingPOC_1[j] = EMPTY_VALUE;
                    DevelopingPOC_2[j] = EMPTY_VALUE;
                }
            }
            delete MPR_Array[i];
            // Move all array elements with greater index down:
            for (int j = i; j < mpr_total - 1; j++)
                MPR_Array[j] = MPR_Array[j + 1];

            mpr_total--;
            ArrayResize(MPR_Array, mpr_total);
        }
    }

    // Find all objects of rectangle type with the name starting with MPR.
    int obj_total = ObjectsTotal(0, 0);
    for (int i = 0; i < obj_total; i++)
    {
        string name = ObjectName(0, i);
        if (ObjectGetInteger(0, name, OBJPROP_TYPE) != OBJ_RECTANGLE) continue;
        if (StringSubstr(name, 0, 3) != "MPR") continue;
        if (StringFind(name, "_") != -1) continue; // Skip chart objects created based on a rectangle session.
        // Find the rectangle among the array's elements by its name.
        bool name_found = false;
        for (int j = 0; j < mpr_total; j++)
        {
            if (MPR_Array[j].name == name)
            {
                name_found = true;

                // Check if it should be moved inside the array to keep sorting intact.
                datetime t1 = (datetime)ObjectGetInteger(0, name, OBJPROP_TIME, 0);
                datetime t2 = (datetime)ObjectGetInteger(0, name, OBJPROP_TIME, 1);

                MPR_Array[j].t1 = t1;
                MPR_Array[j].t2 = t2;

                if (mpr_total == 1) continue; // No need to sort the array if its size is 1.

                t1 = MathMin(t1, t2); // Leftmost side.

                if (t1 == MPR_Array[j].RectangleTimeMin) continue; // No movement needed.
                int k = 0;
                if (k == j) k++; // Skip self.
                while ((k < mpr_total) && (MPR_Array[k].RectangleTimeMin < t1))
                {
                    k++;
                    if (k == j) k++; // Skip self.
                }
                // Now k points either to the first newer rectangle or to beyond the end of the array.
                if (j == k - 1) continue; // Already there.
                CRectangleMP* tmp = MPR_Array[j]; // Moved rectangle -> to temp.
                if (t1 > MPR_Array[j].RectangleTimeMin)
                {
                    // Run a cycle to move others elements lower.
                    for (int n = j; n < k - 1; n++)
                        MPR_Array[n] = MPR_Array[n + 1];

                    MPR_Array[k - 1] = tmp; // Assign the moved rectangle to the final element of the array.
                }
                else
                {
                    // Run a cycle to move others elements higher.
                    for (int n = j; n > k; n--)
                        MPR_Array[n] = MPR_Array[n - 1];

                    MPR_Array[k] = tmp; // Assign the moved rectangle to the final element of the array.
                }
                break;
            }
        }
        // New rectangle:
        if (!name_found)
        {
            // Check if it should be moved inside the array to keep sorting intact.
            datetime t1 = (datetime)ObjectGetInteger(0, name, OBJPROP_TIME, 0);
            datetime t2 = (datetime)ObjectGetInteger(0, name, OBJPROP_TIME, 1);
            datetime t = MathMin(t1, t2); // Leftmost side.

            int k = 0;
            while ((k < mpr_total) && (MPR_Array[k].RectangleTimeMin < t)) k++;
            // Now k points either to the first newer rectangle or to beyond the end of the array.

            mpr_total++;
            ArrayResize(MPR_Array, mpr_total);

            // Run a cycle to move others elements higher.
            for (int n = mpr_total - 1; n > k; n--)
                MPR_Array[n] = MPR_Array[n - 1];

            MPR_Array[k] = new CRectangleMP(name); // Assign the new rectangle to the kth element of the array.
            MPR_Array[k].RectangleTimeMin = t; // Fill in the leftmost time to enable further sorting.
            MPR_Array[k].t1 = t1;
            MPR_Array[k].t2 = t2;
        }
    }

    if (SessionsNumber != mpr_total)
    {
        SessionsNumber = mpr_total;
        ArrayResize(RememberSessionMax, SessionsNumber);
        ArrayResize(RememberSessionMin, SessionsNumber);
        ArrayResize(RememberSessionStart, SessionsNumber);
        ArrayResize(RememberSessionSuffix, SessionsNumber);
    }

    // Process each rectangle.
    for (int i = 0; i < mpr_total; i++)
        MPR_Array[i].Process(i, High, Low, Time, rates_total);

    if ((ShowValueAreaRays != None) || (ShowMedianRays != None)) CheckRays();

    LastRecalculationTime = GetTickCount(); // Remember last calculation time.

    ChartRedraw();
}

CRectangleMP::CRectangleMP(string given_name = "MPR")
{
    name = given_name;
    RectanglePriceMax = -DBL_MAX;
    RectanglePriceMin = DBL_MAX;
    prev_RectanglePriceMax = -DBL_MAX;
    prev_RectanglePriceMin = DBL_MAX;
    RectangleTimeMax = 0;
    RectangleTimeMin = D'31.12.3000';
    prev_Time0 = 0;
    prev_High = -DBL_MAX;
    prev_Low = DBL_MAX;
    Number = -1;
    t1 = 0;
    t2 = 0;
}

// i - order number of the rectangle.
void CRectangleMP::Process(const int i, const double& High[], const double& Low[], const datetime& Time[], const int rates_total)
{
    double p1 = ObjectGetDouble(0, name, OBJPROP_PRICE, 0);
    double p2 = ObjectGetDouble(0, name, OBJPROP_PRICE, 1);

    if (Number == -1) Number = i;

    // Calculate rectangle session's actual time and price boundaries.
    int sessionstart = iBarShift(Symbol(), Period(), (int)MathMin(t1, t2), true);
    int sessionend = iBarShift(Symbol(), Period(), (int)MathMax(t1, t2), true);

    // Rectangle actually doesn't start in the future.
    if ((MathMin(t1, t2) <= Time[0]))
        // But iBarShift fails.
        if ((sessionend == -1) && (sessionstart == -1)) return; // Then the data isn't ready yet.

    bool rectangle_changed = false;
    bool rectangle_time_changed = false;
    bool rectangle_price_changed = false;

    // If any of the rectangle parameters changed.
    if ((RectangleTimeMax != MathMax(t1, t2)) || (RectangleTimeMin != MathMin(t1, t2)))
    {
        rectangle_changed = true;
        rectangle_time_changed = true;
    }
    if ((RectanglePriceMax != MathMax(p1, p2)) || (RectanglePriceMin != MathMin(p1, p2)))
    {
        rectangle_changed = true;
        rectangle_price_changed = true;
    }

    // Buffer cleanup for the Developing POC. Should be run only for a changed rectangle, which isn't brand new.
    if ((EnableDevelopingPOC) && (rectangle_changed) && (RectangleTimeMax != D'01.01.1970'))
    {
        int local_sessionstart = iBarShift(Symbol(), Period(), RectangleTimeMin, true);
        int local_sessionend = iBarShift(Symbol(), Period(), RectangleTimeMax, true);
        if (local_sessionend < 0) local_sessionend = 0; // If the rectangle's rightmost side is in the future, reset it to the current bar.

        // Re-initialize all bars using old rectangle borders:
        for (int j = local_sessionstart; j >= local_sessionend; j--)
        {
            DevelopingPOC_1[j] = EMPTY_VALUE;
            DevelopingPOC_2[j] = EMPTY_VALUE;
        }
    }

    RectangleTimeMax = MathMax(t1, t2);
    RectangleTimeMin = MathMin(t1, t2);
    RectanglePriceMax = MathMax(p1, p2);
    RectanglePriceMin = MathMin(p1, p2);

    bool new_bars_are_not_within_rectangle = true;
    bool current_bar_changed_within_boundaries = false;

    if (Time[0] != prev_Time0)
    {
        new_bars_are_not_within_rectangle = false;
        // Check if any of the new bars fall into rectangle's boundaries:
        if (((prev_Time0 < RectangleTimeMin) && (Time[0] < RectangleTimeMin)) || ((prev_Time0 > RectangleTimeMax) && (Time[0] > RectangleTimeMax))) new_bars_are_not_within_rectangle = true;

        // Now check if the price of any of the new bars is within the rectangle's boundaries:
        if (!new_bars_are_not_within_rectangle)
        {
            int max_index = iHighest(Symbol(), Period(), MODE_HIGH, iBarShift(Symbol(), Period(), prev_Time0, true), 0);
            int min_index = iLowest(Symbol(), Period(), MODE_LOW, iBarShift(Symbol(), Period(), prev_Time0, true), 0);

            if ((High[max_index] < RectanglePriceMin) || (Low[min_index] > RectanglePriceMax)) new_bars_are_not_within_rectangle = true;
        }

        prev_Time0 = Time[0];
    }
    else // No new bars - check if the current bar's high or low changed within the rectangle's boundaries:
    {
        if ((Time[0] >= RectangleTimeMin) && (Time[0] <= RectangleTimeMax)) // Bar within time boundaries.
        {
            if (prev_High != High[0])
            {
                if ((High[0] <= RectanglePriceMax) && (High[0] >= RectanglePriceMin)) current_bar_changed_within_boundaries = true;
            }
            if (prev_Low != Low[0])
            {
                if ((Low[0] <= RectanglePriceMax) && (Low[0] >= RectanglePriceMin)) current_bar_changed_within_boundaries = true;
            }
        }
    }

    prev_High = High[0];
    prev_Low = Low[0];

    if (sessionend < 0) sessionend = 0; // If the rectangles rightmost side is in the future, reset it to the current bar.

    bool need_to_clean_up_dots = false;
    bool rectangle_changed_and_recalc_is_due = false;
    // If rectangle changed:
    if (rectangle_changed)
    {
        if (rectangle_price_changed)
        {
            // Max/min bars of the price range within rectangle's boundaries before and after change:
            int max_index = iHighest(Symbol(), Period(), MODE_HIGH, sessionstart - sessionend, sessionend);
            int min_index = iLowest(Symbol(), Period(), MODE_LOW, sessionstart - sessionend, sessionend);
            if ((max_index != -1) && (min_index != -1))
            {
                if ((RectanglePriceMax > High[max_index]) && (RectanglePriceMin < Low[min_index]) && (prev_RectanglePriceMax > High[max_index]) && (prev_RectanglePriceMin < Low[min_index])) rectangle_changed_and_recalc_is_due = false;
                else
                {
                    need_to_clean_up_dots = true;
                    rectangle_changed_and_recalc_is_due = true;
                }
            }
        }
        if (rectangle_time_changed)
        {
            need_to_clean_up_dots = true;
            if (sessionstart >= 0) rectangle_changed_and_recalc_is_due = true;
        }
    }

    prev_RectanglePriceMax = RectanglePriceMax;
    prev_RectanglePriceMin = RectanglePriceMin;

    // Need to continue drawing profile in the following cases only:
    // 1. New bar came in and it is within the rectangle's borders.
    // 2. Current bar changed its High or Low and it is now within the borders.
    // 3. Rectangle changed its borders.
    // 4. Order of rectangles changed - need recalculation for stopping the rays (only when it is really needed).

    // Need to delete previous dots before going to drawing in the following cases:
    // 1. Rectangle changed its borders.
    // 2. When Max_number_of_bars_in_a_session changes.

    // Number of bars in the rectangle session changed, need to update colors, so a cleanup is due.
    if (sessionstart - sessionend + 1 != Max_number_of_bars_in_a_session)
    {
        Max_number_of_bars_in_a_session = sessionstart - sessionend + 1;
        if (!new_bars_are_not_within_rectangle) need_to_clean_up_dots = true;
    }

    if (need_to_clean_up_dots) ObjectCleanup(name + "_");
    if (sessionstart < 0) return; // Rectangle is drawn in the future.

    RememberSessionStart[i] = RectangleTimeMin;

    if ((!new_bars_are_not_within_rectangle) || (current_bar_changed_within_boundaries) || (rectangle_changed_and_recalc_is_due) || ((Number != i) && ((RaysUntilIntersection != Stop_No_Rays) && ((ShowMedianRays != None) || (ShowValueAreaRays != None))))) ProcessSession(sessionstart, sessionend, i, High, Low, Time, rates_total, &this);

    Number = i;
}

void PutSinglePrintMark(const double price, const int sessionstart, const string rectangle_prefix, const datetime& Time[])
{
    int t1 = sessionstart + 1, t2 = sessionstart;
    bool fill = true;
    if (ShowSinglePrint == Rightside)
    {
        t1 = sessionstart;
        t2 = sessionstart - 1;
        fill = false;
    }
    string LastNameStart = " " + TimeToString(Time[t1]) + " ";
    string LastName = LastNameStart + DoubleToString(price, _Digits);

    // If already there - ignore.
    if (ObjectFind(0, rectangle_prefix + "MPSP" + Suffix + LastName) >= 0) return;
    ObjectCreate(0, rectangle_prefix + "MPSP" + Suffix + LastName, OBJ_RECTANGLE, 0, Time[t1], price, Time[t2], price - onetick);
    ObjectSetInteger(0, rectangle_prefix + "MPSP" + Suffix + LastName, OBJPROP_COLOR, SinglePrintColor);

    // Fills rectangle.
    ObjectSetInteger(0, rectangle_prefix + "MPSP" + Suffix + LastName, OBJPROP_FILL, fill);
    ObjectSetInteger(0, rectangle_prefix + "MPSP" + Suffix + LastName, OBJPROP_SELECTABLE, false);
    ObjectSetInteger(0, rectangle_prefix + "MPSP" + Suffix + LastName, OBJPROP_HIDDEN, true);
}

void RemoveSinglePrintMark(const double price, const int sessionstart, const string rectangle_prefix, const datetime& Time[])
{
    int t = sessionstart + 1;
    if (ShowSinglePrint == Rightside) t = sessionstart;

    string LastNameStart = " " + TimeToString(Time[t]) + " ";
    string LastName = LastNameStart + DoubleToString(price, _Digits);

    if (ObjectFind(0, rectangle_prefix + "MPSP" + Suffix + LastName) >= 0) ObjectDelete(0, rectangle_prefix + "MPSP" + Suffix + LastName);
}

void PutSinglePrintRay(const double price, const int sessionstart, const string rectangle_prefix, const datetime& Time[])
{
    datetime t1 = Time[sessionstart], t2;
    if (sessionstart - 1 >= 0) t2 = Time[sessionstart - 1];
    else t2 = Time[sessionstart] + 1;

    if (ShowSinglePrint == Rightside)
    {
        t1 = Time[sessionstart];
        t2 = Time[sessionstart + 1];
    }

    string LastNameStart = " " + TimeToString(t1) + " ";
    string LastName = LastNameStart + DoubleToString(price, _Digits);

    // If already there - ignore.
    if (ObjectFind(0, rectangle_prefix + "MPSPR" + Suffix + LastName) >= 0) return;
    ObjectCreate(0, rectangle_prefix + "MPSPR" + Suffix + LastName, OBJ_TREND, 0, t1, price, t2, price);
    ObjectSetInteger(0, rectangle_prefix + "MPSPR" + Suffix + LastName, OBJPROP_COLOR, SinglePrintColor);
    ObjectSetInteger(0, rectangle_prefix + "MPSPR" + Suffix + LastName, OBJPROP_STYLE, SinglePrintRayStyle);
    ObjectSetInteger(0, rectangle_prefix + "MPSPR" + Suffix + LastName, OBJPROP_WIDTH, SinglePrintRayWidth);
    ObjectSetInteger(0, rectangle_prefix + "MPSPR" + Suffix + LastName, OBJPROP_RAY_RIGHT, true);
    ObjectSetInteger(0, rectangle_prefix + "MPSPR" + Suffix + LastName, OBJPROP_SELECTABLE, false);
    ObjectSetInteger(0, rectangle_prefix + "MPSPR" + Suffix + LastName, OBJPROP_HIDDEN, true);
}

void RemoveSinglePrintRay(const double price, const int sessionstart, const string rectangle_prefix, const datetime& Time[])
{
    datetime t = Time[sessionstart + 1];
    if (ShowSinglePrint == Rightside) t = Time[sessionstart];

    string LastNameStart = " " + TimeToString(t) + " ";
    string LastName = LastNameStart + DoubleToString(price, _Digits);

    if (ObjectFind(0, rectangle_prefix + "MPSPR" + Suffix + LastName) >= 0) ObjectDelete(0, rectangle_prefix + "MPSPR" + Suffix + LastName);
}

// Called only when RightToLeft is on to update the right-most session.
void RedrawLastSession(const double& High[], const double& Low[], const datetime& Time[], const int rates_total)
{
    if (StartFromCurrentSession) StartDate = Time[0];
    else StartDate = StartFromDate;

    // Get start and end bar numbers of the given session.
    int sessionend = FindSessionEndByDate(Time, StartDate, rates_total);

    int sessionstart = FindSessionStart(Time, sessionend, rates_total);
    if (sessionstart == -1)
    {
        Print("Something went wrong! Waiting for data to load.");
        return;
    }

    // Jump to the last one.
    int SessionToStart = _SessionsToCount - 1;

    if (Session == Intraday)
    {
        if (!ProcessIntradaySession(sessionstart, sessionend, SessionToStart, High, Low, Time, rates_total)) return;
    }
    else
    {
        if (Session == Daily) Max_number_of_bars_in_a_session = PeriodSeconds(PERIOD_D1) / PeriodSeconds();
        else if (Session == Weekly) Max_number_of_bars_in_a_session = 604800 / PeriodSeconds();
        else if (Session == Monthly) Max_number_of_bars_in_a_session = 2678400 / PeriodSeconds();
        if (SaturdaySunday == Append_Saturday_Sunday)
        {
            // The start is on Sunday - add remaining time.
            if (TimeDayOfWeek(Time[sessionstart]) == 0) Max_number_of_bars_in_a_session += (24 * 3600 - (TimeHour(Time[sessionstart]) * 3600 + TimeMinute(Time[sessionstart]) * 60)) / PeriodSeconds();
            // The end is on Saturday. +1 because even 0:00 bar deserves a bar.
            if (TimeDayOfWeek(Time[sessionend]) == 6) Max_number_of_bars_in_a_session += ((TimeHour(Time[sessionend]) * 3600 + TimeMinute(Time[sessionend]) * 60)) / PeriodSeconds() + 1;
        }
        if (!ProcessSession(sessionstart, sessionend, SessionToStart, High, Low, Time, rates_total)) return;
    }

    if ((ShowValueAreaRays != None) || (ShowMedianRays != None)) CheckRays();
}


//+------------------------------------------------------------------+
//| Go through all prices on all N session bars from 1st to kth bar, |
//| where k = 1..N.                                                  |
//+------------------------------------------------------------------+
void CalculateDevelopingPOC(const int sessionstart, const int sessionend, const double& High[], const double& Low[], CRectangleMP* rectangle = NULL)
{
    // Cycle through all possible end bars to calculate the Developing POC.
    for (int max_bar = sessionstart; max_bar >= sessionend; max_bar--)
    {
        if (((DevelopingPOC_1[max_bar] != EMPTY_VALUE) || (DevelopingPOC_2[max_bar] != EMPTY_VALUE)) && (max_bar > 1)) continue; // One of the buffers already filled and it isn't/wasn't the latest bar - skip.
    
        // Determine the local price minimum and maximum.
        double LocalMin =  Low[ iLowest(Symbol(), Period(), MODE_LOW,  sessionstart - max_bar + 1, max_bar)];
        double LocalMax = High[iHighest(Symbol(), Period(), MODE_HIGH, sessionstart - max_bar + 1, max_bar)];
        
        // For rectangles, further restrictions may apply.
        if (Session == Rectangle)
        {
            if (LocalMax > rectangle.RectanglePriceMax) LocalMax = NormalizeDouble(rectangle.RectanglePriceMax, DigitsM);
            if (LocalMin < rectangle.RectanglePriceMin) LocalMin = NormalizeDouble(rectangle.RectanglePriceMin, DigitsM);
        }
        
        double DistanceToCenter = DBL_MAX; // Reset the distance because each piece of the Developing POC should be using its own.
        int DevMaxRange = 0; // Maximum range for the Developing POC.
        double PriceOfMaxRange = EMPTY_VALUE;
        
        // Cycle by price inside the local boundaries:
        for (double price = LocalMax; price >= LocalMin; price -= onetick)
        {
            price = NormalizeDouble(price, DigitsM);
            int range = 0; // Distance from first bar to the current bar.
            // Going through all bars of the session until the current max_bar to see if the price was encountered here.
            for (int bar = sessionstart; bar >= max_bar; bar--)
            {
                // Price is encountered in the given bar.
                if ((price >= Low[bar]) && (price <= High[bar]))
                {
                    // Update maximum distance from session's start to the found bar for the Developing POC.
                    // Using the center-most POC if there are more than one.
                    if ((DevMaxRange < range) || ((DevMaxRange == range) && (MathAbs(price - (LocalMin + (LocalMax - LocalMin) / 2)) < DistanceToCenter))) //SessionMax and SessionMin should be replaced with current N bars' max High and min Low.
                    {
                        DevMaxRange = range;
                        PriceOfMaxRange = price;
                        DistanceToCenter = MathAbs(price - (LocalMin + (LocalMax - LocalMin) / 2));
                    }
                    range++;
                }
            }
        }
        // Both buffer are empty:
        if ((DevelopingPOC_1[max_bar + 1] == EMPTY_VALUE) && (DevelopingPOC_2[max_bar + 1] == EMPTY_VALUE))
        {
            DevelopingPOC_1[max_bar] = PriceOfMaxRange; // Starting with the first one.
            DevelopingPOC_2[max_bar] = EMPTY_VALUE; // The second is initialized to an empty value.
        }
        // Buffer #1 already had a value,
        else if (DevelopingPOC_1[max_bar + 1] != EMPTY_VALUE)
        {
            // and it is different from what we get now.
            if (DevelopingPOC_1[max_bar + 1] != PriceOfMaxRange)
            {
                DevelopingPOC_2[max_bar] = PriceOfMaxRange; // Use new buffer to get an interrupted shift of lines.
                DevelopingPOC_1[max_bar] = EMPTY_VALUE;
            }    
            else // and it is the same price:
            {
                DevelopingPOC_1[max_bar] = PriceOfMaxRange; // Use the same buffer.
                DevelopingPOC_2[max_bar] = EMPTY_VALUE;
            }
        }
        // Buffer #2 already had a value,
        else
        {
            // and it is different from what we get now.
            if (DevelopingPOC_2[max_bar + 1] != PriceOfMaxRange)
            {
                DevelopingPOC_1[max_bar] = PriceOfMaxRange; // Use new buffer to get an interrupted shift of lines.
                DevelopingPOC_2[max_bar] = EMPTY_VALUE;
            }
            else // and it is the same price:
            {
                DevelopingPOC_2[max_bar] = PriceOfMaxRange; // Use the same buffer.
                DevelopingPOC_1[max_bar] = EMPTY_VALUE;
            }
        }
    }
}

//+------------------------------------------------------------------+
//|For keystroke processing in Rectangle sessions.                   |
//+------------------------------------------------------------------+
void OnChartEvent(const int id, const long& lparam, const double& dparam, const string& sparam)
{
    if (Session != Rectangle) return;
    if (id == CHARTEVENT_KEYDOWN)
    {
        if (lparam == 82) // 'r' key pressed.
        {
            // Find the next untaken MPR rectangle name.
            for (int i = 0; i < 1000; i++) // No more than 1000 rectangles!
            {
                string name = "MPR" + IntegerToString(i);
                if (ObjectFind(0, name) >= 0) continue;
                // If name not found, create a new rectangle.
                // Position it at the chart's center with width and height equal to the half of those of the chart.
                int pixel_width = (int)ChartGetInteger(0, CHART_WIDTH_IN_PIXELS);
                int pixel_height = (int)ChartGetInteger(0, CHART_HEIGHT_IN_PIXELS);
                int half_width = pixel_width / 2;
                int half_height = pixel_height / 2;
                int x1 = half_width / 2;
                int x2 = int(half_width * 1.5);
                int y1 = half_height / 2;
                int y2 = int(half_height * 1.5);
                int dummy_subwindow; // Filler variable.
                datetime time1, time2;
                double price1, price2;
                ChartXYToTimePrice(0, x1, y1, dummy_subwindow, time1, price1); // Convert the first pair of coordinates into a time/price pair.
                ChartXYToTimePrice(0, x2, y2, dummy_subwindow, time2, price2); // Convert the second pair of coordinates into a time/price pair.
                ObjectCreate(0, name, OBJ_RECTANGLE, 0, time1, price1, time2, price2);
                ObjectSetInteger(0, name, OBJPROP_SELECTABLE, true);
                ObjectSetInteger(0, name, OBJPROP_HIDDEN, false);
                ObjectSetInteger(0, name, OBJPROP_SELECTED, true);
                ObjectSetInteger(0, name, OBJPROP_FILL, false);
                break;
            }
        }
    }
}
//+------------------------------------------------------------------+