using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Logging;

namespace P1X.TotalCommander.Ba2;

public static class WcxApi
{

    private static readonly ArchiveManager ArchiveManager;
    private static readonly ILogger Logger;

    static WcxApi()
    {
        ArchiveManager = new ArchiveManager();
	    
        var loggerFactory = LoggerFactory.Create(builder =>
        {
	        builder.ClearProviders();
	        builder.SetMinimumLevel(LogLevel.Trace);
	        builder.AddFile(options =>
	        {
		        options.RootPath = AppContext.BaseDirectory;
		        options.IncludeScopes = true;
		        options.Files = new[]
		        {
			        new LogFileOptions
			        {
				        Path = "TotalCommander.Ba2-<date>.log",
			        }
		        };
	        });
        });
        
        LogManager.SetLoggerFactory(loggerFactory, "Global");
        
        Logger = LogManager.GetLogger(nameof(WcxApi));
    }
    
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
		    Logger.LogTrace("Opening archive: Name = {FileName}", path);
		    
		    var archive = !string.IsNullOrEmpty(path) ? ArchiveManager.Open(path) : null;
		    if (archive == null)
		    {
			    Logger.LogError("Opening archive, format not supported: Name = {FileName}", path);
			    archiveData->OpenResult = WcxHead.Errors.E_NOT_SUPPORTED;
			    return IntPtr.Zero;
		    }

		    var handle = GCHandle.Alloc(archive);
		    var pinnedObject = GCHandle.ToIntPtr(handle);

		    Logger.LogTrace("Opening archive, completed: Name = {FileName}", path);
		    
		    return pinnedObject;
	    }
	    catch (Exception e)
	    {
		    Logger.LogCritical(e, nameof(OpenArchive));
		    archiveData->OpenResult = WcxHead.Errors.E_BAD_DATA;
		    return IntPtr.Zero;
	    }
	    catch
	    {
		    Logger.LogCritical(nameof(OpenArchive) + ": Unknown exception");
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
		    Logger.BeginScope("Archive state: Name = {FileName}, FileIndex = {Index}", state.Archive.FileName, state.CurrentFileIndex);

		    
		    var hasMoreFiles = ArchiveManager.ReadHeader(state, out var data);
		    if (!hasMoreFiles)
		    {
			    Logger.LogTrace("Reading header, no more files");
			    return WcxHead.Errors.E_END_ARCHIVE;
		    }

		    data.FillNative(headerData);
		    Logger.LogTrace("Reading header, completed: Name = {FileName}, Time = {FileTime}, Packed = {PackedSize}, Unpacked = {UnpackedSize}", data.FileName, data.FileTime, data.PackedSize, data.UnpackedSize);

		    return 0;
	    }
	    catch (Exception e)
	    {
		    Logger.LogCritical(e, nameof(ReadHeader));
		    return WcxHead.Errors.E_BAD_DATA;
	    }
	    catch
	    {
		    Logger.LogCritical(nameof(ReadHeader) + ": Unknown exception");
		    return WcxHead.Errors.E_BAD_DATA;
	    }
    }
    
    // int __stdcall ProcessFile (HANDLE hArcData, int Operation, char *DestPath, char *DestName);     
    [UnmanagedCallersOnly(EntryPoint = "ProcessFile", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static int ProcessFile(IntPtr hArcData, int operation, IntPtr destPath, IntPtr destName)
    {
	    if (operation == WcxHead.PK_OM_LIST)
	    {
		    Logger.LogTrace("Processing file, list");
		    return 0;
	    }

	    try
	    {
		    if (operation == WcxHead.PK_SKIP)
		    {
			    Logger.LogTrace("Processing file, skip");
			    return 0;
		    }

		    var state = ArchiveState.RestoreState(hArcData, out _);

		    Logger.BeginScope("Archive state: Name = {FileName}, FileIndex = {Index}", state.Archive.FileName, state.CurrentFileIndex);
		    
		    if (operation == WcxHead.PK_TEST)
		    {
			    Logger.LogWarning("Processing file, test. Not supported");
			    return WcxHead.Errors.E_NOT_SUPPORTED;
		    }

		    if (operation != WcxHead.PK_EXTRACT)
		    {
			    Logger.LogError("operation != WcxHead.PK_EXTRACT");
			    return WcxHead.Errors.E_NOT_SUPPORTED;
		    }

		    var destPathStr = destPath == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(destPath);
		    var destNameStr = Marshal.PtrToStringAnsi(destName);

		    if (destNameStr == null)
		    {
			    Logger.LogError("destNameStr == null");
			    return WcxHead.Errors.E_NOT_SUPPORTED;
		    }

		    var entry = state.Archive.Files[state.CurrentFileIndex];
		    var destPathFull = destPathStr == null ? destNameStr : Path.Combine(destPathStr, destNameStr);
		    Logger.LogTrace("Processing file, destination parsed: DestName = {DestName}, DestPath = {DestPath}, DestPathFull = {PathFull}", destNameStr, destPathStr, destPathFull);

		    if (entry == null)
		    {
			    Logger.LogError("entry = null");
			    return WcxHead.Errors.E_BAD_DATA;
		    }

		    Logger.LogTrace("Processing file, entry loaded: FileName = {FileName}, Folder = {Folder}, FullPath = {FullPath}, FullPathOriginal = {FullPathOriginal}, LowerPath = {LowerPath}", entry.FileName, entry.Folder, entry.FullPath, entry.FullPathOriginal, entry.LowerPath);

		    var dir = Path.GetDirectoryName(destPathFull);
		    var fileName = Path.GetFileName(destPathFull);
		    entry.Extract(dir, false, fileName);

		    Logger.LogTrace("Processing file, extracted: Dir = {Directory}, File = {FileName}", dir, fileName);
		    
		    return 0;
	    }
	    catch (Exception e)
	    {
		    Logger.LogCritical(e, nameof(ProcessFile));
		    return WcxHead.Errors.E_BAD_DATA;
	    }
	    catch
	    {
		    Logger.LogCritical(nameof(ProcessFile) + ": Unknown exception");
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
            Logger.LogTrace("Closing archive, begin: Name = {FileName}", state.Archive.FileName);
            
            state.Archive.Close();
            gcHandle.Free();
            
            Logger.LogTrace("Closing archive, completed: Name = {FileName}", state.Archive.FileName);
            return 0;
        }
        catch (Exception e)
        {
	        Logger.LogCritical(e, nameof(CloseArchive));
            return WcxHead.Errors.E_BAD_DATA;
        }
        catch
        {
	        Logger.LogCritical(nameof(CloseArchive)+ ": Unknown exception");
	        return WcxHead.Errors.E_BAD_DATA;
        }
    }
    
    // void __stdcall SetChangeVolProc (HANDLE hArcData, tChangeVolProc pChangeVolProc1);
    [UnmanagedCallersOnly(EntryPoint = "SetChangeVolProc", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static int SetChangeVolProc(IntPtr hArcData, IntPtr pChangeVolProc1)
    {
	    Logger.LogTrace(nameof(SetChangeVolProc));
        return WcxHead.Errors.E_NOT_SUPPORTED;
    }
    
    // void __stdcall SetProcessDataProc (HANDLE hArcData, tProcessDataProc pProcessDataProc);
    [UnmanagedCallersOnly(EntryPoint = "SetProcessDataProc", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static int SetProcessDataProc(IntPtr hArcData, IntPtr pProcessDataProc)
    {
	    Logger.LogTrace(nameof(SetProcessDataProc));
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

public static class LogManager
{
	private static ILogger? _globalLogger;
	private static ILoggerFactory? _loggerFactory;

	public static void SetLoggerFactory(ILoggerFactory loggerFactory, string categoryName)
	{
		_loggerFactory = loggerFactory;
		_globalLogger = loggerFactory.CreateLogger(categoryName);
	}

	public static ILogger? Logger => _globalLogger;

	public static ILogger<T> GetLogger<T>() where T : class => (_loggerFactory ?? throw new InvalidOperationException()).CreateLogger<T>();
	public static ILogger GetLogger(string categoryName) => (_loggerFactory ?? throw new InvalidOperationException()).CreateLogger(categoryName);
}