using NUnit.Framework;
using System.Net.Http;

namespace TestTask
{
    [TestFixture]
    public class Tests
    {
        private HttpClient _client;

        [OneTimeSetUp]
        public void SetUp()
        {
            _client = new HttpClient();
        }
    }
}