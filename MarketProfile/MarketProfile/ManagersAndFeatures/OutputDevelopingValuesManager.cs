using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo;

public interface IOutputDevelopingValuesManagerResources
{
    Chart Chart { get; }
    Bars Bars { get; }
    TimeFrame TimeFrame { get; }
    //--
    int InputOutputDevelopingSessionsToCount { get; }
    int InputValueAreaPercentage { get; }
    Color InputStartColor { get; }
    Color InputEndColor { get; }
    bool InputEnableDevelopingPoC { get; }
    bool InputEnableDevelopingValueAtHighValueAtLow { get; }
    MarketProfileCalculator Calculator { get; }
    MarketProfileRenderer Renderer { get; }
    SessionState SessionState { get; }
    List<IntradaySessionDefinition> IntradaySessions { get; }
    double OneTickSize { get; }
    bool IsNewSessionRequired(MarketProfileSession lastSession, DateTime newBarTime);
    //--
    IndicatorDataSeries OutputDevelopingPoC { get; }
    IndicatorDataSeries OutputDevelopingVah { get; }
    IndicatorDataSeries OutputDevelopingVaL { get; }
}

public class OutputDevelopingValuesManager : 
    IOutputDevelopingValuesManagerResources,
    IIndicatorResources,
    IRenderingModesResources
{
    private readonly IOutputDevelopingValuesManagerResources _resources;
    private readonly IRenderingModesResources _renderingModesResources;

    public List<MarketProfileSession> OutputSessions { get; } = new();

    public OutputDevelopingValuesManager(IOutputDevelopingValuesManagerResources resources, IRenderingModesResources renderingModesResources)
    {
        _resources = resources;
        _renderingModesResources = renderingModesResources;

        // Chart.ScrollChanged += ChartScrollChanged;
        // Chart.ZoomChanged += ChartZoomChanged;
        Bars.BarOpened += OnBarOpened;
    }

    public void SetOutputSessions(int sessionsToCount)
    {
        //var firstVisibleIndex = Chart.FirstVisibleBarIndex;
        var lastVisibleIndex = Chart.LastVisibleBarIndex;
        //var indexRange = lastVisibleIndex - firstVisibleIndex;

        //var renderingStartIndex = (int)Math.Max(0, (firstVisibleIndex - indexRange * 0.5));
        var renderingStartIndex = 0;
        var renderingEndIndex = lastVisibleIndex;

        var renderingStartTime = Bars.OpenTimes[renderingStartIndex];
        var renderingEndTime = Bars.OpenTimes[renderingEndIndex];

        var bars = Bars.Where(x => x.OpenTime >= renderingStartTime && x.OpenTime <= renderingEndTime).ToArray();
        
        var strategy = SessionState.LastSessionState  switch
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

        var ranges = sessionsToCount == 0
            ? strategy.GetSessionRanges(Bars, InputStartColor, InputEndColor, renderingStartTime, renderingEndTime).ToArray()
            : strategy.GetSessionRanges(Bars, sessionsToCount, InputStartColor, InputEndColor).ToArray();
        
        for (var index = 0; index < ranges.Length; index++)
        {
            var range = ranges[index];
            var session = new MarketProfileSession
            {
                Range = range,
            };

            var rangeBars = range.Bars.ToArray();
            
            if (bars.Length == 0)
                continue;
            
            var highMinusLow = bars.Max(x => x.High) - bars.Min(x => x.Low);
            var slices = (int) (highMinusLow / OneTickSize);

            var profileModel = Calculator.FillAndPileMatrixCalculation(rangeBars, slices, range.StartColor, range.EndColor);
            session.Model = profileModel;
            
            if (InputEnableDevelopingPoC)
                Renderer.RenderDevelopingPoc(profileModel.DevelopingPoC);
            
            if (InputEnableDevelopingValueAtHighValueAtLow)
            {
                Renderer.RenderDevelopingVahs(profileModel.DevelopingAreaHigh);
                Renderer.RenderDevelopingVals(profileModel.DevelopingAreaLow);   
            }
            
            OutputSessions.Add(session);
        }
    }
    
    private void OnBarOpened(BarOpenedEventArgs obj)
    {
        if (SessionState.LastSessionState == SessionPeriod.Rectangle)
            return;
        
        var lastSession = OutputSessions.LastOrDefault();
        
        if (lastSession == null)
        {
            SetOutputSessions(1);
            return;
        }
        
        var parsed = DateTime.TryParse(InputStartFromDate, out var startFrom);

        if (parsed && !InputStartFromCurrentSession)
            return;

        var needNewSession = IsNewSessionRequired(lastSession, Bars.LastBar.OpenTime);

        if (needNewSession)
            SetOutputSessions(1);
        else
            UpdateLastOutputSession(session: lastSession);
        
        //
        // ClearOutputsFromSession(lastSession);
        // OutputSessions.Remove(lastSession);
        // SetOutputSessions(1);
    }

    private void UpdateLastOutputSession(MarketProfileSession session)
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
            //Print($"Warning: No bars found for session range {session.Range.Start} to {session.Range.End}");
            return; // or continue to next session
        }
        
        var bars = session.Range.Bars.ToArray();
        
        var highMinusLow = bars.Max(x => x.High) - bars.Min(x => x.Low);
        var slices = (int) (highMinusLow / OneTickSize);

        if (slices == 0)
        {
            //Print($"Slices is 0 | This can't be processed Bars is {bars.Length} (High {bars.Max(x => x.High)} | Low {bars.Min(x => x.Low)} Minus Low is {highMinusLow}) | OneTickSize is {OneTickSize}");
            return;
        }
        
        var profileModel = Calculator.FillAndPileMatrixCalculation(bars, slices, session.Range.StartColor, session.Range.EndColor);
        
        //these two below could be totally removed and reference the latest "developing" values
        var (vah, val) = Calculator.GetValueArea(profileModel.Matrix, InputValueAreaPercentage / 100.0);
        var pointOfControlRowIndex = MarketProfileCalculator.GetPointOfControlRowIndex(profileModel.Matrix);

        if (profileModel.Matrix[pointOfControlRowIndex, 0] == null)
        {
            //Print($"Unable to calculate POC because the row is null");
            return;
        }
        
        session.Model = profileModel;
            
        if (InputEnableDevelopingPoC)
            Renderer.RenderDevelopingPoc(profileModel.DevelopingPoC);
            
        if (InputEnableDevelopingValueAtHighValueAtLow)
        {
            Renderer.RenderDevelopingVahs(profileModel.DevelopingAreaHigh);
            Renderer.RenderDevelopingVals(profileModel.DevelopingAreaLow);   
        }
    }

    public void ClearOutputsFromSession(MarketProfileSession session)
    {
        var sessionStartIndex = Bars.OpenTimes.GetIndexByTime(session.Range.Start);
        var sessionEndIndex = Bars.OpenTimes.GetIndexByTime(session.Range.End);

        for (var i = sessionStartIndex; i <= sessionEndIndex; i++)
        {
            if (!double.IsNaN(OutputDevelopingPoC[i]))
                OutputDevelopingPoC[i] = double.NaN;

            if (!double.IsNaN(OutputDevelopingVah[i]))
                OutputDevelopingVah[i] = double.NaN;

            if (!double.IsNaN(OutputDevelopingVaL[i]))
                OutputDevelopingVaL[i] = double.NaN;
        }
    }

    #region Outputs

    public Chart Chart => _resources.Chart;
    public Bars Bars => _resources.Bars;
    public TimeFrame TimeFrame => _resources.TimeFrame;
    public int InputOutputDevelopingSessionsToCount => _resources.InputOutputDevelopingSessionsToCount;
    public int InputValueAreaPercentage => _resources.InputValueAreaPercentage;
    public Color InputStartColor => _resources.InputStartColor;
    public Color InputEndColor => _resources.InputEndColor;
    public bool InputEnableDevelopingPoC => _resources.InputEnableDevelopingPoC;
    public bool InputEnableDevelopingValueAtHighValueAtLow => _resources.InputEnableDevelopingValueAtHighValueAtLow;
    public MarketProfileCalculator Calculator => _resources.Calculator;
    public MarketProfileRenderer Renderer => _resources.Renderer;
    public SessionState SessionState => _resources.SessionState;
    public List<IntradaySessionDefinition> IntradaySessions => _resources.IntradaySessions;
    public double OneTickSize => _resources.OneTickSize;
    public bool IsNewSessionRequired(MarketProfileSession lastSession, DateTime newBarTime) => 
        _resources.IsNewSessionRequired(lastSession, newBarTime);

    public IndicatorDataSeries OutputDevelopingPoC => _resources.OutputDevelopingPoC;
    public IndicatorDataSeries OutputDevelopingVah => _resources.OutputDevelopingVah;
    public IndicatorDataSeries OutputDevelopingVaL => _resources.OutputDevelopingVaL;
    public string InputStartFromDate => _renderingModesResources.InputStartFromDate;
    public bool InputStartFromCurrentSession => _renderingModesResources.InputStartFromCurrentSession;
    public bool InputSeamlessScrollingMode => _renderingModesResources.InputSeamlessScrollingMode;
    public SatSunSolution InputSaturdaySunday => _renderingModesResources.InputSaturdaySunday;

    #endregion
}