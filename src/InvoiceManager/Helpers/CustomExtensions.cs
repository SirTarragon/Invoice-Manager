namespace CustomExtensions
{
    // Extension methods must be defined in a static class.
    public static class StreamReaderExtensions
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.

        public static string ReadLines(this StreamReader reader, int n = 1)
        {
            return string.Join("", from i in Enumerable.Range(0,n) select reader.ReadLine());
        }
    }
}
