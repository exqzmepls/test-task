using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
        /// Тестирование получения кадров из архива, для каналов сгруппированных в категорию Street.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestGetArchiveFrames()
        {
            #region arrange
            // URI, по которому отправляется запрос получения кадра из архива
            string archiveRequestUri = "http://demo.macroscop.com:8080/site?login=root&channelid={0}&withcontenttype=true&mode=archive&resolutionx=500&resolutiony=500&streamtype=mainvideo&starttime={1}";

            // время для кадра
            DateTime startTime = DateTime.Now;
            #endregion

            #region act

            XDocument configexXml = await GetConfigexXml();

            // id каналов сгруппированных в категорию Street
            var streetChannelsIds = configexXml.Root.Element("RootSecurityObject").Element("ChildSecurityObjects").Elements()
                .First(element => element.Name.LocalName == "SecObjectInfo" && element.Attribute("Name").Value == "Street").Element("ChildChannels").Elements()
                .Where(element => element.Name.LocalName == "ChannelId").Select(channel => channel.Value);

            // получение кадров из архива
            foreach (var id in streetChannelsIds)
            {
                // получаем поток изображения
                using (Stream frameStream = await _client.GetStreamAsync(string.Format(archiveRequestUri, id, startTime)))
                {
                    // кадр из полученного потока
                    Image frame = Image.FromStream(frameStream);
                }
            }
            #endregion

            #region assert
            // Тест считается не проваленным, если не было вызвано исключений
            Assert.Pass();
            #endregion
        }

        /// <summary>
        /// Тестирование запроса конфигурации каналов сервера в XML формате.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestСonfigexXml()
        {
            #region arrange
            // URI, по которому отправляется запрос
            string requestUri = "http://demo.macroscop.com:8080/configex?login=root&password=";

            // минимальное количество каналов в конфигурации, чтобы тест был пройдет
            const int minChannelsToPass = 6;
            #endregion

            #region act
            // текст ответа в виде строки
            string responseString = await _client.GetStringAsync(requestUri);

            // ответ в виде XML-документа
            XDocument responseXml = XDocument.Parse(responseString);

            // количество каналов в полученной конфигурации
            int channels = responseXml.Root.Element("Channels").Elements().Count(element => element.Name.LocalName == "ChannelInfo");
            #endregion

            #region assert
            // Тест считается не проваленным, если в полученной конфигурации не меньше 6 каналов
            Assert.GreaterOrEqual(channels, minChannelsToPass);
            #endregion
        }

        /// <summary>
        /// Тестирование запроса времени сервера формате JSON.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestGetTimeJson()
        {
            #region arrange
            // URI, по которому отправляется запрос
            string requestUri = "http://demo.macroscop.com:8080/command?type=gettime&login=root&password=&responsetype=json";

            // допустимая погрешность между временем на сервере и локальным
            TimeSpan inaccuracy = TimeSpan.FromSeconds(15);

            // локальное время
            DateTime localTime = DateTime.Now;
            #endregion

            #region act
            // текст ответа в виде строки
            string responseString = await GetResponseStringAsync(requestUri);

            // параметры для преобразования в json и из него
            JsonSerializerOptions options = new JsonSerializerOptions();
            // добавляем пользовательский преобразователь для DateTime
            options.Converters.Add(new DateTimeConverterUsingDateTimeParse());

            // время на сервере из ответа
            DateTime serverTime = JsonSerializer.Deserialize<DateTime>(responseString, options);
            #endregion

            #region assert
            // Тест не провальный, если время в ответе не превышает локальное время на 15 секунд
            Assert.LessOrEqual(serverTime, localTime + inaccuracy);
            #endregion
        }

        /// <summary>
        /// Тестирование запроса времени сервера в формате XML.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestGetTimeXml()
        {
            #region arrange
            // URI, по которому отправляется запрос
            string requestUri = "http://demo.macroscop.com:8080/command?type=gettime&login=root&password=";

            // допустимая погрешность между временем на сервере и локальным
            TimeSpan inaccuracy = TimeSpan.FromSeconds(15);

            // локальное время
            DateTime localTime = DateTime.Now;
            #endregion

            #region act
            // текст ответа в виде строки
            string responseString = await GetResponseStringAsync(requestUri);

            // ответ в виде XML-документа
            XDocument responseXml = XDocument.Parse(responseString);

            // время на сервере из ответа
            DateTime serverTime = DateTime.Parse(responseXml.Element("string").Value);
            #endregion

            #region assert
            // Тест не провальный, если время в ответе не превышает локальное время на 15 секунд
            Assert.LessOrEqual(serverTime, localTime + inaccuracy);
            #endregion
        }

        /// <summary>
        /// Получение XML-документа с конфигурацией каналов.
        /// </summary>
        /// <returns>Объект задачи, представляющий асинхронную операцию.</returns>
        private async Task<XDocument> GetConfigexXml()
        {
            // // URI, по которому отправляется запрос
            string configexUri = "http://demo.macroscop.com:8080/configex?login=root&password=";

            // получение кофигурации каналов в виде строки
            string configexResponseString = await _client.GetStringAsync(configexUri);

            // конфигурация каналов в виде XML-документа
            return XDocument.Parse(configexResponseString);
        }

        /// <summary>
        /// Отправка запроса GET согласно указанному URI и возврат текста ответа в виде строки в асинхронной операции.
        /// </summary>
        /// <param name="requestUri">URI, по которому отправляется запрос.</param>
        /// <returns>Объект задачи, представляющий асинхронную операцию.</returns>
        private async Task<string> GetResponseStringAsync(string requestUri)
        {
            // получение текста ответа в виде строки
            string responseString = await _client.GetStringAsync(requestUri);

            // возвращаем ответ, удалив из него "лишнее"
            return RemoveExtraLines(responseString);
        }

        /// <summary>
        /// Удаление строк "Timestamp", "Error code", "Message", "Body-length" из ответа.
        /// </summary>
        /// <param name="responseString">Ответ сервера в виде строки.</param>
        /// <param name="sep">Разделитель.</param>
        /// <returns>Ответ сервера в виде строки, конфертируемой в XML или JSON.</returns>
        private static string RemoveExtraLines(string responseString, string sep = "\r\n")
        {
            // индекс первой строки и количество строк для удаления
            const int firstLineIndex = 0, linesToRemove = 4;

            // разделяем ответ на отдельные строки по заданному разделителю
            var responseLines = responseString.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries).ToList();
            // удаление строк
            responseLines.RemoveRange(firstLineIndex, linesToRemove);

            // соединемя оставшиеся строки в одну и возвращаем её
            return string.Join(null, responseLines);
        }
    }
}