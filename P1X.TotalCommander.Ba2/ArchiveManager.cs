using Microsoft.Extensions.Logging;
using SharpBSABA2;
using SharpBSABA2.BA2Util;
using SharpBSABA2.BSAUtil;

namespace P1X.TotalCommander.Ba2;

public class ArchiveManager(ILogger<ArchiveManager> logger)
{
    private const StringComparison PathComparison = StringComparison.InvariantCultureIgnoreCase;

    public ArchiveState? Open(OpenArchiveData openArchiveData)
    {
        var isExtracting = openArchiveData.OpenMode == WcxHead.PK_OM_EXTRACT;
        logger.LogDebug("Opening archive with mode {Mode}", isExtracting ? nameof(WcxHead.PK_OM_EXTRACT) : WcxHead.PK_OM_LIST);
        
        Archive? archive = openArchiveData.ArcName switch
        {
            { } s when s.EndsWith("ba2", PathComparison) => new BA2(s),
            { } s when s.EndsWith("bsa", PathComparison) => new BSA(s),
            _                                                      => null
        };

        if (archive != null)
        {
            logger.LogDebug("Archive of type {Type} opened", archive.Type.ToString());
            return new ArchiveState(archive, isExtracting);
        }
        else
        {
            logger.LogDebug("Failed to open archive");
            return null;
        }
    }
    
    public int ReadHeader(ReadHeaderData readHeaderData, out HeaderData data)
    {
        var state = readHeaderData.GetState();
        using var _ = logger.BeginScope("Archive state: Name = {FileName}, FileIndex = {Index}", state.Archive.FileName, state.CurrentFileIndex);
        
        var currentFileIndex = state.CurrentFileIndex;
        if (currentFileIndex >= state.Archive.FileCount)
        {
            data = default;
            
            logger.LogTrace("No more files to read");
            return WcxHead.Errors.E_END_ARCHIVE;
        }
        
        var archiveFile = state.Archive.Files[state.CurrentFileIndex];
        state.CurrentFileIndex += 1;

        data = new HeaderData(
            archiveFile.FullPath,
            state.Archive.LastWriteTime,
            (int) archiveFile.RealSize,
            (int) archiveFile.Size
        );
        
        logger.LogTrace("Reading file completed: Name = {FileName}, Time = {FileTime}, Packed = {PackedSize}, Unpacked = {UnpackedSize}", data.FileName, data.FileTime, data.PackedSize, data.UnpackedSize);
        return 0;
    }
    
    public int ProcessFile(ProcessFileData data, int operation)
    {
        logger.LogTrace("Processing file: DestName = {DestName}, DestPath = {DestPath}", data.DestinationName, data.DestinationPath);
        
        switch (operation)
        {
            case WcxHead.PK_SKIP:
                logger.LogTrace("Processing file skipped");
                return 0;
            case WcxHead.PK_TEST:
                logger.LogWarning("Testing file is not supported");
                return WcxHead.Errors.E_NOT_SUPPORTED;
            case WcxHead.PK_EXTRACT:
            {
                if (data.DestinationName == null)
                {
                    logger.LogError("destNameStr == null");
                    return WcxHead.Errors.E_NOT_SUPPORTED;
                }

                var state = data.GetState();
                using var _ = logger.BeginScope("Archive state: Name = {FileName}, FileIndex = {Index}", state.Archive.FileName, state.CurrentFileIndex);

                var entry = state.Archive.Files[state.CurrentFileIndex];
                var destPathFull = data.DestinationPath == null ? data.DestinationName : Path.Combine(data.DestinationPath, data.DestinationName);

                logger.LogTrace("Unpacking file: FileName = {FileName}, Folder = {Folder}, FullPath = {FullPath}, FullPathOriginal = {FullPathOriginal}, LowerPath = {LowerPath}", entry.FileName, entry.Folder, entry.FullPath, entry.FullPathOriginal, entry.LowerPath);

                var dir = Path.GetDirectoryName(destPathFull);
                var fileName = Path.GetFileName(destPathFull);
                entry.Extract(dir, false, fileName);

                logger.LogTrace("Unpacking complete: Dir = {Directory}, File = {FileName}", dir, fileName);
                return 0;
            }
            default:
                logger.LogError("operation != WcxHead.PK_EXTRACT");
                return WcxHead.Errors.E_NOT_SUPPORTED;
        }
    }

    public void Close(ArchiveState state)
    {
        logger.LogTrace("Closing archive: Name = {FileName}", state.Archive.FileName);
        state.Archive.Close();
        logger.LogTrace("Archive closed: Name = {FileName}", state.Archive.FileName);
    }
}