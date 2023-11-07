using SharpBSABA2;
using SharpBSABA2.BA2Util;
using SharpBSABA2.BSAUtil;

namespace P1X.TotalCommander.Ba2;

public class ArchiveManager
{
    
    public ArchiveState? Open(string path)
    {
        Archive? archive = null;
        if (path.EndsWith("ba2", StringComparison.InvariantCultureIgnoreCase))
        {
            archive = new BA2(path);
        }
        else if (path.EndsWith("bsa", StringComparison.InvariantCultureIgnoreCase))
        {
            archive = new BSA(path);
        }

        return archive != null ? new ArchiveState(archive) : null;
    }
    
    public bool ReadHeader(ArchiveState archiveState, out HeaderData data)
    {
        var nextFileIndex = archiveState.NextFileIndex;
        if (nextFileIndex >= archiveState.Archive.FileCount)
        {
            data = default;
            return false;
        }
        
        archiveState.CurrentFileIndex = archiveState.NextFileIndex;
        
        var archiveFile = archiveState.Archive.Files[archiveState.CurrentFileIndex];
        archiveState.NextFileIndex += 1;

        data = new HeaderData(
            archiveFile.FullPath,
            archiveState.Archive.LastWriteTime,
            (int) archiveFile.RealSize,
            (int) archiveFile.Size
        );

        return true;
    }
}