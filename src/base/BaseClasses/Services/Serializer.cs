using System;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Enum;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BaseClasses.Services
{
    /// <summary>
    /// Сериализатор. 
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Настройки сериализации. 
        /// </summary>
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        /// <summary>
        /// Сериализация истории. 
        /// </summary>
        /// <param name="plot">История.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Serialize(Plot plot, string path)
        {
            var json = JsonSerializer.Serialize(plot, Options);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Сериализация элемента истории. 
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Serialize(IElement element, string path)
        {
            var json = JsonSerializer.Serialize(element, Options);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Десериализация. 
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="path">Путь к файлу.</param>
        /// <returns>Объект.</returns>
        public static T Deserialize<T>(string path)
        {
            var resolver = new ElementsReferenceResolver();
            var json = File.ReadAllText(path);
            var document = JsonDocument.Parse(json);
            var rootElement = document.RootElement;
            if (typeof(T) == typeof(Plot) && MatchesProperties(typeof(Plot), rootElement))
            {
                Plot plot = ReadPlot(rootElement, resolver);
                return (T)Convert.ChangeType(plot, typeof(T));
            }
            else if (typeof(T) == typeof(Element) && MatchesProperties(typeof(Element), rootElement))
            {
                Element element = ReadElement(rootElement, resolver) as Element;
                return (T)Convert.ChangeType(element, typeof(T));
            }
            else
            {
                throw new ArgumentException($"Deserialization failed: Only 'Plot' and 'Element' types " +
                    $"are supported by this method. " 
                    + $"Attempted to deserialize type '{typeof(T).Name}', which is not supported. " +
                    $"JSON root properties: [{ string.Join(", ", rootElement .EnumerateObject() .Select(p => p.Name))}]");
            }
        }

        /// <summary>
        /// Чтение истории. 
        /// </summary>
        /// <param name="json">Json элемент.</param>
        /// <param name="resolver">Разрешитель ссылок.</param>
        /// <returns>История.</returns>
        private static Plot ReadPlot(JsonElement json, ReferenceResolver resolver)
        {
            var elementsProperty = json.GetProperty("Elements");
            var timeProperty = json.GetProperty("Time");

            var plot = new Plot();
            plot.Time = timeProperty.GetInt32();

            foreach (var element in elementsProperty.GetProperty("$values").EnumerateArray())
            {
                 var value = ReadValue(element, resolver);
                 if (value is Element el)
                 {
                     plot.Add(el);
                 }
                 else
                 {
                     throw new InvalidCastException("ReadValue did not return an Element as expected.");
                 }
            }

            return plot;
        }

        /// <summary>
        /// Чтение элемента. 
        /// </summary>
        /// <param name="json">Json элемент. </param>
        /// <param name="resolver">Разрешитель ссылок.</param>
        /// <returns>Элемент. </returns>
        private static IElement ReadElement(JsonElement json, ReferenceResolver resolver)
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

            resolver.AddReference(id.GetString(), plotElement);

            var @params = plotElement.Params;
            foreach (var param in json.GetProperty("Params").EnumerateObject())
            {
                if (param.Name == "$id") continue;
                @params.Add(param.Name, ReadValue(param.Value, resolver));
            }

            return plotElement;
        }

        /// <summary>
        /// Чтение значения. 
        /// </summary>
        /// <param name="json">Json элемент.</param>
        /// <param name="resolver">Разрешитель ссылок.</param>
        /// <returns>Значение.</returns>
        /// <exception cref="JsonException">Исключение, если тип не определен.</exception>
        private static object ReadValue(JsonElement json, ReferenceResolver resolver)
        {

            switch (json.ValueKind)
            {
                case JsonValueKind.Object:
                    if (json.TryGetProperty("$values", out JsonElement arrayJson))
                    {
                        List<object> values = new List<object>();
                        foreach (var elem in arrayJson.EnumerateArray())
                        {
                            values.Add(ReadValue(elem, resolver));
                        }
                        return ConvertList(values);
                    }
                    else if (json.TryGetProperty("$ref", out JsonElement @ref))
                    {
                        return resolver.ResolveReference(@ref.GetString());
                    }
                    else
                    {
                        if (MatchesProperties(typeof(Element), json))
                        {
                            return ReadElement(json, resolver);
                        }
                        else if (MatchesProperties(typeof(Relation), json))
                        {
                            Relation rel = new Relation()
                            {
                                Character = ReadValue(json.GetProperty("Character"), resolver) as IElement,
                                Value = json.GetProperty("Value").GetDouble()
                            };
                            return rel;
                        }
                    }
                    break;
                case JsonValueKind.String:
                    if (DateTime.TryParse(json.GetString(), out DateTime date))
                    {
                        return date;
                    }
                    return json.GetString();
                case JsonValueKind.Number:
                    return json.GetDouble();
                case JsonValueKind.Null:
                    return null;
            }
            throw new JsonException($"Could not determine or deserialize type from JSON value. ValueKind: {json.ValueKind}, Value: {json.ToString()}");
        }

        /// <summary>
        /// Печать информации об истории. 
        /// </summary>
        /// <param name="plot">История.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Print(Plot plot, string path)
        {
            string data = plot.FullInfo();
            File.WriteAllText(path, data);
        }

        /// <summary>
        /// Печать информации об элементе истории. 
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Print(IElement element, string path)
        {
            string data = element.FullInfo();
            File.WriteAllText(path, data);
        }

        /// <summary>
        /// Конвертация списка в типизированный список. 
        /// </summary>
        /// <param name="obj">Конвертируемый объект.</param>
        /// <returns>Типизированный список или оригинальный объект, если это не список.</returns>
        private static object ConvertList(object obj)
        {
            if (obj is IList list && list.Count > 0)
            {
                Type elementType = list[0].GetType();
                Type listType;
                if (elementType == typeof(Element))
                {
                    listType = typeof(List<>).MakeGenericType(typeof(IElement));
                }
                else
                {
                    listType = typeof(List<>).MakeGenericType(elementType);
                }
                IList typedList = (IList)Activator.CreateInstance(listType);
                foreach (var item in list)
                {
                    typedList.Add(item);
                }
                return typedList;
            }
            return obj;
        }

        /// <summary>
        /// Проверка на соответствие свойств.
        /// </summary>
        /// <param name="type">Тип.</param>
        /// <param name="json">Json элемент.</param>
        /// <returns>True, если свойства совпадают, иначе False.</returns>
        private static bool MatchesProperties(Type type, JsonElement json)
        {
            foreach (var property in type.GetProperties()
                .Where(p => !p.IsDefined(typeof(JsonIgnoreAttribute), true)))
            {
                if (!json.TryGetProperty(property.Name, out _))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
