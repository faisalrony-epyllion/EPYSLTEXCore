namespace EPYSLTEXCore.API.Extension
{
    public static class StringExtensions
    {
        public static string SplitAndAddUnderscore(this string input, char delimiter = '_', char join = '/',string extension= ".cshtml")
        {
           
            // Split the string into an array of substrings
            string[] parts = input.Split(delimiter);

            // Check if there are elements in the array
            if (parts.Length > 0)
            {
                // Add an underscore to the last element
                parts[parts.Length - 1] = "_"+ parts[parts.Length - 1] + extension;
            }

            // Join the array back into a single string with the same delimiter
            return string.Join(join, parts);
        }
    }
}
