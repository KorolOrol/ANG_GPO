using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AIGenerator.Prompt
{
    /// <summary>
    /// Класс для построения промпта, который будет использоваться для генерации текста с помощью AI.
    /// Позволяет добавлять сообщения разных типов (контекст, запрос, схема) и использовать шаблоны для создания сообщений.
    /// Шаблоны могут быть загружены из JSON-файла или добавлены вручную. В итоге строит список сообщений,
    /// который может быть передан в генератор текста.
    /// </summary>
    public class PromptBuilder
    {
        private readonly List<(PromptEntry Entry, string Text)> _messages = new List<(PromptEntry, string)>();
        private readonly Dictionary<string, string> _templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Инициализирует новый экземпляр класса PromptBuilder без шаблонов.
        /// Шаблоны могут быть добавлены позже с помощью метода AddTemplate или LoadTemplates.
        /// </summary>
        public PromptBuilder()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса PromptBuilder и загружает шаблоны из JSON-файла по указанному пути.
        /// </summary>
        /// <param name="templatePath">Путь к JSON-файлу, содержащему шаблоны. JSON должен представлять собой словарь,
        /// где ключами являются строки (ключи шаблонов), а значениями - строки (тексты шаблонов).</param>
        public PromptBuilder(string templatePath)
        {
            LoadTemplates(templatePath);
        }
        
        /// <summary>
        /// Инициализирует новый экземпляр класса PromptBuilder и загружает шаблоны из предоставленного словаря.
        /// </summary>
        /// <param name="templates">Словарь шаблонов, где ключами являются строки (ключи шаблонов),
        /// а значениями - строки (тексты шаблонов).</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если словарь шаблонов является null.</exception>
        public PromptBuilder(Dictionary<string, string> templates)
        {
            if (templates == null)
            {
                throw new ArgumentNullException(nameof(templates));
            }

            foreach (var pair in templates)
            {
                _templates[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Добавляет шаблон с указанным ключом и текстом. Если шаблон с таким ключом уже существует, он будет перезаписан.
        /// </summary>
        /// <param name="key">Ключ шаблона, который будет использоваться для получения текста шаблона.
        /// Ключи шаблонов не чувствительны к регистру.</param>
        /// <param name="template">Текст шаблона, который может содержать плейсхолдеры для замены аргументов.
        /// Плейсхолдеры должны быть в формате {0}, {1} и т.д., в зависимости от количества аргументов.</param>
        /// <exception cref="ArgumentException">Выбрасывается, если ключ шаблона является null,
        /// пустой строкой или состоит только из пробелов.</exception>
        /// <exception cref="ArgumentNullException">Выбрасывается, если текст шаблона является null.</exception>
        public void AddTemplate(string key, string template)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Template key is required.", nameof(key));
            }

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            _templates[key] = template;
        }

        /// <summary>
        /// Пытается получить текст шаблона по указанному ключу. Ключи шаблонов не чувствительны к регистру.
        /// </summary>
        /// <param name="key">Ключ шаблона, который будет использоваться для получения текста шаблона.
        /// Ключи шаблонов не чувствительны к регистру.</param>
        /// <param name="template">Выходной параметр, который будет содержать текст шаблона,
        /// если шаблон с указанным ключом найден; иначе - пустая строка.</param>
        /// <returns>True, если шаблон с указанным ключом найден; иначе - false.</returns>
        public bool TryGetTemplate(string key, out string template)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                template = string.Empty;
                return false;
            }

            return _templates.TryGetValue(key, out template!);
        }

        /// <summary>
        /// Загружает шаблоны из JSON-файла по указанному пути. JSON должен представлять собой словарь,
        /// где ключами являются строки (ключи шаблонов), а значениями - строки (тексты шаблонов).
        /// Если JSON содержит корневой объект с именем "Templates", то шаблоны будут загружены из этого объекта.
        /// В противном случае, JSON будет десериализован напрямую в словарь шаблонов.
        /// Если шаблон с таким ключом уже существует, он будет перезаписан. Ключи шаблонов не чувствительны к регистру.
        /// </summary>
        /// <param name="path">Путь к JSON-файлу, содержащему шаблоны. JSON должен представлять собой словарь,
        /// где ключами являются строки (ключи шаблонов), а значениями - строки (тексты шаблонов).</param>
        /// <exception cref="ArgumentException">Выбрасывается, если путь является null, пустой строкой
        /// или состоит только из пробелов.</exception>
        /// <exception cref="InvalidDataException">Выбрасывается, если JSON-файл пустой
        /// или содержит недопустимый формат.</exception>
        public void LoadTemplates(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path is required.", nameof(path));
            }

            string json = File.ReadAllText(path);
            Dictionary<string, string>? templates = DeserializeTemplateDictionary(json);
            if (templates == null)
            {
                throw new InvalidDataException("Template JSON is empty or invalid.");
            }

            foreach (var pair in templates)
            {
                _templates[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Добавляет сообщение в промпт с указанным типом (контекст, запрос или схема) и текстом.
        /// </summary>
        /// <param name="entry">Тип сообщения (контекст, запрос или схема).</param>
        /// <param name="text">Текст сообщения. Не может быть null, пустой строкой или состоять только из пробелов.</param>
        /// <exception cref="ArgumentException">Выбрасывается, если текст сообщения является null,
        /// пустой строкой или состоит только из пробелов.</exception>
        public void AddMessage(PromptEntry entry, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Message text is required.", nameof(text));
            }

            _messages.Add((entry, text));
        }

        /// <summary>
        /// Добавляет сообщение в промпт, используя шаблон с указанным ключом и аргументами для замены.
        /// </summary>
        /// <param name="entry">Тип сообщения (контекст, запрос или схема).</param>
        /// <param name="templateKey">Ключ шаблона, который будет использоваться для получения текста шаблона.
        ///     Ключи шаблонов не чувствительны к регистру.</param>
        /// <param name="args">Аргументы для замены в шаблоне. Аргументы будут заменены в шаблоне в порядке их следования.
        ///     Если шаблон не содержит плейсхолдеров, аргументы будут игнорироваться.
        ///     Если аргументы не предоставлены, шаблон будет использоваться без изменений.</param>
        /// <exception cref="KeyNotFoundException">Выбрасывается, если шаблон с указанным ключом не найден.</exception>
        public void AddMessageFromTemplate(PromptEntry entry, string templateKey, params object?[] args)
        {
            if (!_templates.TryGetValue(templateKey, out var template))
            {
                throw new KeyNotFoundException($"Template not found: {templateKey}");
            }

            string text = args == null || args.Length == 0 ? template : string.Format(template, args);
            AddMessage(entry, text);
        }

        /// <summary>
        /// Строит промпт, возвращая список сообщений, где каждое сообщение представлено кортежем
        /// из типа сообщения (PromptEntry) и текста сообщения (string).
        /// </summary>
        /// <returns>Список сообщений, представляющих построенный промпт.</returns>
        public List<(PromptEntry, string)> Build()
        {
            return new List<(PromptEntry, string)>(_messages);
        }

        /// <summary>
        /// Десериализует JSON-строку в словарь шаблонов.
        /// </summary>
        /// <param name="json">JSON-строка, представляющая словарь шаблонов.</param>
        /// <returns>Словарь шаблонов, если десериализация прошла успешно; иначе - null.</returns>
        public static Dictionary<string, string>? DeserializeTemplateDictionary(string json)
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (document.RootElement.TryGetProperty("Templates", out var templatesElement))
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(templatesElement.GetRawText());
            }

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
    }
}