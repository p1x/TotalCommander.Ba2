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
            File.AppendAllText("log.txt", "BA2 detected\r\n");
        }
        else if (path.EndsWith("bsa", StringComparison.InvariantCultureIgnoreCase))
        {
            archive = new BSA(path);
            File.AppendAllText("log.txt", "BSA detected\r\n");
        }
        else
        {
            File.AppendAllText("log.txt", "Archive not detected\r\n");
        }

        return archive != null ? new ArchiveState(archive) : null;
    }
    
    public bool ReadHeader(ArchiveState archiveState, out HeaderData data)
    {
        var currentFileIndex = archiveState.CurrentFileIndex;
        if (currentFileIndex >= archiveState.Archive.FileCount)
        {
            data = default;
            return false;
        }

        var archiveFile = archiveState.Archive.Files[currentFileIndex];
        archiveState.CurrentFileIndex += 1;

        data = new HeaderData(
            archiveFile.FullPath,
            archiveState.Archive.LastWriteTime,
            (int) archiveFile.RealSize,
            (int) archiveFile.Size
        );

        return true;
    }
}