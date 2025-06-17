using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo;

public interface IAlertManagerResources
{
    INotifications Notifications { get; }
    Bars Bars { get; }
    IAccount Account { get; }
    Chart Chart { get; }
    DateTime Time { get; }
    Symbol Symbol { get; }
    //--
    bool InputAlertNative { get; }
    SoundType InputAlertSoundType { get; }
    bool InputAlertEmail { get; }
    string InputAlertEmailFrom { get; }
    string InputAlertEmailTo { get; }
    bool InputAlertArrows { get; }
    AlertCheckBar InputAlertCheckBar { get; }
    bool InputAlertForValueArea { get; }
    bool InputAlertForMedian { get; }
    bool InputAlertForSinglePrint { get; }
    bool InputAlertOnPriceBreak { get; }
    bool InputAlertOnCandleClose { get; }
    bool InputAlertOnGapCross { get; }
    Color InputAlertArrowColorPb { get; }
    Color InputAlertArrowColorCc { get; }
    Color InputAlertArrowColorGc { get; }
}

public class AlertManager : IAlertManagerResources
{
    private readonly IAlertManagerResources _resources;
    private DateTime _lastAlertTime = DateTime.MinValue; // For CheckPreviousBar alerts;
    private double _closePrev = double.NaN;             // Previous price value for Price Break alerts.
    private int _arrowsCounter;                      // Counter for naming of alert arrows.
    private DateTime _lastAlertTimeCandleCross = DateTime.MinValue;
    private DateTime _lastAlertTimeGapCross = DateTime.MinValue; // For CheckCurrentBar alerts.

    public AlertManager(IAlertManagerResources resources)
    {
        _resources = resources;
    }

    /// <summary>
    /// Checks all alert conditions and issues alerts if needed.
    /// </summary>
    /// <param name="index"></param>
    public void CheckAlerts(int index)
    {
        // No need to check further if no alert method is chosen.
        if (!InputAlertNative && !InputAlertEmail && !InputAlertArrows)// && !AlertPush)
            return;

        // Skip alerts if alerts are disabled for Median, for Value Area, and for Single Print rays.
        if (!InputAlertForMedian && !InputAlertForValueArea && !InputAlertForSinglePrint)
            return;

        // Skip alerts if no cross type is chosen.
        if (!InputAlertOnPriceBreak && !InputAlertOnCandleClose && !InputAlertOnGapCross)
            return;

        // Skip alerts if only closed bar should be checked and it has already been done.
        if (InputAlertCheckBar == AlertCheckBar.CheckPreviousBar && _lastAlertTime == Bars[index].OpenTime) return;

        // Cycle through rays starts here.
        foreach (var ctl in Chart.FindAllObjects<ChartTrendLine>())
        {
            var objectName = ctl.Name;

            // Skip if it is either a non-ray or if this particular ray shouldn't get alerted.
            if (!(InputAlertForMedian && objectName.Contains("PointOfControlRay") ||
                  (InputAlertForValueArea && (objectName.Contains("ValueAreaRayHigh") || objectName.Contains("ValueAreaRayLow"))) ||
                  (InputAlertForSinglePrint && objectName.Contains("SinglePrintRay") && ctl.Color != Color.Transparent)))
                continue;

            // If everything is fine, go on:

            var level = ctl.Y1; //NormalizeDouble(ObjectGetDouble(ChartID(), object_name, OBJPROP_PRICE1), _Digits);

            // Price breaks, candle closes, and gap crosses using Close[0].
            if (InputAlertCheckBar == AlertCheckBar.CheckCurrentBar)
            {
                if (InputAlertOnPriceBreak) // Price break alerts.
                {
                    if (!double.IsNaN(_closePrev) && ((Bars[index].Close >= level && _closePrev < level) || (Bars[index].Close <= level && _closePrev > level)))
                    {
                        DoAlerts(AlertTypes.PriceBreak, objectName);
                        if (InputAlertArrows) CreateArrowObject($"ArrPB{objectName}", Bars[index].OpenTime, Bars[index].Close, InputAlertArrowColorPb, ChartIconType.Circle);
                    }
                    _closePrev = Bars[index].Close;
                }

                if (InputAlertOnCandleClose) // Candle close alerts.
                {
                    if ((Bars[index].Close >= level && Bars[index - 1].Close < level) || (Bars[index].Close <= level && Bars[index - 1].Close > level))
                    {
                        DoAlerts(AlertTypes.CandleCloseCrossover, objectName);
                        if (InputAlertArrows) CreateArrowObject($"ArrCC{objectName}", Bars[index].OpenTime, Bars[index].Close, InputAlertArrowColorCc, ChartIconType.Square);
                    }
                }

                if (InputAlertOnGapCross) // Gap cross alerts.
                {
                    if ((Bars[index].Open > level && Bars[index - 1].High < level) || (Bars[index].Open < level && Bars[index - 1].Low > level))
                    {
                        DoAlerts(AlertTypes.GapCrossover, objectName);
                        if (InputAlertArrows) CreateArrowObject($"ArrGC{objectName}", Bars[index].OpenTime, level, InputAlertArrowColorGc, ChartIconType.Diamond);
                    }
                }
            }
            // Price breaks (using pre-previous High and previous Close), candle closes, and gap crosses using Close[1].
            else if (InputAlertCheckBar == AlertCheckBar.CheckPreviousBar)
            {
                if (InputAlertOnPriceBreak) // Price break alerts.
                {
                    if ((Bars[index - 1].High >= level && Bars[index - 1].Close < level && Bars[index - 2].Close < level) ||
                        (Bars[index - 1].Low <= level && Bars[index - 1].Close > level && Bars[index - 2].Close > level))
                    {
                        DoAlerts(AlertTypes.PriceBreak, objectName);
                        if (InputAlertArrows) CreateArrowObject($"ArrPB{objectName}", Bars[index - 1].OpenTime, Bars[index - 1].Close, InputAlertArrowColorPb, ChartIconType.Circle);
                    }
                }

                if (InputAlertOnCandleClose) // Candle close alerts.
                {
                    if ((Bars[index - 1].Close >= level && Bars[index - 2].Close < level) || (Bars[index - 1].Close <= level && Bars[index - 2].Close > level))
                    {
                        DoAlerts(AlertTypes.CandleCloseCrossover, objectName);
                        if (InputAlertArrows) CreateArrowObject($"ArrCC{objectName}", Bars[index - 1].OpenTime, Bars[index - 1].Close, InputAlertArrowColorCc, ChartIconType.Square);
                    }
                }

                if (InputAlertOnGapCross) // Gap cross alerts.
                {
                    if ((Bars[index - 1].Low > level && Bars[index - 2].High < level) || (Bars[index - 2].Low > level && Bars[index - 1].High < level))
                    {
                        DoAlerts(AlertTypes.GapCrossover, objectName);
                        if (InputAlertArrows) CreateArrowObject($"ArrGC{objectName}", Bars[index - 1].OpenTime, level, InputAlertArrowColorGc, ChartIconType.Diamond);
                    }
                }

                _lastAlertTime = Bars[index].OpenTime;
            }
        }
    }
    
    /// <summary>
    /// Issues alerts based on the alert type and includes object name in the message.
    /// </summary>
    /// <param name="alertType"></param>
    /// <param name="objectName"></param>
    private void DoAlerts(AlertTypes alertType, string objectName)
    {
        // Price Breaks for Current Bar should not be be checked for LastAlertTime.
        // Candle Close and Gap Cross for Current Bar need to be checked against LastAlertTime.
        // All CheckPreviousBar alerts can use a single LastAlertTime (they either trigger at the start of the bar or not). The actual check is performed in CheckAlerts().
        // Using TimeCurrent() for all CheckCurrentBar alerts.
        // Using Time[0] for all CheckPreviousBar alerts.

        // Check last alert time for Candle Close alert type.
        if (alertType == AlertTypes.CandleCloseCrossover && InputAlertCheckBar == AlertCheckBar.CheckCurrentBar && Time <= _lastAlertTimeCandleCross)
            return;

        // Check last alert time for Gap Cross alert type.
        if (alertType == AlertTypes.GapCrossover && InputAlertCheckBar == AlertCheckBar.CheckCurrentBar && Time <= _lastAlertTimeGapCross)
            return;

        var subject = $"Market Profile: {Symbol.Name} {alertType} on {objectName}";

        if (InputAlertNative)
        {
            Alert(subject);
        }

        if (InputAlertEmail)
        {
            var emailSubject = subject;
            var emailBody = $"{Account.BrokerName} - {Account.Number}\r\n\r\n{subject}";

            Notifications.SendEmail(InputAlertEmailFrom, InputAlertEmailTo, emailSubject, emailBody);
        }

        // Remember that this alert has already been sent. For CheckPreviousBar, this is done in CheckAlerts().
        if (alertType == AlertTypes.CandleCloseCrossover && InputAlertCheckBar == AlertCheckBar.CheckCurrentBar)
            _lastAlertTimeCandleCross = Time;
        else if (alertType == AlertTypes.GapCrossover && InputAlertCheckBar == AlertCheckBar.CheckCurrentBar)
            _lastAlertTimeGapCross = Time;
    }
    
    // Creates an arrow object and sets its properties.
    public void CreateArrowObject(string name, DateTime time, double price, Color colour, ChartIconType type)
    {
        var objName = $"name{_arrowsCounter}";
        _arrowsCounter++;
        Chart.DrawIcon(objName, type, time, price, colour);
    }

    #region Resources

    public void Alert(string alertText)
    {
        Notifications.PlaySound(InputAlertSoundType);
        MessageBox.Show(alertText, "Alert", MessageBoxButton.OK);  
    }
    public INotifications Notifications => _resources.Notifications;
    public Bars Bars => _resources.Bars;
    public IAccount Account => _resources.Account;
    public Chart Chart => _resources.Chart;
    public DateTime Time => _resources.Time;
    public Symbol Symbol => _resources.Symbol;
    //--
    public bool InputAlertNative => _resources.InputAlertNative;
    public SoundType InputAlertSoundType => _resources.InputAlertSoundType;
    public bool InputAlertEmail => _resources.InputAlertEmail;
    public string InputAlertEmailFrom => _resources.InputAlertEmailFrom;
    public string InputAlertEmailTo => _resources.InputAlertEmailTo; 
    public bool InputAlertArrows => _resources.InputAlertArrows;
    public AlertCheckBar InputAlertCheckBar => _resources.InputAlertCheckBar;
    public bool InputAlertForValueArea => _resources.InputAlertForValueArea;
    public bool InputAlertForMedian => _resources.InputAlertForMedian;
    public bool InputAlertForSinglePrint => _resources.InputAlertForSinglePrint;
    public bool InputAlertOnPriceBreak => _resources.InputAlertOnPriceBreak;
    public bool InputAlertOnCandleClose => _resources.InputAlertOnCandleClose;
    public bool InputAlertOnGapCross => _resources.InputAlertOnGapCross;
    public Color InputAlertArrowColorPb => _resources.InputAlertArrowColorPb;
    public Color InputAlertArrowColorCc => _resources.InputAlertArrowColorCc;
    public Color InputAlertArrowColorGc => _resources.InputAlertArrowColorGc;
    public DataSeries Open => Bars.OpenPrices;
    public DataSeries High => Bars.HighPrices;
    public DataSeries Low => Bars.LowPrices;
    public DataSeries Close => Bars.ClosePrices;
    public TimeSeries Times => Bars.OpenTimes;
    public int Index => Bars.Count - 1; 

    #endregion
}