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
        ArchiveManager = new ArchiveManager(LogManager.GetLogger<ArchiveManager>());
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
		    var state = ArchiveManager.Open(new OpenArchiveData(archiveData));
		    return state?.ToPtr(out _) ?? IntPtr.Zero;
	    }
	    catch (Exception e)
	    {
		    Logger.LogCritical(e, nameof(OpenArchive));
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
		    var result = ArchiveManager.ReadHeader(new ReadHeaderData(hArcData), out var data);
		    if (result == 0) 
			    data.FillNative(headerData);

		    return result;
	    }
	    catch (Exception e)
	    {
		    Logger.LogCritical(e, nameof(ReadHeader));
		    return WcxHead.Errors.E_BAD_DATA;
	    }
    }


    // int __stdcall ProcessFile (HANDLE hArcData, int Operation, char *DestPath, char *DestName);     
    [UnmanagedCallersOnly(EntryPoint = "ProcessFile", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static int ProcessFile(IntPtr hArcData, int operation, IntPtr destPath, IntPtr destName)
    {
	    try
	    {
		    return ArchiveManager.ProcessFile(new ProcessFileData(hArcData, destPath, destName), operation);
	    }
	    catch (Exception e)
	    {
		    Logger.LogCritical(e, nameof(ProcessFile));
		    return WcxHead.Errors.E_BAD_DATA;
	    }
    }
    
    // int __stdcall CloseArchive (HANDLE hArcData);
    [UnmanagedCallersOnly(EntryPoint = "CloseArchive", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static int CloseArchive(IntPtr hArcData)
    {
	    GCHandle gcHandle = default;
	    try
	    {
		    ArchiveManager.Close(ArchiveState.FromPtr(hArcData, out gcHandle));
		    return 0;
	    }
        catch (Exception e)
        {
	        Logger.LogCritical(e, nameof(CloseArchive));
            return WcxHead.Errors.E_BAD_DATA;
        }
	    finally
	    {
		    if (gcHandle.IsAllocated)
			    gcHandle.Free();
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
}