using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestTask
{
    /// <summary>
    /// Считывает и преобразует JSON в тип DateTime.
    /// </summary>
    public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
    {
        /// <summary>
        /// Считывает и преобразует JSON в тип DateTime с помощью метода Parse(string s).
        /// </summary>
        /// <param name="reader">Средство чтения.</param>
        /// <param name="typeToConvert">Тип, преобразование которого выполняется.</param>
        /// <param name="options">Объект, указывающий используемые параметры сериализации.</param>
        /// <returns>Преобразованное значение.</returns>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        /// <summary>
        /// Записывает указанное значение DateTime в формате JSON с помощью метода ToString().
        /// </summary>
        /// <param name="writer">Модуль записи, в который производится запись.</param>
        /// <param name="value">Значение для преобразования в JSON.</param>
        /// <param name="options">Объект, указывающий используемые параметры сериализации.</param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
