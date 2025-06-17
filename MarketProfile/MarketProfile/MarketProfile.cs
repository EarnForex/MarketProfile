// -------------------------------------------------------------------------------
//   
// Displays the Market Profile indicator for intraday, daily, weekly, monthly, quarterly, semiannual, annual, and free-form trading sessions.
// Daily - should be attached to M5-M30 timeframes. M30 is recommended.
// Weekly - should be attached to M30-H4 timeframes. H1 is recommended.
// Weeks start on Sunday.
// Monthly - should be attached to H1-D1 timeframes. H4 is recommended.
// Quarterly - should be attached to H4-D1 timeframes. D1 is recommended.
// Semiannual - should be attached to H4-W1 timeframes. D1 is recommended.
// Annual - should be attached to H4-W1 timeframes. D1 is recommended.
// Intraday - should be attached to M1-M30 timeframes. M5 is recommended.
// Designed for major currency pairs, but should work also with exotic pairs, CFDs, or commodities.
//   
// Version 1.24
// Copyright 2010-2025, EarnForex.com
// https://www.earnforex.com/indicators/MarketProfile/
// -------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using static cAlgo.Helpers;

namespace cAlgo;

[Indicator(AccessRights = AccessRights.FullAccess, IsOverlay = true)]
public class MarketProfile : Indicator,
    IMarketProfileResources,
    IIndicatorResources,
    IMarketProfileRendererSettings,
    ISessionChangeManagerResources,
    IHotkeyStateManagerResources,
    ISeamlessScrollingManagerResources,
    IAlertManagerResources,
    IOutputDevelopingValuesManagerResources,
    IHideRaysFromInvisibleSessionsFeatureResources,
    IRenderingModesResources,
    INewHighLowManagerResources,
    IRectangleModeProcessorResources
{
    #region Parameters
    
    #region Main
    
    [Parameter("Session", DefaultValue = SessionPeriod.Daily, Group = "Main")]
    public SessionPeriod InputSession { get; set; }
    
    [Parameter("Start From Date: Lower Priority.", DefaultValue = "", Group = "Main")]
    public string InputStartFromDate { get; set; }
    
    [Parameter("Start From Current Session: Higher Priority.", DefaultValue = true, Group = "Main")]
    public bool InputStartFromCurrentSession { get; set; }
    
    [Parameter("Sessions To Count: Number of sessions to count Market Profile.", DefaultValue = 2, Group = "Main")]
    public int InputSessionsToCount { get; set; }
    
    //[Parameter("Output Developing Sessions To Count (0 == all).", DefaultValue = 10, Group = "Main")]
    public int InputOutputDevelopingSessionsToCount => InputSessionsToCount;
    
    //
    [Parameter("Seamless Scrolling Mode: Show Sessions on Current Screen.", DefaultValue = false, Group = "Main")]
    public bool InputSeamlessScrollingMode { get; set; }
    
    [Parameter("Enable Developing POC.", DefaultValue = false, Group = "Main")]
    public bool InputEnableDevelopingPoC { get; set; }
    
    [Parameter("Enable Developing VAH/VAL.", DefaultValue = false, Group = "Main")]
    public bool InputEnableDevelopingValueAtHighValueAtLow { get; set; }
    
    [Parameter("ValueAreaPercentage: Percentage of TPO's inside Value Area.", DefaultValue = 70, Group = "Main")]
    public int InputValueAreaPercentage { get; set; }
    
    #endregion
    
    #region ColorsAndLooks
    
    [Parameter("Start Color", DefaultValue = "#990000FF", Group = "Colors and Looks")]
    public Color InputStartColor { get; set; }
    
    [Parameter("End Color", DefaultValue = "#99FF0000", Group = "Colors and Looks")]
    public Color InputEndColor { get; set; }

    [Parameter("ColorBullBear: If true, colors are from bars' direction.", DefaultValue = false, Group = "Colors and Looks")]
    public bool InputColorBullBear { get; set; }
    
    [Parameter("MedianColor", DefaultValue = "White", Group = "Colors and Looks")]
    public Color InputMedianColor { get; set; }
    
    [Parameter("Value Area Sides Color", DefaultValue = "White", Group = "Colors and Looks")]
    public Color InputValueAreaSidesColor { get; set; }
    
    [Parameter("Value Area High Low Color", DefaultValue = "White", Group = "Colors and Looks")]
    public Color InputValueAreaHighLowColor { get; set; }
    
    [Parameter("Median Style", DefaultValue = LineStyle.Solid, Group = "Colors and Looks")]
    public LineStyle InputMedianStyle { get; set; }
    
    [Parameter("Median Ray Style", DefaultValue = LineStyle.Lines, Group = "Colors and Looks")]
    public LineStyle InputMedianRayStyle { get; set; }
    
    [Parameter("Value Area Sides Style", DefaultValue = LineStyle.Solid, Group = "Colors and Looks")]
    public LineStyle InputValueAreaSidesStyle { get; set; }
    
    [Parameter("Value Area High Low Style", DefaultValue = LineStyle.Solid, Group = "Colors and Looks")]
    public LineStyle InputValueAreaHighLowStyle { get; set; }
    
    [Parameter("Value Area Ray High Low Style", DefaultValue = LineStyle.Dots, Group = "Colors and Looks")]
    public LineStyle InputValueAreaRayHighLowStyle { get; set; }
    
    [Parameter("Median Width", DefaultValue = 1, Group = "Colors and Looks")]
    public int InputMedianWidth { get; set; }
    
    [Parameter("Median Ray Width", DefaultValue = 1, Group = "Colors and Looks")]
    public int InputMedianRayWidth { get; set; }
    
    [Parameter("Value Area Sides Width", DefaultValue = 1, Group = "Colors and Looks")]
    public int InputValueAreaSidesWidth { get; set; }
    
    [Parameter("Value Area High Low Width", DefaultValue = 1, Group = "Colors and Looks")]
    public int InputValueAreaHighLowWidth { get; set; }
    
    [Parameter("Value Area Ray High Low Width", DefaultValue = 1, Group = "Colors and Looks")]
    public int InputValueAreaRayHighLowWidth { get; set; }
    
    [Parameter("Show Value Area Rays: draw previous value area high/low rays.", DefaultValue = SessionsToDrawRays.None, Group = "Colors and Looks")]
    public SessionsToDrawRays InputShowValueAreaRays { get; set; }
    
    [Parameter("Show Median Rays: draw previous median rays.", DefaultValue = SessionsToDrawRays.None, Group = "Colors and Looks")]
    public SessionsToDrawRays InputShowMedianRays { get; set; }
    
    [Parameter("Rays Until Intersection: which rays stop when hit another MP.", DefaultValue = WaysToStopRays.StopNoRays, Group = "Colors and Looks")]
    public WaysToStopRays InputRaysUntilIntersection { get; set; }
    
    [Parameter("Hide Rays From Invisible Sessions: hide rays from behind the screen.", DefaultValue = false, Group = "Colors and Looks")]
    public bool InputHideRaysFromInvisibleSessions { get; set; }
    
    [Parameter("Time Shift Minutes: shift session + to the left, - to the right.", DefaultValue = 0, Group = "Colors and Looks")]
    public int InputTimeShiftMinutes { get; set; }
    
    [Parameter("Show Key Values: print out VAH, VAL, POC on chart.", DefaultValue = true, Group = "Colors and Looks")]
    public bool InputShowKeyValues { get; set; }
    
    [Parameter("Key Values Color: color for VAH, VAL, POC printout.", DefaultValue = "White", Group = "Colors and Looks")]
    public Color InputKeyValuesColor { get; set; }
    
    [Parameter("Key Values Size: font size for VAH, VAL, POC printout.", DefaultValue = 12, Group = "Colors and Looks")]
    public int InputKeyValuesSize { get; set; }
    
    [Parameter("Show Single Print: mark Single Print profile levels.", DefaultValue = SinglePrintType.No, Group = "Colors and Looks")]
    public SinglePrintType InputShowSinglePrint { get; set; }
    
    [Parameter("Single Print Color", DefaultValue = "Gold", Group = "Colors and Looks")]
    public Color InputSinglePrintColor { get; set; }
    
    [Parameter("Single Print Rays: mark Single Print edges with rays.", DefaultValue = false, Group = "Colors and Looks")]
    public bool InputSinglePrintRays { get; set; }
    
    [Parameter("Single Print Ray Style", DefaultValue = LineStyle.Solid, Group = "Colors and Looks")]
    public LineStyle InputSinglePrintRayStyle { get; set; }
    
    [Parameter("Single Print Ray Width", DefaultValue = 1, Group = "Colors and Looks")]
    public int InputSinglePrintRayWidth { get; set; }
    
    [Parameter("Prominent Median Color", DefaultValue = "Yellow", Group = "Colors and Looks")]
    public Color InputProminentMedianColor { get; set; }
    
    [Parameter("Prominent Median Style", DefaultValue = LineStyle.Solid, Group = "Colors and Looks")]
    public LineStyle InputProminentMedianStyle { get; set; }
    
    [Parameter("Prominent Median Width", DefaultValue = 4, Group = "Colors and Looks")]
    public int InputProminentMedianWidth { get; set; }
    
    [Parameter("Show TPO Counts", DefaultValue = false, Group = "Colors and Looks")]
    public bool InputShowTpoCounts { get; set; }
    
    [Parameter("TPO Count Above Color", DefaultValue = "Honeydew", Group = "Colors and Looks")]
    public Color InputTpoCountAboveColor { get; set; }
    
    [Parameter("TPO Count Below Color", DefaultValue = "MistyRose", Group = "Colors and Looks")]
    public Color InputTpoCountBelowColor { get; set; }
    
    [Parameter("Right To Left: Draw histogram from right to left.", DefaultValue = false, Group = "Colors and Looks")]
    public bool InputRightToLeft { get; set; }
    
    #endregion
    
    #region Performance
    
    [Parameter("Point Multiplier: higher value = fewer objects. 0 - adaptive.", DefaultValue = 0, MinValue = 0, Step = 10, Group = "Performance")]
    public int InputPointMultiplier { get; set; }
    
    [Parameter("Draw Only Histogram Border", DefaultValue = false, Group = "Performance")]
    public bool InputDrawOnlyHistogramBorder { get; set; }
    
    [Parameter("Disable Histogram: do not draw profile, VAH, VAL, and POC still visible.", DefaultValue = false, Group = "Performance")]
    public bool InputDisableHistogram { get; set; }
    
    #endregion
    
    #region Alerts
    
    [Parameter("Alert Native: issue native pop-up alerts.", DefaultValue = false, Group = "Alerts")]
    public bool InputAlertNative { get; set; }
    
    [Parameter("Alert Native: sound type.", DefaultValue = SoundType.Announcement, Group = "Alerts")]
    public SoundType InputAlertSoundType { get; set; }
    
    [Parameter("Alert Email: issue email alerts.", DefaultValue = false, Group = "Alerts")]
    public bool InputAlertEmail { get; set; }
    
    [Parameter("Alert Email: Email From.", DefaultValue = "", Group = "Alerts")]
    public string InputAlertEmailFrom { get; set; }
    
    [Parameter("Alert Email: Email To.", DefaultValue = "", Group = "Alerts")]
    public string InputAlertEmailTo { get; set; }
    
    [Parameter("Alert Arrows: draw chart arrows on alerts.", DefaultValue = false, Group = "Alerts")]
    public bool InputAlertArrows { get; set; }
    
    [Parameter("Alert Check Bar: which bar to check for alerts?", DefaultValue = AlertCheckBar.CheckPreviousBar, Group = "Alerts")]
    public AlertCheckBar InputAlertCheckBar { get; set; }
    
    [Parameter("Alert For Value Area: alerts for Value Area (VAH, VAL) rays.", DefaultValue = false, Group = "Alerts")]
    public bool InputAlertForValueArea { get; set; }
    
    [Parameter("Alert For Median: alerts for POC (Median) rays' crossing.", DefaultValue = false, Group = "Alerts")]
    public bool InputAlertForMedian { get; set; }
    
    [Parameter("Alert For Single Print: alerts for single print rays' crossing.", DefaultValue = false, Group = "Alerts")]
    public bool InputAlertForSinglePrint { get; set; }
    
    [Parameter("Alert On Price Break: price breaking above/below the ray.", DefaultValue = false, Group = "Alerts")]
    public bool InputAlertOnPriceBreak { get; set; }
    
    [Parameter("Alert On Candle Close: candle closing above/below the ray.", DefaultValue = false, Group = "Alerts")]
    public bool InputAlertOnCandleClose { get; set; }
    
    [Parameter("Alert On Gap Cross: bar gap above/below the ray.", DefaultValue = false, Group = "Alerts")]
    public bool InputAlertOnGapCross { get; set; }
    
    [Parameter("Alert Arrow Color PB: arrow color for price break alerts.", DefaultValue = "Red", Group = "Alerts")]
    public Color InputAlertArrowColorPb { get; set; }
    
    [Parameter("Alert Arrow Color CC: arrow color for candle close alerts.", DefaultValue = "Blue", Group = "Alerts")]
    public Color InputAlertArrowColorCc { get; set; }
    
    [Parameter("Alert Arrow Color GC: arrow color for gap crossover alerts.", DefaultValue = "Yellow", Group = "Alerts")]
    public Color InputAlertArrowColorGc { get; set; }
    
    #endregion
    
    #region IntradaySettings
    
    #region IntradaySession1
    
    [Parameter("Enable", DefaultValue = true, Group = "Intraday Session 1")]
    public bool InputEnableIntradaySession1 { get; set; }
    
    [Parameter("Start Time", DefaultValue = "00:00", Group = "Intraday Session 1")]
    public string InputIntradaySession1StartTime { get; set; }
    
    [Parameter("End Time", DefaultValue = "06:00", Group = "Intraday Session 1")]
    public string InputIntradaySession1EndTime { get; set; }
    
    [Parameter("Color Start", DefaultValue = "#990000FF", Group = "Intraday Session 1")]
    public Color InputIntradaySession1ColorStart { get; set; }
    
    [Parameter("Color End", DefaultValue = "#99FF0000", Group = "Intraday Session 1")]
    public Color InputIntradaySession1ColorEnd { get; set; }
    
    #endregion
    
    #region IntradaySession2
    
    [Parameter("Enable ", DefaultValue = true, Group = "Intraday Session 2")]
    public bool InputEnableIntradaySession2 { get; set; }
    
    [Parameter("Start Time", DefaultValue = "06:00", Group = "Intraday Session 2")]
    public string InputIntradaySession2StartTime { get; set; }
    
    [Parameter("End Time", DefaultValue = "12:00", Group = "Intraday Session 2")]
    public string InputIntradaySession2EndTime { get; set; }
    
    [Parameter("Color Start", DefaultValue = "#99FF0000", Group = "Intraday Session 2")]
    public Color InputIntradaySession2ColorStart { get; set; }
    
    [Parameter("Color End", DefaultValue = "#9900FF00", Group = "Intraday Session 2")]
    public Color InputIntradaySession2ColorEnd { get; set; }
    
    #endregion
    
    #region IntradaySession3
    
    [Parameter("Enable ", DefaultValue = true, Group = "Intraday Session 3")]
    public bool InputEnableIntradaySession3 { get; set; }
    
    [Parameter("Start Time", DefaultValue = "12:00", Group = "Intraday Session 3")]
    public string InputIntradaySession3StartTime { get; set; }
    
    [Parameter("End Time", DefaultValue = "18:00", Group = "Intraday Session 3")]
    public string InputIntradaySession3EndTime { get; set; }
    
    [Parameter("Color Start", DefaultValue = "#9900FF00", Group = "Intraday Session 3")]
    public Color InputIntradaySession3ColorStart { get; set; }
    
    [Parameter("Color End", DefaultValue = "#990000FF", Group = "Intraday Session 3")]
    public Color InputIntradaySession3ColorEnd { get; set; }
    
    #endregion
    
    #region IntradaySession4
    
    [Parameter("Enable", DefaultValue = true, Group = "Intraday Session 4")]
    public bool InputEnableIntradaySession4 { get; set; }
    
    [Parameter("Start Time", DefaultValue = "18:00", Group = "Intraday Session 4")]
    public string InputIntradaySession4StartTime { get; set; }
    
    [Parameter("End Time", DefaultValue = "00:00", Group = "Intraday Session 4")]
    public string InputIntradaySession4EndTime { get; set; }
    
    [Parameter("Color Start", DefaultValue = "#99FFFF00", Group = "Intraday Session 4")]
    public Color InputIntradaySession4ColorStart { get; set; }
    
    [Parameter("Color End", DefaultValue = "#9900FFFF", Group = "Intraday Session 4")]
    public Color InputIntradaySession4ColorEnd { get; set; }
    
    #endregion
    
    #endregion
    
    #region Miscellaneous
    
    /// <summary>
    /// This seems to only work for the sessions Daily, Weekly and Intraday
    /// </summary>
    [Parameter("Saturday Sunday", DefaultValue = SatSunSolution.SaturdaySundayNormalDays, Group = "Miscellaneous")]
    public SatSunSolution InputSaturdaySunday { get; set; }
    
    [Parameter("Disable alerts on wrong timeframes.", DefaultValue = false, Group = "Miscellaneous")]
    public bool InputDisableAlertsOnWrongTimeframes { get; set; }
    
    [Parameter("Percentage of Median TPOs out of total for a Prominent one.", DefaultValue = 101, Group = "Miscellaneous")]
    public int InputProminentMedianPercentage { get; set; }
    
    [Parameter("Debug - Show Exceptions.", DefaultValue = false, Group = "Miscellaneous")]
    public bool InputShowExceptions { get; set; }
    
    #endregion
    
    #region Hotkeys
    
    [Parameter("Daily", DefaultValue = "Ctrl+1", Group = "Hotkeys")] 
    public string InputHotkeyDaily { get; set; }
    
    [Parameter("Weekly", DefaultValue = "Ctrl+2", Group = "Hotkeys")]
    public string InputHotkeyWeekly { get; set; }
    
    [Parameter("Monthly", DefaultValue = "Ctrl+3", Group = "Hotkeys")]
    public string InputHotkeyMonthly { get; set; }
    
    [Parameter("Quarterly", DefaultValue = "Ctrl+4", Group = "Hotkeys")]
    public string InputHotkeyQuarterly { get; set; }
    
    [Parameter("Semiannual", DefaultValue = "Ctrl+5", Group = "Hotkeys")]
    public string InputHotkeySemiannual { get; set; }
    
    [Parameter("Annual", DefaultValue = "Ctrl+6", Group = "Hotkeys")]
    public string InputHotkeyAnnual { get; set; }
    
    [Parameter("Intraday", DefaultValue = "Ctrl+7", Group = "Hotkeys")]
    public string InputHotkeyIntraday { get; set; }
    
    [Parameter("Rectangle", DefaultValue = "Ctrl+8", Group = "Hotkeys")]
    public string InputHotkeyRectangle { get; set; }
    
    #endregion
    
    #endregion
    
    #region Outputs
    
    [Output("Developing POC", LineColor = "Green", PlotType = PlotType.Points, Thickness = 4)]
    public IndicatorDataSeries OutputDevelopingPoC { get; set; }   
    
    [Output("Developing VAH", LineColor = "Goldenrod", PlotType = PlotType.Points, Thickness = 4)]
    public IndicatorDataSeries OutputDevelopingVah { get; set; }
    
    [Output("Developing VAL 1", LineColor = "Salmon", PlotType = PlotType.Points, Thickness = 4)]
    public IndicatorDataSeries OutputDevelopingVaL { get; set; }
    
    #endregion

    private double _numberOfSlices;
    private int _digitsM;
    
    public double OneTickSize { get; private set; }
    public double ValueAreaPercentage { get; private set; }
    public List<IntradaySessionDefinition> IntradaySessions { get; private set; }
    public List<MarketProfileSession> Sessions { get; } = new();
    public MarketProfileCalculator Calculator { get; private set; }
    public MarketProfileRenderer Renderer { get; private set; }
    public HideRaysFromInvisibleSessionsFeature HideRaysFromInvisibleSessionsFeature { get; private set; }
    public SessionState SessionState { get;  set; }
    public SessionChangeManager SessionChangeManager { get; private set; }
    public HotkeyStateManager HotkeyStateManager { get; private set; }
    public SeamlessScrollingManager SeamlessScrollingManager { get; set; }
    public AlertManager AlertManager { get; private set; }
    public NewHighLowManager NewHighLowManager { get; set; }
    public RectangleModeProcessor RectangleModeProcessor { get; private set; }
    
    public event EventHandler<CalculateEventArgs> CalculateEvent;
    private int _lastBarIndex;
    private bool _canDraw;

    protected override void Initialize()
    { 
        //System.Diagnostics.Debugger.Launch();
        
        InitializeOneTickSize();
        ValueAreaPercentage = InputValueAreaPercentage / 100.0;
        
        SessionChangeManager = new SessionChangeManager(this);
        SessionChangeManager.LoadSessionState();
        _canDraw = SessionChangeManager.CheckSessions(SessionState.LastSessionState);
        
        if (!_canDraw)
        {
            Chart.DrawStaticText("MarketProfile-NoSessions", "Please choose a suitable timeframe", VerticalAlignment.Center, HorizontalAlignment.Center, Color.Red);
            return;
        }

        HotkeyStateManager = new HotkeyStateManager(this);
        AlertManager = new AlertManager(this);
        HideRaysFromInvisibleSessionsFeature = new HideRaysFromInvisibleSessionsFeature(this);
        NewHighLowManager = new NewHighLowManager(this);
        
        //ParameterChecks
        CheckIsNotStartFromDateAndSeamlessScrollingMode();
        
        IntradaySessions = GetIntradaySessions();
        
        CheckZeroIntradaySessions();
        
        Chart.RemoveAllObjects();
        
        Calculator = new MarketProfileCalculator(this);
        Renderer = new MarketProfileRenderer(this, this);
        
        SetSessionsAndAddMarketProfiles(InputSessionsToCount);
        
        RectangleModeProcessor = new RectangleModeProcessor(this);
        
        Bars.BarOpened += _ => AddOrUpdateSessions();
        NewHighLowManager.NewHighOnLastBar += (_, _) => AddOrUpdateSessions();
        NewHighLowManager.NewLowOnLastBar += (_, _) => AddOrUpdateSessions();
        
        HotkeyStateManager.AddHotkeys();
        
        SeamlessScrollingManager = new SeamlessScrollingManager(this);
        
        if (InputSeamlessScrollingMode)
            SeamlessScrollingManager.Start();
        
        if (InputHideRaysFromInvisibleSessions) 
            HideRaysFromInvisibleSessionsFeature.Start();
    }

    public override void Calculate(int index)
    {
        CalculateEvent?.Invoke(this, new CalculateEventArgs { IsLastBar = IsLastBar, Index = index, IsNewBar = index > _lastBarIndex });

        if (index > _lastBarIndex) 
            _lastBarIndex = index;
        
        if (IsLastBar)
            AlertManager?.CheckAlerts(index);
    }

    protected override void OnException(Exception exception)
    {
        if (InputShowExceptions)
        {
            var sb = new StringBuilder();
        
            sb.AppendLine($"Exception: {exception.Message}");
            sb.AppendLine($"StackTrace: {exception.StackTrace}");

            Chart.DrawStaticText(
                $"MarketProfile-Exception", 
                sb.ToString(), 
                VerticalAlignment.Top, 
                HorizontalAlignment.Right, 
                Color.Red);   
        }
        
        Print($"Exception: {exception.Message}");
        Print($"StackTrace: {exception.StackTrace}");
    }

    private void AddOrUpdateSessions()
    {
        //must update last volume profile
        if (SessionState.LastSessionState == SessionPeriod.Rectangle)
            return;

        if (Bars.OpenTimes[Chart.LastVisibleBarIndex] != Bars.Last().OpenTime)
            return;

        var lastSession = Sessions.LastOrDefault();

        if (lastSession == null)
        {
            SetSessionsAndAddMarketProfiles(1);
            return;
        }

        var parsed = DateTime.TryParse(InputStartFromDate, out var startFrom);

        if (parsed && !InputStartFromCurrentSession)
            return;

        // Check if new bar belongs to current session or requires new session
        var needNewSession = IsNewSessionRequired(lastSession, Bars.LastBar.OpenTime);
    
        if (needNewSession)
            SetSessionsAndAddMarketProfiles(1);
        else
            UpdateLastSession(session: lastSession);
    }

    private void UpdateLastSession(MarketProfileSession session)
    {
        var strategy = SessionState.LastSessionState switch
        {
            SessionPeriod.Daily => SessionProfileStrategyFactory.CreateDaily(this, this),
            SessionPeriod.Weekly => SessionProfileStrategyFactory.CreateWeekly(this, this),
            SessionPeriod.Monthly => SessionProfileStrategyFactory.CreateMonthly(this, this),
            SessionPeriod.Quarterly => SessionProfileStrategyFactory.CreateQuarterly(this, this),
            SessionPeriod.Semiannual => SessionProfileStrategyFactory.CreateSemiannual(this, this),
            SessionPeriod.Annual => SessionProfileStrategyFactory.CreateAnnual(this, this),
            SessionPeriod.Intraday => SessionProfileStrategyFactory.CreateIntradaySessions(IntradaySessions, this, this),
            SessionPeriod.Rectangle => null,
            _ => null
        };

        if (strategy == null) 
            return;
        
        var ranges = strategy.GetSessionRanges(Bars, 1, InputStartColor, InputEndColor).ToArray();
        
        //there should be only one range
        if (ranges.Length != 1)
            throw new Exception("There should be only one range");

        if (session.Range.Start != ranges[0].Start)
            return;

        session.Range = ranges[0];
        
        if (!session.Range.Bars.Any())
        {
            // Skip this session or handle empty range
            Print($"Warning: No bars found for session range {session.Range.Start} to {session.Range.End}");
            return; // or continue to next session
        }
        
        var bars = session.Range.Bars.ToArray();
        
        var highMinusLow = bars.Max(x => x.High) - bars.Min(x => x.Low);
        var slices = (int) (highMinusLow / OneTickSize);

        if (slices == 0)
        {
            Print($"Slices is 0 | This can't be processed Bars is {bars.Length} (High {bars.Max(x => x.High)} | Low {bars.Min(x => x.Low)} Minus Low is {highMinusLow}) | OneTickSize is {OneTickSize}");
            return;
        }
        
        var profileModel = Calculator.FillAndPileMatrixCalculation(bars, slices, session.Range.StartColor, session.Range.EndColor);
        
        //these two below could be totally removed and reference the latest "developing" values
        var (vah, val) = Calculator.GetValueArea(profileModel.Matrix, InputValueAreaPercentage / 100.0);
        var pointOfControlRowIndex = MarketProfileCalculator.GetPointOfControlRowIndex(profileModel.Matrix);

        if (profileModel.Matrix[pointOfControlRowIndex, 0] == null)
        {
            Print($"Unable to calculate POC because the row is null");
            return;
        }
        
        var pointOfControlPrice = MarketProfileCalculator.GetPointOfControlPrice(profileModel.Matrix, pointOfControlRowIndex);
        
        var (totalTopBlocksAbovePointOfControl, totalBottomBlocksBelowPointOfControl) = MarketProfileCalculator.GetValuesAroundPointOfControl(profileModel.Matrix);
            
        profileModel.ValueAreaHigh = vah;
        profileModel.ValueAreaLow = val;
        profileModel.PointOfControl = pointOfControlPrice;
        profileModel.Median = Calculator.GetMedianPrice(profileModel.Matrix);
        profileModel.TpoCountAbove = totalTopBlocksAbovePointOfControl;
        profileModel.TpoCountBelow = totalBottomBlocksBelowPointOfControl;
        profileModel.SinglePrints.AddRange(MarketProfileCalculator.GetSinglePrints(profileModel.Matrix));
        profileModel.IsProminentLine = Calculator.IsProminentLine(profileModel.Matrix, InputProminentMedianPercentage / 100.0);

        session.Model = profileModel;
        
        Renderer.DeleteAllFromSession(session);

        if (InputRightToLeft)
        {
            if (!InputDisableHistogram)
                session.Profile = Renderer.RenderHorizontallyFlippedProfile(profileModel.Matrix, session);
            
            session.ValueArea = Renderer.RenderHorizontallyFlippedValueArea(profileModel, session);
            session.KeyValues = Renderer.RenderHorizontallyFlippedKeyValues(profileModel, session);
            session.TpoCounts = Renderer.RenderHorizontallyFlippedTpoCounts(profileModel, session);
            session.ProminentLine = Renderer.RenderHorizontallyFlippedProminentLine(profileModel, session);
        }
        else
        {
            if (!InputDisableHistogram)
                session.Profile = Renderer.RenderProfile(profileModel.Matrix);
            
            session.ValueArea = Renderer.RenderValueArea(profileModel);
            session.ValueAreaRays = Renderer.RenderValueAreaRays(profileModel, Sessions.Count - 1);
            session.MedianRays = Renderer.RenderPointOfControlRays(profileModel, Sessions.Count - 1);
            session.KeyValues = Renderer.RenderKeyValues(profileModel);
            session.TpoCounts = Renderer.RenderTpoCounts(profileModel);
            session.SinglePrints = Renderer.RenderSinglePrints(profileModel);
            session.ProminentLine = Renderer.RenderProminentLine(profileModel);
            
            Renderer.PostProcessPointOfControlRays(Sessions);
            Renderer.PostProcessValueAreaRays(Sessions);
        }
    }

    public bool IsNewSessionRequired(MarketProfileSession lastSession, DateTime newBarTime)
    {
        // If new bar's time is after the session's end time, we need a new session
        if (newBarTime > lastSession.Range.End)
            return true;

        return SessionState.LastSessionState switch
        {
            SessionPeriod.Daily =>
                // For daily, check if we've crossed into a new day (adjusted for market open)
                !SameSessionDay(lastSession.Range.Start, newBarTime),
            SessionPeriod.Weekly =>
                // For weekly, check if we've crossed into a new week
                GetWeekNumber(lastSession.Range.Start) != GetWeekNumber(newBarTime),
            SessionPeriod.Monthly =>
                // For monthly, check if we've crossed into a new month
                lastSession.Range.Start.Month != newBarTime.Month || lastSession.Range.Start.Year != newBarTime.Year,
            SessionPeriod.Quarterly =>
                // For quarterly, check if we've crossed into a new quarter
                GetQuarter(lastSession.Range.Start) != GetQuarter(newBarTime) || lastSession.Range.Start.Year != newBarTime.Year,
            SessionPeriod.Semiannual =>
                // For semiannual, check if we've crossed into a new half-year
                (lastSession.Range.Start.Month <= 6 && newBarTime.Month > 6) || (lastSession.Range.Start.Month > 6 && newBarTime.Month <= 6) || lastSession.Range.Start.Year != newBarTime.Year,
            SessionPeriod.Annual =>
                // For annual, check if we've crossed into a new year
                lastSession.Range.Start.Year != newBarTime.Year,
            SessionPeriod.Intraday =>
                // For intraday, check if the new bar is in a different intraday session
                !SameIntradaySession(lastSession.Range.Start, newBarTime),
            _ => false
        };
    }

    private bool SameIntradaySession(DateTime sessionStart, DateTime barTime)
    {
        // Implement logic to check if a bar belongs to the same intraday session
        // This would need to check the intraday session definitions
        foreach (var session in IntradaySessions)
        {
            // Check if both times fall within the same intraday session
            // This is a simplified check and may need to be enhanced
            if (IsInIntradaySession(sessionStart, session) && 
                IsInIntradaySession(barTime, session))
                return true;
        }
        return false;
    }
    
    private void InitializeOneTickSize()
    {
        if (InputPointMultiplier == 0)
        {
            var quote = Ask;
            
            //Get chart dimensions and price range
            var chartHeight = Chart.Height;
            var chartPriceMax = Chart.TopY;
            var chartPriceMin = Chart.BottomY;
            var priceDiff = chartPriceMax - chartPriceMin;
            
            //todo Chart.BottomY is -0 when backtesting, should report for fixing
            //Print($"Chart Price Max {chartPriceMax} Min {chartPriceMin} Diff {priceDiff}");

            if ((chartHeight <= 0) || (priceDiff <= 0)) //If no chart yet, do it old fashion
            {
                var s = quote.ToString($"F{Symbol.Digits}");
                var totalDigits = s.Length;

                // If there is a dot in a quote.
                if (s.Contains(',') || s.Contains('.'))
                    totalDigits--; // Decrease the count of digits by one.

                _numberOfSlices = totalDigits <= 5 ? 1 : (int)Math.Pow(10, totalDigits - 5);
            }
            else // otherwise, calculate the multiplier so that 1 TPO = 1 pixel
            {
                var pricePerPixel = priceDiff / chartHeight;
                _numberOfSlices = (int)Math.Round(pricePerPixel / Symbol.TickSize);
            }
        }
        else
        {
            _numberOfSlices = InputPointMultiplier;
        }
        
        // Based on number of digits in PointMultiplier_calculated. -1 because if PointMultiplier_calculated < 10, it does not modify the number of digits.
        _digitsM = Math.Max(0, Symbol.Digits - (_numberOfSlices.ToString(CultureInfo.InvariantCulture).Length - 1));
        OneTickSize = Math.Round(Symbol.TickSize * _numberOfSlices, _digitsM);

        // Adjust for TickSize granularity if needed.
        var tickSize = Symbol.TickSize;
        if (OneTickSize < tickSize)
        {
            _digitsM = Symbol.Digits - (((int)Math.Round(tickSize / Symbol.TickSize)).ToString().Length - 1);
            OneTickSize = Math.Round(tickSize, _digitsM);
        }
    }
    
    public bool SetSessionsAndAddMarketProfiles(int sessionsToCount, DateTime? endAt = null)
    {
        var strategy = SessionState.LastSessionState switch
        {
            SessionPeriod.Daily => SessionProfileStrategyFactory.CreateDaily(this, this),
            SessionPeriod.Weekly => SessionProfileStrategyFactory.CreateWeekly(this, this),
            SessionPeriod.Monthly => SessionProfileStrategyFactory.CreateMonthly(this, this),
            SessionPeriod.Quarterly => SessionProfileStrategyFactory.CreateQuarterly(this, this),
            SessionPeriod.Semiannual => SessionProfileStrategyFactory.CreateSemiannual(this, this),
            SessionPeriod.Annual => SessionProfileStrategyFactory.CreateAnnual(this, this),
            SessionPeriod.Intraday => SessionProfileStrategyFactory.CreateIntradaySessions(IntradaySessions, this, this),
            SessionPeriod.Rectangle => null,
            _ => null
        };

        if (strategy == null) 
            return false;
        
        var ranges = strategy.GetSessionRanges(Bars, sessionsToCount, InputStartColor, InputEndColor, endAt).ToArray();

        for (var index = 0; index < ranges.Length; index++)
        {
            var range = ranges[index];
            
            Renderer.ClearOutputsOnRange(range.Start, range.End);
            
            var session = new MarketProfileSession
            {
                Range = range,
            };

            BuildAndRender(range, session, index, ranges.Length);
        }
        
        Renderer.PostProcessPointOfControlRays(Sessions);
        Renderer.PostProcessValueAreaRays(Sessions);

        return true;
    }

    private bool BuildAndRender(SessionRange range, MarketProfileSession session, int index, int total)
    {
        if (!range.Bars.Any())
        {
            // Skip this session or handle empty range
            Print($"Warning: No bars found for session range {range.Start} to {range.End}");
            return false; // or continue to next session
        }
        
        var bars = range.Bars.ToArray();
        var slices = Calculator.GetSlices(bars);

        if (slices == 0)
        {
            Print($"Slices is 0 | This can't be processed Bars is {bars.Length} (High {bars.Max(x => x.High)} | Low {bars.Min(x => x.Low)} | OneTickSize is {OneTickSize}");
            return false;
        }
        
        var profileModel = Calculator.FillAndPileMatrixCalculation(bars, slices, range.StartColor, range.EndColor);
        
        //these two below could be totally removed and reference the latest "developing" values
        //var (vah, val) = Calculator.GetValueArea(profileModel.Matrix, InputValueAreaPercentage / 100.0);
        var vah = profileModel.DevelopingAreaHigh.LastOrDefault().Value;
        var val = profileModel.DevelopingAreaLow.LastOrDefault().Value;
        var pointOfControlRowIndex = MarketProfileCalculator.GetPointOfControlRowIndex(profileModel.Matrix);

        if (profileModel.Matrix[pointOfControlRowIndex, 0] == null)
        {
            Print($"Unable to calculate POC because the row is null");
            return false;
        }
        
        var pointOfControlPrice = MarketProfileCalculator.GetPointOfControlPrice(profileModel.Matrix, pointOfControlRowIndex);
        
        var (totalTopBlocksAbovePointOfControl, totalBottomBlocksBelowPointOfControl) = MarketProfileCalculator.GetValuesAroundPointOfControl(profileModel.Matrix);
            
        profileModel.ValueAreaHigh = vah;
        profileModel.ValueAreaLow = val;
        profileModel.PointOfControl = pointOfControlPrice;
        profileModel.Median = Calculator.GetMedianPrice(profileModel.Matrix);
        profileModel.TpoCountAbove = totalTopBlocksAbovePointOfControl;
        profileModel.TpoCountBelow = totalBottomBlocksBelowPointOfControl;
        profileModel.SinglePrints.AddRange(MarketProfileCalculator.GetSinglePrints(profileModel.Matrix));
        profileModel.IsProminentLine = Calculator.IsProminentLine(profileModel.Matrix, InputProminentMedianPercentage / 100.0);

        session.Model = profileModel;
        
        if (index == 0 && InputRightToLeft)
        {
            if (!InputDisableHistogram)
                session.Profile = Renderer.RenderHorizontallyFlippedProfile(profileModel.Matrix, session);
                
            session.ValueArea = Renderer.RenderHorizontallyFlippedValueArea(profileModel, session);
            session.KeyValues = Renderer.RenderHorizontallyFlippedKeyValues(profileModel, session);
            session.TpoCounts = Renderer.RenderHorizontallyFlippedTpoCounts(profileModel, session);
            session.ProminentLine = Renderer.RenderHorizontallyFlippedProminentLine(profileModel, session);
        }
        else
        {
            if (!InputDisableHistogram)
                session.Profile = Renderer.RenderProfile(profileModel.Matrix);
                
            session.ValueArea = Renderer.RenderValueArea(profileModel);
            session.ValueAreaRays = Renderer.RenderValueAreaRays(profileModel, index);
            session.MedianRays = Renderer.RenderPointOfControlRays(profileModel, index);
            session.KeyValues = Renderer.RenderKeyValues(profileModel);
            session.TpoCounts = Renderer.RenderTpoCounts(profileModel);
            session.ProminentLine = Renderer.RenderProminentLine(profileModel);
        }
        
        if (InputEnableDevelopingPoC)
            Renderer.RenderDevelopingPoc(profileModel.DevelopingPoC);
            
        if (InputEnableDevelopingValueAtHighValueAtLow)
        {
            Renderer.RenderDevelopingVahs(profileModel.DevelopingAreaHigh);
            Renderer.RenderDevelopingVals(profileModel.DevelopingAreaLow);   
        }

        session.SinglePrints = Renderer.RenderSinglePrints(profileModel);
            
        Sessions.Add(session);

        return true;
    }

    public List<IntradaySessionDefinition> GetIntradaySessions()
    {
        //initialize intraday sessions
        var sessions = new List<IntradaySessionDefinition>();
        
        if (InputEnableIntradaySession1)
            sessions.Add(new IntradaySessionDefinition
            {
                Name = "Intraday 1",
                Start = TimeSpan.Parse(InputIntradaySession1StartTime),
                End = TimeSpan.Parse(InputIntradaySession1EndTime),
                StartColor = InputIntradaySession1ColorStart,
                EndColor = InputIntradaySession1ColorEnd
            });
        
        if (InputEnableIntradaySession2)
            sessions.Add(new IntradaySessionDefinition
            {
                Name = "Intraday 2",
                Start = TimeSpan.Parse(InputIntradaySession2StartTime),
                End = TimeSpan.Parse(InputIntradaySession2EndTime),
                StartColor = InputIntradaySession2ColorStart,
                EndColor = InputIntradaySession2ColorEnd
            });
        
        if (InputEnableIntradaySession3)
            sessions.Add(new IntradaySessionDefinition
            {
                Name = "Intraday 3",
                Start = TimeSpan.Parse(InputIntradaySession3StartTime),
                End = TimeSpan.Parse(InputIntradaySession3EndTime),
                StartColor = InputIntradaySession3ColorStart,
                EndColor = InputIntradaySession3ColorEnd
            });
        
        if (InputEnableIntradaySession4)
            sessions.Add(new IntradaySessionDefinition
            {
                Name = "Intraday 4",
                Start = TimeSpan.Parse(InputIntradaySession4StartTime),
                End = TimeSpan.Parse(InputIntradaySession4EndTime),
                StartColor = InputIntradaySession4ColorStart,
                EndColor = InputIntradaySession4ColorEnd
            });

        return sessions;
    }

    #region Checks
    
    private void CheckIsNotStartFromDateAndSeamlessScrollingMode()
    {
        var parsed = DateTime.TryParse(InputStartFromDate, out var startFrom);
        
        if (parsed && InputSeamlessScrollingMode && !InputStartFromCurrentSession)
            throw new ArgumentException("Seamless scrolling mode doesn't work with Start From Date Mode.");
    }
    
    public void CheckZeroIntradaySessions()
    {
        if (IntradaySessions.Count == 0 && SessionState.LastSessionState == SessionPeriod.Intraday)
            throw new ArgumentException("Enable at least one intraday session if you want to use Intraday mode.");
    }
    
    #endregion
}