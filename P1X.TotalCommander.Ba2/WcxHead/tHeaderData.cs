namespace P1X.TotalCommander.Ba2;

public unsafe struct tHeaderData {
	public const int MaxFileNameLength = 260;
	
	public fixed byte ArcName[MaxFileNameLength];
	public fixed byte FileName[MaxFileNameLength];
	public int Flags;
	public int PackSize;
	public int UnpSize;
	public int HostOS;
	public int FileCRC;
	public int FileTime;
	public int UnpVer;
	public int Method;
	public int FileAttr;
	public byte* CmtBuf;
	public int CmtBufSize;
	public int CmtSize;
	public int CmtState;
}

public unsafe struct tHeaderDataEx {
	public const int MaxFileNameLength = 1024;

	public fixed byte ArcName[MaxFileNameLength];
	public fixed byte FileName[MaxFileNameLength];
	public int Flags;
	public uint PackSize;
	public uint PackSizeHigh;
	public uint UnpSize;
	public uint UnpSizeHigh;
	public int HostOS;
	public int FileCRC;
	public int FileTime;
	public int UnpVer;
	public int Method;
	public int FileAttr;
	public byte* CmtBuf;
	public int CmtBufSize;
	public int CmtSize;
	public int CmtState;
	public fixed byte Reserved[MaxFileNameLength];
}

public unsafe struct tHeaderDataExW {
	public const int MaxFileNameLength = 1024;

	public fixed char ArcName[MaxFileNameLength];
	public fixed char FileName[MaxFileNameLength];
	public int Flags;
	public uint PackSize;
	public uint PackSizeHigh;
	public uint UnpSize;
	public uint UnpSizeHigh;
	public int HostOS;
	public int FileCRC;
	public int FileTime;
	public int UnpVer;
	public int Method;
	public int FileAttr;
	public byte* CmtBuf;
	public int CmtBufSize;
	public int CmtSize;
	public int CmtState;
	public fixed byte Reserved[MaxFileNameLength];
}