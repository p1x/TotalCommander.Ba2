namespace P1X.TotalCommander.Ba2;

public readonly struct ReadHeaderData(IntPtr hArcData)
{
    public ArchiveState GetState() => ArchiveState.FromPtr(hArcData, out _);
}