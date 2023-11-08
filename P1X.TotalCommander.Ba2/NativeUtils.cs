using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace P1X.TotalCommander.Ba2;

public static class NativeUtils
{
    //FileTime = (year - 1980) << 25 | month << 21 | day << 16 | hour << 11 | minute << 5 | second/2;
    public static int GetFileTime(DateTime dateTime) => ((dateTime.Year - 1980) << 25) | (dateTime.Month << 21) | (dateTime.Day << 16) | (dateTime.Hour << 11) | (dateTime.Minute << 5) | (dateTime.Second / 2);

    public static unsafe void SetString(byte* target, int targetLength, string str)
    {
        var filenameBytes = new Span<byte>(target, targetLength);
        var byteCount = Encoding.Default.GetBytes(str.AsSpan(), filenameBytes);

        FixDirectorySeparator(filenameBytes[..byteCount]);
        SetNullTerminated(filenameBytes, byteCount, targetLength);
    }
    
    public static unsafe void SetString(char* target, int targetLength, string str)
    {
        var filenameChars = new Span<char>(target, targetLength);
        str.CopyTo(filenameChars);

        FixDirectorySeparator(filenameChars[..str.Length]);
        SetNullTerminated(filenameChars, str.Length, targetLength);
    }
    
    private static void FixDirectorySeparator(Span<byte> filenameSpan) => filenameSpan.Replace((byte) Path.AltDirectorySeparatorChar, (byte) Path.DirectorySeparatorChar);
    private static void FixDirectorySeparator(Span<char> filenameSpan) => filenameSpan.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    private static void SetNullTerminated(Span<byte> str, int actualLength, int maxLength) => str[Math.Min(actualLength, maxLength - 1)] = 0;
    private static void SetNullTerminated(Span<char> str, int actualLength, int maxLength) => str[Math.Min(actualLength, maxLength - 1)] = '\0';
}