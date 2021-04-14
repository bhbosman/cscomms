namespace FixConnection
{
    public static class SessionHelper
    {
        public static string SessionName(string iniatorCompId, string acceptorCompId)
        {
            return $"{iniatorCompId}-{acceptorCompId}";
        }
    }
}