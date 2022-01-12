namespace FaaSDES.Sim.NodeStatistics
{
    /// <summary>
    /// Enumeration that defines the <see cref="EventStatistic"/>'s type.
    /// </summary>
    public enum EventStatisticType
    {
        NodeExecuted,
        QueueAbandon,
        QueueOverflow,
        MaxQueueLengthIncrease,
        TokenGenerated,
        TokenJoinedWaitingQueue,
        TokenLeftWaitingQueue,
        TokenJoinedExecutionQueue,
        TokenLeftExecutionQueue

    }
}