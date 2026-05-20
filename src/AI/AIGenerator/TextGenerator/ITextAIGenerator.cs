using System.Collections.Generic;
using System.Threading.Tasks;
using AIGenerator.Prompt;

namespace AIGenerator.TextGenerator
{
    /// <summary>
    /// Интерфейс для генерации текста с помощью AI.
    /// </summary>
    public interface ITextAiGenerator
    {
        /// <summary>
        /// Ключ API для AI.
        /// </summary>
        public string ApiKey { set; }

        /// <summary>
        /// Адрес API.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Модель для генерации текста.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Генерация текста
        /// </summary>
        /// <param name="messages">Список сообщений</param>
        /// <returns>Сгенерированный текст</returns>
        public Task<string> GenerateTextAsync(List<(PromptEntry, string)> messages);
    }
}
