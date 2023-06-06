namespace IC_Assignment.Helpers
{
    // why does the string have to be potentially weird?
    // why does it have to be Jann when there's also Jan?
    public static class StringDateFixer
    {
        public static string FixStringDate(string str)
        { // requires fromat to be MMM-dd-yyyy but has a weird extension of the month
            string[] s = str.Split('-');

            s[0] = s[0].Substring(0, 3);
            s[1] = s[1].Substring(0, 2);
            s[2] = s[2].Substring(0, 4);

            return string.Join("-", s);
        }
    }
}
