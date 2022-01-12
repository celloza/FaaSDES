namespace FaaSDES.Sim.Tokens
{
    /// <summary>
    /// Enumeration that defines a <see cref="SimToken"/>'s current
    /// status.
    /// </summary>
    public enum SimTokenStatus
    {
        Active,
        InQueue,
        Error,
        Complete,
        Abandoned
    }
}
