using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo;

public interface IMarketProfileRendererSettings
{
    bool InputColorBullBear { get; }
    Color InputStartColor { get; }
    Color InputEndColor { get; }
    Color InputMedianColor { get; }
    Color InputValueAreaSidesColor { get; }
    Color InputValueAreaHighLowColor { get; }
    LineStyle InputMedianStyle { get; }
    LineStyle InputMedianRayStyle { get; }
    LineStyle InputValueAreaSidesStyle { get; }
    LineStyle InputValueAreaHighLowStyle { get; }
    LineStyle InputValueAreaRayHighLowStyle { get; }
    int InputMedianWidth { get; }
    int InputMedianRayWidth { get; }
    int InputValueAreaSidesWidth { get; }
    int InputValueAreaHighLowWidth { get; }
    int InputValueAreaRayHighLowWidth { get; }
    SessionsToDrawRays InputShowValueAreaRays { get; }
    SessionsToDrawRays InputShowMedianRays { get; }
    WaysToStopRays InputRaysUntilIntersection { get; }
    // bool InputHideRaysFromInvisibleSessions { get; }
    int InputTimeShiftMinutes { get; }
    bool InputShowKeyValues { get; }
    Color InputKeyValuesColor { get; }
    int InputKeyValuesSize { get; }
    SinglePrintType InputShowSinglePrint { get; }
    Color InputSinglePrintColor { get; }
    bool InputSinglePrintRays { get; }
    LineStyle InputSinglePrintRayStyle { get; }
    int InputSinglePrintRayWidth { get; }
    Color InputProminentMedianColor { get; }
    bool InputDrawOnlyHistogramBorder { get; }
    LineStyle InputProminentMedianStyle { get; } 
    int InputProminentMedianWidth { get; }
    bool InputShowTpoCounts { get; }
    Color InputTpoCountAboveColor { get; }
    Color InputTpoCountBelowColor { get; }
    // bool InputRightToLeft { get; }
    MarketProfileCalculator Calculator { get; }
    Bars Bars { get; }
    IndicatorDataSeries OutputDevelopingPoC { get; }
    IndicatorDataSeries OutputDevelopingVah { get; }
    IndicatorDataSeries OutputDevelopingVaL { get; }
}

public class MarketProfileRenderer : IMarketProfileResources, IMarketProfileRendererSettings
{
    private readonly IMarketProfileResources _resources;
    private readonly IMarketProfileRendererSettings _settings;
    private readonly string _format;
    private readonly TimeSpan _barTimeSpan;

    public MarketProfileRenderer(IMarketProfileResources resources, IMarketProfileRendererSettings settings)
    {
        _resources = resources;
        _settings = settings;
        _format = $"0.{new string('0', Symbol.Digits)}";
        _barTimeSpan = Helpers.GetBarTimeSpan(TimeFrame);
    }

    public List<ChartObject> RenderProfile(MatrixPoint[,] matrix)
    {
        // var stopwatch = Stopwatch.StartNew();
        
        var result = new List<ChartObject>();
        
        var matrixStartTime = DateTime.MinValue.Ticks;
        
        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            if (matrix[row, 0] == null)
                continue;
            
            matrixStartTime = matrix[row, 0].StartTime.Ticks;
            break;
        }
        
        for (var row = 0; row < matrix.GetLength(0); row++)
        for (var column = 0; column < matrix.GetLength(1); column++)
        {
            var point = matrix[row, column];

            if (point == null)
                continue;

            var nextPoint = matrix[row, Math.Min(column + 1, matrix.GetLength(1) - 1)];

            if (InputDrawOnlyHistogramBorder && nextPoint != null)
                continue;

            //Override the color with the direction if InputColorBullBear is true
            Color color;
            
            if (InputColorBullBear)
                color = point.Direction == Direction.Up ? InputStartColor : InputEndColor;
            else
                color = point.Color;

            var r = Chart.DrawRectangle(
                $"{matrixStartTime}-row{row}column{column}", 
                point.StartTime.AddMinutes(-InputTimeShiftMinutes), 
                point.Bottom, 
                point.EndTime.AddMinutes(-InputTimeShiftMinutes), 
                point.Top, 
                color);
            //r.LineStyle = LineStyle.DotsVeryRare;

            r.Thickness = 0;
            r.IsFilled = true;
            //r.IsInteractive = true;
            
            result.Add(r);
        }
        
        // stopwatch.Stop();
        // Print($"RenderProfile Took: {stopwatch.ElapsedMilliseconds} ms");
        //
        return result;
    }

    public List<ChartObject> RenderHorizontallyFlippedProfile(MatrixPoint[,] matrix, MarketProfileSession session)
    {
        var result = new List<ChartObject>();

        // Find the earliest and latest times in the matrix
        DateTime earliestTime = DateTime.MaxValue;
        DateTime latestTime = DateTime.MinValue;
        
        var startPoint = session.Range.Bars.Last().OpenTime;

        for (var row = 0; row < matrix.GetLength(0); row++)
        for (var column = 0; column < matrix.GetLength(1); column++)
        {
            var point = matrix[row, column];
            if (point == null)
                continue;

            earliestTime = DateTime.Compare(earliestTime, point.StartTime) > 0 ? point.StartTime : earliestTime;
            latestTime = DateTime.Compare(latestTime, point.EndTime) < 0 ? point.EndTime : latestTime;
        }

        var timeOffset = startPoint - latestTime;

        var matrixStartTime = DateTime.MinValue.Ticks;
        
        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            if (matrix[row, 0] == null)
                continue;
            
            matrixStartTime = matrix[row, 0].StartTime.Ticks;
            break;
        }

        // Draw each point with flipped times
        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            for (var column = 0; column < matrix.GetLength(1); column++)
            {
                var point = matrix[row, column];
                if (point == null)
                    continue;
                
                var nextPoint = matrix[row, Math.Min(column + 1, matrix.GetLength(1) - 1)];
                
                if (InputDrawOnlyHistogramBorder && nextPoint != null)
                    continue;

                // Calculate flipped times
                TimeSpan startOffset = point.StartTime - earliestTime;
                TimeSpan endOffset = point.EndTime - earliestTime;
                
                DateTime flippedStartTime = latestTime - endOffset + timeOffset;
                DateTime flippedEndTime = latestTime - startOffset + timeOffset;

                // Set color
                Color color;
                if (InputColorBullBear)
                    color = point.Direction == Direction.Up ? InputStartColor : InputEndColor;
                else
                    color = point.Color;

                var r = Chart.DrawRectangle(
                    $"{matrixStartTime}-{row}{column}",
                    flippedStartTime.AddMinutes(-InputTimeShiftMinutes),
                    point.Bottom,
                    flippedEndTime.AddMinutes(-InputTimeShiftMinutes),
                    point.Top,
                    color);

                r.Thickness = 0;
                r.IsFilled = true;

                result.Add(r);
            }
        }

        return result;
    }
    
    public List<ChartObject> RenderValueArea(MarketProfileModel model)
    {
        var vah = model.ValueAreaHigh;
        var val = model.ValueAreaLow;
        var pointOfControl = model.PointOfControl;
        var pointOfControlRowEndTime = Calculator.GetPointOfControlRowEndTime(model.Matrix);
        
        var matrixStartTime = DateTime.MinValue.Ticks;
        
        for (var row = 0; row < model.Matrix.GetLength(0); row++)
        {
            if (model.Matrix[row, 0] == null)
                continue;
            
            matrixStartTime = model.Matrix[row, 0].StartTime.Ticks;
            break;
        }
        
        //Print($"Median: {median} val: {val} vah: {vah}");
        //Chart.DrawVerticalLine($"Left-{Guid.NewGuid()}", model.EndTime, Color.White);

        var result = new List<ChartObject>
        {
            Chart.DrawTrendLine($"VAH-{matrixStartTime}", model.StartTime, val, pointOfControlRowEndTime, val, InputValueAreaHighLowColor, InputValueAreaHighLowWidth, InputValueAreaHighLowStyle),
            Chart.DrawTrendLine($"VAL-{matrixStartTime}", model.StartTime, vah, pointOfControlRowEndTime, vah, InputValueAreaHighLowColor, InputValueAreaHighLowWidth, InputValueAreaHighLowStyle),
            Chart.DrawTrendLine($"Left-Boundary-{matrixStartTime}", model.StartTime, val, model.StartTime, vah, InputValueAreaSidesColor, InputValueAreaSidesWidth, InputValueAreaSidesStyle),
            Chart.DrawTrendLine($"Right-Boundary-{matrixStartTime}", pointOfControlRowEndTime, val, pointOfControlRowEndTime, vah, InputValueAreaSidesColor, InputValueAreaSidesWidth, InputValueAreaSidesStyle),
            Chart.DrawTrendLine($"PointOfControl-{matrixStartTime}", model.StartTime, pointOfControl, pointOfControlRowEndTime, pointOfControl, InputMedianColor, InputMedianWidth, InputMedianStyle),
        };

        return result;
    }

    public List<ChartObject> RenderHorizontallyFlippedValueArea(MarketProfileModel model, MarketProfileSession session)
    {
        var valueAreaObjects = RenderValueArea(model);
        var pointOfControlRowEndTime = Calculator.GetPointOfControlRowEndTime(model.Matrix);
        var timeSpan = pointOfControlRowEndTime - model.StartTime;

        foreach (var obj in valueAreaObjects)
        {
            if (obj is not ChartTrendLine trendLine)
                continue;

            if (trendLine.Name.Contains("VAH") || trendLine.Name.Contains("VAL"))
            {
                trendLine.Time1 = session.Range.Bars.Last().OpenTime;
                trendLine.Time2 = trendLine.Time1.Add(-timeSpan);
            }
            else if (trendLine.Name.Contains("Left"))
            {
                trendLine.Time1 = session.Range.Bars.Last().OpenTime;
                trendLine.Time2 = session.Range.Bars.Last().OpenTime;
            }
            else if (trendLine.Name.Contains("Right"))
            {
                trendLine.Time1 = session.Range.Bars.Last().OpenTime.Add(-timeSpan);
                trendLine.Time2 = session.Range.Bars.Last().OpenTime.Add(-timeSpan);
            }
            else if (trendLine.Name.Contains("PointOfControl"))
            {
                trendLine.Time1 = session.Range.Bars.Last().OpenTime;
                trendLine.Time2 = trendLine.Time1.Add(-timeSpan);
            }
        }

        return valueAreaObjects;
    }

    public List<ChartObject> RenderPointOfControlRays(MarketProfileModel model, int index)
    {
        var result = new List<ChartObject>();
        var pointOfControlRowEndTime = Calculator.GetPointOfControlRowEndTime(model.Matrix);
        var medianRowEndTime1MinAfter = pointOfControlRowEndTime.AddMinutes(1);
        
        switch (InputShowMedianRays)
        {
            case SessionsToDrawRays.None:
                break;
            case SessionsToDrawRays.Previous when index == 1:
            case SessionsToDrawRays.Current when index == 0:
            case SessionsToDrawRays.PreviousCurrent when index <= 1:
            case SessionsToDrawRays.AllPrevious when index > 0:
            case SessionsToDrawRays.All:
                var pointOfControlRay = Chart.DrawTrendLine($"PointOfControlRay-Previous-{model.StartTime}", pointOfControlRowEndTime, model.PointOfControl, medianRowEndTime1MinAfter, model.PointOfControl, InputMedianColor, InputMedianRayWidth, InputMedianRayStyle);
                pointOfControlRay.ExtendToInfinity = true;
                result.Add(pointOfControlRay);
                break;
        }
        
        return result;
    }

    public void RenderDevelopingVals(Dictionary<DateTime, double> developingVals)
    {
        int i = 0;
        foreach (var (col, valRow) in developingVals)
        {
            if (i == developingVals.Count - 1)
                break;
            
            var idx = Bars.OpenTimes.GetIndexByTime(col);
            
            OutputDevelopingVaL[idx] = valRow;
            i++;
        }
    }

    public void ClearDevelopingVals(Dictionary<DateTime, double> developingVals)
    {
        foreach (var (col, valRow) in developingVals)
        {
            var idx = Bars.OpenTimes.GetIndexByTime(col);
            OutputDevelopingVaL[idx] = double.NaN;
        }
    }
    
    public void RenderDevelopingVahs(Dictionary<DateTime, double> developingVahs)
    {
        int i = 0;
        foreach (var (col, vahRow) in developingVahs)
        {
            if (i == developingVahs.Count - 1)
                break;
            
            var idx = Bars.OpenTimes.GetIndexByTime(col);
            
            OutputDevelopingVah[idx] = vahRow;
            //lastVah = vahRow;
            i++;
        }
    }

    public void ClearDevelopingVahs(Dictionary<DateTime, double> developingVahs)
    {
        foreach (var (col, vahRow) in developingVahs)
        {
            var idx = Bars.OpenTimes.GetIndexByTime(col);
            OutputDevelopingVah[idx] = double.NaN;
        }
    }
    
    public void RenderDevelopingPoc(Dictionary<DateTime, double> developingPoC)
    {
        int i = 0;
        foreach (var (col, pocRow) in developingPoC)
        {
            if (i == developingPoC.Count - 1)
                break;
            
            var idx = Bars.OpenTimes.GetIndexByTime(col);
            
            OutputDevelopingPoC[idx] = pocRow;
            i++;
        }
    }

    public void ClearDevelopingPoc(Dictionary<DateTime, double> developingPoC)
    {
        foreach (var (col, pocRow) in developingPoC)
        {
            var idx = Bars.OpenTimes.GetIndexByTime(col);
            OutputDevelopingPoC[idx] = double.NaN;
        }
    }

    public void PostProcessPointOfControlRays(List<MarketProfileSession> sessions)
    {
        var sortedSessions = sessions.OrderBy(s => s.Model.StartTime).ToList();
        
        for (var i = 0; i < sortedSessions.Count; i++)
        {
            if (sortedSessions[i].MedianRays != null)
                PostProcessPointOfControlRay(sortedSessions[i].MedianRays.FirstOrDefault(), i, sortedSessions);
        }
    }

    public void PostProcessPointOfControlRay(ChartObject pointOfControlRay, int index, List<MarketProfileSession> sessions)
    {
        if (pointOfControlRay is not ChartTrendLine trendLine)
            return;
        
        //let's try only with index = 0
        var session = sessions[index];

        if (index == sessions.Count - 1)
            return;

        for (var i = index + 1; i < sessions.Count; i++)
        {
            var nextSession = sessions[i];
            
            var nextSessionTopPrice = MarketProfileCalculator.GetTopPrice(nextSession.Model.Matrix);
            var nextSessionBottomPrice = MarketProfileCalculator.GetBottomPrice(nextSession.Model.Matrix);

            //For debugging
            // switch (InputRaysUntilIntersection)
            // {
            //     case WaysToStopRays.StopNoRays:
            //         break;
            //     case WaysToStopRays.StopAllRays:
            //         break;
            //     case WaysToStopRays.StopAllRaysExceptPrevSession:
            //         if (index != sessions.Count - 2)
            //             trendLine.Color = Color.HotPink;
            //         break;
            //     case WaysToStopRays.StopOnlyPreviousSession:
            //         if (index == sessions.Count - 2)
            //             trendLine.Color = Color.Lime;
            //         break;
            // }

            if (session.Model.PointOfControl >= nextSessionBottomPrice && session.Model.PointOfControl <= nextSessionTopPrice)
            {
                switch (InputRaysUntilIntersection)
                {
                    case WaysToStopRays.StopNoRays:
                        trendLine.ExtendToInfinity = true;
                        return;
                    case WaysToStopRays.StopAllRays:
                        trendLine.ExtendToInfinity = false;
                        trendLine.Time2 = nextSession.Model.StartTime;
                        return;
                    case WaysToStopRays.StopAllRaysExceptPrevSession:
                        if (index != sessions.Count - 2)
                        {
                            trendLine.ExtendToInfinity = false;
                            trendLine.Time2 = nextSession.Model.StartTime;;
                        }
                        return;
                    case WaysToStopRays.StopOnlyPreviousSession:
                        if (index == sessions.Count - 2)
                        {
                            trendLine.ExtendToInfinity = false;
                            trendLine.Time2 = nextSession.Model.StartTime;
                        }
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public List<ChartObject> RenderValueAreaRays(MarketProfileModel model, int index)
    {
        var result = new List<ChartObject>();
        var pointOfControlRowEndTime = Calculator.GetPointOfControlRowEndTime(model.Matrix);
        //since this is a ray, I just need 1 min after to draw in the right direction
        var pointOfControlRowEndTime1MinAfter = pointOfControlRowEndTime.AddMinutes(1);

        var matrixStartTime = DateTime.MinValue.Ticks;

        for (var row = 0; row < model.Matrix.GetLength(0); row++)
        {
            if (model.Matrix[row, 0] == null)
                continue;
            
            matrixStartTime = model.Matrix[row, 0].StartTime.Ticks;
            break;
        }
        
        switch (InputShowValueAreaRays)
        {
            case SessionsToDrawRays.None:
                break;
            case SessionsToDrawRays.Previous when index == 1:
            case SessionsToDrawRays.Current when index == 0:
            case SessionsToDrawRays.PreviousCurrent when index <= 1:
            case SessionsToDrawRays.AllPrevious when index > 0:
            case SessionsToDrawRays.All:
                var varHigh = Chart.DrawTrendLine($"ValueAreaRayHigh-Previous-{matrixStartTime}", pointOfControlRowEndTime, model.ValueAreaHigh, pointOfControlRowEndTime1MinAfter, model.ValueAreaHigh, InputValueAreaHighLowColor, InputValueAreaRayHighLowWidth, InputValueAreaRayHighLowStyle);
                varHigh.ExtendToInfinity = true;
                result.Add(varHigh);
                    
                var varLow = Chart.DrawTrendLine($"ValueAreaRayLow-Previous-{matrixStartTime}", pointOfControlRowEndTime, model.ValueAreaLow, pointOfControlRowEndTime1MinAfter, model.ValueAreaLow, InputValueAreaHighLowColor, InputValueAreaRayHighLowWidth, InputValueAreaRayHighLowStyle);
                varLow.ExtendToInfinity = true;
                result.Add(varLow);
                break;
        }
        
        return result;
    }

    public void PostProcessValueAreaRays(List<MarketProfileSession> sessions)
    {
        var sortedSessions = sessions.OrderBy(s => s.Model.StartTime).ToList();
        
        for (var i = 0; i < sortedSessions.Count; i++)
        {
            if (sortedSessions[i].ValueAreaRays == null)
                continue;
            
            //PostProcessValueAreaRay(sessions[i].ValueAreaRays, i, sessions);
            PostProcessValueAreaRay(sortedSessions[i].ValueAreaRays.FirstOrDefault(), i, sortedSessions);
            PostProcessValueAreaRay(sortedSessions[i].ValueAreaRays.LastOrDefault(), i, sortedSessions);
        }
    }

    private void PostProcessValueAreaRay(ChartObject valueAreaRay, int index, List<MarketProfileSession> sessions)
    {
        if (valueAreaRay is not ChartTrendLine trendLine)
            return;
        
        var referencePrice = trendLine.Name.Contains("High") ? sessions[index].Model.ValueAreaHigh : sessions[index].Model.ValueAreaLow;
        
        if (index == sessions.Count - 1)
            return;

        for (var i = index + 1; i < sessions.Count; i++)
        {
            var nextSession = sessions[i];
            
            var nextSessionTopPrice = MarketProfileCalculator.GetTopPrice(nextSession.Model.Matrix);
            var nextSessionBottomPrice = MarketProfileCalculator.GetBottomPrice(nextSession.Model.Matrix);

            //For debugging
            // switch (InputRaysUntilIntersection)
            // {
            //     case WaysToStopRays.StopNoRays:
            //         break;
            //     case WaysToStopRays.StopAllRays:
            //         break;
            //     case WaysToStopRays.StopAllRaysExceptPrevSession:
            //         if (index != sessions.Count - 2)
            //             trendLine.Color = Color.HotPink;
            //         break;
            //     case WaysToStopRays.StopOnlyPreviousSession:
            //         if (index == sessions.Count - 2)
            //             trendLine.Color = Color.Lime;
            //         break;
            // }

            if (referencePrice >= nextSessionBottomPrice && referencePrice <= nextSessionTopPrice)
            {
                switch (InputRaysUntilIntersection)
                {
                    case WaysToStopRays.StopNoRays:
                        trendLine.ExtendToInfinity = true;
                        return;
                    case WaysToStopRays.StopAllRays:
                        trendLine.ExtendToInfinity = false;
                        trendLine.Time2 = nextSession.Model.StartTime;
                        return;
                    case WaysToStopRays.StopAllRaysExceptPrevSession:
                        if (index != sessions.Count - 2)
                        {
                            trendLine.ExtendToInfinity = false;
                            trendLine.Time2 = nextSession.Model.StartTime;;
                        }
                        return;
                    case WaysToStopRays.StopOnlyPreviousSession:
                        if (index == sessions.Count - 2)
                        {
                            trendLine.ExtendToInfinity = false;
                            trendLine.Time2 = nextSession.Model.StartTime;
                        }
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public List<ChartObject> RenderKeyValues(MarketProfileModel model)
    {
        var result = new List<ChartObject>();
        
        if (!InputShowKeyValues) 
            return result;
        
        var vah = model.ValueAreaHigh;
        var val = model.ValueAreaLow;
        var pointOfControl = model.PointOfControl;

        var matrixStartTime = DateTime.MinValue.Ticks;
        
        for (var row = 0; row < model.Matrix.GetLength(0); row++)
        {
            if (model.Matrix[row, 0] == null)
                continue;
            
            matrixStartTime = model.Matrix[row, 0].StartTime.Ticks;
            break;
        }
        
        var vahText = Chart.DrawText($"VAH-Text-{matrixStartTime}", $"{vah.ToString(_format)}", model.StartTime, vah, InputKeyValuesColor);
            
        vahText.FontSize = InputKeyValuesSize;
        vahText.HorizontalAlignment = HorizontalAlignment.Left;
        vahText.VerticalAlignment = VerticalAlignment.Center;
            
        var valText = Chart.DrawText($"VAL-Text-{matrixStartTime}", $"{val.ToString(_format)}", model.StartTime, val, InputKeyValuesColor);
        valText.FontSize = InputKeyValuesSize;
        valText.HorizontalAlignment = HorizontalAlignment.Left;
        valText.VerticalAlignment = VerticalAlignment.Center;
            
        var pointOfControlText = Chart.DrawText($"PointOfControl-Text-{matrixStartTime}", $"{pointOfControl.ToString(_format)}", model.StartTime, pointOfControl, InputKeyValuesColor);
        pointOfControlText.FontSize = InputKeyValuesSize;
        pointOfControlText.HorizontalAlignment = HorizontalAlignment.Left;
        pointOfControlText.VerticalAlignment = VerticalAlignment.Center;
        
        result.Add(vahText);
        result.Add(valText);
        result.Add(pointOfControlText);
        
        return result;
    }

    public List<ChartObject> RenderHorizontallyFlippedKeyValues(MarketProfileModel model, MarketProfileSession session)
    {
        var result = new List<ChartObject>();

        if (!InputShowKeyValues)
            return result;

        result = RenderKeyValues(model);

        foreach (var obj in result)
        {
            if (obj is not ChartText text)
                continue;

            text.HorizontalAlignment = HorizontalAlignment.Right;
            text.Time = session.Range.Bars.Last().OpenTime;
        }
        
        return result;
    }

    public List<ChartObject> RenderTpoCounts(MarketProfileModel model)
    {
        var result = new List<ChartObject>();
        
        if (!InputShowTpoCounts)
            return result;
        
        var pointOfControl = model.PointOfControl;
        var pointOfControlRowEndTime = Calculator.GetPointOfControlRowEndTime(model.Matrix);
        
        if (model.TpoCountAbove == -1 || model.TpoCountBelow == -1)
            return result;
        
        var matrixStartTime = model.Matrix.GetLength(0) > 0 ? model.Matrix[0, 0].StartTime.Ticks : DateTime.MinValue.Ticks;
        
        var tpoCountAbove = Chart.DrawText($"TPO-Above-{matrixStartTime}", $"{model.TpoCountAbove}", pointOfControlRowEndTime, pointOfControl, InputTpoCountAboveColor);
        tpoCountAbove.FontSize = InputKeyValuesSize;
        tpoCountAbove.HorizontalAlignment = HorizontalAlignment.Right;
        tpoCountAbove.VerticalAlignment = VerticalAlignment.Top;
        
        var tpoCountBelow = Chart.DrawText($"TPO-Below-{matrixStartTime}", $"{model.TpoCountBelow}", pointOfControlRowEndTime, pointOfControl, InputTpoCountBelowColor);
        tpoCountBelow.FontSize = InputKeyValuesSize;
        tpoCountBelow.HorizontalAlignment = HorizontalAlignment.Right;
        tpoCountBelow.VerticalAlignment = VerticalAlignment.Bottom;
        
        result.Add(tpoCountAbove);
        result.Add(tpoCountBelow);
        
        return result;
    }

    public List<ChartObject> RenderHorizontallyFlippedTpoCounts(MarketProfileModel model, MarketProfileSession session)
    {
        if (!InputShowTpoCounts) 
            return new List<ChartObject>();
        
        var matrix = model.Matrix;
        
        var earliestTime = DateTime.MaxValue;
        var latestTime = DateTime.MinValue;
        
        for (var row = 0; row < matrix.GetLength(0); row++)
        for (var column = 0; column < matrix.GetLength(1); column++)
        {
            var point = matrix[row, column];
            if (point == null) 
                continue;
            
            earliestTime = point.StartTime < earliestTime ? point.StartTime : earliestTime;
            latestTime = point.EndTime > latestTime ? point.EndTime : latestTime;
        }
        
        var startPoint = session.Range.Bars.Last().OpenTime;
        var timeOffset = startPoint - latestTime;
        
        DateTime Flip(DateTime t) => latestTime - (t - earliestTime) + timeOffset;
        
        var pointOfControl = model.PointOfControl;
        var pointOfControlRowEndTime = Calculator.GetPointOfControlRowEndTime(model.Matrix);
        var flippedEnd = Flip(pointOfControlRowEndTime);
        var result = new List<ChartObject>();
        
        if (model.TpoCountAbove == -1 || model.TpoCountBelow == -1)
            return result;
        
        double matrixStartTime = DateTime.MinValue.Ticks;
        
        for (var row = 0; row < model.Matrix.GetLength(0); row++)
        {
            if (model.Matrix[row, 0] == null)
                continue;
            
            matrixStartTime = model.Matrix[row, 0].StartTime.Ticks;
            break;
        }
        
        var tpoCountAbove = Chart.DrawText($"TPO-Above-Flipped-{matrixStartTime}", $"{model.TpoCountAbove}", flippedEnd, pointOfControl, InputTpoCountAboveColor);
        tpoCountAbove.FontSize = InputKeyValuesSize;
        tpoCountAbove.HorizontalAlignment = HorizontalAlignment.Right;
        tpoCountAbove.VerticalAlignment = VerticalAlignment.Top;
        
        var tpoCountBelow = Chart.DrawText($"TPO-Below-Flipped-{matrixStartTime}", $"{model.TpoCountBelow}", flippedEnd, pointOfControl, InputTpoCountBelowColor);
        tpoCountBelow.FontSize = InputKeyValuesSize;
        tpoCountBelow.HorizontalAlignment = HorizontalAlignment.Right;
        tpoCountBelow.VerticalAlignment = VerticalAlignment.Bottom;
        result.Add(tpoCountAbove);
        result.Add(tpoCountBelow);

        return result;
    }

    public List<ChartObject> RenderSinglePrints(MarketProfileModel model)
    {
        return InputShowSinglePrint switch
        {
            SinglePrintType.No => new List<ChartObject>(),
            SinglePrintType.LeftSide => RenderLeftSideSinglePrints(model),
            SinglePrintType.RightSide => RenderRightSideSinglePrints(model),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public List<ChartObject> RenderRightSideSinglePrints(MarketProfileModel model)
    {
        var result = new List<ChartObject>();
        
        //must draw 1 rectangle per each tpo point
        //must draw 1 ray but going backwards in time

        for (var index = 0; index < model.SinglePrints.Count; index++)
        {
            var singlePrint = model.SinglePrints[index];
            var startIndex = singlePrint.BottomTpoIndex;
            var endIndex = singlePrint.TopTpoIndex;

            for (var i = startIndex; i <= endIndex; i++)
            {
                var tpoBlock = model.Matrix[i, 0];
                var tpoStartTime = tpoBlock.StartTime;
                var tpoEndTime = tpoBlock.EndTime;
                var tpoHigh = tpoBlock.Top;
                var tpoLow = tpoBlock.Bottom;

                result.Add(Chart.DrawRectangle($"SinglePrintBlock-{i}-{model.StartTime}-{index}", tpoStartTime, tpoHigh, tpoEndTime, tpoLow, InputSinglePrintColor));
            }

            //now draw the rays backwards in time, starting from startTime
            var s = singlePrint.StartTime;
            var top = singlePrint.High;
            var bottom = singlePrint.Low;
            //

            result.Add(Chart.DrawTrendLine($"SinglePrintRay-top-{model.StartTime}-{index}", s, top, s.AddYears(-10), bottom, InputSinglePrintColor, InputSinglePrintRayWidth, InputSinglePrintRayStyle));
            result.Add(Chart.DrawTrendLine($"SinglePrintRay-bottom-{model.StartTime}-{index}", s, bottom, s.AddYears(-10), top, InputSinglePrintColor, InputSinglePrintRayWidth, InputSinglePrintRayStyle));
        }

        return result;
    }

    public List<ChartObject> RenderLeftSideSinglePrints(MarketProfileModel model)
    {
        var result = new List<ChartObject>();

        for (var index = 0; index < model.SinglePrints.Count; index++)
        {
            var singlePrint = model.SinglePrints[index];
            //get time of 2 bars behind model.StartTime
            var rectangleStartTime = Bars.OpenTimes[Bars.OpenTimes.GetIndexByTime(model.StartTime) - 2];
            var high = singlePrint.High;
            var low = singlePrint.Low;

            var singlePrintRay = Chart.DrawRectangle($"SinglePrintBlock-{model.StartTime}-{index}", rectangleStartTime, high, model.StartTime, low, InputSinglePrintColor);
            singlePrintRay.Thickness = 0;
            singlePrintRay.IsFilled = true;

            result.Add(singlePrintRay);

            if (InputSinglePrintRays)
            {
                var topLine = Chart.DrawTrendLine($"SinglePrintRay-top-{model.StartTime}-{index}", model.StartTime, high, model.StartTime.AddSeconds(1), high, InputSinglePrintColor, InputSinglePrintRayWidth,
                    InputSinglePrintRayStyle);
                topLine.ExtendToInfinity = true;

                result.Add(topLine);

                var bottomLine = Chart.DrawTrendLine($"SinglePrintRay-bottom-{model.StartTime}-{index}", model.StartTime, low, model.StartTime.AddSeconds(1), low, InputSinglePrintColor,
                    InputSinglePrintRayWidth, InputSinglePrintRayStyle);
                bottomLine.ExtendToInfinity = true;

                result.Add(bottomLine);
            }
        }

        return result;
    }
    
    public ChartObject RenderProminentLine(MarketProfileModel profileModel)
    {
        if (!profileModel.IsProminentLine)
            return null;
        
        var startTime = profileModel.StartTime;
        var endTime = Calculator.GetPointOfControlRowEndTime(profileModel.Matrix);
        var pointOfControl = profileModel.PointOfControl;
        
        var matrixStartTime = profileModel.Matrix.GetLength(0) > 0 ? profileModel.Matrix[0, 0].StartTime.Ticks : DateTime.MinValue.Ticks;
        
        return Chart.DrawTrendLine($"ProminentLine-{matrixStartTime}", startTime, pointOfControl, endTime, pointOfControl, InputProminentMedianColor, InputProminentMedianWidth, InputProminentMedianStyle);
    }

    public ChartObject RenderHorizontallyFlippedProminentLine(MarketProfileModel model, MarketProfileSession session)
    {
        if (!model.IsProminentLine)
            return null;
        
        var pointOfControlRowEndTime = Calculator.GetPointOfControlRowEndTime(model.Matrix);
        var timeSpan = pointOfControlRowEndTime - model.StartTime;
        
        var valueAreaObj = RenderProminentLine(model);
        
        if (valueAreaObj is not ChartTrendLine trendLine)
            return null;
        
        trendLine.Time1 = session.Range.Bars.Last().OpenTime;
        trendLine.Time2 = trendLine.Time1.Add(-timeSpan);
        
        return trendLine;
    }
    
    public void DeleteObjects(IEnumerable<ChartObject> objects)
    {
        if (objects == null)
            return;
        
        foreach (var obj in objects)
        {
            if (obj == null)
                continue;
            
            Chart.RemoveObject(obj.Name);
        }
    }
    
    public void DeleteObject(ChartObject obj)
    {
        if (obj == null)
            return;
        
        Chart.RemoveObject(obj.Name);
    }
    
    public void ClearOutputsTillDate(DateTime start)
    {
        for (int i = 0; Bars.OpenTimes[i] < start; i++)
        {
            OutputDevelopingPoC[i] = double.NaN;
            OutputDevelopingVah[i] = double.NaN;
            OutputDevelopingVaL[i] = double.NaN;
        }
    }
    
    public void ClearOutputsOnRange(DateTime start, DateTime end)
    {
        for (int i = Bars.OpenTimes.GetIndexByTime(start); i < Bars.OpenTimes.GetIndexByTime(end); i++)
        {
            OutputDevelopingPoC[i] = double.NaN;
            OutputDevelopingVah[i] = double.NaN;
            OutputDevelopingVaL[i] = double.NaN;
        }
    }

    public void ClearOutputsAfterDate(DateTime start)
    {
        for (int i = Bars.OpenTimes.GetIndexByTime(start); i < Bars.OpenTimes.Count; i++)
        {
            OutputDevelopingPoC[i] = double.NaN;
            OutputDevelopingVah[i] = double.NaN;
            OutputDevelopingVaL[i] = double.NaN;
        }
    }
    
    public void DeleteAllFromSession(MarketProfileSession session)
    {
        DeleteObjects(session.Profile);
        DeleteObjects(session.ValueArea);
        DeleteObjects(session.ValueAreaRays);
        DeleteObjects(session.MedianRays);
        DeleteObjects(session.KeyValues);
        DeleteObjects(session.TpoCounts);
        DeleteObjects(session.SinglePrints);
        DeleteObject(session.ProminentLine);
                
        session.ClearObjects();
    }
    
    public void DeleteAllSessions(List<MarketProfileSession> sessions)
    {
        foreach (var session in sessions)
            DeleteAllFromSession(session);
    }

    #region SettingsAndResources

    public double ValueAreaPercentage => _resources.ValueAreaPercentage;
    public TimeFrame TimeFrame => _resources.TimeFrame;
    public Bars Bars => _resources.Bars;
    public IndicatorDataSeries OutputDevelopingPoC => _settings.OutputDevelopingPoC;
    public IndicatorDataSeries OutputDevelopingVah => _settings.OutputDevelopingVah;
    public IndicatorDataSeries OutputDevelopingVaL => _settings.OutputDevelopingVaL;

    public Symbol Symbol => _resources.Symbol;
    public Chart Chart => _resources.Chart;
    public double OneTickSize => _resources.OneTickSize;
    public void Print(object message) => _resources.Print(message);
    public bool InputColorBullBear => _settings.InputColorBullBear;
    public Color InputStartColor => _settings.InputStartColor;
    public Color InputEndColor => _settings.InputEndColor;
    public Color InputMedianColor => _settings.InputMedianColor;
    public Color InputValueAreaSidesColor => _settings.InputValueAreaSidesColor;
    public Color InputValueAreaHighLowColor => _settings.InputValueAreaHighLowColor;
    public LineStyle InputMedianStyle => _settings.InputMedianStyle;
    public LineStyle InputMedianRayStyle => _settings.InputMedianRayStyle;
    public LineStyle InputValueAreaSidesStyle => _settings.InputValueAreaSidesStyle;
    public LineStyle InputValueAreaHighLowStyle => _settings.InputValueAreaHighLowStyle;
    public LineStyle InputValueAreaRayHighLowStyle => _settings.InputValueAreaRayHighLowStyle;
    public int InputMedianWidth => _settings.InputMedianWidth;
    public int InputMedianRayWidth => _settings.InputMedianRayWidth;
    public int InputValueAreaSidesWidth => _settings.InputValueAreaSidesWidth;
    public int InputValueAreaHighLowWidth => _settings.InputValueAreaHighLowWidth;
    public int InputValueAreaRayHighLowWidth => _settings.InputValueAreaRayHighLowWidth;
    public SessionsToDrawRays InputShowValueAreaRays => _settings.InputShowValueAreaRays;
    public SessionsToDrawRays InputShowMedianRays => _settings.InputShowMedianRays;
    public WaysToStopRays InputRaysUntilIntersection => _settings.InputRaysUntilIntersection;
    public int InputTimeShiftMinutes => _settings.InputTimeShiftMinutes;
    public bool InputShowKeyValues => _settings.InputShowKeyValues;
    public Color InputKeyValuesColor => _settings.InputKeyValuesColor;
    public int InputKeyValuesSize => _settings.InputKeyValuesSize;
    public SinglePrintType InputShowSinglePrint => _settings.InputShowSinglePrint;
    public Color InputSinglePrintColor => _settings.InputSinglePrintColor;
    public bool InputSinglePrintRays => _settings.InputSinglePrintRays;
    public LineStyle InputSinglePrintRayStyle => _settings.InputSinglePrintRayStyle;
    public int InputSinglePrintRayWidth => _settings.InputSinglePrintRayWidth;
    public Color InputProminentMedianColor => _settings.InputProminentMedianColor;
    public bool InputDrawOnlyHistogramBorder => _settings.InputDrawOnlyHistogramBorder;
    public LineStyle InputProminentMedianStyle => _settings.InputProminentMedianStyle;
    public int InputProminentMedianWidth => _settings.InputProminentMedianWidth;
    public bool InputShowTpoCounts => _settings.InputShowTpoCounts;
    public Color InputTpoCountAboveColor => _settings.InputTpoCountAboveColor;
    public Color InputTpoCountBelowColor => _settings.InputTpoCountBelowColor;
    public MarketProfileCalculator Calculator => _settings.Calculator;

    #endregion
}
