using System.Runtime.InteropServices;

namespace P1X.TotalCommander.Ba2;

public unsafe struct OpenArchiveData(tOpenArchiveData* archiveData, bool isWChar)
{
    private string? _arcName;
    
    public string? ArcName
    {
        get
        {
            var ptr = new IntPtr(archiveData->ArcName);
            return _arcName ??= isWChar ? Marshal.PtrToStringUni(ptr) : Marshal.PtrToStringAnsi(ptr);
        }
    }

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