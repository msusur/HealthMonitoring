namespace HealthMonitoring.Model
{
    public enum EndpointStatus
    {
        NotRun = -1,
        NotExists = 0,
        Offline,
        Healthy,
        Faulty,
        Unhealthy,
        TimedOut
    }
}