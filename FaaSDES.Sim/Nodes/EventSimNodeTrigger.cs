namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Enumeration that defines the <see cref="EventSimNode"/>'s trigger
    /// based on those defined in the BPMN standard.
    /// </summary>
    public enum EventSimNodeTrigger
    {
        None,
        Message,
        Timer,
        Conditional,
        Link,
        Signal,
        Error,
        Escalation,
        Termination,
        Compensation,
        Cancel,
        Multiple,
        MutlipleParallel
            
    }
}
