using System.Runtime.InteropServices;

namespace P1X.TotalCommander.Ba2;

public struct ProcessFileData(IntPtr hArcData, IntPtr destPath, IntPtr destName, bool isWChar)
{
    private string? _destinationPath;
    private string? _destinationName;
    public ArchiveState GetState() => ArchiveState.FromPtr(hArcData, out _);
    public string? DestinationPath => _destinationPath ??= isWChar ? Marshal.PtrToStringUni(destPath) : Marshal.PtrToStringAnsi(destPath);
    public string? DestinationName => _destinationName ??= isWChar ? Marshal.PtrToStringUni(destName) : Marshal.PtrToStringAnsi(destName);
}