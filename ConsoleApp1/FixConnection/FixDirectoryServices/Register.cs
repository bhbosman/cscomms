using System.Reflection;
using QuickFix.DataDictionary;
using Unity;

namespace FixConnection.FixDirectoryServices
{
    public static class Register
    {
        private static readonly DataDictionary Dictionary40;
        private static readonly DataDictionary Dictionary41;
        private static readonly DataDictionary Dictionary42;
        private static readonly DataDictionary Dictionary43;
        private static readonly DataDictionary Dictionary44;

        private static DataDictionary Create(Assembly assembly, string resource)
        {
            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                var dictionary = new DataDictionary();
                dictionary.Load(stream);
                return dictionary;
            }
        }
        static Register()
        {
            var assembly = typeof(Register).Assembly;
            Dictionary40 = Create(assembly, "FixConnection.FixDirectoryServices.FIX40.xml");
            Dictionary41 = Create(assembly, "FixConnection.FixDirectoryServices.FIX41.xml");
            Dictionary42 = Create(assembly, "FixConnection.FixDirectoryServices.FIX42.xml");
            Dictionary43 = Create(assembly, "FixConnection.FixDirectoryServices.FIX43.xml");
            Dictionary44 = Create(assembly, "FixConnection.FixDirectoryServices.FIX44.xml");
        }
        public static void RegisterFixDictionaries(this IUnityContainer container)
        {
            container.RegisterInstance(QuickFix.Values.BeginString_FIX40, Dictionary40);
            container.RegisterInstance(QuickFix.Values.BeginString_FIX41, Dictionary41);
            container.RegisterInstance(QuickFix.Values.BeginString_FIX42, Dictionary42);
            container.RegisterInstance(QuickFix.Values.BeginString_FIX43, Dictionary43);
            container.RegisterInstance(QuickFix.Values.BeginString_FIX44, Dictionary44);
        }
    }
}