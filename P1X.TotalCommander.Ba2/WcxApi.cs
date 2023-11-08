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

    /// <inheritdoc cref="OpenArchive"/>
    /// <summary>
    /// <p>This is an ANSI version of <see cref="OpenArchive"/>.</p>
    /// <inheritdoc cref="OpenArchive" select="summary"/>
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "OpenArchive", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static unsafe IntPtr OpenArchiveA(tOpenArchiveData *archiveData) => OpenArchive(new OpenArchiveData(archiveData, false));
    
    /// <inheritdoc cref="OpenArchive"/>
    /// <summary>
    /// <p>This is a Unicode version of <see cref="OpenArchive"/>.</p>
    /// <inheritdoc cref="OpenArchive" select="summary"/>
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "OpenArchiveW", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static unsafe IntPtr OpenArchiveW(tOpenArchiveData *archiveData) => OpenArchive(new OpenArchiveData(archiveData, true));

    /// <summary>
    /// OpenArchive should perform all necessary operations when an archive is to be opened.
    /// <code>__stdcall HANDLE STDCALL OpenArchive(tOpenArchiveData *ArchiveData);</code>
    /// </summary>
    /// <remarks>
    /// <p>OpenArchive should return a unique "handle" representing the archive.</p>
    /// <p>Most likely this will be some data structure or object address on heap, to and from which you can cast for every subsequent function call. A static or global structure will of course also work, but would interfere with thread safety and having multiple pack/unpack operations at the same time (GetBackgroundFlags). Since the interface was developed around the Windows API, an actual Windows HANDLE is meant, which is defined as pointer to void (PVOID → void*). Therefore use an explicit cast to HANDLE for your data.</p>
    /// <p>The handle should remain valid until <see cref="CloseArchive"/> is called. In there you can safely delete your data, to free the heap (if you used it).</p>
    /// <p>If an error occurs, you should return zero, and specify the error by setting <see cref="tOpenArchiveData.OpenResult"/> member of <see cref="tOpenArchiveData"/>.</p>
    ///	<p>You can use the <see cref="tOpenArchiveData"/> <paramref name="archiveData"/> structure to query information about the archive being open. If you need that information for subsequent function calls, you should store it to some location that can be accessed via the handle.</p>
    /// </remarks>
    private static IntPtr OpenArchive(OpenArchiveData archiveData)
    {
	    try
	    {
		    var state = ArchiveManager.Open(archiveData);
		    return state?.ToPtr(out _) ?? IntPtr.Zero;
	    }
	    catch (Exception e)
	    {
		    Logger.LogCritical(e, nameof(OpenArchive));
		    archiveData.OpenResult = WcxHead.Errors.E_BAD_DATA;
		    return IntPtr.Zero;
	    }
    }

    /// <summary>
    /// Totalcmd calls ReadHeader to find out what files are in the archive.
    /// <code>int __stdcall ReadHeader (HANDLE hArcData, tHeaderData *HeaderData);</code>
    /// </summary>
    /// <remarks>
    /// <p>ReadHeader is called as long as it returns zero (as long as the previous call to this function returned zero). Each time it is called, HeaderData is supposed to provide Totalcmd with information about the next file contained in the archive. When all files in the archive have been returned, ReadHeader should return E_END_ARCHIVE which will prevent ReaderHeader from being called again. If an error occurs, ReadHeader should return one of the error values or 0 for no error.</p>
    /// <p>hArcData contains the handle returned by OpenArchive. The programmer is encouraged to store other information in the location that can be accessed via this handle. For example, you may want to store the position in the archive when returning files information in ReadHeader.</p>
    /// <p>In short, you are supposed to set at least PackSize, UnpSize, FileTime, and FileName members of tHeaderData. Totalcmd will use this information to display content of the archive when the archive is viewed as a directory.</p>
    /// </remarks>
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
    
    /// <inheritdoc cref="ReadHeaderEx"/>
    /// <summary>
    /// <p>This is an ANSI version of <see cref="ReadHeaderEx"/>.</p>
    /// <inheritdoc cref="ReadHeaderEx" select="summary"/>
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "ReadHeaderEx", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static unsafe int ReadHeaderExA(IntPtr hArcData, tHeaderDataEx *headerData) => ReadHeaderEx(new ReadHeaderData(hArcData), headerData, false);
    
    /// <inheritdoc cref="ReadHeaderEx"/>
    /// <summary>
    /// <p>This is a Unicode version of <see cref="ReadHeaderEx"/>.</p>
    /// <inheritdoc cref="ReadHeaderEx" select="summary"/>
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "ReadHeaderExW", CallConvs = new[] { typeof(CallConvStdcall) } )]
    public static unsafe int ReadHeaderExW(IntPtr hArcData, tHeaderDataExW *headerData) => ReadHeaderEx(new ReadHeaderData(hArcData), headerData, true);

    /// <summary>
    /// Totalcmd calls ReadHeaderEx to find out what files are in the archive. This function is always called instead of ReadHeader if it is present. It only needs to be implemented if the supported archive type may contain files >2 GB. You should implement both ReadHeader and ReadHeaderEx in this case, for compatibility with older versions of Total Commander.
    /// <code>int __stdcall ReadHeaderEx (HANDLE hArcData, tHeaderDataEx *HeaderDataEx);</code>
    /// </summary>
    /// <remarks>
    /// <p>ReadHeaderEx is called as long as it returns zero (as long as the previous call to this function returned zero). Each time it is called, HeaderDataEx is supposed to provide Totalcmd with information about the next file contained in the archive. When all files in the archive have been returned, ReadHeaderEx should return E_END_ARCHIVE which will prevent ReaderHeaderEx from being called again. If an error occurs, ReadHeaderEx should return one of the error values or 0 for no error.</p>
    /// <p>hArcData contains the handle returned by OpenArchive. The programmer is encouraged to store other information in the location that can be accessed via this handle. For example, you may want to store the position in the archive when returning files information in ReadHeaderEx.</p>
    /// <p>In short, you are supposed to set at least PackSize, PackSizeHigh, UnpSize, UnpSizeHigh, FileTime, and FileName members of tHeaderDataEx. Totalcmd will use this information to display content of the archive when the archive is viewed as a directory.</p>
    /// </remarks>
    private static unsafe int ReadHeaderEx(ReadHeaderData readHeaderData, void* headerData, bool isWChar)
    {
	    try
	    {
		    var result = ArchiveManager.ReadHeader(readHeaderData, out var data);
		    if (result != 0) 
			    return result;
		    
		    if (isWChar)
			    data.FillNativeEx((tHeaderDataExW*) headerData);
		    else
			    data.FillNativeEx((tHeaderDataEx*) headerData);

		    return 0;
	    }
	    catch (Exception e)
	    {
		    Logger.LogCritical(e, nameof(ReadHeaderEx));
		    return WcxHead.Errors.E_BAD_DATA;
	    }
    }

    /// <inheritdoc cref="ProcessFile"/>
    /// <summary>
    /// <p>This is an ANSI version of <see cref="ProcessFile"/>.</p>
    /// <inheritdoc cref="ProcessFile" select="summary"/>
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "ProcessFile", CallConvs = new[] { typeof(CallConvStdcall) })]
    public static int ProcessFileA(IntPtr hArcData, int operation, IntPtr destPath, IntPtr destName) => ProcessFile(operation, new ProcessFileData(hArcData, destPath, destName, false));
    
    /// <inheritdoc cref="ProcessFile"/>
    /// <summary>
    /// <p>This is a Unicode version of <see cref="ProcessFile"/>.</p>
    /// <inheritdoc cref="ProcessFile" select="summary"/>
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "ProcessFileW", CallConvs = new[] { typeof(CallConvStdcall) })]
    public static int ProcessFileW(IntPtr hArcData, int operation, IntPtr destPath, IntPtr destName) => ProcessFile(operation, new ProcessFileData(hArcData, destPath, destName, true));
	
    /// <summary>
    /// ProcessFile should unpack the specified file or test the integrity of the archive.
    /// <code>int __stdcall ProcessFile (HANDLE hArcData, int Operation, char *DestPath, char *DestName);</code>
    /// </summary>
    /// <remarks>
    /// <p>ProcessFile should return zero on success, or one of the error values otherwise.</p>
    /// <p>hArcData contains the handle previously returned by you in OpenArchive. Using this, you should be able to find out information (such as the archive filename) that you need for extracting files from the archive.</p>
    /// <p>Unlike PackFiles, ProcessFile is passed only one filename. Either DestName contains the full path and file name and DestPath is NULL, or DestName contains only the file name and DestPath the file path. This is done for compatibility with unrar.dll.</p>
    /// <p>When Total Commander first opens an archive, it scans all file names with OpenMode==PK_OM_LIST, so ReadHeader() is called in a loop with calling ProcessFile(...,PK_SKIP,...). When the user has selected some files and started to decompress them, Total Commander again calls ReadHeader() in a loop. For each file which is to be extracted, Total Commander calls ProcessFile() with Operation==PK_EXTRACT immediately after the ReadHeader() call for this file. If the file needs to be skipped, it calls it with Operation==PK_SKIP.</p>
    ///	<p>Each time DestName is set to contain the filename to be extracted, tested, or skipped. To find out what operation out of these last three you should apply to the current file within the archive, Operation is set to one of the following:</p>
    /// <list type="list">
    ///	  <listheader>
    ///     <term>Constant</term>
    ///     <description>Value Description</description>
    ///   </listheader>
    ///   <item>
    ///     <term>PK_SKIP = 0</term>
    ///     <description>Skip this file</description>
    ///   </item>
    ///   <item>
    ///     <term>PK_TEST = 1</term>
    ///     <description>Test file integrity</description>
    ///   </item>
    ///   <item>
    ///     <term>PK_EXTRACT = 2</term>
    ///     <description>Extract to disk</description>
    ///   </item>
    /// </list>
    /// </remarks> 
    private static int ProcessFile(int operation, ProcessFileData processFileData)
    {
	    try
	    {
		    return ArchiveManager.ProcessFile(processFileData, operation);
	    }
	    catch (Exception e)
	    {
		    Logger.LogCritical(e, nameof(ProcessFileA));
		    return WcxHead.Errors.E_BAD_DATA;
	    }
    }

    /// <summary>
    /// CloseArchive should perform all necessary operations when an archive is about to be closed.
    /// <code>int __stdcall CloseArchive (HANDLE hArcData);</code>
    /// </summary>
    /// <remarks>
    /// <p>CloseArchive should return zero on success, or one of the error values otherwise. It should free all the resources associated with the open archive.</p>
    /// <p>The parameter hArcData refers to the value returned by a programmer within a previous call to OpenArchive.</p>
    /// </remarks>
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