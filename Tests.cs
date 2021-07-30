using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        /// <summary>
        /// ������������ ������� ������� ������� � ������� XML.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestGetTimeXml()
        {
            #region arrange
            // URI, �� �������� ������������ ������
            string requestUri = "http://demo.macroscop.com:8080/command?type=gettime&login=root&password=";

            // ���������� ����������� ����� �������� �� ������� � ���������
            TimeSpan inaccuracy = TimeSpan.FromSeconds(15);

            // ��������� �����
            DateTime localTime = DateTime.Now;
            #endregion

            #region act
            // ����� ������ � ���� ������
            string responseString = await GetResponseStringAsync(requestUri);

            // ����� � ���� XML-���������
            XDocument responseXml = XDocument.Parse(responseString);

            // ����� �� ������� �� ������
            DateTime serverTime = DateTime.Parse(responseXml.Element("string").Value);
            #endregion

            #region assert
            // ���� �� ����������, ���� ����� � ������ �� ��������� ��������� ����� �� 15 ������
            Assert.LessOrEqual(serverTime, localTime + inaccuracy);
            #endregion
        }

        /// <summary>
        /// �������� ������� GET �������� ���������� URI � ������� ������ ������ � ���� ������ � ����������� ��������.
        /// </summary>
        /// <param name="requestUri">URI, �� �������� ������������ ������.</param>
        /// <returns>������ ������, �������������� ����������� ��������.</returns>
        private async Task<string> GetResponseStringAsync(string requestUri)
        {
            // ��������� ������ ������ � ���� ������
            string responseString = await _client.GetStringAsync(requestUri);

            // ���������� �����, ������ �� ���� "������"
            return RemoveExtraLines(responseString);
        }

        /// <summary>
        /// �������� ����� "Timestamp", "Error code", "Message", "Body-length" �� ������.
        /// </summary>
        /// <param name="responseString">����� ������� � ���� ������.</param>
        /// <param name="sep">�����������.</param>
        /// <returns>����� ������� � ���� ������, �������������� � XML ��� JSON.</returns>
        private static string RemoveExtraLines(string responseString, string sep = "\r\n")
        {
            // ������ ������ ������ � ���������� ����� ��� ��������
            const int firstLineIndex = 0, linesToRemove = 4;

            // ��������� ����� �� ��������� ������ �� ��������� �����������
            var responseLines = responseString.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries).ToList();
            // �������� �����
            responseLines.RemoveRange(firstLineIndex, linesToRemove);

            // ��������� ���������� ������ � ���� � ���������� �
            return string.Join(null, responseLines);
        }
    }
}