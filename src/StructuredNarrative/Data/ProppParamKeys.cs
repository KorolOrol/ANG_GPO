using BaseClasses.Model.Params;

namespace StructuredNarrative.Data
{
    /// <summary>
    /// Ключи параметров для функций Проппа.
    /// </summary>
    public static class ProppParamKeys
    {
        /// <summary>
        /// Роль, участвующая в функции Проппа (Hero, Villain, Helper и т.д.).
        /// </summary>
        public static ParamKey<string> Role = new ParamKey<string>("Role", "Propp");
        
        /// <summary>
        /// Функция Проппа (например, "Борьба", "Появление помощника" и т.д.).
        /// </summary>
        public static ParamKey<string> Function = new ParamKey<string>("Function", "Propp");
        
        /// <summary>
        /// Символ функции по Проппу (α, β, γ, А, В, С и т.д.).
        /// </summary>
        public static ParamKey<string> Symbol = new ParamKey<string>("Symbol", "Propp");
        
        /// <summary>
        /// Акт, к которому принадлежит функция (Setup, Confrontation, Resolution).
        /// </summary>
        public static ParamKey<string> NarrativePhase = new ParamKey<string>("NarrativePhase", "Propp");
        
        /// <summary>
        /// Порядковый номер функции в канонической последовательности Проппа (1-31).
        /// </summary>
        public static ParamKey<int> Order = new ParamKey<int>("Order", "Propp");
    }
}