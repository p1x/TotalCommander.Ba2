using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpBSABA2;

namespace P1X.TotalCommander.Ba2;

public static class WcxApi
{

    private static readonly ArchiveManager ArchiveManager;
    
    static WcxApi()
    {
        ArchiveManager = new ArchiveManager();
    }
    
    // HANDLE __stdcall OpenArchive (tOpenArchiveData *ArchiveData);

    /// <summary>
    /// OpenArchive should perform all necessary operations when an archive is to be opened.
    /// <code>__stdcall HANDLE STDCALL OpenArchive(tOpenArchiveData *ArchiveData);</code>
    /// </summary>
    /// <remarks>
    /// <p>OpenArchive should return a unique "handle" representing the archive.</p>
    /// <p>Most likely this will be some data structure or object address on heap, to and from which you can cast for every subsequent function call.
    /// A static or global structure will of course also work, but would interfere with thread safety and having multiple pack/unpack operations at the same time (GetBackgroundFlags).
    /// Since the interface was developed around the Windows API, an actual Windows HANDLE is meant, which is defined as pointer to void (PVOID → void*).
    /// Therefore use an explicit cast to HANDLE for your data.</p>
    /// <p>The handle should remain valid until <see cref="CloseArchive"/> is called. In there you can safely delete your data, to free the heap (if you used it).</p>
    /// <p>If an error occurs, you should return zero, and specify the error by setting <see cref="tOpenArchiveData.OpenResult"/> member of <see cref="tOpenArchiveData"/>.</p>
    ///	<p>You can use the <see cref="tOpenArchiveData"/> <paramref name="archiveData"/> structure to query information about the archive being open.
    /// If you need that information for subsequent function calls, you should store it to some location that can be accessed via the handle.</p>
    /// </remarks>
    [UnmanagedCallersOnly(EntryPoint = "OpenArchive", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static unsafe IntPtr OpenArchive(tOpenArchiveData *archiveData)
    {
        try
        {
            var pathPtr = new IntPtr(archiveData->ArcName);
            var path = Marshal.PtrToStringAnsi(pathPtr);
            
            var archive = !string.IsNullOrEmpty(path) ? ArchiveManager.Open(path) : null;
            if (archive == null)
            {
                archiveData->OpenResult = WcxHead.Errors.E_NOT_SUPPORTED;
                return IntPtr.Zero;
            }
            
            var handle = GCHandle.Alloc(archive);
            var pinnedObject = GCHandle.ToIntPtr(handle);
            
            //File.WriteAllText("log.txt", "");
            
            return pinnedObject;
        }
        catch
        {
            archiveData->OpenResult = WcxHead.Errors.E_BAD_DATA;
            return IntPtr.Zero;
        }
    }

    // int __stdcall ReadHeader (HANDLE hArcData, tHeaderData *HeaderData);
    [UnmanagedCallersOnly(EntryPoint = "ReadHeader", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static unsafe int ReadHeader(IntPtr hArcData, tHeaderData *headerData)
    {
        try
        {
            var state = ArchiveState.RestoreState(hArcData, out _); 
            var hasMoreFiles = ArchiveManager.ReadHeader(state, out var data);
            if (!hasMoreFiles)
                return WcxHead.Errors.E_END_ARCHIVE;

            data.FillNative(headerData);
        
            return 0;
        }
        catch
        {
            return WcxHead.Errors.E_BAD_DATA;
        }
    }
    
    // int __stdcall ProcessFile (HANDLE hArcData, int Operation, char *DestPath, char *DestName);     
    [UnmanagedCallersOnly(EntryPoint = "ProcessFile", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static int ProcessFile(IntPtr hArcData, int operation, IntPtr destPath, IntPtr destName)
    {
	    if (operation == WcxHead.PK_OM_LIST)
		    return 0;

	    try
	    {
		    if (operation == WcxHead.PK_SKIP)
			    return 0;

		    var state = ArchiveState.RestoreState(hArcData, out _);
		    if (operation == WcxHead.PK_TEST)
			    return WcxHead.Errors.E_NOT_SUPPORTED;

		    if (operation != WcxHead.PK_EXTRACT)
			    return WcxHead.Errors.E_NOT_SUPPORTED;
		    
		    var destPathStr = destPath == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(destPath);
		    var destNameStr = Marshal.PtrToStringAnsi(destName);

		    if (destNameStr == null)
			    return WcxHead.Errors.E_NOT_SUPPORTED;

		    var entry = destPathStr == null 
			    ? state.Archive.Files.Find(x => destNameStr.EndsWith(x.FileName, StringComparison.InvariantCultureIgnoreCase))
			    : state.Archive.Files.Find(x => string.Equals(destNameStr, x.FileName, StringComparison.InvariantCultureIgnoreCase));

		    var logPath = Path.Combine(Path.GetDirectoryName(state.Archive.FullPath), "log.txt");
		    
		    File.AppendAllText(logPath, "Entry:" + Environment.NewLine);
		    
		    File.AppendAllText(logPath,  "destName" + destNameStr + Environment.NewLine);
		    File.AppendAllText(logPath, "destPath" + destPathStr + Environment.NewLine);
		    
		    var destPathFull = destPathStr == null ? destNameStr : Path.Combine(destPathStr, destNameStr);

		    File.AppendAllText(logPath, "destPathFull" + destPathFull + Environment.NewLine);
		    
		    if (entry == null)
			    return WcxHead.Errors.E_BAD_DATA;
		    
		    File.AppendAllText(logPath, "FileName = " + entry.FileName  + Environment.NewLine);
		    File.AppendAllText(logPath, "Folder = " + entry.Folder  + Environment.NewLine);
		    File.AppendAllText(logPath, "FullPath = " + entry.FullPath  + Environment.NewLine);
		    File.AppendAllText(logPath, "FullPathOriginal = " + entry.FullPathOriginal  + Environment.NewLine);
		    File.AppendAllText(logPath, "LowerPath = " + entry.LowerPath  + Environment.NewLine);

		    var dir = Path.GetDirectoryName(destPathFull);
		    var fileName = Path.GetFileName(destPathFull);
		    entry.Extract(dir, false, fileName);
		    
		    return 0;
	    }
	    catch
	    {
		    return WcxHead.Errors.E_BAD_DATA;
	    }
    }
    
    // int __stdcall CloseArchive (HANDLE hArcData);
    [UnmanagedCallersOnly(EntryPoint = "CloseArchive", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static int CloseArchive(IntPtr hArcData)
    {
        try
        {
            var state = ArchiveState.RestoreState(hArcData, out var gcHandle);
            state.Archive.Close();
            gcHandle.Free();
            return 0;
        }
        catch
        {
            return WcxHead.Errors.E_BAD_DATA;
        }
    }
    
    // void __stdcall SetChangeVolProc (HANDLE hArcData, tChangeVolProc pChangeVolProc1);
    [UnmanagedCallersOnly(EntryPoint = "SetChangeVolProc", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static int SetChangeVolProc(IntPtr hArcData, IntPtr pChangeVolProc1)
    {
        return WcxHead.Errors.E_NOT_SUPPORTED;
    }
    
    // void __stdcall SetProcessDataProc (HANDLE hArcData, tProcessDataProc pProcessDataProc);
    [UnmanagedCallersOnly(EntryPoint = "SetProcessDataProc", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static int SetProcessDataProc(IntPtr hArcData, IntPtr pProcessDataProc)
    {
        return WcxHead.Errors.E_NOT_SUPPORTED;
    }
    
    /*


// WinCmd calls ReadHeaderEx to find out what files are in the archive
// It is called if the supported archive type may contain files >2 GB.
__stdcall	int STDCALL
	ReadHeaderEx (
		HANDLE hArcData,
		tHeaderDataEx *HeaderDataEx
		);

// WinCmd calls ReadHeader to find out what files are in the archive
__stdcall	int STDCALL
	ReadHeader (
		HANDLE hArcData,
		tHeaderData *HeaderData
		);

// ProcessFile should unpack the specified file
// or test the integrity of the archive
__stdcall int STDCALL
	ProcessFile (
		HANDLE hArcData,
		int Operation,
		char *DestPath,
		char *DestName
		);

// CloseArchive should perform all necessary operations
// when an archive is about to be closed.
__stdcall int STDCALL
	CloseArchive (
		HANDLE hArcData
		);

// This function allows you to notify user
// about changing a volume when packing files
__stdcall void STDCALL
	SetChangeVolProc (
		HANDLE hArcData,
		tChangeVolProc pChangeVolProc1
		);

// This function allows you to notify user about
// the progress when you un/pack files
__stdcall void STDCALL
	SetProcessDataProc (
		HANDLE hArcData,
		tProcessDataProc pProcessDataProc
		);

// GetPackerCaps tells WinCmd what features your packer plugin supports
__stdcall int STDCALL
	GetPackerCaps ();

// PackFiles specifies what should happen when a user creates,
// or adds files to the archive.
__stdcall int STDCALL
	PackFiles (
		char *PackedFile,
		char *SubPath,
		char *SrcPath,
		char *AddList,
		int Flags
		);

// ConfigurePacker gets called when the user clicks the Configure button
// from within "Pack files..." dialog box in WinCmd
__stdcall void STDCALL
	ConfigurePacker (
		HWND Parent,
		HINSTANCE DllInstance
		);

__stdcall void STDCALL
	PackSetDefaultParams (
		PackDefaultParamStruct* dps
		);

__stdcall BOOL STDCALL
	CanYouHandleThisFile (
		char*FileName
		);

__stdcall HANDLE STDCALL
	StartMemPack (
		int Options,
		char*FileName
		);

__stdcall int STDCALL
	PackToMem (
		HANDLE hMemPack,
		char*BufIn,
		int InLen,
		int*Taken,
		char*BufOut,
		int OutLen,
		int*Written,
		int SeekBy
		);

__stdcall int STDCALL
	DoneMemPack (
		HANDLE hMemPack
		);

__stdcall int STDCALL
	GetBackgroundFlags(
		void
		);

     */
}