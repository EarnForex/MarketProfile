namespace cAlgo;

public interface IRenderingModesResources
{
    string InputStartFromDate { get; }
    bool InputStartFromCurrentSession { get; }
    bool InputSeamlessScrollingMode { get; }
    SatSunSolution InputSaturdaySunday { get; }
}