﻿using System.Runtime.InteropServices;
using SharpBSABA2;

namespace P1X.TotalCommander.Ba2;

public class ArchiveState(Archive archive)
{
    public Archive Archive { get; } = archive;
    public int NextFileIndex { get; set; }
    public int CurrentFileIndex { get; set; } = -1;

    public static ArchiveState RestoreState(IntPtr ptr, out GCHandle gcHandle) => 
        (ArchiveState?) (gcHandle = GCHandle.FromIntPtr(ptr)).Target ?? throw new NullReferenceException("(ArchiveState?) handle.Target");
}