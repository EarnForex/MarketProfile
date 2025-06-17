using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo;

public interface IRectangleModeProcessorResources
{
    TimeFrame TimeFrame { get; }
    Chart Chart { get; }
    Bars Bars { get; }
    MarketProfileRenderer Renderer { get; }
    MarketProfileCalculator Calculator { get; }
    List<MarketProfileSession> Sessions { get; }
    SessionState SessionState { get; }
    //--
    Color InputStartColor { get; }
    Color InputEndColor { get; }
    void Print(object obj);
    double OneTickSize { get; }
    int InputProminentMedianPercentage { get; }
    bool InputRightToLeft { get; }
    bool InputDisableHistogram { get; }
    bool InputEnableDevelopingPoC { get; }
    bool InputEnableDevelopingValueAtHighValueAtLow { get; }
}

public class RectangleModeProcessor : IRectangleModeProcessorResources, IIndicatorResources
{
    private readonly IRectangleModeProcessorResources _resources;

    public RectangleModeProcessor(IRectangleModeProcessorResources resources)
    {
        _resources = resources;
        
        Chart.KeyDown += ChartKeyDown;
        Chart.ObjectsMoveEnded += ChartObjectsMoveEnded;
        Chart.ObjectsRemoved += ChartObjectsRemoved;
    }

    private void ChartObjectsRemoved(ChartObjectsRemovedEventArgs obj)
    {
        if (SessionState.LastSessionState != SessionPeriod.Rectangle)
            return;
        
        if (obj.ChartObjects.First() is not ChartRectangle rectangle)
            return;
        
        var rectangleSession = Sessions.FirstOrDefault(x => x.Rectangle.Name == rectangle.Name);
        
        if (rectangleSession == null)
            return;
        
        //need to delete all relevant to this rectangleSession
        Renderer.DeleteAllFromSession(rectangleSession);
        Sessions.Remove(rectangleSession);
    }

    private void ChartObjectsMoveEnded(ChartObjectsMoveEndedEventArgs obj)
    {
        if (SessionState.LastSessionState != SessionPeriod.Rectangle)
            return;
        
        if (obj.ChartObjects.First() is not ChartRectangle rectangle)
            return;
        
        var rectangleSession = Sessions.FirstOrDefault(x => x.Rectangle.Name == rectangle.Name);
        
        if (rectangleSession == null)
            return;
        
        Renderer.DeleteAllFromSession(rectangleSession);
        
        //now redo the calculation

        var startOfRectangleTime = rectangle.Time1;
        var endOfRectangleTime = rectangle.Time2;
        
        var strategy = SessionProfileStrategyFactory.CreateRectangle(this, startOfRectangleTime, endOfRectangleTime);
        var ranges = strategy.GetSessionRanges(Bars, 1, InputStartColor, InputEndColor).ToArray();
        
        if (ranges.Length != 0)
        {
            Renderer.ClearOutputsTillDate(ranges.Last().Start);
            Renderer.ClearOutputsAfterDate(ranges.First().End);
        }

        for (var index = 0; index < ranges.Length; index++)
        {
            var range = ranges[index];
            var session = Sessions.FirstOrDefault(x => x.Rectangle.Name == rectangle.Name);

            if (session == null)
                throw new Exception("Rectangle session not found");

            session.Range = range;

            BuildAndRender(range, session, index, ranges.Length, cropTop: Math.Max(rectangle.Y1, rectangle.Y2), cropBottom: Math.Min(rectangle.Y1, rectangle.Y2));
        }
    }

    private void ChartKeyDown(ChartKeyboardEventArgs obj)
    {
        if (obj.Key != Key.R)
            return;
        
        if (SessionState.LastSessionState != SessionPeriod.Rectangle)
            return;

        //var strategy = SessionProfileStrategyFactory.CreateRectangle(this);
        
        var startIndex = Chart.FirstVisibleBarIndex;
        var endIndex = Chart.LastVisibleBarIndex;
        
        //I want to divide the screen into 3/5, so the rectangle occupy 3/5 of the screen and be located around the center
        
        var startOfRectangleIndex = startIndex + (endIndex - startIndex) / 5;
        var endOfRectangleIndex = endIndex - (endIndex - startIndex) / 5;
        
        var startOfRectangleTime = Bars.OpenTimes[startOfRectangleIndex];
        var endOfRectangleTime = Bars.OpenTimes[endOfRectangleIndex];
        
        var selectedBars = Bars.Where(x => x.OpenTime >= startOfRectangleTime && x.OpenTime <= endOfRectangleTime).ToArray();
        
        var minPrice = selectedBars.Min(x => x.Low);
        var maxPrice = selectedBars.Max(x => x.High);
        var rectangleRange = (maxPrice - minPrice) * 0.9;
        var rectangleLow = maxPrice - rectangleRange;
        var rectangleHigh = minPrice + rectangleRange;
        
        var rectangle = Chart.DrawRectangle($"MarketProfileRectangle-{Guid.NewGuid()}", startOfRectangleTime, rectangleLow, endOfRectangleTime, rectangleHigh, Color.Blue);
        rectangle.IsInteractive = true;
        
        var strategy = SessionProfileStrategyFactory.CreateRectangle(this, startOfRectangleTime, endOfRectangleTime);
        var ranges = strategy.GetSessionRanges(Bars, 1, InputStartColor, InputEndColor).ToArray();
        
        if (ranges.Length != 0)
        {
            Renderer.ClearOutputsTillDate(ranges.Last().Start);
            Renderer.ClearOutputsAfterDate(ranges.First().End);
        }

        for (var index = 0; index < ranges.Length; index++)
        {
            var range = ranges[index];
            var session = new MarketProfileSession
            {
                Range = range,
                Rectangle = rectangle
            };

            BuildAndRender(range, session, index, ranges.Length, rectangleHigh, rectangleLow);
        }
    }
    
    private bool BuildAndRender(SessionRange range, MarketProfileSession session, int index, int total, double cropTop, double cropBottom)
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
        
        var profileModel = Calculator.FillAndPileMatrixCalculationCropped(bars, slices, range.StartColor, range.EndColor, cropTop, cropBottom);
        
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
    

    #region Resources

    public TimeFrame TimeFrame => _resources.TimeFrame;
    public Chart Chart => _resources.Chart;
    public Bars Bars => _resources.Bars;
    public MarketProfileRenderer Renderer => _resources.Renderer;
    public MarketProfileCalculator Calculator => _resources.Calculator;
    public List<MarketProfileSession> Sessions => _resources.Sessions;
    public SessionState SessionState => _resources.SessionState;
    public Color InputStartColor => _resources.InputStartColor;
    public Color InputEndColor => _resources.InputEndColor;
    public void Print(object obj) => _resources.Print(obj);
    public double OneTickSize => _resources.OneTickSize;
    public int InputProminentMedianPercentage => _resources.InputProminentMedianPercentage;
    public bool InputRightToLeft => _resources.InputRightToLeft;
    public bool InputDisableHistogram => _resources.InputDisableHistogram;
    public bool InputEnableDevelopingPoC => _resources.InputEnableDevelopingPoC;
    public bool InputEnableDevelopingValueAtHighValueAtLow => _resources.InputEnableDevelopingValueAtHighValueAtLow;

    #endregion
}