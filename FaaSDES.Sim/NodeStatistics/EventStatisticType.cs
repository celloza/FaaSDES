namespace FaaSDES.Sim.NodeStatistics
{
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