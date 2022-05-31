// -------------------------------------------------------------------------------
//   
// Displays the Market Profile indicator for intraday, daily, weekly, or monthly trading sessions.
// Daily - should be attached to M5-M30 timeframes. M30 is recommended.
// Weekly - should be attached to M30-H4 timeframes. H1 is recommended.
// Weeks start on Sunday.
// Monthly - should be attached to H1-D1 timeframes. H4 is recommended.
// Intraday - should be attached to M1-M15 timeframes. M5 is recommended.
// Designed for major currency pairs, but should work also with exotic pairs, CFDs, or commodities.
//   
// Version 1.19
// Copyright 2010-2022, EarnForex.com
// https://www.earnforex.com/metatrader-indicators/MarketProfile/
// -------------------------------------------------------------------------------
using cAlgo.API;
using cAlgo.API.Internals;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class MarketProfile : Indicator
    {
        #region Parameters

        [Parameter("=== Main", DefaultValue = "=================")]
        public string MainSettings { get; set; }

        [Parameter("Session", DefaultValue = session_period.Daily)]
        public session_period Session { get; set; }

        [Parameter("StartFromDate: lower priority.", DefaultValue = "")]
        public string StartFromDate { get; set; }

        [Parameter("StartFromCurrentSession: higher priority.", DefaultValue = true)]
        public bool StartFromCurrentSession { get; set; }

        [Parameter("SessionsToCount: Number of sessions to count Market Profile.", DefaultValue = 2)]
        public int SessionsToCount { get; set; }

        [Parameter("SeamlessScrollingMode: show sessions on current screen.", DefaultValue = false)]
        public bool SeamlessScrollingMode { get; set; }

        [Parameter("Enable Developing POC.", DefaultValue = false)]
        public bool EnableDevelopingPOC { get; set; }

        [Parameter("ValueAreaPercentage: Percentage of TPO's inside Value Area.", DefaultValue = 70)]
        public int ValueAreaPercentage { get; set; }


        [Parameter("=== Colors and looks", DefaultValue = "=================")]
        public string ColorsAndLooksSettings { get; set; }

        [Parameter("ColorScheme", DefaultValue = color_scheme.Blue_to_Red)]
        public color_scheme ColorScheme { get; set; }

        [Parameter("Opacity", DefaultValue = 60, MaxValue = 100, MinValue = 0)]
        public int ColorOpacity { get; set; }

        [Parameter("SingleColor: if ColorScheme is set to Single_Color.", DefaultValue = "Blue")]
        public string SingleColor { get; set; }

        [Parameter("ColorBullBear: If true, colors are from bars' direction.", DefaultValue = false)]
        public bool ColorBullBear { get; set; }

        [Parameter("MedianColor", DefaultValue = "White")]
        public string MedianColor { get; set; }

        [Parameter("ValueAreaSidesColor", DefaultValue = "White")]
        public string ValueAreaSidesColor { get; set; }

        [Parameter("ValueAreaHighLowColor", DefaultValue = "White")]
        public string ValueAreaHighLowColor { get; set; }

        [Parameter("MedianStyle", DefaultValue = LineStyle.Solid)]
        public LineStyle MedianStyle { get; set; }

        [Parameter("MedianRayStyle", DefaultValue = LineStyle.Lines)]
        public LineStyle MedianRayStyle { get; set; }

        [Parameter("ValueAreaSidesStyle", DefaultValue = LineStyle.Solid)]
        public LineStyle ValueAreaSidesStyle { get; set; }

        [Parameter("ValueAreaHighLowStyle", DefaultValue = LineStyle.Solid)]
        public LineStyle ValueAreaHighLowStyle { get; set; }

        [Parameter("ValueAreaRayHighLowStyle", DefaultValue = LineStyle.Dots)]
        public LineStyle ValueAreaRayHighLowStyle { get; set; }

        [Parameter("MedianWidth", DefaultValue = 1)]
        public int MedianWidth { get; set; }

        [Parameter("MedianRayWidth", DefaultValue = 1)]
        public int MedianRayWidth { get; set; }

        [Parameter("ValueAreaSidesWidth", DefaultValue = 1)]
        public int ValueAreaSidesWidth { get; set; }

        [Parameter("ValueAreaHighLowWidth", DefaultValue = 1)]
        public int ValueAreaHighLowWidth { get; set; }

        [Parameter("ValueAreaRayHighLowWidth", DefaultValue = 1)]
        public int ValueAreaRayHighLowWidth { get; set; }

        [Parameter("ShowValueAreaRays: draw previous value area high/low rays.", DefaultValue = sessions_to_draw_rays.None)]
        public sessions_to_draw_rays ShowValueAreaRays { get; set; }

        [Parameter("ShowMedianRays: draw previous median rays.", DefaultValue = sessions_to_draw_rays.None)]
        public sessions_to_draw_rays ShowMedianRays { get; set; }

        [Parameter("RaysUntilIntersection: which rays stop when hit another MP.", DefaultValue = ways_to_stop_rays.Stop_No_Rays)]
        public ways_to_stop_rays RaysUntilIntersection { get; set; }

        [Parameter("HideRaysFromInvisibleSessions: hide rays from behind the screen.", DefaultValue = false)]
        public bool HideRaysFromInvisibleSessions { get; set; }

        [Parameter("TimeShiftMinutes: shift session + to the left, - to the right.", DefaultValue = 0)]
        public int TimeShiftMinutes { get; set; }

        [Parameter("ShowKeyValues: print out VAH, VAL, POC on chart.", DefaultValue = true)]
        public bool ShowKeyValues { get; set; }

        [Parameter("KeyValuesColor: color for VAH, VAL, POC printout.", DefaultValue = "White")]
        public string KeyValuesColor { get; set; }

        [Parameter("KeyValuesSize: font size for VAH, VAL, POC printout.", DefaultValue = 12)]
        public int KeyValuesSize { get; set; }

        [Parameter("ShowSinglePrint: mark Single Print profile levels.", DefaultValue = single_print_type.No)]
        public single_print_type ShowSinglePrint { get; set; }

        [Parameter("SinglePrintColor", DefaultValue = "Gold")]
        public string SinglePrintColor { get; set; }

        [Parameter("SinglePrintRays: mark Single Print edges with rays.", DefaultValue = false)]
        public bool SinglePrintRays { get; set; }

        [Parameter("SinglePrintRayStyle", DefaultValue = LineStyle.Solid)]
        public LineStyle SinglePrintRayStyle { get; set; }

        [Parameter("SinglePrintRayWidth", DefaultValue = 1)]
        public int SinglePrintRayWidth { get; set; }

        [Parameter("ProminentMedianColor", DefaultValue = "Yellow")]
        public string ProminentMedianColor { get; set; }

        [Parameter("ProminentMedianStyle", DefaultValue = LineStyle.Solid)]
        public LineStyle ProminentMedianStyle { get; set; }

        [Parameter("ProminentMedianWidth", DefaultValue = 4)]
        public int ProminentMedianWidth { get; set; }

        [Parameter("RightToLeft: Draw histogram from right to left.", DefaultValue = false)]
        public bool RightToLeft { get; set; }


        [Parameter("=== Performance", DefaultValue = "=================")]
        public string PerformanceSettings { get; set; }

        [Parameter("PointMultiplier: higher value = fewer objects. 0 - adaptive.", DefaultValue = 0)]
        public int PointMultiplier { get; set; }

        [Parameter("ThrottleRedraw: delay (in seconds) for updating Market Profile.", DefaultValue = 0)]
        public int ThrottleRedraw { get; set; }

        [Parameter("DisableHistogram: do not draw profile, VAH, VAL, and POC still visible.", DefaultValue = false)]
        public bool DisableHistogram { get; set; }


        [Parameter("=== Alerts", DefaultValue = "=================")]
        public string AlertsSettings { get; set; }

        [Parameter("AlertNative: issue native pop-up alerts.", DefaultValue = false)]
        public bool AlertNative { get; set; }

        [Parameter("AlertNative: sound file.", DefaultValue = "")]
        public string AlertNativeSoundFile { get; set; }

        [Parameter("AlertEmail: issue email alerts.", DefaultValue = false)]
        public bool AlertEmail { get; set; }

        [Parameter("AlertEmail: Email From.", DefaultValue = "")]
        public string AlertEmailFrom { get; set; }

        [Parameter("AlertEmail: Email To.", DefaultValue = "")]
        public string AlertEmailTo { get; set; }

        [Parameter("AlertArrows: draw chart arrows on alerts.", DefaultValue = false)]
        public bool AlertArrows { get; set; }

        [Parameter("AlertCheckBar: which bar to check for alerts?", DefaultValue = alert_check_bar.CheckCurrentBar)]
        public alert_check_bar AlertCheckBar { get; set; }

        [Parameter("AlertForValueArea: alerts for Value Area (VAH, VAL) rays.", DefaultValue = false)]
        public bool AlertForValueArea { get; set; }

        [Parameter("AlertForMedian: alerts for POC (Median) rays' crossing.", DefaultValue = false)]
        public bool AlertForMedian { get; set; }

        [Parameter("AlertForSinglePrint: alerts for single print rays' crossing.", DefaultValue = false)]
        public bool AlertForSinglePrint { get; set; }

        [Parameter("AlertOnPriceBreak: price breaking above/below the ray.", DefaultValue = false)]
        public bool AlertOnPriceBreak { get; set; }

        [Parameter("AlertOnCandleClose: candle closing above/below the ray.", DefaultValue = false)]
        public bool AlertOnCandleClose { get; set; }

        [Parameter("AlertOnGapCross: bar gap above/below the ray.", DefaultValue = false)]
        public bool AlertOnGapCross { get; set; }


        [Parameter("=== Intraday settings", DefaultValue = "=================")]
        public string IntradaySettings { get; set; }

        [Parameter("EnableIntradaySession1", DefaultValue = true)]
        public bool EnableIntradaySession1 { get; set; }

        [Parameter("IntradaySession1StartTime", DefaultValue = "00:00")]
        public string IntradaySession1StartTime { get; set; }

        [Parameter("IntradaySession1EndTime", DefaultValue = "06:00")]
        public string IntradaySession1EndTime { get; set; }

        [Parameter("IntradaySession1ColorScheme", DefaultValue = color_scheme.Blue_to_Red)]
        public color_scheme IntradaySession1ColorScheme { get; set; }


        [Parameter("EnableIntradaySession2", DefaultValue = true)]
        public bool EnableIntradaySession2 { get; set; }

        [Parameter("IntradaySession2StartTime", DefaultValue = "06:00")]
        public string IntradaySession2StartTime { get; set; }

        [Parameter("IntradaySession2EndTime", DefaultValue = "12:00")]
        public string IntradaySession2EndTime { get; set; }

        [Parameter("IntradaySession2ColorScheme", DefaultValue = color_scheme.Red_to_Green)]
        public color_scheme IntradaySession2ColorScheme { get; set; }


        [Parameter("EnableIntradaySession3", DefaultValue = true)]
        public bool EnableIntradaySession3 { get; set; }

        [Parameter("IntradaySession3StartTime", DefaultValue = "12:00")]
        public string IntradaySession3StartTime { get; set; }

        [Parameter("IntradaySession3EndTime", DefaultValue = "18:00")]
        public string IntradaySession3EndTime { get; set; }

        [Parameter("IntradaySession3ColorScheme", DefaultValue = color_scheme.Green_to_Blue)]
        public color_scheme IntradaySession3ColorScheme { get; set; }


        [Parameter("EnableIntradaySession4", DefaultValue = true)]
        public bool EnableIntradaySession4 { get; set; }

        [Parameter("IntradaySession4StartTime", DefaultValue = "18:00")]
        public string IntradaySession4StartTime { get; set; }

        [Parameter("IntradaySession4EndTime", DefaultValue = "00:00")]
        public string IntradaySession4EndTime { get; set; }

        [Parameter("IntradaySession4ColorScheme", DefaultValue = color_scheme.Yellow_to_Cyan)]
        public color_scheme IntradaySession4ColorScheme { get; set; }


        [Parameter("=== Miscellaneous", DefaultValue = "=================")]
        public string MiscellaneousSettings { get; set; }

        [Parameter("SaturdaySunday", DefaultValue = sat_sun_solution.Saturday_Sunday_Normal_Days)]
        public sat_sun_solution SaturdaySunday { get; set; }

        [Parameter("Disable alerts on wrong timeframes.", DefaultValue = false)]
        public bool DisableAlertsOnWrongTimeframes { get; set; }

        [Parameter("Percentage of Median TPOs out of total for a Prominent one.", DefaultValue = 101)]
        public int ProminentMedianPercentage { get; set; }

        #endregion

        #region Outputs

        [Output("Developing POC 1", LineColor = "Green", LineStyle = LineStyle.Solid, PlotType = PlotType.DiscontinuousLine, Thickness = 5)]
        public IndicatorDataSeries DevelopingPOC_1 { get; set; }

        [Output("Developing POC 2", LineColor = "Green", LineStyle = LineStyle.Solid, PlotType = PlotType.DiscontinuousLine, Thickness = 5)]
        public IndicatorDataSeries DevelopingPOC_2 { get; set; }

        [Output("Price break", LineColor = "Red", PlotType = PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries ArrowsPB { get; set; }

        [Output("Candle close crossover", LineColor = "Blue", PlotType = PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries ArrowsCC { get; set; }

        [Output("Gap crossover", LineColor = "Yellow", PlotType = PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries ArrowsGC { get; set; }

        #endregion

        #region Enums

        public enum color_scheme
        {
            Blue_to_Red,
            // Blue to Red
            Red_to_Green,
            // Red to Green
            Green_to_Blue,
            // Green to Blue
            Yellow_to_Cyan,
            // Yellow to Cyan
            Magenta_to_Yellow,
            // Magenta to Yellow
            Cyan_to_Magenta,
            // Cyan to Magenta
            Single_Color
            // Single Color
        }

        public enum session_period
        {
            Daily,
            Weekly,
            Monthly,
            Intraday,
            Rectangle
        }

        public enum sat_sun_solution
        {
            Saturday_Sunday_Normal_Days,
            // Normal sessions
            Ignore_Saturday_Sunday,
            // Ignore Saturday and Sunday
            Append_Saturday_Sunday
            // Append Saturday and Sunday
        }

        public enum sessions_to_draw_rays
        {
            None,
            Previous,
            Current,
            PreviousCurrent,
            // Previous & Current
            AllPrevious,
            // All Previous
            All
        }

        public enum ways_to_stop_rays
        {
            Stop_No_Rays,
            // Stop no rays
            Stop_All_Rays,
            // Stop all rays
            Stop_All_Rays_Except_Prev_Session,
            // Stop all rays except previous session
            Stop_Only_Previous_Session
            // Stop only previous session's rays
        }

        // Only for dot coloring choice in PutDot() when ColorBullBear == true.
        enum bar_direction
        {
            Bullish,
            Bearish,
            Neutral
        }

        public enum single_print_type
        {
            No,
            Leftside,
            Rightside
        }

        public enum alert_check_bar
        {
            CheckCurrentBar,
            // Current
            CheckPreviousBar
            // Previous
        }

        enum alert_types
        {
            // Required to type a parameter of DoAlerts().
            PriceBreak,
            // Price Break
            CandleCloseCrossover,
            // Candle Close Crossover
            GapCrossover
            // Gap Crossover
        }

        #endregion

        #region Classes

        private class SessionInfo
        {
            public double Max;
            public double Min;
            public DateTime Start;
            public string Suffix;

            public SessionInfo() { }
        }

        private class CRectangleMP
        {
            public DateTime prev_Time0;
            public double prev_High;
            public double prev_Low;
            public double prev_RectanglePriceMax;
            public double prev_RectanglePriceMin;
            public int Number; // Order number of the rectangle;

            public double RectanglePriceMax;
            public double RectanglePriceMin;
            public DateTime RectangleTimeMax;
            public DateTime RectangleTimeMin;
            public DateTime t1;
            public DateTime t2; // To avoid reading object properties in Process() after sorting was done.
            public string name;

            public CRectangleMP(string given_name)
            {
                name = given_name;
                RectanglePriceMax = double.MinValue;
                RectanglePriceMin = double.MaxValue;
                prev_RectanglePriceMax = double.MinValue;
                prev_RectanglePriceMin = double.MaxValue;
                RectangleTimeMax = DateTime.MinValue;
                RectangleTimeMin = DateTime.MaxValue;
                prev_Time0 = DateTime.MinValue;
                prev_High = double.MinValue;
                prev_Low = double.MaxValue;
                Number = -1;
            }
        }

        private class Intraday
        {
            public int StartHours;
            public int StartMinutes;
            public int StartTime;
            public int EndHours;
            public int EndMinutes;
            public int EndTime;
            public color_scheme ColorScheme;

            public Intraday()
            {
                StartHours = 0;
                StartMinutes = 0;
                StartTime = 0;
                EndHours = 0;
                EndMinutes = 0;
                EndTime = 0;

                ColorScheme = color_scheme.Single_Color;
            }
        }

        #endregion

        #region Variables

        private int PointMultiplier_calculated;             // Will have to be calculated based number digits in a quote if PointMultiplier input is 0.
        private int DigitsM;                                // Number of digits normalized based on PointMultiplier_calculated.
        private bool InitFailed;                            // Used for soft INIT_FAILED. Hard INIT_FAILED resets input parameters.
        private DateTime StartDate;                         // Will hold either StartFromDate or Time[0].
        private double onetick;                             // One normalized pip.
        private bool FirstRunDone = false;                  // If true - OnCalculate() was already executed once.
        private string Suffix = "_";                        // Will store object name suffix depending on timeframe.
        private color_scheme CurrentColorScheme;            // Required due to intraday sessions.
        private int Max_number_of_bars_in_a_session = 1;
        private DateTime _Timer = DateTime.MinValue;        // For throttling updates of market profiles in slow systems.
        private bool NeedToRestartDrawing = false;          // Global flag for RightToLeft redrawing;
        private double ValueAreaPercentage_double = 0.7;    // Will be calculated based on the input parameter in OnInit().
        private DateTime LastAlertTime_CandleCross = DateTime.MinValue;
        private DateTime LastAlertTime_GapCross = DateTime.MinValue; // For CheckCurrentBar alerts.
        private DateTime LastAlertTime = DateTime.MinValue; // For CheckPreviousBar alerts;
        private double Close_prev = double.NaN;             // Previous price value for Price Break alerts.

        // Used for ColorBullBear.
        private bar_direction CurrentBarDirection = bar_direction.Neutral;
        private bar_direction PreviousBarDirection = bar_direction.Neutral;
        private bool NeedToReviewColors = false;

        // For intraday sessions' start and end times.
        private Intraday[] ID;
        private int IntradaySessionCount = 0;
        private int _SessionsToCount;
        private int IntradayCrossSessionDefined = -1; // For special case used only with Ignore_Saturday_Sunday on Monday.

        // We need to know where each session starts and its price range for when RaysUntilIntersection != Stop_No_Rays.
        // These are used also when RaysUntilIntersection == Stop_No_Rays for Intraday sessions counting.
        private int SessionsNumber = 0;     // Different from _SessionsToCount when working with Intraday sessions and for RaysUntilIntersection != Stop_No_Rays.

        private List<SessionInfo> RememberSession;
        private List<CRectangleMP> MPR_Array;

        private DateTime LastRecalculationTime = DateTime.MinValue;
        private DateTime LastBarTime = DateTime.MinValue;

        private DateTime prev_time_start_bar;
        private double PreviousSessionMax;
        private DateTime PreviousSessionStartTime;
        private DateTime prev_converted_time;

        #endregion

        #region Initialize

        protected override void Initialize()
        {
            InitFailed = false;

            // Sessions to count for the object creation.
            _SessionsToCount = SessionsToCount;

            // Check for user Session settings.
            if (Session == session_period.Daily)
            {
                Suffix = "_D";
                if (TimeFrame < TimeFrame.Minute5 || TimeFrame > TimeFrame.Minute30)
                {
                    string alert_text = "Timeframe should be between M5 and M30 for a Daily session.";
                    if (!DisableAlertsOnWrongTimeframes)
                        Alert(alert_text);
                    else
                        Print("Initialization failed: " + alert_text);
                    InitFailed = true; // Soft INIT_FAILED.
                }
            }
            else if (Session == session_period.Weekly)
            {
                Suffix = "_W";
                if (TimeFrame < TimeFrame.Minute30 || TimeFrame > TimeFrame.Hour4)
                {
                    string alert_text = "Timeframe should be between M30 and H4 for a Weekly session.";
                    if (!DisableAlertsOnWrongTimeframes)
                        Alert(alert_text);
                    else
                        Print("Initialization failed: " + alert_text);
                    InitFailed = true; // Soft INIT_FAILED.
                }
            }
            else if (Session == session_period.Monthly)
            {
                Suffix = "_M";
                if (TimeFrame < TimeFrame.Hour || TimeFrame > TimeFrame.Daily)
                {
                    string alert_text = "Timeframe should be between H1 and D1 for a Monthly session.";
                    if (!DisableAlertsOnWrongTimeframes)
                        Alert(alert_text);
                    else
                        Print("Initialization failed: " + alert_text);
                    InitFailed = true; // Soft INIT_FAILED.
                }
            }
            else if (Session == session_period.Intraday)
            {
                if (TimeFrame > TimeFrame.Minute15)
                {
                    string alert_text = "Timeframe should not be higher than M15 for an Intraday sessions.";
                    if (!DisableAlertsOnWrongTimeframes)
                        Alert(alert_text);
                    else
                        Print("Initialization failed: " + alert_text);
                    InitFailed = true; // Soft INIT_FAILED.
                }

                ID = new Intraday[4];

                // Check if Intraday User Settings are valid.
                IntradaySessionCount = 0;
                if (!CheckIntradaySession(EnableIntradaySession1, IntradaySession1StartTime, IntradaySession1EndTime, IntradaySession1ColorScheme))
                    InitFailed = true;
                if (!CheckIntradaySession(EnableIntradaySession2, IntradaySession2StartTime, IntradaySession2EndTime, IntradaySession2ColorScheme))
                    InitFailed = true;
                if (!CheckIntradaySession(EnableIntradaySession3, IntradaySession3StartTime, IntradaySession3EndTime, IntradaySession3ColorScheme))
                    InitFailed = true;
                if (!CheckIntradaySession(EnableIntradaySession4, IntradaySession4StartTime, IntradaySession4EndTime, IntradaySession4ColorScheme))
                    InitFailed = true;

                // Warn user about Intraday mode
                if (IntradaySessionCount == 0)
                {
                    string alert_text = "Enable at least one intraday session if you want to use Intraday mode.";
                    if (!DisableAlertsOnWrongTimeframes)
                        Alert(alert_text);
                    else
                        Print("Initialization failed: " + alert_text);
                    InitFailed = true; // Soft INIT_FAILED.
                }
            }
            else if ((Session == session_period.Rectangle) && SeamlessScrollingMode) // No point in seamless scrolling mode with rectangle sessions.
            {
                string alert_text = "Seamless scrolling mode doesn't work with Rectangle sessions.";
                if (!DisableAlertsOnWrongTimeframes)
                    Alert(alert_text);
                else
                    Print("Initialization failed: " + alert_text);
                InitFailed = true; // Soft INIT_FAILED.
            }

            // Adaptive point multiplier. Calculate based on number of digits in quote (before plus after the dot).
            if (PointMultiplier == 0)
            {
                double quote = Ask;
                string s = quote.ToString("F" + Symbol.Digits.ToString());
                int total_digits = s.Length;

                // If there is a dot in a quote.
                if (s.Contains(",") || s.Contains("."))
                    total_digits--; // Decrease the count of digits by one.

                if (total_digits <= 5)
                    PointMultiplier_calculated = 1;
                else
                    PointMultiplier_calculated = (int)Math.Pow(10, total_digits - 5);
            }
            else // Normal point multiplier.
            {
                PointMultiplier_calculated = PointMultiplier;
            }

            // Based on number of digits in PointMultiplier_calculated. -1 because if PointMultiplier_calculated < 10, it does not modify the number of digits.
            DigitsM = Math.Max(0, Symbol.Digits - (PointMultiplier_calculated.ToString().Length - 1));

            onetick = Math.Round(Symbol.TickSize * PointMultiplier_calculated, DigitsM);

            // Adjust for TickSize granularity if needed.
            double TickSize = Symbol.TickSize;
            if (onetick < TickSize)
            {
                DigitsM = Symbol.Digits - (((int)Math.Round(TickSize / Symbol.TickSize)).ToString().Length - 1);
                onetick = Math.Round(TickSize, DigitsM);
            }

            // Get color scheme from user input.
            CurrentColorScheme = ColorScheme;

            // To clean up potential leftovers when applying a chart template.
            ObjectCleanup();

            // Check if user wants Session mode as Rectangle or if it is a right-to-left session, or if rays should be constantly monitored, or seamless scrolling is on.
            if (Session == session_period.Rectangle || RightToLeft || HideRaysFromInvisibleSessions || SeamlessScrollingMode)
            {
                Timer.Start(new TimeSpan(0, 0, 0, 0, 500));
            }

            ValueAreaPercentage_double = ValueAreaPercentage * 0.01;

            MPR_Array = new List<CRectangleMP>();
            RememberSession = new List<SessionInfo>();

            PreviousSessionMax = double.MinValue;
            PreviousSessionStartTime = DateTime.MinValue;

            prev_time_start_bar = DateTime.MinValue;

            Chart.KeyDown += Chart_KeyDown;
        }

        private void Chart_KeyDown(ChartKeyboardEventArgs a)
        {
            if (Session != session_period.Rectangle)
                return;

            if (a.Key != Key.R)
                return;

            // Find the next untaken MPR rectangle name.
            for (int i = 0; i < 1000; i++) // No more than 1000 rectangles!
            {
                string name = "MPR" + i.ToString();
                if (Chart.FindObject(name) != null)
                    continue;

                int x1 = Chart.FirstVisibleBarIndex + (int)((Chart.LastVisibleBarIndex - Chart.FirstVisibleBarIndex) / 5);
                int x2 = Chart.LastVisibleBarIndex - (int)((Chart.LastVisibleBarIndex - Chart.FirstVisibleBarIndex) / 5);

                double max_price = Bars[GetHighestHighIdx(x1, x2)].High;
                double min_price = Bars[GetLowestLowIdx(x1, x2)].Low;

                double y1 = max_price;
                double y2 = min_price;

                ChartRectangle cr = Chart.DrawRectangle(name, x1, y1, x2, y2, Color.Blue);
                cr.IsInteractive = true;

                MPR_Array.Add(new CRectangleMP(name));
                break;
            }
        }

        #endregion

        #region OnDestroy

        protected override void OnDestroy()
        {
            Timer.Stop();

            if (Session == session_period.Rectangle)
            {
                for (int i = 0; i < MPR_Array.Count; i++)
                {
                    ObjectCleanup(MPR_Array[i].name + "_");
                    MPR_Array.Clear();
                }
            }
            else
                ObjectCleanup();
        }

        #endregion

        #region Calculate

        public override void Calculate(int index)
        {
            if (InitFailed)
            {
                if (!DisableAlertsOnWrongTimeframes)
                    Print("Initialization failed. Please see the alert message for details.");
                return;
            }

            if (!IsLastBar)
                return;

            CheckAlerts(index);

            // Check if seamless scrolling mode should be on, else if user requests current session, else a specific date.
            if (SeamlessScrollingMode)
            {
                int last_visible_bar = Chart.LastVisibleBarIndex;
                if (last_visible_bar >= Bars.Count)
                    last_visible_bar = Bars.Count - 1;

                StartDate = Bars[last_visible_bar].OpenTime;
            }
            else if (StartFromCurrentSession)
                StartDate = Bars[index].OpenTime;
            else
            {
                if (!DateTime.TryParse(StartFromDate, out StartDate))
                    StartDate = Bars[index].OpenTime;
            }

            // Adjust date if Ignore_Saturday_Sunday is set.
            if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday)
            {
                // Saturday? Switch to Friday.
                if (StartDate.DayOfWeek == DayOfWeek.Saturday)
                    StartDate.AddDays(-1);
                // Sunday? Switch to Friday too.
                else if (StartDate.DayOfWeek == DayOfWeek.Sunday)
                    StartDate.AddDays(-2);
            }

            // If we calculate profiles for the past sessions, no need to run it again.
            if (FirstRunDone && StartDate != Bars[index].OpenTime)
                return;

            // Delay the update of Market Profile if ThrottleRedraw is given.
            if (ThrottleRedraw > 0 && _Timer > DateTime.MinValue)
            {
                if (DateTime.Now.Subtract(_Timer).TotalSeconds < ThrottleRedraw)
                    return;
            }

            // Calculate rectangle
            if (Session == session_period.Rectangle) // Everything becomes very simple if rectangle sessions are used.
            {
                CheckRectangles();
                _Timer = DateTime.Now;
                return;
            }

            bool new_bar = false;
            if (LastBarTime != Bars.LastBar.OpenTime)
            {
                LastBarTime = Bars.LastBar.OpenTime;
                new_bar = true;
            }

            // Recalculate everything if there were missing bars or something like that. Or if RightToLeft is on and a new right-most session arrived.
            if (new_bar || NeedToRestartDrawing)
            {
                FirstRunDone = false;
                ObjectCleanup();
                NeedToRestartDrawing = false;
            }

            // Get start and end bar numbers of the given session.
            int sessionend = FindSessionEndByDate(StartDate);
            int sessionstart = FindSessionStart(sessionend);

            if (sessionstart == -1)
            {
                Print("Something went wrong! Waiting for data to load.");
                return;
            }

            int SessionToStart = 0;
            // If all sessions have already been counted, jump to the current one.
            if (FirstRunDone)
                SessionToStart = _SessionsToCount - 1;
            else
            {
                // Move back to the oldest session to count to start from it.
                for (int i = 1; i < _SessionsToCount; i++)
                {
                    sessionend = sessionstart - 1;

                    if (sessionend < 0)
                        return;

                    if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday)
                    {
                        // Pass through Sunday and Saturday.
                        while (Bars[sessionend].OpenTime.DayOfWeek == DayOfWeek.Sunday || Bars[sessionend].OpenTime.DayOfWeek == DayOfWeek.Saturday)
                        {
                            sessionend--;
                            if (sessionend < 0)
                                break;
                        }
                    }

                    sessionstart = FindSessionStart(sessionend);
                }
            }

            // We begin from the oldest session coming to the current session or to StartFromDate.
            for (int i = SessionToStart; i < _SessionsToCount; i++)
            {
                if (Session == session_period.Intraday)
                {
                    if (!ProcessIntradaySession(sessionstart, sessionend, i))
                        return;
                }
                else
                {
                    if (Session == session_period.Daily)
                        Max_number_of_bars_in_a_session = PeriodSeconds(TimeFrame.Daily) / PeriodSeconds(TimeFrame);
                    else if (Session == session_period.Weekly)
                        Max_number_of_bars_in_a_session = 604800 / PeriodSeconds(TimeFrame);
                    else if (Session == session_period.Monthly)
                        Max_number_of_bars_in_a_session = 2678400 / PeriodSeconds(TimeFrame);

                    if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday)
                    {
                        // The start is on Sunday - add remaining time.
                        if ((int)Bars[sessionstart].OpenTime.DayOfWeek == 0)
                            Max_number_of_bars_in_a_session += (24 * 3600 - (Bars[sessionstart].OpenTime.Hour * 3600 + Bars[sessionstart].OpenTime.Minute * 60)) / PeriodSeconds(TimeFrame);

                        // The end is on Saturday. +1 because even 0:00 bar deserves a bar.
                        if ((int)Bars[sessionstart].OpenTime.DayOfWeek == 6)
                            Max_number_of_bars_in_a_session += (Bars[sessionstart].OpenTime.Hour * 3600 + Bars[sessionstart].OpenTime.Minute * 60) / PeriodSeconds(TimeFrame) + 1;
                    }

                    if (!ProcessSession(sessionstart, sessionend, i, null))
                        return;
                }

                // Go to the newer session only if there is one or more left.
                if (_SessionsToCount - i > 1)
                {
                    sessionstart = sessionend + 1;
                    if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday)
                    {
                        // Pass through Sunday and Saturday.
                        while (Bars[sessionstart].OpenTime.DayOfWeek == DayOfWeek.Sunday || Bars[sessionstart].OpenTime.DayOfWeek == DayOfWeek.Saturday)
                        {
                            sessionstart++;
                            if (sessionstart == Bars.Count - 1)
                                break;
                        }
                    }

                    sessionend = FindSessionEndByDate(Bars[sessionstart].OpenTime);
                }
            }

            if (ShowValueAreaRays != sessions_to_draw_rays.None || ShowMedianRays != sessions_to_draw_rays.None)
                CheckRays();

            FirstRunDone = true;

            _Timer = DateTime.Now;
        }

        #endregion

        #region OnTimer

        protected override void OnTimer()
        {
            base.OnTimer();

            if (DateTime.Now.Subtract(LastRecalculationTime).TotalMilliseconds < 500)
                return; // Do not recalculate on timer if less than 500 ms passed.

            if (HideRaysFromInvisibleSessions)
                CheckRays(); // Should be checked regularly if the input parameter requires ray hiding/unhiding.

            if (Session == session_period.Rectangle)
            {
                CheckRectangles();
                return; // No need to call RedrawLastSession() even if RightToLeft is on because in that case all Rectangles are all right-to-left and are redrawn as needed.
            }

            if ((!RightToLeft && !SeamlessScrollingMode) || !FirstRunDone)
                return; // Need to finish normal drawing before reacting to timer.
                        // This what goes below works for RightToLeft mode and for seamless scrolling mode, but only after the first run has been finished.

            DateTime converted_time = Bars[Chart.LastVisibleBarIndex].OpenTime;
            if (converted_time == prev_converted_time)
                return; // Do not call RedrawLastSession() if the screen hasn't been scrolled.

            prev_converted_time = converted_time;

            if (SeamlessScrollingMode)
            {
                ObjectCleanup(); // Delete everything to make sure there are no leftover sessions behind the screen.
                if (Session == session_period.Intraday)
                    FirstRunDone = false; // Turn off because FirstRunDone should be false for Intraday sessions to draw properly in the past.

                if (EnableDevelopingPOC || AlertArrows)
                {
                    for (int i = Bars.Count - 1; i >= 0; i--) // Clean indicator buffers.
                    {
                        DevelopingPOC_1[i] = double.NaN;
                        DevelopingPOC_2[i] = double.NaN;
                        ArrowsPB[i] = double.NaN;
                        ArrowsCC[i] = double.NaN;
                        ArrowsGC[i] = double.NaN;
                    }
                }
            }

            // Check right-most time - did it change?
            RedrawLastSession();

            if (SeamlessScrollingMode && Session == session_period.Intraday)
                FirstRunDone = true; // Turn back on after processing Intraday sessions.

            LastRecalculationTime = DateTime.Now; // Remember last calculation time.
        }

        #endregion

        #region Tools

        #region FindSessionStart
        //+------------------------------------------------------------------+
        //| Finds the session's starting bar number for any given bar number.|
        //| n - bar number for which to find starting bar.                   |
        //+------------------------------------------------------------------+
        private int FindSessionStart(int n)
        {
            if (Session == session_period.Daily)
                return FindDayStart(n);
            else if (Session == session_period.Weekly)
                return FindWeekStart(n);
            else if (Session == session_period.Monthly)
                return FindMonthStart(n);
            else if (Session == session_period.Intraday)
            {
                // A special case when Append_Saturday_Sunday is on and n is on Monday.
                if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && Bars[n].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday)
                {
                    // One of the intraday sessions should start at 00:00 or have end < start.
                    for (int intraday_i = 0; intraday_i < IntradaySessionCount; intraday_i++)
                    {
                        if ((ID[intraday_i].StartTime == 0) || (ID[intraday_i].StartTime > ID[intraday_i].EndTime))
                        {
                            // "Monday" part of the day. Effective only for "end < start" sessions.
                            if (Bars[n].OpenTime.Hour * 60 + Bars[n].OpenTime.Minute >= ID[intraday_i].EndTime && ID[intraday_i].StartTime > ID[intraday_i].EndTime)
                            {
                                // Find the first bar on Monday after the borderline session.
                                int x = n;
                                while (x > 0 && Bars[x].OpenTime.Hour * 60 + Bars[x].OpenTime.Minute >= ID[intraday_i].EndTime)
                                {
                                    x--;
                                    // If there is no Sunday session (stepped into Saturday or another non-Sunday/non-Monday day, return normal day start.
                                    if (Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek > DayOfWeek.Monday)
                                        return FindDayStart(n);
                                }

                                return (x + 1);
                            }
                            else
                            {
                                // Find the first Sunday bar.
                                int x = n;
                                while (x > 0 &&
                                    (Bars[n].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear == Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear) ||
                                    (Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday))
                                    x--;

                                // Number of sessions should be increased as we "lose" one session to Sunday.
                                _SessionsToCount++;
                                return (x + 1);
                            }
                        }
                    }
                }

                return FindDayStart(n);
            }

            return -1;
        }

        //+------------------------------------------------------------------+
        //| Finds the day's starting bar number for any given bar number.    |
        //| n - bar number for which to find starting bar.                   |
        //+------------------------------------------------------------------+
        private int FindDayStart(int n)
        {
            if (n < 0)
                return -1;

            int x = n;
            int time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;
            int time_n_day_of_week = time_x_day_of_week;

            // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday or it is Friday but the bar n is on Saturday.
            while (Bars[n].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear == Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear ||
                (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && (time_x_day_of_week == 0 || (time_x_day_of_week == 5 && time_n_day_of_week == 6))))
            {
                x--;
                if (x < 0) break;
                time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;
            }

            return (x + 1);
        }

        //+------------------------------------------------------------------+
        //| Finds the week's starting bar number for any given bar number.   |
        //| n - bar number for which to find starting bar.                   |
        //+------------------------------------------------------------------+
        private int FindWeekStart(int n)
        {
            if (n < 0)
                return -1;

            int x = n;
            int time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;

            // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday.
            while (SameWeek(Bars[n].OpenTime.AddMinutes(TimeShiftMinutes), Bars[x].OpenTime.AddMinutes(TimeShiftMinutes)) ||
                (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && time_x_day_of_week == 0))
            {
                // If Ignore_Saturday_Sunday is on and we stepped into Sunday, stop.
                if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday && time_x_day_of_week == 0)
                    break;

                x--;
                if (x < 0)
                    break;

                time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;
            }

            return (x + 1);
        }

        //+------------------------------------------------------------------+
        //| Finds the month's starting bar number for any given bar number.  |
        //| n - bar number for which to find starting bar.                   |
        //+------------------------------------------------------------------+
        private int FindMonthStart(int n)
        {
            if (n < 0)
                return -1;

            int x = n;
            int time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;
            // These don't change:
            int time_n_day_of_week = (int)Bars[n].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;
            int time_n_day = Bars[n].OpenTime.AddMinutes(TimeShiftMinutes).Day;
            int time_n_month = Bars[n].OpenTime.AddMinutes(TimeShiftMinutes).Month;

            // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday or Saturday the 1st day of month.
            while (time_n_month == Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Month ||
                (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && (time_x_day_of_week == 0 || (time_n_day_of_week == 6 && time_n_day == 1))))
            {
                // If month distance somehow becomes greater than 1, break.
                int month_distance = time_n_month - Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Month;
                if (month_distance < 0)
                    month_distance = 12 - month_distance;

                if (month_distance > 1)
                    break;

                // Check if Append_Saturday_Sunday is on and today is Saturday the 1st day of month. Despite it being current month, it should be skipped because it is appended to the previous month. Unless it is the sessionend day, which is the Saturday of the next month attached to this session.
                if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday)
                {
                    if (time_x_day_of_week == 6 && Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Day == 1 && time_n_day != Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Day)
                        break;
                }

                // Check if Ignore_Saturday_Sunday is on and today is Sunday or Saturday the 2nd or the 1st day of month. Despite it being current month, it should be skipped because it is ignored.
                if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday)
                {
                    if ((time_x_day_of_week == 0 || time_x_day_of_week == 6) && (Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Day == 1 || Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Day == 2))
                        break;
                }

                x--;
                if (x < 0)
                    break;

                time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;
            }

            return (x + 1);
        }

        #endregion

        #region FindSessionEndByDate
        //+------------------------------------------------------------------+
        //| Finds the session's end bar by the session's date.               |
        //+------------------------------------------------------------------+
        private int FindSessionEndByDate(DateTime date)
        {
            if (Session == session_period.Daily)
                return FindDayEndByDate(date);
            else if (Session == session_period.Weekly)
                return FindWeekEndByDate(date);
            else if (Session == session_period.Monthly)
                return FindMonthEndByDate(date);
            else if (Session == session_period.Intraday)
            {
                // A special case when Append_Saturday_Sunday is on and the date is on Sunday.
                if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && (int)date.AddMinutes(TimeShiftMinutes).DayOfWeek == 0)
                {
                    // One of the intraday sessions should start at 00:00 or have end < start.
                    for (int intraday_i = 0; intraday_i < IntradaySessionCount; intraday_i++)
                    {
                        if (ID[intraday_i].StartTime == 0 || ID[intraday_i].StartTime > ID[intraday_i].EndTime)
                        {
                            // Find the last bar of this intraday session and return it as sessionend.
                            int x = Bars.Count - 1;
                            int abs_day = TimeAbsoluteDay(date.AddMinutes(TimeShiftMinutes));
                            // TimeAbsoluteDay is used for cases when the given date is Dec 30 (#364) and the current date is Jan 1 (#1) for example.
                            while (x >= 0 && abs_day < TimeAbsoluteDay(Bars[x].OpenTime.AddMinutes(TimeShiftMinutes))) // It's Sunday.
                            {
                                // On Monday.
                                if (TimeAbsoluteDay(Bars[x].OpenTime.AddMinutes(TimeShiftMinutes)) == abs_day + 1)
                                {
                                    // Inside the session.
                                    if (Bars[x].OpenTime.Hour * 60 + Bars[x].OpenTime.Minute < ID[intraday_i].EndTime)
                                        break;

                                    // Break out earlier (on Monday's end bar) if working with 00:00-XX:XX session.
                                    if (ID[intraday_i].StartTime == 0)
                                        break;
                                }

                                x--;
                            }

                            return x;
                        }
                    }
                }

                return FindDayEndByDate(date);
            }

            return -1;
        }

        //+------------------------------------------------------------------+
        //| Finds the day's end bar by the day's date.                       |
        //+------------------------------------------------------------------+
        private int FindDayEndByDate(DateTime date)
        {
            int x = Bars.Count - 1;

            // TimeAbsoluteDay is used for cases when the given date is Dec 30 (#364) and the current date is Jan 1 (#1) for example.
            while (x >= 0 && TimeAbsoluteDay(date.AddMinutes(TimeShiftMinutes)) < TimeAbsoluteDay(Bars[x].OpenTime.AddMinutes(TimeShiftMinutes)))
            {
                // Check if Append_Saturday_Sunday is on and if the found end of the day is on Saturday and the given date is the previous Friday; or it is a Monday and the sought date is the previous Sunday.
                if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday)
                {
                    if (((int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == 6 || (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == 1) &&
                        TimeAbsoluteDay(Bars[x].OpenTime.AddMinutes(TimeShiftMinutes)) - TimeAbsoluteDay(date.AddMinutes(TimeShiftMinutes)) == 1)
                        break;
                }

                x--;
            }

            return x;
        }

        //+------------------------------------------------------------------+
        //| Finds the week's end bar by the week's date.                     |
        //+------------------------------------------------------------------+
        private int FindWeekEndByDate(DateTime date)
        {
            int x = Bars.Count - 1;
            int time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;

            // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday; and also if Ignore_Saturday_Sunday is on and it is Saturday or Sunday.
            while (!SameWeek(date.AddMinutes(TimeShiftMinutes), Bars[x].OpenTime.AddMinutes(TimeShiftMinutes)) ||
                (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && time_x_day_of_week == 0) ||
                (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday && (time_x_day_of_week == 0 || time_x_day_of_week == 6)))
            {
                x--;
                if (x < 0)
                    break;

                time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;
            }

            return x;
        }

        //+------------------------------------------------------------------+
        //| Finds the month's end bar by the month's date.                   |
        //+------------------------------------------------------------------+
        private int FindMonthEndByDate(DateTime date)
        {
            int x = Bars.Count - 1;
            int time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;

            // Condition should pass also if Append_Saturday_Sunday is on and it is Sunday; and also if Ignore_Saturday_Sunday is on and it is Saturday or Sunday.
            while (!SameMonth(date.AddMinutes(TimeShiftMinutes), Bars[x].OpenTime.AddMinutes(TimeShiftMinutes)) ||
                (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && time_x_day_of_week == 0) ||
                (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday && (time_x_day_of_week == 0 || time_x_day_of_week == 6)))
            {
                // Check if Append_Saturday_Sunday is on.
                if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday)
                {
                    // Today is Saturday the 1st day of the next month. Despite it being in a next month, it should be appended to the current month.
                    if (time_x_day_of_week == 6 && Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Day == 1 &&
                        Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Year * 12 + Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Month -
                        date.AddMinutes(TimeShiftMinutes).Year * 12 - date.AddMinutes(TimeShiftMinutes).Month == 1)
                        break;
                    // Given date is Sunday of a previous month. It was rejected in the previous month and should be appended to beginning of this one.
                    // Works because date here can be only the end or the beginning of the month.
                    if ((int)date.AddMinutes(TimeShiftMinutes).DayOfWeek == 0 &&
                        Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Year * 12 + Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).Month -
                        date.AddMinutes(TimeShiftMinutes).Year * 12 - date.AddMinutes(TimeShiftMinutes).Month == 1)
                        break;
                }

                x--;

                if (x < 0)
                    break;

                time_x_day_of_week = (int)Bars[x].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek;
            }

            return x;
        }

        #endregion

        #region PutDot
        //+------------------------------------------------------------------+
        //| Puts a dot (rectangle) at a given position and color.            |
        //| price and time are coordinates.                                  |
        //| range is for the second coordinate.                              |
        //| bar is to determine the color of the dot.                        |
        //| Returns inverted end time only for the RightToLeft session.      |
        //+------------------------------------------------------------------+
        private DateTime PutDot(double price, int start_bar, int range, int bar, DateTime converted_time, string rectangle_prefix = "")
        {
            double divisor, color_shift;
            Color colour = -1;

            // All dots are with the same date/time for a given origin bar, but with a different price.
            string LastNameStart = " " + Bars[bar].OpenTime.ToString() + " ";
            string LastName = LastNameStart + Math.Round(price, Symbol.Digits).ToString();

            if (ColorBullBear)
                colour = CalculateProperColor();

            string obj_prefix = rectangle_prefix + "MP" + Suffix;

            // Bull/bear coloring part.
            if (NeedToReviewColors)
            {
                // Finding all dots (rectangle objects) with proper suffix and start of last name (date + time of the bar, but not price).
                // This is needed to change their color if candle changed its direction.
                foreach (var obj in Chart.FindAllObjects<ChartRectangle>())
                {
                    // Probably some other object.
                    if (!obj.Name.StartsWith(obj_prefix))
                        continue;

                    // Previous bar's dot found.
                    if (!obj.Name.StartsWith(obj_prefix + LastNameStart))
                        break;

                    // Change color.
                    obj.Color = colour;
                }
            }

            if (Chart.FindObject(obj_prefix + LastName) != null)
            {
                if (!RightToLeft || converted_time == DateTime.MinValue)
                    return DateTime.MinValue; // Normal case;
            }

            DateTime time_end, time_start;
            DateTime prev_time = converted_time; // For drawing, we need two times.
            if (converted_time != DateTime.MinValue) // This is the right-to-left mode and the right-most session.
            {
                // Check if we have started a new right-most session, so the previous one should be cleaned up.
                //prev_time_start_bar = DateTime.MinValue;

                if (Bars[start_bar].OpenTime != prev_time_start_bar && prev_time_start_bar != DateTime.MinValue) // New right-most session arrived - recalculate everything.
                {
                    NeedToRestartDrawing = true;
                }
                prev_time_start_bar = Bars[start_bar].OpenTime;

                int x = Bars.OpenTimes.GetIndexByTime(converted_time) - range;

                time_end = Bars[x].OpenTime;
                time_start = Bars[x - 1].OpenTime;

                converted_time = time_end;
            }
            else
            {
                if (start_bar + (range + 1) > Bars.Count)
                    time_end = Bars.LastBar.OpenTime.AddSeconds(PeriodSeconds(TimeFrame)); // Protection from 'Array out of range' error.
                else
                    time_end = Bars[start_bar + (range + 1)].OpenTime;

                time_start = Bars[start_bar + range].OpenTime;
            }

            if (!ColorBullBear) // Otherwise, colour is already calculated.
            {
                // Color switching depending on the distance of the bar from the session's beginning.
                int offset1, offset2;
                // Using 3 as a step for color switching because MT4 has buggy invalid color codes that mess up with the chart.
                // Anyway, the number of supported colors is much lower than we get here, even with step = 3.
                switch (CurrentColorScheme)
                {
                    case color_scheme.Blue_to_Red:
                        colour = Color.Blue;
                        offset1 = 0x030000;
                        offset2 = 0x000003;
                        break;
                    case color_scheme.Red_to_Green:
                        colour = Color.DarkRed;
                        offset1 = 0x000003;
                        offset2 = 0x000300;
                        break;
                    case color_scheme.Green_to_Blue:
                        colour = Color.DarkGreen;
                        offset1 = 0x000300;
                        offset2 = 0x030000;
                        break;
                    case color_scheme.Yellow_to_Cyan:
                        colour = Color.Yellow;
                        offset1 = 0x000003;
                        offset2 = 0x030000;
                        break;
                    case color_scheme.Magenta_to_Yellow:
                        colour = Color.Magenta;
                        offset1 = 0x030000;
                        offset2 = 0x000300;
                        break;
                    case color_scheme.Cyan_to_Magenta:
                        colour = Color.Cyan;
                        offset1 = 0x000300;
                        offset2 = 0x000003;
                        break;
                    case color_scheme.Single_Color:
                        colour = Color.FromName(SingleColor);
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
                if (CurrentColorScheme != color_scheme.Single_Color)
                {
                    divisor = 3.0 / 0xFF * (double)Max_number_of_bars_in_a_session;

                    // bar is negative.
                    color_shift = Math.Floor((double)(bar - start_bar) / divisor);

                    // Prevents color overflow.
                    if ((int)color_shift < -85)
                        color_shift = -85;

                    string hex = colour.ToHexString().Replace("#", "");
                    int colour_hex = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);

                    colour_hex += (int)color_shift * offset1;
                    colour_hex -= (int)color_shift * offset2;

                    colour = Color.FromHex(colour_hex.ToString("X4"));
                }

                int opacity = (ColorOpacity * 255) / 100;
                colour = Color.FromArgb(opacity, colour);
            }

            ChartRectangle cr = Chart.FindObject(obj_prefix + LastName) as ChartRectangle;
            if (cr != null) // Need to move the rectangle.
            {
                cr.Time1 = time_start;
                cr.Time2 = time_end;
                cr.Color = colour;
            }
            else
            {
                cr = Chart.DrawRectangle(obj_prefix + LastName, time_start, price, time_end, price - onetick, colour);
                cr.IsFilled = true;
            }

            cr.Thickness = 0;
            cr.ZIndex = -1000;

            return time_end;
        }

        #endregion

        #region CalculateProperColor
        //+------------------------------------------------------------------+
        //| Calculates dot color based on bar direction and color scheme.    |
        //| Used only when ColorBullBear == true.                            |
        //+------------------------------------------------------------------+
        private Color CalculateProperColor()
        {
            Color colour = Color.Transparent;
            switch (CurrentColorScheme)
            {
                case color_scheme.Blue_to_Red:
                    if (CurrentBarDirection == bar_direction.Bullish)
                        colour = Color.Blue;
                    else if (CurrentBarDirection == bar_direction.Bearish)
                        colour = Color.DarkRed;
                    else if (CurrentBarDirection == bar_direction.Neutral)
                        colour = Color.Pink;
                    break;
                case color_scheme.Red_to_Green:
                    if (CurrentBarDirection == bar_direction.Bullish)
                        colour = Color.DarkRed;
                    else if (CurrentBarDirection == bar_direction.Bearish)
                        colour = Color.DarkGreen;
                    else if (CurrentBarDirection == bar_direction.Neutral)
                        colour = Color.Brown;
                    break;
                case color_scheme.Green_to_Blue:
                    if (CurrentBarDirection == bar_direction.Bullish)
                        colour = Color.DarkGreen;
                    else if (CurrentBarDirection == bar_direction.Bearish)
                        colour = Color.Blue;
                    else if (CurrentBarDirection == bar_direction.Neutral)
                        colour = Color.DarkGray;
                    break;
                case color_scheme.Yellow_to_Cyan:
                    if (CurrentBarDirection == bar_direction.Bullish)
                        colour = Color.Yellow;
                    else if (CurrentBarDirection == bar_direction.Bearish)
                        colour = Color.Cyan;
                    else if (CurrentBarDirection == bar_direction.Neutral)
                        colour = Color.Green;
                    break;
                case color_scheme.Magenta_to_Yellow:
                    if (CurrentBarDirection == bar_direction.Bullish)
                        colour = Color.Magenta;
                    else if (CurrentBarDirection == bar_direction.Bearish)
                        colour = Color.Yellow;
                    else if (CurrentBarDirection == bar_direction.Neutral)
                        colour = Color.Green;
                    break;
                case color_scheme.Cyan_to_Magenta:
                    if (CurrentBarDirection == bar_direction.Bullish)
                        colour = Color.Cyan;
                    else if (CurrentBarDirection == bar_direction.Bearish)
                        colour = Color.Magenta;
                    else if (CurrentBarDirection == bar_direction.Neutral)
                        colour = Color.Green;
                    break;
                case color_scheme.Single_Color:
                    if (CurrentBarDirection == bar_direction.Bullish)
                    {
                        colour = Color.FromName(SingleColor);
                    }
                    else if (CurrentBarDirection == bar_direction.Bearish)
                    {
                        int colour_hex = 0x00FFFFFF - Convert.ToInt32(Color.FromName(SingleColor).ToHexString());
                        colour = Color.FromHex(colour_hex.ToString("X4"));
                    }
                    else if (CurrentBarDirection == bar_direction.Neutral)
                    {
                        int colour_hex = Math.Max(Convert.ToInt32(Color.FromName(SingleColor).ToHexString()), 0x00FFFFFF - Convert.ToInt32(Color.FromName(SingleColor).ToHexString()));
                        colour = Color.FromHex(colour_hex.ToString("X4"));
                    }
                    break;
                default:
                    if (CurrentBarDirection == bar_direction.Bullish)
                    {
                        colour = Color.FromName(SingleColor);
                    }
                    else if (CurrentBarDirection == bar_direction.Bearish)
                    {
                        int colour_hex = 0x00FFFFFF - Convert.ToInt32(Color.FromName(SingleColor).ToHexString());
                        colour = Color.FromHex(colour_hex.ToString("X4"));
                    }
                    else if (CurrentBarDirection == bar_direction.Neutral)
                    {
                        int colour_hex = Math.Max(Convert.ToInt32(Color.FromName(SingleColor).ToHexString()), 0x00FFFFFF - Convert.ToInt32(Color.FromName(SingleColor).ToHexString()));
                        colour = Color.FromHex(colour_hex.ToString("X4"));
                    }
                    break;
            }

            return colour;
        }

        #endregion

        #region CheckRectangles
        // Find rectangles, create objects, process rectangle sessions, delete unneeded sessions (where rectangle no longer exists).
        // Make sure rectangles are added to the array in a sorted manner from oldest T1 to newest T1.
        private void CheckRectangles()
        {
            // Check if any existing MPR objects need to be deleted or moved:
            for (int i = MPR_Array.Count - 1; i >= 0; i--)
            {
                ChartRectangle mpr = Chart.FindObject(MPR_Array[i].name) as ChartRectangle;
                if (mpr == null)
                {
                    ObjectCleanup(MPR_Array[i].name + "_");

                    // Buffer cleanup for the Developing POC.
                    if (EnableDevelopingPOC)
                    {
                        int sessionstart = Bars.OpenTimes.GetIndexByTime(MPR_Array[i].RectangleTimeMin);
                        int sessionend = Bars.OpenTimes.GetIndexByTime(MPR_Array[i].RectangleTimeMax);
                        if (sessionend < 0)
                            sessionend = Bars.Count - 1; // If the rectangle's rightmost side is in the future, reset it to the current bar. Re-initialize all bars using old rectangle borders:

                        for (int j = sessionstart; j <= sessionend; j++)
                        {
                            DevelopingPOC_1[j] = double.NaN;
                            DevelopingPOC_2[j] = double.NaN;
                        }
                    }

                    MPR_Array.RemoveAt(i);
                }
            }

            // Find all objects of rectangle type with the name starting with MPR.
            foreach (var cr in Chart.FindAllObjects<ChartRectangle>())
            {
                string name = cr.Name;

                if (!name.StartsWith("MPR"))
                    continue;

                if (name.Contains("_"))
                    continue; // Skip chart objects created based on a rectangle session.

                DateTime t1 = cr.Time1;
                DateTime t2 = cr.Time2;

                // Find the rectangle among the array's elements by its name.
                bool name_found = false;
                for (int j = 0; j < MPR_Array.Count; j++)
                {
                    // Check if it should be moved inside the array to keep sorting intact.
                    if (MPR_Array[j].name == name)
                    {
                        name_found = true;

                        MPR_Array[j].t1 = t1;
                        MPR_Array[j].t2 = t2;

                        break;
                    }
                }

                // New rectangle:
                if (!name_found)
                {
                    // Check if it should be moved inside the array to keep sorting intact.
                    DateTime t = (t1 < t2 ? t1 : t2); // Leftmost side.

                    MPR_Array.Add(new CRectangleMP(name));
                    int k = MPR_Array.Count - 1;

                    MPR_Array[k].RectangleTimeMin = t;
                    MPR_Array[k].t1 = t1;
                    MPR_Array[k].t2 = t2;
                }
            }

            if (SessionsNumber != MPR_Array.Count)
            {
                SessionsNumber = MPR_Array.Count;
            }

            // Process each rectangle.
            for (int i = 0; i < MPR_Array.Count; i++)
                MPR_Process(i);

            if (ShowValueAreaRays != sessions_to_draw_rays.None || ShowMedianRays != sessions_to_draw_rays.None)
                CheckRays();

            LastRecalculationTime = DateTime.Now; // Remember last calculation time.
        }

        #endregion

        #region MPR_Process

        private void MPR_Process(int i)
        {
            CRectangleMP mp = MPR_Array[i];
            ChartRectangle rect = Chart.FindObject(mp.name) as ChartRectangle;

            double p1 = rect.Y1;
            double p2 = rect.Y2;

            if (mp.Number == -1)
                mp.Number = i;

            bool rectangle_changed = false;
            bool rectangle_time_changed = false;
            bool rectangle_price_changed = false;

            // If any of the rectangle parameters changed.
            if (mp.RectangleTimeMax != (mp.t1 > mp.t2 ? mp.t1 : mp.t2) || mp.RectangleTimeMin != (mp.t1 < mp.t2 ? mp.t1 : mp.t2))
            {
                rectangle_changed = true;
                rectangle_time_changed = true;
            }

            if (mp.RectanglePriceMax != Math.Max(p1, p2) || mp.RectanglePriceMin != Math.Min(p1, p2))
            {
                rectangle_changed = true;
                rectangle_price_changed = true;
            }

            // Buffer cleanup for the Developing POC. Should be run only for a changed rectangle, which isn't brand new.
            if (EnableDevelopingPOC && rectangle_changed && mp.RectangleTimeMax != DateTime.MinValue)
            {
                int _sessionstart = Bars.OpenTimes.GetIndexByTime(mp.RectangleTimeMin);
                int _sessionend = Bars.OpenTimes.GetIndexByTime(mp.RectangleTimeMax);
                if (_sessionend < 0)
                    _sessionend = Bars.Count - 1; // If the rectangle's rightmost side is in the future, reset it to the current bar. Re-initialize all bars using old rectangle borders:

                for (int j = _sessionstart; j <= _sessionend; j++)
                {
                    DevelopingPOC_1[j] = double.NaN;
                    DevelopingPOC_2[j] = double.NaN;
                }
            }

            mp.RectangleTimeMax = (mp.t1 > mp.t2 ? mp.t1 : mp.t2);
            mp.RectangleTimeMin = (mp.t1 < mp.t2 ? mp.t1 : mp.t2);
            mp.RectanglePriceMax = Math.Max(p1, p2);
            mp.RectanglePriceMin = Math.Min(p1, p2);

            bool new_bars_are_not_within_rectangle = true;
            bool current_bar_changed_within_boundaries = false;

            DateTime Time0 = Bars.LastBar.OpenTime;

            if (Time0 != mp.prev_Time0)
            {
                new_bars_are_not_within_rectangle = false;
                // Check if any of the new bars fall into rectangle's boundaries:
                if ((mp.prev_Time0 < mp.RectangleTimeMin && Time0 < mp.RectangleTimeMin) || (mp.prev_Time0 > mp.RectangleTimeMax && Time0 > mp.RectangleTimeMax))
                    new_bars_are_not_within_rectangle = true;

                // Now check if the price of any of the new bars is within the rectangle's boundaries:
                if (mp.prev_Time0 != DateTime.MinValue && !new_bars_are_not_within_rectangle)
                {
                    int prev_Time0_idx = Bars.OpenTimes.GetIndexByTime(mp.prev_Time0);

                    int max_index = GetHighestHighIdx(prev_Time0_idx, Bars.Count - 1);
                    int min_index = GetLowestLowIdx(prev_Time0_idx, Bars.Count - 1);

                    if (Bars[max_index].High < mp.RectanglePriceMin || Bars[min_index].Low > mp.RectanglePriceMax)
                        new_bars_are_not_within_rectangle = true;
                }

                mp.prev_Time0 = Time0;
            }
            else // No new bars - check if the current bar's high or low changed within the rectangle's boundaries:
            {
                if (Time0 >= mp.RectangleTimeMin && Time0 <= mp.RectangleTimeMax) // Bar within time boundaries.
                {
                    if (mp.prev_High != Bars.LastBar.High)
                    {
                        if (Bars.LastBar.High <= mp.RectanglePriceMax && Bars.LastBar.High >= mp.RectanglePriceMin)
                            current_bar_changed_within_boundaries = true;
                    }

                    if (mp.prev_Low != Bars.LastBar.Low)
                    {
                        if (Bars.LastBar.Low <= mp.RectanglePriceMax && Bars.LastBar.Low >= mp.RectanglePriceMin)
                            current_bar_changed_within_boundaries = true;
                    }
                }
            }

            mp.prev_High = Bars.LastBar.High;
            mp.prev_Low = Bars.LastBar.Low;

            // Calculate rectangle session's actual time and price boundaries.
            int sessionstart = Bars.OpenTimes.GetIndexByTime(mp.t1 < mp.t2 ? mp.t1 : mp.t2);
            int sessionend = Bars.OpenTimes.GetIndexByTime(mp.t1 > mp.t2 ? mp.t1 : mp.t2);
            if (sessionend < 0)
                sessionend = Bars.Count - 1; // If the rectangles rightmost side is in the future, reset it to the current bar.

            bool need_to_clean_up_dots = false;
            bool rectangle_changed_and_recalc_is_due = false;

            if (rectangle_changed)
            {
                if (rectangle_price_changed)
                {
                    // Max/min bars of the price range within rectangle's boundaries before and after change:
                    int max_index = GetHighestHighIdx(sessionstart, sessionend);
                    int min_index = GetLowestLowIdx(sessionstart, sessionend);

                    if (max_index != -1 && min_index != -1)
                    {
                        if (mp.RectanglePriceMax > Bars[max_index].High && mp.RectanglePriceMin < Bars[min_index].Low &&
                            mp.prev_RectanglePriceMax > Bars[max_index].High && mp.prev_RectanglePriceMin < Bars[min_index].Low)
                        {
                            rectangle_changed_and_recalc_is_due = false;
                        }
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
                    rectangle_changed_and_recalc_is_due = true;
                }
            }

            mp.prev_RectanglePriceMax = mp.RectanglePriceMax;
            mp.prev_RectanglePriceMin = mp.RectanglePriceMin;

            // Need to continue drawing profile in the following cases only:
            // 1. New bar came in and it is within the rectangle's borders.
            // 2. Current bar changed its High or Low and it is now within the borders.
            // 3. Rectangle changed its borders.
            // 4. Order of rectangles changed - need recalculation for stopping the rays (only when it is really needed).

            // Need to delete previous dots before going to drawing in the following cases:
            // 1. Rectangle changed its borders.
            // 2. When Max_number_of_bars_in_a_session changes.

            // Number of bars in the rectangle session changed, need to update colors, so a cleanup is due.
            if (sessionend - sessionstart + 1 != Max_number_of_bars_in_a_session)
            {
                Max_number_of_bars_in_a_session = sessionend - sessionstart + 1;
                if (!new_bars_are_not_within_rectangle)
                    need_to_clean_up_dots = true;
            }

            if (need_to_clean_up_dots)
                ObjectCleanup(mp.name + "_");

            if (sessionstart < 0)
                return; // Rectangle is drawn in the future.

            RememberSession.Add(new SessionInfo());
            RememberSession[RememberSession.Count - 1].Start = mp.RectangleTimeMin;

            if (!new_bars_are_not_within_rectangle || current_bar_changed_within_boundaries || rectangle_changed_and_recalc_is_due || (EnableDevelopingPOC && rectangle_changed) ||
                (mp.Number != i && RaysUntilIntersection != ways_to_stop_rays.Stop_No_Rays &&
                (ShowMedianRays != sessions_to_draw_rays.None || ShowValueAreaRays != sessions_to_draw_rays.None)))
                ProcessSession(sessionstart, sessionend, i, mp);

            mp.Number = i;
        }

        #endregion

        #region ObjectCleanup
        //+------------------------------------------------------------------+
        //| Deletes all chart objects created by the indicator.              |
        //+------------------------------------------------------------------+
        void ObjectCleanup(string rectangle_prefix = "")
        {
            // Delete all rectangles with set prefix.
            var chart_rectangles = Chart.FindAllObjects(ChartObjectType.Rectangle);
            for (int i = chart_rectangles.Length - 1; i >= 0; i--)
            {
                if (chart_rectangles[i].Name.StartsWith(rectangle_prefix))
                    Chart.RemoveObject(chart_rectangles[i].Name);
            }

            var chart_lines = Chart.FindAllObjects(ChartObjectType.TrendLine);
            for (int i = chart_lines.Length - 1; i >= 0; i--)
            {
                if (chart_lines[i].Name.StartsWith(rectangle_prefix))
                    Chart.RemoveObject(chart_lines[i].Name);
            }

            var chart_texts = Chart.FindAllObjects(ChartObjectType.Text);
            for (int i = chart_texts.Length - 1; i >= 0; i--)
            {
                if (chart_texts[i].Name.StartsWith(rectangle_prefix))
                    Chart.RemoveObject(chart_texts[i].Name);
            }
        }

        #endregion

        #region GetHoursAndMinutes
        //+------------------------------------------------------------------+
        //| Extract hours and minutes from a time string.                    |
        //| Returns false in case of an error.                               |
        //+------------------------------------------------------------------+
        private bool GetHoursAndMinutes(string time_string, ref int hours, ref int minutes, ref int time)
        {
            if (time_string.Length == 4)
                time_string = "0" + time_string;

            if (
                // Wrong length.
                (time_string.Length != 5) ||
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
                ((time_string[4] < '0') || (time_string[4] > '9')))
            {
                Print(String.Format("Wrong time string: {0}. Please use HH:MM format.", time_string));
                return false;
            }

            string[] result = time_string.Split(':');

            hours = int.Parse(result[0]);
            minutes = int.Parse(result[1]);
            time = hours * 60 + minutes;
            return true;
        }

        #endregion

        #region CheckIntradaySession
        //+------------------------------------------------------------------+
        //| Extract hours and minutes from a time string.                    |
        //| Returns false in case of an error.                               |
        //+------------------------------------------------------------------+
        private bool CheckIntradaySession(bool enable, string start_time, string end_time, color_scheme cs)
        {
            if (!enable)
                return (true);

            ID[IntradaySessionCount] = new Intraday();

            if (!GetHoursAndMinutes(start_time, ref ID[IntradaySessionCount].StartHours, ref ID[IntradaySessionCount].StartMinutes, ref ID[IntradaySessionCount].StartTime))
            {
                string alert_text = String.Format("Wrong time string format: {0}.", start_time);
                Alert(alert_text);

                return false;
            }

            if (!GetHoursAndMinutes(end_time, ref ID[IntradaySessionCount].EndHours, ref ID[IntradaySessionCount].EndMinutes, ref ID[IntradaySessionCount].EndTime))
            {
                string alert_text = String.Format("Wrong time string format: {0}.", end_time);
                Alert(alert_text);

                return false;
            }

            // Special case of the intraday session ending at "00:00".
            if (ID[IntradaySessionCount].EndTime == 0)
            {
                // Turn it into "24:00".
                ID[IntradaySessionCount].EndHours = 24;
                ID[IntradaySessionCount].EndMinutes = 0;
                ID[IntradaySessionCount].EndTime = 24 * 60;
            }

            ID[IntradaySessionCount].ColorScheme = cs;

            // For special case used only with Ignore_Saturday_Sunday on Monday.
            if (ID[IntradaySessionCount].EndTime < ID[IntradaySessionCount].StartTime)
                IntradayCrossSessionDefined = IntradaySessionCount;

            IntradaySessionCount++;

            return true;
        }

        #endregion

        #region ProcessSession
        //+------------------------------------------------------------------+
        //| Main procedure to draw the Market Profile based on a session     |
        //| start bar and session end bar.                                   |
        //| i - session number with 0 being the oldest one.                  |
        //| Returns true on success, false - on failure.                     |
        //+------------------------------------------------------------------+
        private bool ProcessSession(int sessionstart, int sessionend, int i, CRectangleMP rectangle)
        {
            string rectangle_prefix = ""; // Only for rectangle sessions.

            if (sessionstart < 0)
                return false; // Data not yet ready.

            double SessionMax = double.MinValue, SessionMin = double.MaxValue;

            // Find the session's high and low.
            for (int bar = sessionstart; bar <= sessionend; bar++)
            {
                if (Bars[bar].High > SessionMax)
                    SessionMax = Bars[bar].High;
                if (Bars[bar].Low < SessionMin)
                    SessionMin = Bars[bar].Low;
            }

            SessionMax = Math.Round(SessionMax, DigitsM);
            SessionMin = Math.Round(SessionMin, DigitsM);

            int session_counter = i;

            if (Session == session_period.Rectangle)
            {
                rectangle_prefix = rectangle.name + "_";
                if (SessionMax > rectangle.RectanglePriceMax)
                    SessionMax = Math.Round(rectangle.RectanglePriceMax, DigitsM);
                if (SessionMin < rectangle.RectanglePriceMin)
                    SessionMin = Math.Round(rectangle.RectanglePriceMin, DigitsM);
            }
            else
            {
                // Find Time[sessionstart] among RememberSessionStart[].
                bool need_to_increment = true;

                for (int j = 0; j < RememberSession.Count; j++)
                {
                    if (RememberSession[j].Start == Bars[sessionstart].OpenTime)
                    {
                        need_to_increment = false;
                        session_counter = j; // Real number of the session.
                        break;
                    }
                }

                // Raise the number of sessions and resize arrays.
                if (need_to_increment)
                {
                    RememberSession.Add(new SessionInfo());
                    SessionsNumber++;
                    session_counter = RememberSession.Count - 1; // Newest session.
                }
            }

            // Adjust SessionMin, SessionMax for onetick granularity.
            SessionMax = Math.Round(Math.Round(SessionMax / onetick) * onetick, DigitsM);
            SessionMin = Math.Round(Math.Round(SessionMin / onetick) * onetick, DigitsM);

            RememberSession[session_counter].Max = SessionMax;
            RememberSession[session_counter].Min = SessionMin;
            RememberSession[session_counter].Start = Bars[sessionstart].OpenTime;
            RememberSession[session_counter].Suffix = Suffix;

            // Reset PreviousSessionMax when a new session becomes the 'latest one'.
            if (Bars[sessionstart].OpenTime > PreviousSessionStartTime)
            {
                PreviousSessionMax = double.MinValue;
                PreviousSessionStartTime = Bars[sessionstart].OpenTime;
            }

            if (FirstRunDone && i == _SessionsToCount - 1 && PointMultiplier_calculated > 1) // Updating the latest trading session.
            {
                if (SessionMax - PreviousSessionMax < onetick) // SessionMax increased only slightly - too small to use the new value with the current onetick.
                {
                    SessionMax = PreviousSessionMax; // Do not update session max.
                }
                else
                {
                    if (PreviousSessionMax != double.MinValue)
                    {
                        // Calculate number of increments.
                        double nc = (SessionMax - PreviousSessionMax) / onetick;
                        // Adjust SessionMax.
                        SessionMax = Math.Round(PreviousSessionMax + Math.Round(nc) * onetick, DigitsM);
                    }
                    PreviousSessionMax = SessionMax;
                }
            }

            // Possible price levels if multiplied to integer.
            double dmax = (SessionMax - SessionMin) / onetick;
            int max = (int)Math.Round(dmax + 2); // + 2 because further we will be possibly checking array at SessionMax + 1.

            int[] TPOperPrice = new int[max];
            bool[] SinglePrintTracking_array = new bool[max]; // For SinglePrint rays.

            int MaxRange = 0; // Maximum distance from session start to the drawn dot.
            double PriceOfMaxRange = 0; // Level of the maximum range, required to draw Median.
            double DistanceToCenter = double.MaxValue; // Closest distance to center for the Median.

            // Right to left for the final session:
            // 1. Get rightmost time.
            // 2a. If it <= Time[0] - use normal bar-walking, else:
            // 2b. To "move" to the left - subtract PeriodSeconds().
            // 3. Draw everything based on that Time.
            // 4. Redraw everything every time the rightmost time changes.
            // 5. Ray lines to the left.

            // Right-to-left depiction of the rightmost session.
            DateTime converted_time = DateTime.MinValue;
            DateTime converted_end_time = DateTime.MinValue;
            DateTime min_converted_end_time = DateTime.MaxValue;

            if (RightToLeft && (sessionend == Bars.Count - 1 || Session == session_period.Rectangle))
            {
                if (Session == session_period.Rectangle)
                    converted_time = rectangle.RectangleTimeMax;
                else
                    converted_time = Bars[Chart.LastVisibleBarIndex].OpenTime;
            }

            int TotalTPO = 0; // Total amount of dots (TPO's).

            // Going through all possible quotes from session's High to session's Low.
            for (double price = SessionMax; price >= SessionMin; price -= onetick)
            {
                price = Math.Round(price, DigitsM);
                int range = 0; // Distance from first bar to the current bar.

                // Going through all bars of the session to see if the price was encountered here.
                for (int bar = sessionstart; bar <= sessionend; bar++)
                {
                    // Price is encountered in the given bar.
                    if (price >= Bars[bar].Low && price <= Bars[bar].High)
                    {
                        // Update maximum distance from session's start to the found bar (needed for Median).
                        // Using the center-most Median if there are more than one.
                        if (MaxRange < range || (MaxRange == range && (Math.Abs(price - (SessionMin + (SessionMax - SessionMin) / 2)) < DistanceToCenter)))
                        {
                            MaxRange = range;
                            PriceOfMaxRange = price;
                            DistanceToCenter = Math.Abs(price - (SessionMin + (SessionMax - SessionMin) / 2));
                        }

                        if (!DisableHistogram)
                        {
                            if (ColorBullBear)
                            {
                                // These are needed in all cases when we color dots according to bullish/bearish bars.
                                if (Bars[bar].Close == Bars[bar].Open)
                                    CurrentBarDirection = bar_direction.Neutral;
                                else if (Bars[bar].Close > Bars[bar].Open)
                                    CurrentBarDirection = bar_direction.Bullish;
                                else if (Bars[bar].Close < Bars[bar].Open)
                                    CurrentBarDirection = bar_direction.Bearish;

                                // This is for recoloring of the dots from the current (most-latest) bar.
                                if (bar == Bars.Count - 1)
                                {
                                    if (PreviousBarDirection == CurrentBarDirection)
                                        NeedToReviewColors = false;
                                    else
                                    {
                                        NeedToReviewColors = true;
                                        PreviousBarDirection = CurrentBarDirection;
                                    }
                                }
                            }

                            // Draws rectangle.
                            if (!RightToLeft)
                                PutDot(price, sessionstart, range, bar, DateTime.MinValue, rectangle_prefix);
                            // Inverted drawing.
                            else
                            {
                                converted_end_time = PutDot(price, sessionstart, range, bar, converted_time, rectangle_prefix);
                                if (converted_end_time < min_converted_end_time)
                                    min_converted_end_time = converted_end_time; // Find the leftmost time to use for the left border of the value area.
                            }
                        }

                        // Remember the number of encountered bars for this price.
                        int idx = (int)Math.Round((price - SessionMin) / onetick);
                        TPOperPrice[idx]++;
                        range++;
                        TotalTPO++;
                    }
                }

                // Single print marking is due at this price.
                if (ShowSinglePrint != single_print_type.No)
                {
                    if (range == 1)
                        PutSinglePrintMark(price, sessionstart, rectangle_prefix);
                    else if (range > 1)
                        RemoveSinglePrintMark(price, sessionstart, rectangle_prefix); // Remove single print max if it exists.
                }

                if (SinglePrintRays)
                {
                    int idx = (int)Math.Round((price - SessionMin) / onetick);
                    if (range == 1)
                        SinglePrintTracking_array[idx] = true; // Remember the single print's position relative to the price.
                    else
                        SinglePrintTracking_array[idx] = false;
                }
            }

            // Single Print Rays
            // Go through all prices again, check TPOs - whether they are single and whether they aren't bordered by another single print TPOs?
            if (SinglePrintRays)
            {
                Color spr_color = Color.FromName(SinglePrintColor); // Normal ray color.
                if (HideRaysFromInvisibleSessions && Bars[Chart.FirstVisibleBarIndex].OpenTime >= Bars[sessionstart].OpenTime)
                    spr_color = Color.Transparent; // Hide rays if behind the screen.

                for (double price = SessionMax; price >= SessionMin; price -= onetick)
                {
                    price = Math.Round(price, DigitsM);
                    int idx = (int)Math.Round((price - SessionMin) / onetick);
                    if (SinglePrintTracking_array[idx])
                    {
                        if (price == SessionMax) // Top of the session.
                        {
                            PutSinglePrintRay(price, sessionstart, rectangle_prefix, spr_color);
                        }
                        else
                        {
                            if (SinglePrintTracking_array[idx + 1] == false) // Above is a non-single print.
                                PutSinglePrintRay(price, sessionstart, rectangle_prefix, spr_color);
                            else
                                RemoveSinglePrintRay(price, sessionstart, rectangle_prefix);
                        }

                        if (price == SessionMin) // Bottom of the session.
                        {
                            PutSinglePrintRay(price - onetick, sessionstart, rectangle_prefix, spr_color);
                        }
                        else
                        {
                            if (SinglePrintTracking_array[idx - 1] == false) // Below is a non-single print.
                                PutSinglePrintRay(price - onetick, sessionstart, rectangle_prefix, spr_color);
                            else
                                RemoveSinglePrintRay(price - onetick, sessionstart, rectangle_prefix);
                        }
                    }
                    else
                    {
                        // Attempt to remove a horizontal line above and below the "potentially no longer existing" single print mark.
                        RemoveSinglePrintRay(price - onetick, sessionstart, rectangle_prefix);
                    }
                }
            }

            if (EnableDevelopingPOC)
                CalculateDevelopingPOC(sessionstart, sessionend, rectangle); // Developing POC if necessary.

            double TotalTPOdouble = TotalTPO;

            // Calculate amount of TPO's in the Value Area.
            int ValueControlTPO = (int)Math.Round(TotalTPOdouble * ValueAreaPercentage_double);

            // Start with the TPO's of the Median.
            int index = (int)((PriceOfMaxRange - SessionMin) / onetick);
            if (index < 0)
                return false; // Data not yet ready.

            int TPOcount = TPOperPrice[index];

            // Go through the price levels above and below median adding the biggest to TPO count until the 70% of TPOs are inside the Value Area.
            int up_offset = 1;
            int down_offset = 1;

            while (TPOcount < ValueControlTPO)
            {
                double abovePrice = PriceOfMaxRange + up_offset * onetick;
                double belowPrice = PriceOfMaxRange - down_offset * onetick;

                // If belowPrice is out of the session's range then we should add only abovePrice's TPO's, and vice versa.
                index = (int)Math.Round((abovePrice - SessionMin) / onetick);
                int index2 = (int)Math.Round((belowPrice - SessionMin) / onetick);
                if ((belowPrice < SessionMin || TPOperPrice[index] >= TPOperPrice[index2]) && abovePrice <= SessionMax)
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

            string LastName = " " + Bars[sessionstart].OpenTime.ToString();
            string median_name = rectangle_prefix + "Median" + Suffix + LastName;

            // Delete old Median.
            Chart.RemoveObject(median_name);

            // Draw a new one.
            index = Math.Min(sessionstart + (MaxRange + 1), Bars.Count - 1);
            DateTime time_start, time_end;

            if (RightToLeft && (sessionend == Bars.Count - 1 || Session == session_period.Rectangle))
            {
                time_end = min_converted_end_time;
                time_start = converted_time;
            }
            else
            {
                time_end = Bars[index].OpenTime;
                time_start = Bars[sessionstart].OpenTime;
            }

            Color m_color = Color.FromName(MedianColor);
            int m_thickness = MedianWidth;
            LineStyle m_linestyle = MedianStyle;

            // Prominent Median (PPOC):
            if ((double)(index - sessionstart) / (double)Max_number_of_bars_in_a_session * 100 >= ProminentMedianPercentage)
            {
                m_color = ProminentMedianColor;
                m_thickness = ProminentMedianWidth;
                m_linestyle = ProminentMedianStyle;
            }

            Chart.DrawTrendLine(median_name, time_start, PriceOfMaxRange, time_end, PriceOfMaxRange, m_color, m_thickness, m_linestyle);

            string va_leftside_name = rectangle_prefix + "VA_LeftSide" + Suffix + LastName;
            Chart.RemoveObject(va_leftside_name);

            // Draw a new one.
            Chart.DrawTrendLine(va_leftside_name, time_start, PriceOfMaxRange + up_offset * onetick, time_start, PriceOfMaxRange - down_offset * onetick + onetick,
                Color.FromName(ValueAreaSidesColor), ValueAreaSidesWidth, ValueAreaSidesStyle);

            string va_rightside_name = rectangle_prefix + "VA_RightSide" + Suffix + LastName;
            Chart.RemoveObject(va_rightside_name);

            // Draw a new one.
            Chart.DrawTrendLine(va_rightside_name, time_end, PriceOfMaxRange + up_offset * onetick, time_end, PriceOfMaxRange - down_offset * onetick + onetick,
                Color.FromName(ValueAreaSidesColor), ValueAreaSidesWidth, ValueAreaSidesStyle);

            string va_top_name = rectangle_prefix + "VA_Top" + Suffix + LastName;
            Chart.RemoveObject(va_top_name);

            // Draw a new one.
            Chart.DrawTrendLine(va_top_name, time_start, PriceOfMaxRange + up_offset * onetick, time_end, PriceOfMaxRange + up_offset * onetick,
                Color.FromName(ValueAreaHighLowColor), ValueAreaHighLowWidth, ValueAreaHighLowStyle);

            string va_bottom_name = rectangle_prefix + "VA_Bottom" + Suffix + LastName;
            Chart.RemoveObject(va_bottom_name);

            // Draw a new one.
            Chart.DrawTrendLine(va_bottom_name, time_start, PriceOfMaxRange - down_offset * onetick + onetick, time_end, PriceOfMaxRange - down_offset * onetick + onetick,
                Color.FromName(ValueAreaHighLowColor), ValueAreaHighLowWidth, ValueAreaHighLowStyle);

            // VAH, VAL, and POC printout.
            if (ShowKeyValues)
            {
                cAlgo.API.HorizontalAlignment poc_ha = cAlgo.API.HorizontalAlignment.Left, va_ha = cAlgo.API.HorizontalAlignment.Left;
                VerticalAlignment poc_va = VerticalAlignment.Center, va_va = VerticalAlignment.Center;

                if (RightToLeft && (sessionend == Bars.Count - 1 || Session == session_period.Rectangle))
                {
                    time_start = time_end; // Inverting label display position.
                                           // Value Area printout position.

                    if ((Session != session_period.Rectangle && (ShowValueAreaRays == sessions_to_draw_rays.All || ShowValueAreaRays == sessions_to_draw_rays.Current || ShowValueAreaRays == sessions_to_draw_rays.PreviousCurrent)) || // For non-rectangle sessions, it is already known that it is the current session, so just check if current session uses rays.
                        (Session == session_period.Rectangle && // For rectangles, need to check which session is it and whether it has rays.
                        ((ShowValueAreaRays == sessions_to_draw_rays.AllPrevious && SessionsNumber - session_counter >= 2) ||
                        ((ShowValueAreaRays == sessions_to_draw_rays.Previous || ShowValueAreaRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - session_counter == 2) ||
                        ((ShowValueAreaRays == sessions_to_draw_rays.Current || ShowValueAreaRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - session_counter == 1) ||
                        ShowValueAreaRays == sessions_to_draw_rays.All)))
                    {
                        va_va = VerticalAlignment.Bottom;
                    }

                    // Median printout position.
                    if ((Session != session_period.Rectangle && (ShowMedianRays == sessions_to_draw_rays.All || ShowMedianRays == sessions_to_draw_rays.Current || ShowMedianRays == sessions_to_draw_rays.PreviousCurrent)) || // For non-rectangle sessions, it is already known that it is the current session, so just check if current session uses rays.
                        (Session == session_period.Rectangle && // For rectangles, need to check which session is it and whether it has rays.
                        ((ShowMedianRays == sessions_to_draw_rays.AllPrevious && SessionsNumber - session_counter >= 2) ||
                        ((ShowMedianRays == sessions_to_draw_rays.Previous || ShowMedianRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - session_counter == 2) ||
                        ((ShowMedianRays == sessions_to_draw_rays.Current || ShowMedianRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - session_counter == 1) ||
                        ShowMedianRays == sessions_to_draw_rays.All)))
                    {
                        poc_va = VerticalAlignment.Bottom;
                    }
                }

                ValuePrintOut(rectangle_prefix + "VAH" + Suffix + LastName, time_start, PriceOfMaxRange + up_offset * onetick, va_ha, va_va);
                ValuePrintOut(rectangle_prefix + "VAL" + Suffix + LastName, time_start, PriceOfMaxRange - down_offset * onetick, va_ha, va_va);
                ValuePrintOut(rectangle_prefix + "POC" + Suffix + LastName, time_start, PriceOfMaxRange, poc_ha, poc_va);
            }

            return true;
        }

        #endregion

        #region ProcessIntradaySession
        //+------------------------------------------------------------------+
        //| A cycle through intraday sessions for a given day with necessary |
        //| checks.                                                          |
        //| Returns true on success, false - on failure.                     |
        //+------------------------------------------------------------------+
        private bool ProcessIntradaySession(int sessionstart, int sessionend, int i)
        {
            // 'remember_*' vars point at day start and day end throughout this function.
            int remember_sessionstart = sessionstart;
            int remember_sessionend = sessionend;

            if (remember_sessionend >= Bars.Count)
                return false;

            // Special case stuff.
            bool ContinuePreventionFlag = false;

            // Start a cycle through intraday sessions if needed.
            // For each intraday session, find its own sessionstart and sessionend.
            int IntradaySessionCount_tmp = IntradaySessionCount;
            // If Ignore_Saturday_Sunday is on, day's start is on Monday, and there is a "22:00-06:00"-style intraday session defined, increase the counter to run the "later" "22:00-06:00" session and create this temporary dummy session.
            if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday && Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday && IntradayCrossSessionDefined > -1)
            {
                IntradaySessionCount_tmp++;
            }

            for (int intraday_i = 0; intraday_i < IntradaySessionCount_tmp; intraday_i++)
            {
                // Continue was triggered during the special case iteration.
                if (ContinuePreventionFlag)
                    break;

                // Special case iteration.
                if (intraday_i == IntradaySessionCount)
                {
                    intraday_i = IntradayCrossSessionDefined;
                    ContinuePreventionFlag = true;
                }

                Suffix = "_ID" + intraday_i.ToString();
                CurrentColorScheme = ID[intraday_i].ColorScheme;
                // Get minutes.
                Max_number_of_bars_in_a_session = ID[intraday_i].EndTime - ID[intraday_i].StartTime;
                // If end is less than beginning:
                if (Max_number_of_bars_in_a_session < 0)
                {
                    Max_number_of_bars_in_a_session = 24 * 60 + Max_number_of_bars_in_a_session;
                    if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday)
                    {
                        // Day start is on Monday. And it is not a special additional intra-Monday session.
                        if (Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday && !ContinuePreventionFlag)
                        {
                            // Cut out Sunday part.
                            Max_number_of_bars_in_a_session -= 24 * 60 - ID[intraday_i].StartTime;
                        }
                        // Day start is on Friday.
                        else if (Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Friday)
                        {
                            // Cut out Saturday part.
                            Max_number_of_bars_in_a_session -= ID[intraday_i].EndTime;
                        }
                    }
                }

                // If Append_Saturday_Sunday is on:
                if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday)
                {
                    // The intraday session starts on 00:00 or otherwise captures midnight, and remember_sessionstart points to Sunday:
                    if ((ID[intraday_i].StartTime == 0 || ID[intraday_i].StartTime > ID[intraday_i].EndTime) && Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday)
                    {
                        // Add Sunday hours.
                        Max_number_of_bars_in_a_session += 24 * 60 - (Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute);
                        // Remove the part of Sunday that has already been added before.
                        if (ID[intraday_i].StartTime > ID[intraday_i].EndTime)
                            Max_number_of_bars_in_a_session -= 24 * 60 - ID[intraday_i].StartTime;
                    }
                    // The intraday session ends on 00:00 or otherwise captures midnight, and remember_sessionstart points to Friday:
                    else if ((ID[intraday_i].EndTime == 24 * 60 || ID[intraday_i].StartTime > ID[intraday_i].EndTime) && Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Friday)
                    {
                        // Add Saturday hours. The thing is we don't know how many hours there will be on Saturday. So add to max.
                        Max_number_of_bars_in_a_session += 24 * 60;
                        // Remove the part of Saturday that has already been added before.
                        if (ID[intraday_i].StartTime > ID[intraday_i].EndTime)
                            Max_number_of_bars_in_a_session -= 24 * 60 - ID[intraday_i].EndTime;
                    }
                }

                Max_number_of_bars_in_a_session = Max_number_of_bars_in_a_session / (PeriodSeconds(TimeFrame) / 60);

                // If it is the updating stage, we need to recalculate only those intraday sessions that include the current bar.
                int hour, minute, time;
                if (FirstRunDone)
                {
                    //sessionstart = day_start;
                    hour = Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).Hour;
                    minute = Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).Minute;
                    time = hour * 60 + minute;

                    // For example, 13:00-18:00.
                    if (ID[intraday_i].StartTime < ID[intraday_i].EndTime)
                    {
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday)
                        {
                            // Skip all sessions that do not absorb Sunday session:
                            if (ID[intraday_i].StartTime != 0 && Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday)
                                continue;
                            // Skip all sessions that do not absorb Saturday session:
                            if (ID[intraday_i].EndTime != 24 * 60 && Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday)
                                continue;
                        }

                        // If Append_Saturday_Sunday is on and the session starts on 00:00, and now is either Sunday or Monday before the session's end:
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && ID[intraday_i].StartTime == 0 &&
                            (Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday ||
                            (Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday && time < ID[intraday_i].EndTime)))
                        {
                            // Then we can use remember_sessionstart as the session's start.
                            sessionstart = remember_sessionstart;
                        }
                        else if ((time < ID[intraday_i].EndTime && time >= ID[intraday_i].StartTime) ||
                                 // If Append_Saturday_Sunday is on and the session ends on 24:00, and now is Saturday, then go on in case, for example, of 18:00 Saturday time and 16:00-00:00 defined session.
                                 (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && ID[intraday_i].EndTime == 24 * 60 && Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday))
                        {
                            sessionstart = Bars.Count - 1;
                            int sessiontime = Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute;
                            while ((sessiontime > ID[intraday_i].StartTime &&
                                    // Prevents problems when the day has partial data (e.g. Sunday) when neither appending not ignoring Saturday/Sunday. Alternatively, continue looking for the sessionstart bar if we moved from Saturday to Friday with Append_Saturday_Sunday and for XX:XX-00:00 session.
                                    (Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear == Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear ||
                                    (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && ID[intraday_i].EndTime == 24 * 60 &&
                                    Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday))) ||
                                    // If Append_Saturday_Sunday is on and the session ends on 24:00 and the session start is now going through Saturday, then go on in case, for example, of 13:00 Saturday time and 16:00-00:00 defined session.
                                    (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && ID[intraday_i].EndTime == 24 * 60 &&
                                    Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday))
                            {
                                sessionstart--;
                                sessiontime = Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute;
                            }
                            // This check is necessary because sessionstart may pass to the wrong day in some cases.
                            if (sessionstart > remember_sessionstart)
                                sessionstart = remember_sessionstart;
                        }
                        else
                            continue;
                    }
                    // For example, 22:00-6:00.
                    else if (ID[intraday_i].StartTime > ID[intraday_i].EndTime)
                    {
                        // If Append_Saturday_Sunday is on and now is either Sunday or Monday before the session's end:
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday &&
                            (Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday ||
                            (Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday && time < ID[intraday_i].EndTime)))
                        {
                            // Then we can use remember_sessionstart as the session's start.
                            sessionstart = remember_sessionstart;
                        }
                        // If Ignore_Saturday_Sunday is on and it is Monday before the session's end:
                        else if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday &&
                            Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday && time < ID[intraday_i].EndTime)
                        {
                            // Then we can use remember_sessionstart as the session's start.
                            sessionstart = remember_sessionstart;
                        }
                        else if (time < ID[intraday_i].EndTime || time >= ID[intraday_i].StartTime ||
                                 // If Append_Saturday_Sunday is on and now is Saturday, then go on in case, for example, of 18:00 Saturday time and 22:00-06:00 defined session.
                                 (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday))
                        {
                            sessionstart = Bars.Count - 1;
                            int sessiontime = Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute;
                            // Within 24 hours of the current time - but can be today or yesterday.
                            while ((sessiontime > ID[intraday_i].StartTime && Bars.LastBar.OpenTime.Subtract(Bars[sessionstart].OpenTime).TotalDays <= 1) ||
                                    // Same day only.
                                    (sessiontime < ID[intraday_i].EndTime && Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear == Bars.LastBar.OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear) ||
                                    // If Append_Saturday_Sunday is on and the session start is now going through Saturday, then go on in case, for example, of 18:00 Saturday time and 22:00-06:00 defined session.
                                    (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday))
                            {
                                sessionstart--;
                                sessiontime = Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute;
                            }
                            // When the same condition in the above while cycle fails and sessionstart is one step farther than needed.
                            if (Bars.LastBar.OpenTime.Subtract(Bars[sessionstart].OpenTime).TotalDays > 1)
                                sessionstart++;
                        }
                        else
                            continue;
                    }
                    // If start time equals end time, we can skip the session.
                    else
                        continue;

                    // Because apparently, we are still inside the session.
                    sessionend = Bars.Count - 1;

                    if (!ProcessSession(sessionstart, sessionend, i, null))
                        return false;
                }
                // If it is the first run.
                else
                {
                    sessionend = remember_sessionend;

                    // Process the sessions that start today.
                    // For example, 13:00-18:00.
                    if (ID[intraday_i].StartTime < ID[intraday_i].EndTime)
                    {
                        // If Append_Saturday_Sunday is on and the session ends on 24:00, and day's start is on Friday and day's end is on Saturday, then do not trigger 'continue' in case, for example, of 15:00 Saturday end and 16:00-00:00 defined session.
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday &&
                            Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday &&
                            Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Friday)
                        {
                        }
                        // Intraday session starts after the today's actual session ended (for Friday/Saturday cases).
                        else if (Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).Minute < ID[intraday_i].StartTime)
                        {
                            continue;
                        }

                        // If Append_Saturday_Sunday is on and the session starts on 00:00, and the session end points to Sunday or end points to Monday and start points to Sunday, then do not trigger 'continue' in case, for example, of 18:00 Sunday start and 00:00-16:00 defined session.
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday &&
                            ((ID[intraday_i].StartTime == 0 && Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday) ||
                            (Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday &&
                            Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday)))
                        {
                        }
                        // Intraday session ends before the today's actual session starts (for Sunday cases).
                        else if (Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute >= ID[intraday_i].EndTime)
                        {
                            continue;
                        }

                        // If Append_Saturday_Sunday is on and the session ends on 24:00, and the start points to Friday:
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && ID[intraday_i].EndTime == 24 * 60 &&
                            Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Friday)
                        {
                            // We already have sessionend right because it is the same as remember_sessionend (end of Saturday).
                        }
                        // If Append_Saturday_Sunday is on and the session starts on 00:00 and the session end points to Sunday (it is current Sunday session , no Monday bars yet):
                        else if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday &&
                            ID[intraday_i].StartTime == 0 && Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday)
                        {
                            // We already have sessionend right because it is the same as remember_sessionend (current bar and it is on Sunday).
                        }
                        // Otherwise find the session end.
                        else
                        {
                            while (sessionend >= 0 &&
                                ((Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).Minute >= ID[intraday_i].EndTime) ||
                                (Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday && SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday)))
                            {
                                sessionend--;
                            }
                        }

                        if (sessionend == Bars.Count)
                            sessionend++;

                        // If Append_Saturday_Sunday is on and the session starts on 00:00 and the session start is now going through Sunday:
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && ID[intraday_i].StartTime == 0 &&
                            Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday)
                        {
                            // We already have sessionstart right because it is the same as remember_sessionstart (start of Sunday).
                            sessionstart = remember_sessionstart;
                        }
                        else
                        {
                            sessionstart = sessionend;

                            int _start_time = Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute;

                            while (sessionstart >= 0 &&
                                (((Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute >= ID[intraday_i].StartTime) &&
                                // Same day - for cases when the day does not contain intraday session start time. Alternatively, continue looking for the sessionstart bar if we moved from Saturday to Friday with Append_Saturday_Sunday and for XX:XX-00:00 session.
                                (Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear == Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear ||
                                (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && ID[intraday_i].EndTime == 24 * 60 && Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday))) ||
                                // If Append_Saturday_Sunday is on and the session ends on 24:00, and the session start is now going through Saturday, then go on in case, for example, of 15:00 Saturday end and 16:00-00:00 defined session.
                                (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && ID[intraday_i].EndTime == 24 * 60 && Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday)))
                            {
                                sessionstart--;
                            }

                            sessionstart++;
                        }
                    }
                    // For example, 22:00-6:00.
                    else if (ID[intraday_i].StartTime > ID[intraday_i].EndTime)
                    {
                        // If Append_Saturday_Sunday is on and the start points to Friday, then do not trigger 'continue' in case, for example, of 15:00 Saturday end and 22:00-06:00 defined session.
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday &&
                            ((Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Friday && Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday) ||
                            (Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday && Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday)))
                        {
                        }
                        // Today's intraday session starts after the end of the actual session (for Friday/Saturday cases).
                        else if (Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).Minute < ID[intraday_i].StartTime)
                            continue;

                        // If Append_Saturday_Sunday is on and the session start is on Sunday:
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday)
                        {
                            // We already have sessionstart right because it is the same as remember_sessionstart (start of Sunday).
                            sessionstart = remember_sessionstart;
                        }
                        // If Ignore_Saturday_Sunday is on and it is Monday: (and it is not a special additional intra-Monday session.)
                        else if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday &&
                            Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday && !ContinuePreventionFlag)
                        {
                            // Then we can use remember_sessionstart as the session's start.
                            sessionstart = remember_sessionstart;
                            // Monday starts on 7:00 and we have 22:00-6:00. Skip it.
                            if (Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute >= ID[intraday_i].EndTime)
                                continue;
                        }
                        else
                        {
                            // Find starting bar.
                            sessionstart = remember_sessionend; // Start from the end.
                            while (sessionstart >= 0 &&
                                ((Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).Minute >= ID[intraday_i].StartTime &&
                                // Same day - for cases when the day does not contain intraday session start time.
                                (Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear == Bars[remember_sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear ||
                                Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear == Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfYear)) ||
                                // If Append_Saturday_Sunday is on and the session start is now going through Saturday, then go on in case, for example, of 15:00 Saturday end and 22:00-06:00 defined session.
                                (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday)))
                            {
                                sessionstart--;
                            }
                            sessionstart++;
                        }

                        int sessionlength; // In seconds.
                                           // If Append_Saturday_Sunday is on and the end points to Saturday, don't go through this calculation because sessionend = remember_sessionend.
                        if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Saturday)
                        {
                            // We already have sessionend right because it is the same as remember_sessionend (end of Saturday).
                        }
                        // If Append_Saturday_Sunday is on and the start points to Sunday, use a simple method to find the end.
                        else if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday && Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Sunday)
                        {
                            // While we are on Monday and sessionend is pointing on bar after IDEndTime.
                            while (sessionend >= 0 &&
                                Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday &&
                                Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).Hour * 60 + Bars[sessionend].OpenTime.AddMinutes(TimeShiftMinutes).Minute >= ID[intraday_i].EndTime)
                            {
                                sessionend--;
                            }
                        }
                        // If Ignore_Saturday_Sunday is on and the session starts on Friday:
                        else if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday && Bars[remember_sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Friday)
                        {
                            // Then it also ends on Friday.
                            sessionend = remember_sessionend;
                        }
                        else
                        {
                            sessionend = sessionstart;
                            sessionlength = (24 * 60 - ID[intraday_i].StartTime + ID[intraday_i].EndTime) * 60;
                            // If ignoring Sundays and session start is on Monday, cut out Sunday part of the intraday session. And it is not a special additional intra-Monday session.
                            if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday &&
                                Bars[sessionstart].OpenTime.AddMinutes(TimeShiftMinutes).DayOfWeek == DayOfWeek.Monday && !ContinuePreventionFlag)
                                sessionlength -= (24 * 60 - ID[intraday_i].StartTime) * 60;
                            while (sessionend >= 0 && Bars[sessionend].OpenTime.Subtract(Bars[sessionstart].OpenTime).TotalSeconds < sessionlength)
                            {
                                sessionend++;
                            }

                            sessionend--;
                        }
                    }
                    // If start time equals end time, we can skip the session.
                    else
                    {
                        continue;
                    }

                    if (sessionend == sessionstart)
                        continue; // No need to process such an intraday session.

                    if (!ProcessSession(sessionstart, sessionend, i, null))
                        return false;
                }
            }

            Suffix = "_ID";

            return true;
        }

        #endregion

        #region CheckRays
        //+------------------------------------------------------------------+
        //| Checks whether Median/VA rays are required and whether they      |
        //| should be cut.                                                   |
        //+------------------------------------------------------------------+
        private void CheckRays()
        {
            for (int i = 0; i < RememberSession.Count; i++)
            {
                string last_name = " " + RememberSession[i].Start.ToString();
                string suffix = RememberSession[i].Suffix;
                string rec_name = "";

                if (Session == session_period.Rectangle)
                {
                    if (MPR_Array.Count <= i)
                        continue;

                    rec_name = MPR_Array[i].name + "_";
                }

                // Process Single Print Rays to hide those that shouldn't be visible.
                if (HideRaysFromInvisibleSessions && SinglePrintRays)
                {
                    foreach (var ctl in Chart.FindAllObjects<ChartTrendLine>())
                    {
                        string obj_name = ctl.Name;
                        string mpspr_prefix = rec_name + "MPSPR" + suffix + last_name;
                        if (!obj_name.StartsWith(mpspr_prefix))
                            continue; // Not a Single Print ray.

                        if (Bars[Chart.FirstVisibleBarIndex].OpenTime >= RememberSession[i].Start) // Too old.
                            ctl.Color = Color.Transparent; // Hide
                        else
                            ctl.Color = Color.FromName(SinglePrintColor); // Unhide
                    }
                }

                string median_ray_name = rec_name + "Median Ray" + suffix + last_name;
                // If the median rays have to be created for the given trading session:
                if ((ShowMedianRays == sessions_to_draw_rays.AllPrevious && SessionsNumber - i >= 2) ||
                        ((ShowMedianRays == sessions_to_draw_rays.Previous || ShowMedianRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - i == 2) ||
                        ((ShowMedianRays == sessions_to_draw_rays.Current || ShowMedianRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - i == 1) ||
                        ShowMedianRays == sessions_to_draw_rays.All)
                {
                    ChartTrendLine median_tl = Chart.FindObject(rec_name + "Median" + suffix + last_name) as ChartTrendLine;
                    if (median_tl != null)
                    {
                        double median_price = median_tl.Y1;
                        DateTime median_time = median_tl.Time2;

                        // Create the rays only if the median doesn't end behind the screen's edge.
                        if (!(HideRaysFromInvisibleSessions && Bars[Chart.FirstVisibleBarIndex].OpenTime >= median_time))
                        {
                            // Delete old Median Ray.
                            Chart.RemoveObject(median_ray_name);

                            // Draw a new Median Ray.
                            ChartTrendLine median_ray_tl = Chart.DrawTrendLine(median_ray_name, RememberSession[i].Start, median_price, median_time, median_price, MedianColor, MedianRayWidth, MedianRayStyle);
                            if (RightToLeft && i == SessionsNumber - 1 && Session != session_period.Rectangle)
                            {
                                median_ray_tl.ExtendToInfinity = false;
                            }
                            else
                            {
                                median_ray_tl.ExtendToInfinity = true;
                            }
                        }
                        else
                        {
                            Chart.RemoveObject(median_ray_name); // Delete the ray that starts from behind the screen.
                        }
                    }
                }

                // We should also delete outdated rays that no longer should be there.
                if (((ShowMedianRays == sessions_to_draw_rays.Previous || ShowMedianRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - i > 2) ||
                    (ShowMedianRays == sessions_to_draw_rays.Current && SessionsNumber - i > 1))
                {
                    Chart.RemoveObject(median_ray_name);
                }

                string va_highray_name = rec_name + "Value Area HighRay" + suffix + last_name;
                string va_lowray_name = rec_name + "Value Area LowRay" + suffix + last_name;

                // If the value area rays have to be created for the given trading session:
                if ((ShowValueAreaRays == sessions_to_draw_rays.AllPrevious && SessionsNumber - i >= 2) ||
                    ((ShowValueAreaRays == sessions_to_draw_rays.Previous || ShowValueAreaRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - i == 2) ||
                    ((ShowValueAreaRays == sessions_to_draw_rays.Current || ShowValueAreaRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - i == 1) ||
                    ShowValueAreaRays == sessions_to_draw_rays.All)
                {
                    ChartTrendLine va_top_tl = Chart.FindObject(rec_name + "VA_Top" + suffix + last_name) as ChartTrendLine;
                    ChartTrendLine va_bottom_tl = Chart.FindObject(rec_name + "VA_Bottom" + suffix + last_name) as ChartTrendLine;
                    if (va_top_tl != null && va_bottom_tl != null)
                    {
                        double va_high_price = va_top_tl.Y1;
                        double va_low_price = va_bottom_tl.Y1;
                        DateTime va_time = va_top_tl.Time2;

                        // Create the rays only if the value area doesn't end behind the screen's edge.
                        if (!(HideRaysFromInvisibleSessions && Bars[Chart.FirstVisibleBarIndex].OpenTime >= va_time))
                        {
                            // Delete old Value Area Rays.
                            Chart.RemoveObject(va_highray_name);
                            Chart.RemoveObject(va_lowray_name);

                            // Draw a new Value Area High Ray.
                            ChartTrendLine va_highray_tl = Chart.DrawTrendLine(va_highray_name, RememberSession[i].Start, va_high_price, va_time, va_high_price,
                                ValueAreaHighLowColor, ValueAreaRayHighLowWidth, ValueAreaRayHighLowStyle);

                            if (RightToLeft && i == SessionsNumber - 1 && Session != session_period.Rectangle)
                                va_highray_tl.ExtendToInfinity = false;
                            else
                                va_highray_tl.ExtendToInfinity = true;

                            // Draw a new Value Area Low Ray.
                            ChartTrendLine va_lowray_tl = Chart.DrawTrendLine(va_lowray_name, RememberSession[i].Start, va_low_price, va_time, va_low_price,
                                ValueAreaHighLowColor, ValueAreaRayHighLowWidth, ValueAreaRayHighLowStyle);

                            if (RightToLeft && i == SessionsNumber - 1 && Session != session_period.Rectangle)
                                va_lowray_tl.ExtendToInfinity = false;
                            else
                                va_lowray_tl.ExtendToInfinity = true;
                        }
                        else
                        {
                            Chart.RemoveObject(va_highray_name);
                            Chart.RemoveObject(va_lowray_name);
                        }
                    }
                }

                // We should also delete outdated rays that no longer should be there.
                if (((ShowValueAreaRays == sessions_to_draw_rays.Previous || ShowValueAreaRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - i > 2) ||
                    (ShowValueAreaRays == sessions_to_draw_rays.Current && SessionsNumber - i > 1))
                {
                    Chart.RemoveObject(va_highray_name);
                    Chart.RemoveObject(va_lowray_name);
                }

                if (RaysUntilIntersection == ways_to_stop_rays.Stop_No_Rays)
                    continue;

                if (((ShowMedianRays == sessions_to_draw_rays.Previous || ShowMedianRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - i == 2) ||
                    ((ShowMedianRays == sessions_to_draw_rays.AllPrevious || ShowMedianRays == sessions_to_draw_rays.All) && SessionsNumber - i >= 2))
                {
                    if (RaysUntilIntersection == ways_to_stop_rays.Stop_All_Rays ||
                        (RaysUntilIntersection == ways_to_stop_rays.Stop_All_Rays_Except_Prev_Session && SessionsNumber - i > 2) ||
                        (RaysUntilIntersection == ways_to_stop_rays.Stop_Only_Previous_Session && SessionsNumber - i == 2))
                        CheckRayIntersections(median_ray_name, i + 1);
                }

                if (((ShowValueAreaRays == sessions_to_draw_rays.Previous || ShowValueAreaRays == sessions_to_draw_rays.PreviousCurrent) && SessionsNumber - i == 2) ||
                    ((ShowValueAreaRays == sessions_to_draw_rays.AllPrevious || ShowValueAreaRays == sessions_to_draw_rays.All) && SessionsNumber - i >= 2))
                {
                    if (RaysUntilIntersection == ways_to_stop_rays.Stop_All_Rays ||
                        (RaysUntilIntersection == ways_to_stop_rays.Stop_All_Rays_Except_Prev_Session && SessionsNumber - i > 2) ||
                        (RaysUntilIntersection == ways_to_stop_rays.Stop_Only_Previous_Session && SessionsNumber - i == 2))
                    {
                        CheckRayIntersections(va_highray_name, i + 1);
                        CheckRayIntersections(va_lowray_name, i + 1);
                    }
                }
            }
        }

        #endregion

        #region CheckRayIntersections
        //+------------------------------------------------------------------+
        //| Checks price intersection and cuts a ray for a given object.     |
        //+------------------------------------------------------------------+
        private void CheckRayIntersections(string obj_name, int start_j)
        {
            ChartTrendLine ctl = Chart.FindObject(obj_name) as ChartTrendLine;
            if (ctl == null)
                return;

            double price = ctl.Y1;
            for (int j = start_j; j < SessionsNumber; j++) // Find the nearest intersecting session.
            {
                if (price <= RememberSession[j].Max && price >= RememberSession[j].Min)
                {
                    ctl.ExtendToInfinity = false;
                    ctl.Time2 = RememberSession[j].Start;

                    break;
                }
            }
        }

        #endregion

        #region ValuePrintOut
        //+------------------------------------------------------------------+
        //| Print out VAH, VAL, or POC value on the chart.                   |
        //+------------------------------------------------------------------+
        private void ValuePrintOut(string obj_name, DateTime time, double price, cAlgo.API.HorizontalAlignment ha, VerticalAlignment va)
        {
            ChartText text = Chart.FindObject(obj_name) as ChartText;
            // Find object if it exists.
            if (text != null)
            {
                text.Time = time;
                text.Y = price;
                text.Text = price.ToString("F" + Symbol.Digits.ToString());
            }
            else
            {
                text = Chart.DrawText(obj_name, price.ToString("F" + Symbol.Digits.ToString()), time, price, Color.FromName(KeyValuesColor));
                text.FontSize = KeyValuesSize;
                text.HorizontalAlignment = ha;
                text.VerticalAlignment = va;
            }
        }

        #endregion

        #region PutSinglePrintMark

        private void PutSinglePrintMark(double price, int sessionstart, string rectangle_prefix)
        {
            int t1 = sessionstart, t2 = sessionstart - 1;
            bool fill = true;

            if (ShowSinglePrint == single_print_type.Rightside)
            {
                t1 = sessionstart + 1;
                t2 = sessionstart;
                fill = false;
            }
            string LastNameStart = " " + Bars[t1].OpenTime.ToString() + " ";
            string LastName = LastNameStart + price.ToString("F" + Symbol.Digits.ToString());

            string mpsp_name = rectangle_prefix + "MPSP" + Suffix + LastName;
            ChartRectangle mpsp_r = Chart.FindObject(mpsp_name) as ChartRectangle;

            // If already there - ignore.
            if (mpsp_r != null)
                return;

            mpsp_r = Chart.DrawRectangle(mpsp_name, Bars[t1].OpenTime, price, Bars[t2].OpenTime, price - onetick, Color.FromName(SinglePrintColor));
            if (fill)
                mpsp_r.IsFilled = true;
        }

        #endregion

        #region RemoveSinglePrintMark

        private void RemoveSinglePrintMark(double price, int sessionstart, string rectangle_prefix)
        {
            int t = sessionstart + 1;
            if (ShowSinglePrint == single_print_type.Rightside)
                t = sessionstart;

            string LastNameStart = " " + Bars[t].OpenTime.ToString() + " ";
            string LastName = LastNameStart + price.ToString("F" + Symbol.Digits.ToString());

            Chart.RemoveObject(rectangle_prefix + "MPSP" + Suffix + LastName);
        }

        #endregion

        #region PutSinglePrintRay

        private void PutSinglePrintRay(double price, int sessionstart, string rectangle_prefix, Color spr_color)
        {
            DateTime t1 = Bars[sessionstart].OpenTime, t2;
            if (sessionstart - 1 >= 0)
                t2 = Bars[sessionstart + 1].OpenTime;
            else
                t2 = Bars[sessionstart].OpenTime.AddSeconds(1);

            if (ShowSinglePrint == single_print_type.Rightside)
            {
                t1 = Bars[sessionstart].OpenTime;
                t2 = Bars[sessionstart - 1].OpenTime;
            }

            string LastNameStart = " " + t1.ToString() + " ";
            string LastName = LastNameStart + price.ToString("F" + Symbol.Digits.ToString());

            string mpspr_name = rectangle_prefix + "MPSPR" + Suffix + LastName;
            ChartTrendLine mpspr_tl = Chart.FindObject(mpspr_name) as ChartTrendLine;

            // If already there - ignore.
            if (mpspr_tl != null)
                return;

            mpspr_tl = Chart.DrawTrendLine(mpspr_name, t1, price, t2, price, spr_color, SinglePrintRayWidth, SinglePrintRayStyle);
            mpspr_tl.ExtendToInfinity = true;
        }

        #endregion

        #region RemoveSinglePrintRay

        private void RemoveSinglePrintRay(double price, int sessionstart, string rectangle_prefix)
        {
            DateTime t = Bars[sessionstart].OpenTime;

            string LastNameStart = " " + t.ToString() + " ";
            string LastName = LastNameStart + price.ToString("F" + Symbol.Digits.ToString());

            Chart.RemoveObject(rectangle_prefix + "MPSPR" + Suffix + LastName);
        }

        #endregion

        #region RedrawLastSession
        // Called only when RightToLeft is on to update the right-most session.
        private void RedrawLastSession()
        {
            if (SeamlessScrollingMode)
            {
                int last_visible_bar = Chart.LastVisibleBarIndex;
                StartDate = Bars[last_visible_bar].OpenTime;
            }
            else if (StartFromCurrentSession)
            {
                StartDate = Bars.LastBar.OpenTime;
            }
            else
            {
                if (!DateTime.TryParse(StartFromDate, out StartDate))
                    StartDate = DateTime.Now.Date;
            }

            // Get start and end bar numbers of the given session.
            int sessionend = FindSessionEndByDate(StartDate);
            int sessionstart = FindSessionStart(sessionend);
            if (sessionstart == -1)
            {
                Print("Something went wrong! Waiting for data to load.");
                return;
            }

            int SessionToStart = 0;
            // In all cases except for the seamless scrolling mode, jump to the latest session.
            if (!SeamlessScrollingMode)
                SessionToStart = _SessionsToCount - 1;
            else
            {
                // Move back to the oldest session to count to start from it.
                for (int i = 1; i < _SessionsToCount; i++)
                {
                    sessionend = sessionstart - 1;
                    if (sessionend < 0)
                        return;

                    if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday)
                    {
                        // Pass through Sunday and Saturday.
                        while (Bars[sessionend].OpenTime.DayOfWeek == DayOfWeek.Sunday || Bars[sessionend].OpenTime.DayOfWeek == DayOfWeek.Saturday)
                        {
                            sessionend--;
                            if (sessionend < 0)
                                break;
                        }
                    }

                    sessionstart = FindSessionStart(sessionend);
                }
            }

            // We begin from the oldest session coming to the current session or to StartFromDate.
            for (int i = SessionToStart; i < _SessionsToCount; i++)
            {
                if (Session == session_period.Intraday)
                {
                    if (!ProcessIntradaySession(sessionstart, sessionend, i))
                        return;
                }
                else
                {
                    if (Session == session_period.Daily)
                        Max_number_of_bars_in_a_session = PeriodSeconds(TimeFrame.Daily) / PeriodSeconds(TimeFrame);
                    else if (Session == session_period.Weekly)
                        Max_number_of_bars_in_a_session = PeriodSeconds(TimeFrame.Weekly) / PeriodSeconds(TimeFrame);
                    else if (Session == session_period.Monthly)
                        Max_number_of_bars_in_a_session = PeriodSeconds(TimeFrame.Monthly) / PeriodSeconds(TimeFrame);

                    if (SaturdaySunday == sat_sun_solution.Append_Saturday_Sunday)
                    {
                        // The start is on Sunday - add remaining time.
                        if (Bars[sessionstart].OpenTime.DayOfWeek == DayOfWeek.Sunday)
                            Max_number_of_bars_in_a_session += (24 * 3600 - (Bars[sessionstart].OpenTime.Hour * 3600 + Bars[sessionstart].OpenTime.Minute * 60)) / PeriodSeconds(TimeFrame);
                        // The end is on Saturday. +1 because even 0:00 bar deserves a bar.
                        if (Bars[sessionstart].OpenTime.DayOfWeek == DayOfWeek.Saturday)
                            Max_number_of_bars_in_a_session += (Bars[sessionstart].OpenTime.Hour * 3600 + Bars[sessionstart].OpenTime.Minute) / PeriodSeconds(TimeFrame) + 1;
                    }

                    if (!ProcessSession(sessionstart, sessionend, i, null))
                        return;
                }
                // Go to the newer session only if there is one or more left.
                if (_SessionsToCount - i > 1)
                {
                    sessionstart = sessionend + 1;
                    if (SaturdaySunday == sat_sun_solution.Ignore_Saturday_Sunday)
                    {
                        // Pass through Sunday and Saturday.
                        while (Bars[sessionstart].OpenTime.DayOfWeek == DayOfWeek.Sunday || Bars[sessionstart].OpenTime.DayOfWeek == DayOfWeek.Saturday)
                        {
                            sessionstart++;
                            if (sessionstart == Bars.Count - 1)
                                break;
                        }
                    }

                    sessionend = FindSessionEndByDate(Bars[sessionstart].OpenTime);
                }
            }

            if (ShowValueAreaRays != sessions_to_draw_rays.None || ShowMedianRays != sessions_to_draw_rays.None)
                CheckRays();
        }

        #endregion

        #region CalculateDevelopingPOC
        //+------------------------------------------------------------------+
        //| Go through all prices on all N session bars from 1st to kth bar, |
        //| where k = 1..N.                                                  |
        //+------------------------------------------------------------------+
        private void CalculateDevelopingPOC(int sessionstart, int sessionend, CRectangleMP rectangle)
        {
            // Cycle through all possible end bars to calculate the Developing POC.
            for (int max_bar = sessionstart; max_bar <= sessionend; max_bar++)
            {
                if (!double.IsNaN(DevelopingPOC_1[max_bar]) || !double.IsNaN(DevelopingPOC_2[max_bar]) && max_bar > 1)
                    continue; // One of the buffers already filled and it isn't/wasn't the latest bar - skip. Valid only for non-Rectangle sessions.

                // Determine the local price minimum and maximum.
                double LocalMin = Bars[GetLowestLowIdx(sessionstart, max_bar)].Low;
                double LocalMax = Bars[GetHighestHighIdx(sessionstart, max_bar)].High;

                // For rectangles, further restrictions may apply.
                if (Session == session_period.Rectangle)
                {
                    if (LocalMax > rectangle.RectanglePriceMax)
                        LocalMax = Math.Round(rectangle.RectanglePriceMax, DigitsM);
                    if (LocalMin < rectangle.RectanglePriceMin)
                        LocalMin = Math.Round(rectangle.RectanglePriceMin, DigitsM);
                }

                double DistanceToCenter = double.MaxValue; // Reset the distance because each piece of the Developing POC should be using its own.
                int DevMaxRange = 0; // Maximum range for the Developing POC.
                double PriceOfMaxRange = double.NaN;

                // Cycle by price inside the local boundaries:
                for (double price = LocalMax; price >= LocalMin; price -= onetick)
                {
                    price = Math.Round(price, DigitsM);
                    int range = 0; // Distance from first bar to the current bar.
                                   // Going through all bars of the session until the current max_bar to see if the price was encountered here.

                    for (int bar = sessionstart; bar <= max_bar; bar++)
                    {
                        // Price is encountered in the given bar.
                        if (price >= Bars[bar].Low && price <= Bars[bar].High)
                        {
                            // Update maximum distance from session's start to the found bar for the Developing POC.
                            // Using the center-most POC if there are more than one.
                            if (DevMaxRange < range || (DevMaxRange == range && Math.Abs(price - (LocalMin + (LocalMax - LocalMin) / 2)) < DistanceToCenter)) //SessionMax and SessionMin should be replaced with current N bars' max High and min Low.
                            {
                                DevMaxRange = range;
                                PriceOfMaxRange = price;
                                DistanceToCenter = Math.Abs(price - (LocalMin + (LocalMax - LocalMin) / 2));
                            }
                            range++;
                        }
                    }
                }

                // Both buffer are empty:
                if (double.IsNaN(DevelopingPOC_1[max_bar - 1]) && double.IsNaN(DevelopingPOC_2[max_bar - 1]))
                {
                    DevelopingPOC_1[max_bar] = PriceOfMaxRange; // Starting with the first one.
                    DevelopingPOC_2[max_bar] = double.NaN; // The second is initialized to an empty value.
                }
                // Buffer #1 already had a value,
                else if (!double.IsNaN(DevelopingPOC_1[max_bar - 1]))
                {
                    // and it is different from what we get now.
                    if (DevelopingPOC_1[max_bar - 1] != PriceOfMaxRange)
                    {
                        DevelopingPOC_2[max_bar] = PriceOfMaxRange; // Use new buffer to get an interrupted shift of lines.
                        DevelopingPOC_1[max_bar] = double.NaN;
                    }
                    else // and it is the same price:
                    {
                        DevelopingPOC_1[max_bar] = PriceOfMaxRange; // Use the same buffer.
                        DevelopingPOC_2[max_bar] = double.NaN;
                    }
                }
                // Buffer #2 already had a value,
                else
                {
                    // and it is different from what we get now.
                    if (DevelopingPOC_2[max_bar - 1] != PriceOfMaxRange)
                    {
                        DevelopingPOC_1[max_bar] = PriceOfMaxRange; // Use new buffer to get an interrupted shift of lines.
                        DevelopingPOC_2[max_bar] = double.NaN;
                    }
                    else // and it is the same price:
                    {
                        DevelopingPOC_2[max_bar] = PriceOfMaxRange; // Use the same buffer.
                        DevelopingPOC_1[max_bar] = double.NaN;
                    }
                }
            }
        }

        #endregion

        #region CheckAlerts
        //+------------------------------------------------------------------+
        //| Checks all alert conditions and issues alerts if needed.         |
        //+------------------------------------------------------------------+
        private void CheckAlerts(int index)
        {
            // No need to check further if no alert method is chosen.
            if (!AlertNative && !AlertEmail)// && !AlertPush)
                return;

            // Skip alerts if alerts are disabled for Median, for Value Area, and for Single Print rays.
            if (!AlertForMedian && !AlertForValueArea && !AlertForSinglePrint)
                return;

            // Skip alerts if no cross type is chosen.
            if (!AlertOnPriceBreak && !AlertOnCandleClose && !AlertOnGapCross)
                return;

            // Skip alerts if only closed bar should be checked and it has already been done.
            if (AlertCheckBar == alert_check_bar.CheckPreviousBar && LastAlertTime == Bars[index].OpenTime) return;

            // Cycle through rays starts here.
            foreach (var ctl in Chart.FindAllObjects<ChartTrendLine>())
            {
                string object_name = ctl.Name;

                // Skip if it is either a non-ray or if this particular ray shouldn't get alerted.
                if (!(AlertForMedian && object_name.Contains("Median Ray") ||
                    (AlertForValueArea && (object_name.Contains("Value Area HighRay") || object_name.Contains("Value Area LowRay"))) ||
                    (AlertForSinglePrint && object_name.Contains("MPSPR") && ctl.Color != Color.Transparent)))
                    continue;

                // If everything is fine, go on:

                double level = ctl.Y1; //NormalizeDouble(ObjectGetDouble(ChartID(), object_name, OBJPROP_PRICE1), _Digits);

                // Price breaks, candle closes, and gap crosses using Close[0].
                if (AlertCheckBar == alert_check_bar.CheckCurrentBar)
                {
                    if (AlertOnPriceBreak) // Price break alerts.
                    {
                        if (!double.IsNaN(Close_prev) && ((Bars[index].Close >= level && Close_prev < level) || (Bars[index].Close <= level && Close_prev > level)))
                        {
                            DoAlerts(alert_types.PriceBreak, object_name);
                            ArrowsPB[index] = Bars[index].Close;
                        }
                        else
                            ArrowsPB[0] = double.NaN;
                        Close_prev = Bars[index].Close;
                    }

                    if (AlertOnCandleClose) // Candle close alerts.
                    {
                        if ((Bars[index].Close >= level && Bars[index - 1].Close < level) || (Bars[index].Close <= level && Bars[index - 1].Close > level))
                        {
                            DoAlerts(alert_types.CandleCloseCrossover, object_name);
                            ArrowsCC[index] = Bars[index].Close;
                        }
                        else
                            ArrowsCC[index] = double.NaN;
                    }

                    if (AlertOnGapCross) // Gap cross alerts.
                    {
                        if ((Bars[index].Open > level && Bars[index - 1].High < level) || (Bars[index].Open < level && Bars[index - 1].Low > level))
                        {
                            DoAlerts(alert_types.GapCrossover, object_name);
                            ArrowsGC[index] = level;
                        }
                        else
                            ArrowsGC[index] = double.NaN;
                    }
                }
                // Price breaks (using pre-previous High and previous Close), candle closes, and gap crosses using Close[1].
                else if (AlertCheckBar == alert_check_bar.CheckPreviousBar)
                {
                    if (AlertOnPriceBreak) // Price break alerts.
                    {
                        if ((Bars[index - 1].High >= level && Bars[index - 1].Close < level && Bars[index - 2].Close < level) ||
                            (Bars[index - 1].Low <= level && Bars[index - 1].Close > level && Bars[index - 2].Close > level))
                        {
                            DoAlerts(alert_types.PriceBreak, object_name);
                            ArrowsPB[index - 1] = Bars[index - 1].Close;
                        }
                    }

                    if (AlertOnCandleClose) // Candle close alerts.
                    {
                        if ((Bars[index - 1].Close >= level && Bars[index - 2].Close < level) || (Bars[index - 1].Close <= level && Bars[index - 2].Close > level))
                        {
                            DoAlerts(alert_types.CandleCloseCrossover, object_name);
                            ArrowsCC[index - 1] = Bars[index - 1].Close;
                        }
                    }

                    if (AlertOnGapCross) // Gap cross alerts.
                    {
                        if ((Bars[index - 1].Low > level && Bars[index - 2].High < level) || (Bars[index - 2].Low > level && Bars[index - 1].High < level))
                        {
                            DoAlerts(alert_types.GapCrossover, object_name);
                            ArrowsGC[index - 1] = level;
                        }
                    }

                    LastAlertTime = Bars[index].OpenTime;
                }
            }
        }

        //+------------------------------------------------------------------+
        //| Issues alerts based on the alert type and includes object name   |
        //| in the message.                                                  |
        //+------------------------------------------------------------------+
        private void DoAlerts(alert_types alert_type, string object_name)
        {
            // Price Breaks for Current Bar should not be be checked for LastAlertTime.
            // Candle Close and Gap Cross for Current Bar need to be checked against LastAlertTime.
            // All CheckPreviousBar alerts can use a single LastAlertTime (they either trigger at the start of the bar or not). The actual check is performed in CheckAlerts().
            // Using TimeCurrent() for all CheckCurrentBar alerts.
            // Using Time[0] for all CheckPreviousBar alerts.

            // Check last alert time for Candle Close alert type.
            if (alert_type == alert_types.CandleCloseCrossover && AlertCheckBar == alert_check_bar.CheckCurrentBar && Time <= LastAlertTime_CandleCross)
                return;

            // Check last alert time for Gap Cross alert type.
            if (alert_type == alert_types.GapCrossover && AlertCheckBar == alert_check_bar.CheckCurrentBar && Time <= LastAlertTime_GapCross)
                return;

            string Subject = "Market Profile: " + Symbol.Name + " " + alert_type.ToString() + " on " + object_name;

            if (AlertNative)
            {
                string AlertText = Subject;
                Alert(AlertText);
            }

            if (AlertEmail)
            {
                string EmailSubject = Subject;
                string EmailBody = Account.BrokerName + " - " + Account.Number.ToString() + "\r\n\r\n" + Subject;

                Notifications.SendEmail(AlertEmailFrom, AlertEmailTo, EmailSubject, EmailBody);
            }

            // Remember that this alert has already been sent. For CheckPreviousBar, this is done in CheckAlerts().
            if (alert_type == alert_types.CandleCloseCrossover && AlertCheckBar == alert_check_bar.CheckCurrentBar)
                LastAlertTime_CandleCross = Time;
            else if (alert_type == alert_types.GapCrossover && AlertCheckBar == alert_check_bar.CheckCurrentBar)
                LastAlertTime_GapCross = Time;
        }

        private void Alert(string alert_text)
        {
            MessageBox.Show(alert_text, "Alert", MessageBoxButtons.OK);

            if(File.Exists(AlertNativeSoundFile))
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(AlertNativeSoundFile);
                player.Play();
            }
        }

        #endregion

        #endregion

        #region Helpers

        //+------------------------------------------------------------------+
        //| Check if two dates are in the same week.                         |
        //+------------------------------------------------------------------+
        private bool SameWeek(DateTime date1, DateTime date2)
        {
            int seconds_from_start = (int)date1.DayOfWeek * 24 * 3600 + date1.Hour * 3600 + date1.Minute * 60 + date1.Second;
            int _date1 = (int)date1.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            int _date2 = (int)date2.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            if (date1 == date2)
                return true;
            else if (date2 < date1)
            {
                if (_date1 - _date2 <= seconds_from_start)
                    return true;
            }
            // 604800 - seconds in one week.
            else if (_date2 - _date1 < 604800 - seconds_from_start)
                return true;

            return false;
        }

        //+------------------------------------------------------------------+
        //| Check if two dates are in the same month.                        |
        //+------------------------------------------------------------------+
        private bool SameMonth(DateTime date1, DateTime date2)
        {
            return (date1.Month == date2.Month && date1.Year == date2.Year);
        }

        //+------------------------------------------------------------------+
        //| Returns absolute day number.                                     |
        //+------------------------------------------------------------------+
        private int TimeAbsoluteDay(DateTime time)
        {
            return ((int)time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds / 86400);
        }

        private int PeriodSeconds(TimeFrame tf)
        {
            TimeSpan period = new TimeSpan(1, 0, 0, 0);

            if (tf == TimeFrame.Minute)
                period = new TimeSpan(0, 0, 1, 0);

            if (tf == TimeFrame.Minute2)
                period = new TimeSpan(0, 0, 2, 0);

            if (tf == TimeFrame.Minute3)
                period = new TimeSpan(0, 0, 3, 0);

            if (tf == TimeFrame.Minute4)
                period = new TimeSpan(0, 0, 4, 0);

            if (tf == TimeFrame.Minute5)
                period = new TimeSpan(0, 0, 5, 0);

            if (tf == TimeFrame.Minute6)
                period = new TimeSpan(0, 0, 6, 0);

            if (tf == TimeFrame.Minute7)
                period = new TimeSpan(0, 0, 7, 0);

            if (tf == TimeFrame.Minute8)
                period = new TimeSpan(0, 0, 8, 0);

            if (tf == TimeFrame.Minute9)
                period = new TimeSpan(0, 0, 9, 0);

            if (tf == TimeFrame.Minute10)
                period = new TimeSpan(0, 0, 10, 0);

            if (tf == TimeFrame.Minute15)
                period = new TimeSpan(0, 0, 15, 0);

            if (tf == TimeFrame.Minute20)
                period = new TimeSpan(0, 0, 20, 0);

            if (tf == TimeFrame.Minute30)
                period = new TimeSpan(0, 0, 30, 0);

            if (tf == TimeFrame.Minute45)
                period = new TimeSpan(0, 0, 45, 0);

            if (tf == TimeFrame.Hour)
                period = new TimeSpan(0, 1, 0, 0);

            if (tf == TimeFrame.Hour2)
                period = new TimeSpan(0, 2, 0, 0);

            if (tf == TimeFrame.Hour3)
                period = new TimeSpan(0, 3, 0, 0);

            if (tf == TimeFrame.Hour4)
                period = new TimeSpan(0, 4, 0, 0);

            if (tf == TimeFrame.Hour6)
                period = new TimeSpan(0, 6, 0, 0);

            if (tf == TimeFrame.Hour8)
                period = new TimeSpan(0, 8, 0, 0);

            if (tf == TimeFrame.Hour12)
                period = new TimeSpan(0, 12, 0, 0);

            if (tf == TimeFrame.Daily)
                period = new TimeSpan(1, 0, 0, 0);

            if (tf == TimeFrame.Day2)
                period = new TimeSpan(2, 0, 0, 0);

            if (tf == TimeFrame.Day3)
                period = new TimeSpan(3, 0, 0, 0);

            if (tf == TimeFrame.Weekly)
                period = new TimeSpan(7, 0, 0, 0);

            if (tf == TimeFrame.Monthly)
                period = new TimeSpan(30, 0, 0, 0);


            return ((int)period.TotalSeconds);
        }

        private int GetHighestHighIdx(int start_idx, int end_idx)
        {
            double hh = double.MinValue;
            int result = start_idx;
            for (int i = start_idx; i <= end_idx; i++)
            {
                if (hh < Bars[i].High)
                {
                    hh = Bars[i].High;
                    result = i;
                }
            }

            return (result);
        }

        private int GetLowestLowIdx(int start_idx, int end_idx)
        {
            double ll = double.MaxValue;
            int result = start_idx;
            for (int i = start_idx; i <= end_idx; i++)
            {
                if (ll > Bars[i].Low)
                {
                    ll = Bars[i].Low;
                    result = i;
                }
            }

            return (result);
        }
        #endregion
    }
}