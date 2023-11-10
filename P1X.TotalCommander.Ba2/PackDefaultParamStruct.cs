using System.Runtime.InteropServices;

namespace P1X.TotalCommander.Ba2;

/// <summary>
/// PackDefaultParamStruct is passed to PackSetDefaultParams to inform the plugin about the current plugin interface version and ini file location.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct PackDefaultParamStruct
{
    public const int MaxFileNameLength = 260;
	
    /// <summary>
    /// The size of the structure, in bytes. Later revisions of the plugin interface may add more structure members, and will adjust this size field accordingly.
    /// </summary>
    public int size;
	
    /// <summary>
    /// Low value of plugin interface version. This is the value after the comma, multiplied by 100! Example. For plugin interface version 2.1, the low DWORD is 10 and the high DWORD is 2.
    /// </summary>
    public uint PluginInterfaceVersionLow;
	
    /// <summary>
    /// High value of plugin interface version.
    /// </summary>
    public uint PluginInterfaceVersionHi;
	
    /// <summary>
    /// Suggested location+name of the ini file where the plugin could store its data. This is a fully qualified path+file name, and will be in the same directory as the wincmd.ini. It's recommended to store the plugin data in this file or at least in this directory, because the plugin directory or the Windows directory may not be writable! 
    /// </summary>
    public fixed byte DefaultIniName[MaxFileNameLength];
}