namespace AIGenerator.TextGenerator
{
    /// <summary>
    /// Интерфейс для поддержки структурированного вывода
    /// </summary>
    public interface ISupportStructuredOutput
    {
        /// <summary>
        /// Использовать структурированный вывод
        /// </summary>
        public bool UseStructuredOutput { get; set; }
    }
}
