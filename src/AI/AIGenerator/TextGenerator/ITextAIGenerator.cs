using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIGenerator.TextGenerator
{
    /// <summary>
    /// Интерфейс для генерации текста с помощью AI
    /// </summary>
    public interface ITextAiGenerator
    {
        /// <summary>
        /// Ключ API для AI
        /// </summary>
        public string ApiKey { set; }

        /// <summary>
        /// Адрес API
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Модель для генерации текста
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Генерация текста
        /// </summary>
        /// <param name="messages">Список сообщений</param>
        /// <returns>Сгенерированный текст</returns>
        public async Task<string> GenerateTextAsync(List<string> messages)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }
    }
}
