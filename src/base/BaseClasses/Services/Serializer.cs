﻿using BaseClasses.Interface;
using BaseClasses.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseClasses.Services
{
    /// <summary>
    /// Сериализатор
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Настройки сериализации
        /// </summary>
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// Сериализация истории
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="path">Путь к файлу</param>
        public static void Serialize(Plot plot, string path)
        {
            var json = JsonSerializer.Serialize(plot, Options);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Сериализация элемента истории
        /// </summary>
        /// <param name="character">Элемент</param>
        /// <param name="path">Путь к файлу</param>
        public static void Serialize(IElement element, string path)
        {
            var json = JsonSerializer.Serialize(element, Options);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Десериализация
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Объект</returns>
        public static T Deserialize<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, Options);
        }

        /// <summary>
        /// Печать информации об истории
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="path">Путь к файлу</param>
        public static void Print(Plot plot, string path)
        {
            string data = plot.FullInfo();
            File.WriteAllText(path, data);
        }

        /// <summary>
        /// Печать информации об элементе истории
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <param name="path">Путь к файлу</param>
        public static void Print(IElement element, string path)
        {
            string data = element.FullInfo();
            File.WriteAllText(path, data);
        }
    }
}
