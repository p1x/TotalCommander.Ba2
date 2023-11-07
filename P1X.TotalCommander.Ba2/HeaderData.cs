namespace P1X.TotalCommander.Ba2;

public readonly struct HeaderData(string fileName, DateTime fileTime, int unpackedSize, int packedSize)
{
    public readonly string FileName = fileName;
    public readonly DateTime FileTime = fileTime;
    public readonly int UnpackedSize = unpackedSize;
    public readonly int PackedSize = packedSize;
    
    public unsafe void FillNative(tHeaderData *headerData)
    {
        NativeUtils.SetString(headerData->FileName, tHeaderData.MaxFileNameLength, FileName);
        headerData->FileTime = NativeUtils.GetFileTime(FileTime);
        headerData->UnpSize = UnpackedSize;
        headerData->PackSize = PackedSize;
    }
}