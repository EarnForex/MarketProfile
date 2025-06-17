using System;
using System.Collections.Generic;

namespace cAlgo;

public static class SessionProfileStrategyFactory
{
    public static ISessionProfileStrategy CreateDaily(IIndicatorResources resources, IRenderingModesResources renderingModesResources)
    {
        return new DailySessionProfileStrategy(resources, renderingModesResources);
    }
    
    public static ISessionProfileStrategy CreateWeekly(IIndicatorResources resources, IRenderingModesResources renderingModesResources)
    {
        return new WeeklySessionProfileStrategy(resources, renderingModesResources);
    }
    
    public static ISessionProfileStrategy CreateMonthly(IIndicatorResources resources, IRenderingModesResources renderingModesResources)
    {
        return new MonthlySessionProfileStrategy(resources, renderingModesResources);
    }
    
    public static ISessionProfileStrategy CreateQuarterly(IIndicatorResources resources, IRenderingModesResources renderingModesResources)
    {
        return new QuarterlySessionProfileStrategy(resources, renderingModesResources);
    }
    
    public static ISessionProfileStrategy CreateSemiannual(IIndicatorResources resources, IRenderingModesResources renderingModesResources)
    {
        return new SemiannualSessionProfileStrategy(resources, renderingModesResources);
    }
    
    public static ISessionProfileStrategy CreateAnnual(IIndicatorResources resources, IRenderingModesResources renderingModesResources)
    {
        return new AnnualSessionProfileStrategy(resources, renderingModesResources);
    }

    public static ISessionProfileStrategy CreateIntradaySessions(List<IntradaySessionDefinition> sessionDefinitions, IIndicatorResources resources, IRenderingModesResources renderingModesResources)
    {
        if (sessionDefinitions.Count == 0)
            throw new ArgumentException("At least one intraday session must be defined");
        
        return new IntradaySessionProfileStrategy(sessionDefinitions, resources, renderingModesResources);
    }

    public static ISessionProfileStrategy CreateRectangle(IIndicatorResources resources, DateTime startTime, DateTime endTime)
    {
        return new RectangleSessionProfileStrategy(resources, startTime, endTime);
    }
}