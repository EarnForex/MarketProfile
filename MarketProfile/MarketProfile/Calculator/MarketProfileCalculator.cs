using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo;

public interface IMarketProfileResources
{
    double ValueAreaPercentage { get; }
    
     TimeFrame TimeFrame { get; }
     Bars Bars { get; }
     Symbol Symbol { get; }
     Chart Chart { get; }
     
     double OneTickSize { get; }
     
     void Print(object message);
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Colors should not be tied to the calculations, but so far I found it convenient to do so
/// Inside MarketProfileRenderer, the color might be overriden with the direction of the bar
/// </remarks>
public class MarketProfileCalculator : IMarketProfileResources
{
    private readonly IMarketProfileResources _resources;
    private readonly TimeSpan _barTimeSpan;

    public MarketProfileCalculator(IMarketProfileResources resources)
    {
        _resources = resources;
        _barTimeSpan = Helpers.GetBarTimeSpan(TimeFrame);
    }

    /// <summary>
    /// Get a section of bars between startTime and endTime
    /// </summary>
    public IEnumerable<Bar> GetBarSection(DateTime startTime, DateTime endTime)
    {
        return Bars.Where(x => x.OpenTime >= startTime && x.OpenTime <= endTime);
    }
    
    /// <summary>
    /// The number of histograms is determined by the number of slices you want from the range you have
    /// </summary>
    /// <param name="bars"></param>
    /// <param name="numberOfSlices"></param>
    /// <returns></returns>
    public double GetHistogramSize(Bar[] bars, int numberOfSlices)
    {
        var max = bars.Select(x => x.High).Max();
        var min = bars.Select(x => x.Low).Min();
        
        var range = max - min;
        return range / numberOfSlices;
    }

    public int GetSlices(Bar[] bars)
    {
        var highMinusLow = bars.Max(x => x.High) - bars.Min(x => x.Low);
        return (int) (highMinusLow / OneTickSize);
    }
    
    /// <summary>
    /// This way of calculating the matrix is a bit different from the one above, because it fills and piles on each iteration
    /// instead of looping though the rows first, now it loops through the columns first, when it's done, it piles the matrix to the left
    /// this is useful to calculate the developing PoC and VAH/VAL
    /// </summary>
    /// <param name="bars"></param>
    /// <param name="numberOfSlices"></param>
    /// <param name="startColor"></param>
    /// <param name="endColor"></param>
    /// <returns></returns>
    public MarketProfileModel FillAndPileMatrixCalculation(Bar[] bars, int numberOfSlices, Color startColor, Color endColor)
    {
        var rows = numberOfSlices;
        var columns = bars.Length;
        var startTime = bars.First().OpenTime;
        var endTime = bars.Last().OpenTime;
        var histogramSize = GetHistogramSize(bars, numberOfSlices);
        var bottomOfRegion = bars.Select(x => x.Low).Min();
        var colors = Helpers.GenerateColorGradient(startColor, endColor, columns);
        var marketProfileModel = new MarketProfileModel();
        
        var matrix = new MatrixPoint[rows, columns];

        for (var column = 0; column < columns; column++)
        {
            var bar = bars[column];
            var pointsPerColumn = new Dictionary<int,MatrixPoint>();
            
            for (var row = 0; row < rows; row++)
            {
                var sliceBottom = bottomOfRegion + histogramSize * row;
                var sliceTop = sliceBottom + histogramSize;

                if (sliceTop < bar.Low || sliceBottom > bar.High)
                    continue;
                
                var point = new MatrixPoint
                {
                    Direction = bar.Close > bar.Open ? Direction.Up : Direction.Down,
                    StartTime = bar.OpenTime,
                    EndTime = column + 1 < columns ? bars[column + 1].OpenTime: bar.OpenTime.Add(_barTimeSpan),
                    Top = sliceBottom + histogramSize,
                    Bottom = sliceBottom,
                    Color = colors[column]
                };
                
                pointsPerColumn.Add(row, point);
            }
            
            //now we will pile all the points to the existing matrix
            foreach (var point in pointsPerColumn)
            {
                var row = point.Key;
                var pointToPile = point.Value;
                
                var newColumnIndex = GetTotalRowPoints(matrix, row);
                var newColumnBar = bars[newColumnIndex];
                
                var piledPoint = new MatrixPoint
                {
                    Direction = pointToPile.Direction,
                    StartTime = newColumnBar.OpenTime,
                    EndTime = newColumnIndex + 1 < columns ? bars[newColumnIndex + 1].OpenTime : newColumnBar.OpenTime.Add(_barTimeSpan),
                    Top = pointToPile.Top,
                    Bottom = pointToPile.Bottom,
                    Color = pointToPile.Color
                };
                
                matrix[row, newColumnIndex] = piledPoint;
            }
            
            var pocRowIndex = GetPointOfControlRowIndex(matrix);
            
            if (matrix[pocRowIndex, 0] == null)
            {
                marketProfileModel.DevelopingPoC.Add(bar.OpenTime, double.NaN);
                marketProfileModel.DevelopingAreaHigh.Add(bar.OpenTime, double.NaN);
                marketProfileModel.DevelopingAreaLow.Add(bar.OpenTime, double.NaN);
                continue;
            }

            var poc = GetPointOfControlPrice(matrix, pocRowIndex);
            var (valueAreaHigh, valueAreaLow) = GetValueArea(matrix, ValueAreaPercentage);
                
            marketProfileModel.DevelopingPoC.Add(bar.OpenTime, poc);
            if (!double.IsNaN(valueAreaHigh))
                marketProfileModel.DevelopingAreaHigh.Add(bar.OpenTime, valueAreaHigh);
            
            if (!double.IsNaN(valueAreaLow))
                marketProfileModel.DevelopingAreaLow.Add(bar.OpenTime, valueAreaLow);
        }
        
        marketProfileModel.Matrix = matrix;
        marketProfileModel.StartTime = startTime;
        marketProfileModel.EndTime = endTime;

        return marketProfileModel;
    }
    
    public MarketProfileModel FillAndPileMatrixCalculationCropped(
    Bar[] bars,
    int numberOfSlices,
    Color startColor,
    Color endColor,
    double cropTop,
    double cropBottom)
    {
        var rows = numberOfSlices;
        var columns = bars.Length;
        var startTime = bars.First().OpenTime;
        var endTime = bars.Last().OpenTime;
        var histogramSize = GetHistogramSize(bars, numberOfSlices);
        var bottomOfRegion = bars.Select(x => x.Low).Min();
        var colors = Helpers.GenerateColorGradient(startColor, endColor, columns);
        var marketProfileModel = new MarketProfileModel();

        var matrix = new MatrixPoint[rows, columns];

        for (var column = 0; column < columns; column++)
        {
            var bar = bars[column];
            var pointsPerColumn = new Dictionary<int, MatrixPoint>();

            for (var row = 0; row < rows; row++)
            {
                var sliceBottom = bottomOfRegion + histogramSize * row;
                var sliceTop = sliceBottom + histogramSize;

                // Only include slices within the crop range
                if (sliceTop < bar.Low || sliceBottom > bar.High)
                    continue;
                if (sliceTop > cropTop || sliceBottom < cropBottom)
                    continue;

                var point = new MatrixPoint
                {
                    Direction = bar.Close > bar.Open ? Direction.Up : Direction.Down,
                    StartTime = bar.OpenTime,
                    EndTime = column + 1 < columns ? bars[column + 1].OpenTime : bar.OpenTime.Add(_barTimeSpan),
                    Top = sliceBottom + histogramSize,
                    Bottom = sliceBottom,
                    Color = colors[column]
                };

                pointsPerColumn.Add(row, point);
            }

            foreach (var point in pointsPerColumn)
            {
                var row = point.Key;
                var pointToPile = point.Value;

                var newColumnIndex = GetTotalRowPoints(matrix, row);
                var newColumnBar = bars[newColumnIndex];

                var piledPoint = new MatrixPoint
                {
                    Direction = pointToPile.Direction,
                    StartTime = newColumnBar.OpenTime,
                    EndTime = newColumnIndex + 1 < columns ? bars[newColumnIndex + 1].OpenTime : newColumnBar.OpenTime.Add(_barTimeSpan),
                    Top = pointToPile.Top,
                    Bottom = pointToPile.Bottom,
                    Color = pointToPile.Color
                };

                matrix[row, newColumnIndex] = piledPoint;
            }

            var pocRowIndex = GetPointOfControlRowIndex(matrix);

            if (matrix[pocRowIndex, 0] == null)
            {
                marketProfileModel.DevelopingPoC.Add(bar.OpenTime, double.NaN);
                marketProfileModel.DevelopingAreaHigh.Add(bar.OpenTime, double.NaN);
                marketProfileModel.DevelopingAreaLow.Add(bar.OpenTime, double.NaN);
                continue;
            }

            var poc = GetPointOfControlPrice(matrix, pocRowIndex);
            var (valueAreaHigh, valueAreaLow) = GetValueArea(matrix, ValueAreaPercentage);

            marketProfileModel.DevelopingPoC.Add(bar.OpenTime, poc);
            if (!double.IsNaN(valueAreaHigh))
                marketProfileModel.DevelopingAreaHigh.Add(bar.OpenTime, valueAreaHigh);

            if (!double.IsNaN(valueAreaLow))
                marketProfileModel.DevelopingAreaLow.Add(bar.OpenTime, valueAreaLow);
        }

        marketProfileModel.Matrix = matrix;
        marketProfileModel.StartTime = startTime;
        marketProfileModel.EndTime = endTime;

        return marketProfileModel;
    }

    /// <summary>
    /// Fills a matrix with the bars' values (it would overlap the bars if you draw this matrix on the chart, little blocks would be drawn)
    /// </summary>
    /// <param name="bars"></param>
    /// <param name="numberOfSlices"></param>
    /// <param name="startColor"></param>
    /// <param name="endColor"></param>
    /// <returns></returns>
    /// <remarks>This is the old method, the new one works better for calculating the developing POC, VAH and VAL</remarks>
    [Obsolete("Use FillAndPileMatrixCalculation instead")]
    public MarketProfileModel FillMatrix(Bar[] bars, int numberOfSlices, Color startColor, Color endColor)
    {
        var rows = numberOfSlices;
        var columns = bars.Length;
        var startTime = bars.First().OpenTime;
        var endTime = bars.Last().OpenTime;
        var histogramSize = GetHistogramSize(bars, numberOfSlices);
        var bottomOfRegion = bars.Select(x => x.Low).Min();
        var colors = Helpers.GenerateColorGradient(startColor, endColor, columns);

        var matrix = new MatrixPoint[rows, columns];

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var bar = bars[column];
                //slice goes from the bottom to the top
                var sliceBottom = bottomOfRegion + histogramSize * row;
                var sliceTop = sliceBottom + histogramSize;

                if (sliceTop < bar.Low || sliceBottom > bar.High)
                    continue;
                
                var point = new MatrixPoint
                {
                    Direction = bar.Close > bar.Open ? Direction.Up : Direction.Down,
                    StartTime = bar.OpenTime,
                    EndTime = column + 1 < columns ? bars[column + 1].OpenTime: bar.OpenTime.Add(_barTimeSpan),
                    Top = sliceBottom + histogramSize,
                    Bottom = sliceBottom,
                    Color = colors[column]
                };
                
                matrix[row, column] = point;
            }
        }

        return new MarketProfileModel
        {
            Matrix = matrix,
            StartTime = startTime,
            EndTime = endTime
        };
    }
    
    /// <summary>
    /// Moves all matrix points to the left, so it looks like a profile
    /// </summary>
    /// <param name="bars"></param>
    /// <param name="matrix"></param>
    /// <returns></returns>
    [Obsolete("We are piling the matrix on every bar now, so this is not needed anymore")]
    // ReSharper disable once UnusedMember.Global
    public MatrixPoint[,] PileMatrixPointsToLeft(Bar[] bars, MatrixPoint[,] matrix)
    {
        var piled = new MatrixPoint[matrix.GetLength(0), matrix.GetLength(1)];

        /// we're traversing each row === of the matrix, getting the values of each column
        for (var rowIndex = 0; rowIndex < matrix.GetLength(0); rowIndex++)
        {
            var columnValues = new List<MatrixPoint>();

            for (var columnIndex = 0; columnIndex < matrix.GetLength(1); columnIndex++)
            {
                var point = matrix[rowIndex, columnIndex];
                
                if (point == null)
                    continue;
                
                columnValues.Add(point);
            }

            for (var newColumnIndex = 0; newColumnIndex < columnValues.Count; newColumnIndex++)
            {
                var rowValue = columnValues[newColumnIndex];
                var bar = bars[newColumnIndex];
                
                var piledPoint = new MatrixPoint
                {
                    Direction = rowValue.Direction,
                    StartTime = bar.OpenTime,
                    EndTime = newColumnIndex + 1 < columnValues.Count ? bars[newColumnIndex + 1].OpenTime : bar.OpenTime.Add(_barTimeSpan),
                    Top = rowValue.Top,
                    Bottom = rowValue.Bottom,
                    Color = rowValue.Color
                };
                
                piled[rowIndex, newColumnIndex] = piledPoint;
            }
        }
        
        return piled;
    }
    
    /// <summary>
    /// Moves the matrix points to the right, so it looks like a profile
    /// </summary>
    /// <param name="bars"></param>
    /// <param name="matrix"></param>
    /// <returns></returns>
    [Obsolete("We are piling the matrix on every bar now, so this is not needed anymore")]
    // ReSharper disable once UnusedMember.Global
    public MatrixPoint[,] PileMatrixPointsToRight(Bar[] bars, MatrixPoint[,] matrix)
    {
        var piled = new MatrixPoint[matrix.GetLength(0), matrix.GetLength(1)];

        for (var rowIndex = 0; rowIndex < matrix.GetLength(0); rowIndex++)
        {
            var columnValues = new List<MatrixPoint>();

            for (var columnIndex = 0; columnIndex < matrix.GetLength(1); columnIndex++)
            {
                var point = matrix[rowIndex, columnIndex];

                if (point == null)
                    continue;

                columnValues.Add(point);
            }

            for (var newColumnIndex = 0; newColumnIndex < columnValues.Count; newColumnIndex++)
            {
                var rowValue = columnValues[newColumnIndex];
                // Place at the rightmost available columns
                var bar = bars[bars.Length - columnValues.Count + newColumnIndex];

                var piledPoint = new MatrixPoint
                {
                    Direction = rowValue.Direction,
                    StartTime = bar.OpenTime,
                    EndTime = newColumnIndex + 1 < columnValues.Count ? bars[newColumnIndex + 1].OpenTime : bar.OpenTime.Add(_barTimeSpan),
                    Top = rowValue.Top,
                    Bottom = rowValue.Bottom,
                    Color = rowValue.Color
                };

                piled[rowIndex, bars.Length - columnValues.Count + newColumnIndex] = piledPoint;
            }
        }

        return piled;
    }
    
    public static double GetPointOfControlPrice(MatrixPoint[,] matrix, int rowIndex)
    {
        return (matrix[rowIndex, 0].Top + matrix[rowIndex, 0].Bottom) / 2.0;
    }
    
    public static int GetPointOfControlRowIndex(MatrixPoint[,] matrix)
    {
        var maxPoints = 0;
        var pointOfControlRow = 0;
        
        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            var point = 0;
            
            for (var column = 0; column < matrix.GetLength(1); column++)
            {
                if (matrix[row, column] == null)
                    continue;

                point++;
            }
            
            if (point > maxPoints)
            {
                pointOfControlRow = row;
                maxPoints = point;
            }
        }

        return pointOfControlRow;
    }

    /// <summary>
    /// The median is the price level at which 50% of the total volume occurred above and 50% occurred below. 
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public double GetMedianPrice(MatrixPoint[,] matrix)
    {
        //from bottom to top
        var halfOfBlocks = GetTotalBlocks(matrix) / 2;
        var blocks = 0;
        var price = 0.0;

        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            for (var column = 0; column < matrix.GetLength(1); column++)
            {
                if (matrix[row, column] == null)
                    continue;
                
                blocks++;
                price = (matrix[row, column].Top + matrix[row, column].Bottom) / 2.0;
            }
            
            if (blocks >= halfOfBlocks)
                break;
        }

        return price;
    }

    public int GetMedianRowIndex(MatrixPoint[,] matrix)
    {
        //from bottom to top
        var halfOfBlocks = GetTotalBlocks(matrix) / 2;
        var blocks = 0;

        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            for (var column = 0; column < matrix.GetLength(1); column++)
            {
                if (matrix[row, column] == null)
                    continue;
                
                blocks++;
            }

            if (blocks > halfOfBlocks)
                return row;
        }
        
        throw new Exception("Median row not found");
    }
    
    public DateTime GetMedianRowEndTime(MatrixPoint[,] matrix)
    {
        var medianRow = GetMedianRowIndex(matrix);
        var endTimeColumnIndex = 0;

        for (var columnIndex = 0; columnIndex < matrix.GetLength(1); columnIndex++)
        {
            if (matrix[medianRow, columnIndex] == null)
                break;
            
            endTimeColumnIndex = columnIndex;
        }
        
        return matrix[medianRow, endTimeColumnIndex].EndTime;
    }

    public DateTime GetPointOfControlRowEndTime(MatrixPoint[,] matrix)
    {
        var pointOfControlRow = GetPointOfControlRowIndex(matrix);
        var endTimeColumnIndex = 0;
        
        for (var columnIndex = 0; columnIndex < matrix.GetLength(1); columnIndex++)
        {
            if (matrix[pointOfControlRow, columnIndex] == null)
                break;
            
            endTimeColumnIndex = columnIndex;
        }
        
        return matrix[pointOfControlRow, endTimeColumnIndex].EndTime;
    }

    public static (int totalTopBlocks, int totalBottomBlocks) GetValuesAroundPointOfControl(MatrixPoint[,] matrix)
    {
        //from bottom to top
        var topBlocks = 0;
        var bottomBlocks = 0;
        
        var pointOfControlIndex = GetPointOfControlRowIndex(matrix);
        
        if (matrix[pointOfControlIndex, 0] == null)
            return (-1, -1);

        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            if (matrix[row, 0] == null)
                continue;

            var rowTotalTpo = GetTotalRowPoints(matrix, row);

            if (row < pointOfControlIndex)
                bottomBlocks += rowTotalTpo;
            else if (row > pointOfControlIndex) 
                topBlocks += rowTotalTpo;
        }

        return (topBlocks, bottomBlocks);
    }

    public (double vahPrice, double valPrice) GetValueArea(MatrixPoint[,] matrix, double percentage = 0.7)
    {
        var totalBlocks = GetTotalBlocks(matrix);
        var targetBlocks = (int)Math.Round(totalBlocks * percentage);

        var pointOfControlRowIndex = GetPointOfControlRowIndex(matrix);
        var blockCounter = GetTotalRowPoints(matrix, pointOfControlRowIndex);
        var topCounter = pointOfControlRowIndex;
        var bottomCounter = pointOfControlRowIndex;

        while (blockCounter < targetBlocks)
        {
            var canMoveUp = topCounter + 1 < matrix.GetLength(0) && matrix[topCounter + 1, 0] != null;
            var canMoveDown = bottomCounter - 1 >= 0 && matrix[bottomCounter - 1, 0] != null;

            if (!canMoveUp && !canMoveDown)
                break;

            int topRowPoints = 0, bottomRowPoints = 0;

            if (canMoveUp)
            {
                topCounter++;
                topRowPoints = GetTotalRowPoints(matrix, topCounter);
            }
            if (canMoveDown)
            {
                bottomCounter--;
                bottomRowPoints = GetTotalRowPoints(matrix, bottomCounter);
            }

            blockCounter += topRowPoints + bottomRowPoints;
        }

        var vah = matrix[topCounter, 0] != null ? matrix[topCounter, 0].Middle : double.NaN;
        var val = matrix[bottomCounter, 0] != null ? matrix[bottomCounter, 0].Middle : double.NaN;
        return (vah, val);
    }

    public bool IsProminentLine(MatrixPoint[,] matrix, double prominencePercentage)
    {
        var totalBlocks = GetTotalBlocks(matrix);
        var targetBlocks = totalBlocks * prominencePercentage;
        
        var pointOfControlRowIndex = GetPointOfControlRowIndex(matrix);
        var totalPointOfControlBlocks = GetTotalRowPoints(matrix, pointOfControlRowIndex);
        
        return totalPointOfControlBlocks > targetBlocks;
    }

    public static double GetTopPrice(MatrixPoint[,] matrix)
    {
        return matrix[matrix.GetLength(0) - 1, 0].Top;
    }

    public static double GetBottomPrice(MatrixPoint[,] matrix)
    {
        return matrix[0, 0].Bottom;
    }

    public static SinglePrint[] GetSinglePrints(MatrixPoint[,] matrix)
    {
        var singlePrints = new List<SinglePrint>();
        var singlePrintRowIndexes = new List<int>();
        
        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            var columns = 0;
            
            for (var column = 0; column < matrix.GetLength(1); column++)
            {
                if (matrix[row, column] == null)
                    break;
                
                columns++;
            }
            
            if (columns == 1)
            {
                singlePrintRowIndexes.Add(row);
            }
        }

        var groups = Helpers.GroupAdjacent(singlePrintRowIndexes.ToArray());

        foreach (var group in groups)
        {
            var startTime = matrix[group[0], 0].StartTime;
            var endTime = matrix[group[0], 0].EndTime;
            var high = matrix[group[^1], 0].Top;
            var low = matrix[group[0], 0].Bottom;
            var topTpoIndex = group[^1];
            var bottomTpoIndex = group[0];
            
            singlePrints.Add(new SinglePrint(startTime, endTime, high, low, topTpoIndex, bottomTpoIndex));
        }
        
        return singlePrints.ToArray();
    }

    private static int GetTotalBlocks(MatrixPoint[,] matrix)
    {
        var totalBlocks = 0;

        for (var row = 0; row < matrix.GetLength(0); row++)
        for (var column = 0; column < matrix.GetLength(1); column++)
        {
            if (matrix[row, column] == null)
                continue;
                
            totalBlocks++;
        }
        
        return totalBlocks;
    }

    private static int GetTotalRowPoints(MatrixPoint[,] matrix, int row)
    {
        var point = 0;
            
        for (var column = 0; column < matrix.GetLength(1); column++)
        {
            if (matrix[row, column] == null)
                continue;

            point++;
        }
        
        return point;
    }

    #region Resources

    public double ValueAreaPercentage => _resources.ValueAreaPercentage;
    public TimeFrame TimeFrame => _resources.TimeFrame;
    public Bars Bars => _resources.Bars;
    public Symbol Symbol => _resources.Symbol;
    public Chart Chart => _resources.Chart;
    public double OneTickSize => _resources.OneTickSize;
    public void Print(object message) => _resources.Print(message);

    #endregion
}