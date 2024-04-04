using BaseClasses.Model;
using Newtonsoft.Json;

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
        public static JsonSerializerSettings Settings { get; } = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

        /// <summary>
        /// Сериализация истории
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="path">Путь к файлу</param>
        public static void Serialize(Plot plot, string path)
        {
            var json = JsonConvert.SerializeObject(plot, Settings);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Сериализация персонажа
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="path">Путь к файлу</param>
        public static void Serialize(Character character, string path)
        {
            var json = JsonConvert.SerializeObject(character, Settings);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Сериализация локации
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="path">Путь к файлу</param>
        public static void Serialize(Location location, string path)
        {
            var json = JsonConvert.SerializeObject(location, Settings);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Сериализация предмета
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="path">Путь к файлу</param>
        public static void Serialize(Item item, string path)
        {
            var json = JsonConvert.SerializeObject(item, Settings);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Сериализация события
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="path">Путь к файлу</param>
        public static void Serialize(Event @event, string path)
        {
            var json = JsonConvert.SerializeObject(@event, Settings);
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
            return JsonConvert.DeserializeObject<T>(json);
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
        /// Печать информации о персонаже
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="path">Путь к файлу</param>
        public static void Print(Character character, string path)
        {
            string data = character.FullInfo();
            File.WriteAllText(path, data);
        }

        /// <summary>
        /// Печать информации о локации
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="path">Путь к файлу</param>
        public static void Print(Location location, string path)
        {
            string data = location.FullInfo();
            File.WriteAllText(path, data);
        }

        /// <summary>
        /// Печать информации о предмете
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="path">Путь к файлу</param>
        public static void Print(Item item, string path)
        {
            string data = item.FullInfo();
            File.WriteAllText(path, data);
        }

        /// <summary>
        /// Печать информации о событии
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="path">Путь к файлу</param>
        public static void Print(Event @event, string path)
        {
            string data = @event.FullInfo();
            File.WriteAllText(path, data);
        }
    }
}
