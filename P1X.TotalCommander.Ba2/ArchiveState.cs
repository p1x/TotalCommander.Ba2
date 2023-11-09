using System.Runtime.InteropServices;
using SharpBSABA2;

namespace P1X.TotalCommander.Ba2;

public class ArchiveState(Archive archive, bool isExtracting)
{
    public unsafe delegate* unmanaged[Stdcall] <byte*, int, int> ProcessDataProc { get; set; }
    
    public Archive Archive { get; } = archive;
    public int CurrentFileIndex { get; set; } = -1;
    
    public bool IsExtracting { get; } = isExtracting;

    public static ArchiveState FromPtr(IntPtr ptr, out GCHandle gcHandle) => (ArchiveState?) (gcHandle = GCHandle.FromIntPtr(ptr)).Target ?? throw new NullReferenceException("(ArchiveState?) handle.Target");
    public IntPtr ToPtr(out GCHandle handle) => GCHandle.ToIntPtr(handle = GCHandle.Alloc(this));
}