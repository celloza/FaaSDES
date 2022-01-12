namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Enumeration that defines the <see cref="EventSimNode"/>'s type
    /// based on those defined in the BPMN standard.
    /// </summary>
    public enum EventSimNodeType
    {
        Start,
        IntermediateThrow,
        IntermediateCatch,
        End
    }
}