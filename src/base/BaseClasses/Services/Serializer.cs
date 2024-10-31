using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Enum;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

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
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        /// <summary>
        /// Разрешитель ссылок
        /// </summary>
        private static ReferenceResolver ReferenceResolver { get; set; }

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
            ReferenceResolver = new ElementsReferenceResolver();
            var json = File.ReadAllText(path);
            var document = JsonDocument.Parse(json);
            var rootElement = document.RootElement;
            if (typeof(T) == typeof(Plot) && rootElement.GetProperty("$type").GetString() == "Plot")
            {
                Plot plot = ReadPlot(rootElement);
                return (T)Convert.ChangeType(plot, typeof(T));
            }
            else if (typeof(T) == typeof(Element) && 
                rootElement.GetProperty("$type").GetString() == "Element")
            {
                Element element = ReadElement(rootElement);
                return (T)Convert.ChangeType(element, typeof(T));
            }
            else
            {
                throw new ArgumentException("Invalid type");
            }
        }

        /// <summary>
        /// Чтение истории
        /// </summary>
        /// <param name="json">Json элемент</param>
        /// <returns>История</returns>
        private static Plot ReadPlot(JsonElement json)
        {
            var elementsProperty = json.GetProperty("Elements");
            var timeProperty = json.GetProperty("Time");

            var plot = new Plot();
            plot.Time = timeProperty.GetInt32();

            foreach (var element in elementsProperty.GetProperty("$values").EnumerateArray())
            {
                 plot.Add((Element)ReadValue(element));
            }

            return plot;
        }

        /// <summary>
        /// Чтение элемента
        /// </summary>
        /// <param name="json">Json элемент</param>
        /// <returns>Элемент</returns>
        private static Element ReadElement(JsonElement json)
        {
            var typeProperty = json.GetProperty("Type");
            var nameProperty = json.GetProperty("Name");
            var descriptionProperty = json.GetProperty("Description");
            var timeProperty = json.GetProperty("Time");
            var id = json.GetProperty("$id");

            var plotElement = new Element((ElemType)typeProperty.GetInt32());
            plotElement.Name = nameProperty.GetString();
            plotElement.Description = descriptionProperty.GetString();
            plotElement.Time = timeProperty.GetInt32();

            ReferenceResolver.AddReference(id.GetString(), plotElement);

            var @params = plotElement.Params;
            foreach (var param in json.GetProperty("Params").EnumerateObject())
            {
                if (param.Name == "$id") continue;
                @params.Add(param.Name, ReadValue(param.Value));
            }

            return plotElement;
        }

        /// <summary>
        /// Чтение значения
        /// </summary>
        /// <param name="json">Json элемент</param>
        /// <returns>Значение</returns>
        /// <exception cref="JsonException">Исключение, если тип не определен</exception>
        private static object ReadValue(JsonElement json)
        {
            
            switch (json.ValueKind)
            {
                case JsonValueKind.Object:
                    if (json.TryGetProperty("$values", out JsonElement arrayJson))
                    {
                        List<object> values = new List<object>();
                        foreach (var elem in arrayJson.EnumerateArray())
                        {
                            values.Add(ReadValue(elem));
                        }
                        return values;
                    }
                    else if (json.TryGetProperty("$ref", out JsonElement @ref))
                    {
                        return ReferenceResolver.ResolveReference(@ref.GetString());
                    }
                    else if (json.TryGetProperty("$type", out JsonElement type))
                    {
                        if (type.GetString() == "Relation")
                        {
                            Relation rel = new();
                            rel.Character = (Element)ReadValue(json.GetProperty("Character"));
                            rel.Value = json.GetProperty("Value").GetDouble();
                            return rel;
                        }
                        else if (type.GetString() == "Element")
                        {
                            return ReadElement(json);
                        }
                    }
                    break;
                case JsonValueKind.String:
                    return json.GetString();
                case JsonValueKind.Number:
                    return json.GetDouble();
                case JsonValueKind.Null:
                    return null;
            }
            throw new JsonException(json.GetString());
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
