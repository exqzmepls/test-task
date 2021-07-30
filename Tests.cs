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
        /// “естирование запроса времени сервера в формате XML.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestGetTimeXml()
        {
            #region arrange
            // URI, по которому отправл€етс€ запрос
            string requestUri = "http://demo.macroscop.com:8080/command?type=gettime&login=root&password=";

            // допустима€ погрешность между временем на сервере и локальным
            TimeSpan inaccuracy = TimeSpan.FromSeconds(15);

            // локальное врем€
            DateTime localTime = DateTime.Now;
            #endregion

            #region act
            // текст ответа в виде строки
            string responseString = await GetResponseStringAsync(requestUri);

            // ответ в виде XML-документа
            XDocument responseXml = XDocument.Parse(responseString);

            // врем€ на сервере из ответа
            DateTime serverTime = DateTime.Parse(responseXml.Element("string").Value);
            #endregion

            #region assert
            // “ест не провальный, если врем€ в ответе не превышает локальное врем€ на 15 секунд
            Assert.LessOrEqual(serverTime, localTime + inaccuracy);
            #endregion
        }

        /// <summary>
        /// ќтправка запроса GET согласно указанному URI и возврат текста ответа в виде строки в асинхронной операции.
        /// </summary>
        /// <param name="requestUri">URI, по которому отправл€етс€ запрос.</param>
        /// <returns>ќбъект задачи, представл€ющий асинхронную операцию.</returns>
        private async Task<string> GetResponseStringAsync(string requestUri)
        {
            // получение текста ответа в виде строки
            string responseString = await _client.GetStringAsync(requestUri);

            // возвращаем ответ, удалив из него "лишнее"
            return RemoveExtraLines(responseString);
        }

        /// <summary>
        /// ”даление строк "Timestamp", "Error code", "Message", "Body-length" из ответа.
        /// </summary>
        /// <param name="responseString">ќтвет сервера в виде строки.</param>
        /// <param name="sep">–азделитель.</param>
        /// <returns>ќтвет сервера в виде строки, конфертируемой в XML или JSON.</returns>
        private static string RemoveExtraLines(string responseString, string sep = "\r\n")
        {
            // индекс первой строки и количество строк дл€ удалени€
            const int firstLineIndex = 0, linesToRemove = 4;

            // раздел€ем ответ на отдельные строки по заданному разделителю
            var responseLines = responseString.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries).ToList();
            // удаление строк
            responseLines.RemoveRange(firstLineIndex, linesToRemove);

            // соединем€ оставшиес€ строки в одну и возвращаем еЄ
            return string.Join(null, responseLines);
        }
    }
}