namespace EpsiLibraryCore.Utilitaires
{
    public sealed class RegularExpression
    {
        public static bool IsCorrectFileName(string fileName)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("^[a-z-A-Z0-9][a-z-A-Z0-9_.]+$");
            return regex.IsMatch(fileName);
        }
    }
}
