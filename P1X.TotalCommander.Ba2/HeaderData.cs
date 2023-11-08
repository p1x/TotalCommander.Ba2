namespace P1X.TotalCommander.Ba2;

public readonly struct HeaderData(string fileName, DateTime fileTime, uint unpackedSize, uint packedSize)
{
    public readonly string FileName = fileName;
    public readonly DateTime FileTime = fileTime;
    public readonly uint UnpackedSize = unpackedSize;
    public readonly uint PackedSize = packedSize;
    
    public unsafe void FillNative(tHeaderData *headerData)
    {
        NativeUtils.SetString(headerData->FileName, tHeaderData.MaxFileNameLength, FileName);
        //headerData->Flags = 0;
        
        headerData->UnpSize = (int) UnpackedSize;
        headerData->PackSize = (int) PackedSize;
        headerData->HostOS = 0;
        headerData->FileCRC = 0;
        headerData->FileTime = NativeUtils.GetFileTime(FileTime);
        headerData->FileAttr = 0;
    
        headerData->CmtBuf = (byte*) IntPtr.Zero;
        headerData->CmtSize = 0;
        headerData->CmtState = 0;
        headerData->CmtBufSize = 0;
    }
    
    public unsafe void FillNativeEx(tHeaderDataEx *headerData)
    {
        NativeUtils.SetString(headerData->FileName, tHeaderDataEx.MaxFileNameLength, FileName);
        //headerData->Flags = 0;

        headerData->UnpSize = UnpackedSize;
        headerData->UnpSizeHigh = 0;
        headerData->PackSize = PackedSize;
        headerData->PackSizeHigh = 0;
        headerData->HostOS = 0;
        headerData->FileCRC = 0;
        headerData->FileTime = NativeUtils.GetFileTime(FileTime);
        headerData->FileAttr = 0;
        
         headerData->CmtBuf = (byte*) IntPtr.Zero;
         headerData->CmtSize = 0;
         headerData->CmtState = 0;
         headerData->CmtBufSize = 0;
        
         new Span<byte>(headerData->Reserved, tHeaderDataEx.MaxFileNameLength).Clear();
    }
    
    public unsafe void FillNativeEx(tHeaderDataExW *headerData)
    {
        NativeUtils.SetString(headerData->FileName, tHeaderDataEx.MaxFileNameLength, FileName);
        //headerData->Flags = 0;

        headerData->UnpSize = UnpackedSize;
        headerData->UnpSizeHigh = 0;
        headerData->PackSize = PackedSize;
        headerData->PackSizeHigh = 0;
        headerData->HostOS = 0;
        headerData->FileCRC = 0;
        headerData->FileTime = NativeUtils.GetFileTime(FileTime);
        headerData->FileAttr = 0;
        
        headerData->CmtBuf = (byte*) IntPtr.Zero;
        headerData->CmtSize = 0;
        headerData->CmtState = 0;
        headerData->CmtBufSize = 0;
        
        new Span<byte>(headerData->Reserved, tHeaderDataEx.MaxFileNameLength).Clear();
    }
}