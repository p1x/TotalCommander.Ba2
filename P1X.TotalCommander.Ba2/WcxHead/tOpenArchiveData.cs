using JetBrains.Annotations;

namespace P1X.TotalCommander.Ba2;

/// <summary>
/// OpenArchiveData is used in OpenArchive.
/// </summary> 
/// <remarks>
/// If the file is opened with OpenMode==PK_OM_LIST, ProcessFile will never be called by Total Commander.
/// </remarks>
[PublicAPI]
// ReSharper disable once InconsistentNaming
public unsafe struct tOpenArchiveData {
	
    /// <summary>
    /// ArcName contains the name of the archive to open.
    /// </summary>
    public byte* ArcName;
    
    /// <summary>
    /// OpenMode is set to one of the following values:
    /// <list type="list">
    ///	  <listheader>
    ///     <term>Constant</term>
    ///     <description>Description</description>
    ///   </listheader>
    ///   <item>
    ///     <term>PK_OM_LIST = 0</term>
    ///     <description>Open file for reading of file names only</description>
    ///   </item>
    ///   <item>
    ///     <term>PK_OM_EXTRACT = 1</term>
    ///     <description>Open file for processing (extract or test)</description>
    ///   </item>
    /// </list>
    /// </summary>
    public int OpenMode;
    
    /// <summary>
    /// OpenResult used to return one of the <see cref="WcxHead.Errors"/> values if an error occurs.
    /// </summary>
    public int OpenResult;
    
    /// <summary>
    /// The Cmt* variables are for the file comment. They are currently not used by Total Commander, so may be set to NULL.
    /// </summary>
    [Obsolete("The Cmt* variables are for the file comment. They are currently not used by Total Commander, so may be set to NULL.")]
    public byte* CmtBuf;
    
    /// <summary>
    /// The Cmt* variables are for the file comment. They are currently not used by Total Commander, so may be set to NULL.
    /// </summary>
    [Obsolete("The Cmt* variables are for the file comment. They are currently not used by Total Commander, so may be set to NULL.")]
    public int CmtBufSize;
    
    /// <summary>
    /// The Cmt* variables are for the file comment. They are currently not used by Total Commander, so may be set to NULL.
    /// </summary>
    [Obsolete("The Cmt* variables are for the file comment. They are currently not used by Total Commander, so may be set to NULL.")]
    public int CmtSize;
    
    /// <summary>
    /// The Cmt* variables are for the file comment. They are currently not used by Total Commander, so may be set to NULL.
    /// </summary>
    [Obsolete("The Cmt* variables are for the file comment. They are currently not used by Total Commander, so may be set to NULL.")]
    public int CmtState;
}