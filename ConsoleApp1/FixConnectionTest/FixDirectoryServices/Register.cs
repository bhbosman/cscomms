using FixConnection.FixDirectoryServices;
using NUnit.Framework;
using QuickFix.DataDictionary;
using Unity;

namespace FixConnectionTest.FixDirectoryServices
{
    public class RegisterFixDictionariesTest
    {
        [Test]
        public void Test()
        {
            var container = new UnityContainer();
            container.RegisterFixDictionaries();
            Assert.True(container.IsRegistered<DataDictionary>("FIX.4.4"));
            Assert.True(container.IsRegistered<DataDictionary>("FIX.4.1"));
            Assert.True(container.IsRegistered<DataDictionary>("FIX.4.2"));
            Assert.True(container.IsRegistered<DataDictionary>("FIX.4.3"));
            Assert.True(container.IsRegistered<DataDictionary>("FIX.4.4"));
                
        }
    }
}