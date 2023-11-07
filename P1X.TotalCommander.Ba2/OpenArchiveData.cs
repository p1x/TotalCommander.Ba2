using System.Runtime.InteropServices;

namespace P1X.TotalCommander.Ba2;

public unsafe struct OpenArchiveData(tOpenArchiveData* archiveData)
{
    private string? _arcName;
    
    public string? ArcName => _arcName ??= Marshal.PtrToStringAnsi(new IntPtr(archiveData->ArcName));

    [JetBrains.Annotations.ValueProvider("P1X.TotalCommander.Ba2.WcxHead.Errors")]
    public int OpenResult
    {
        get => archiveData->OpenResult;
        set => archiveData->OpenResult = value;
    }

    [JetBrains.Annotations.ValueProvider("P1X.TotalCommander.Ba2.WcxHead")]
    public int OpenMode
    {
        get => archiveData->OpenMode;
        set => archiveData->OpenMode = value;
    }
}