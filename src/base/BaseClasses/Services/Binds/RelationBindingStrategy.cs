using BaseClasses.Interface;
using BaseClasses.Model;

namespace BaseClasses.Services.Binds
{
    /// <summary>
    /// Делегат для стратегии связывания элементов истории.
    /// </summary>
    public delegate void RelationBindingStrategy(IElement source, IElement target, IParamKey param, object? value,
        Plot plot);
}