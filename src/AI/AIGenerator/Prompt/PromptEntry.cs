namespace AIGenerator.Prompt
{
    /// <summary>
    /// Типы частей промпта.
    /// </summary>
    public enum PromptEntry
    {
        /// <summary>
        /// Контекст (системный промпт).
        /// </summary>
        Context,
        
        /// <summary>
        /// Запрос (пользовательский промпт).
        /// </summary>
        Request
    }
}