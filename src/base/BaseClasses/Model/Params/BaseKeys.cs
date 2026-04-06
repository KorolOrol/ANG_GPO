using System.Collections.Generic;
using BaseClasses.Interface;

namespace BaseClasses.Model.Params
{
    public static class BaseKeys
    {
        public static readonly ParamKey<List<Relation>> Relations = new ParamKey<List<Relation>>("Relations");
        public static readonly ParamKey<List<IElement>> Characters = new ParamKey<List<IElement>>("Characters");
        public static readonly ParamKey<List<IElement>> Locations = new ParamKey<List<IElement>>("Locations");
        public static readonly ParamKey<List<IElement>> Items = new ParamKey<List<IElement>>("Items");
        public static readonly ParamKey<List<IElement>> Events = new ParamKey<List<IElement>>("Events");
        
        public static readonly ParamKey<IElement> Host = new ParamKey<IElement>("Host");
        public static readonly ParamKey<IElement> Location = new ParamKey<IElement>("Location");
    }
}