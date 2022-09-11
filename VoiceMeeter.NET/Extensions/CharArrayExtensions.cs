namespace VoiceMeeter.NET.Extensions;

public static class CharArrayExtensions
{
    /// <summary>
    /// Transforms a char array containing a null terminated string into a string
    /// </summary>
    /// <param name="nullTerminatedArray">A char array containing a null terminated string</param>
    /// <returns>The string that was contained in <see cref="nullTerminatedArray"/></returns>
    /// <remarks>Uses <see cref="Span{T}"/> internally, no new arrays are created in the method</remarks>
    public static string GetStringFromNullTerminatedCharArray(this char[] nullTerminatedArray)
    {
        Span<char> charSpan = nullTerminatedArray.AsSpan();
        int terminator = charSpan.IndexOf('\0');

        return new string(charSpan[..terminator]);
    }
}