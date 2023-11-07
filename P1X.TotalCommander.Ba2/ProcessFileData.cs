using System.Runtime.InteropServices;

namespace P1X.TotalCommander.Ba2;

public struct ProcessFileData(IntPtr hArcData, IntPtr destPath, IntPtr destName)
{
    private string? _destinationPath;
    private string? _destinationName;
    public ArchiveState GetState() => ArchiveState.FromPtr(hArcData, out _);
    public string? DestinationPath => _destinationPath ??= Marshal.PtrToStringAnsi(destPath);
    public string? DestinationName => _destinationName ??= Marshal.PtrToStringAnsi(destName);
}