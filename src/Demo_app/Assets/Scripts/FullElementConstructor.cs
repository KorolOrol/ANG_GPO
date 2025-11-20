using System;
using System.Collections.Generic;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;

public static class FullElementConstructor
{
    public static Element CreateFullElement(ElemType type)
    {
        Dictionary<string, object> @params = new Dictionary<string, object>();
        switch (type)
        {
            case ElemType.Character:
                {
                    @params.Add("Traits", new List<string>());
                    @params.Add("Phobias", new List<string>());
                    @params.Add("Relations", new List<Relation>());
                    @params.Add("Items", new List<IElement>());
                    @params.Add("Locations", new List<IElement>());
                    @params.Add("Events", new List<IElement>());
                }
                break;
            case ElemType.Item:
                {
                    @params.Add("Host", null);
                    @params.Add("Location", null);
                    @params.Add("Events", new List<IElement>());
                }
                break;
            case ElemType.Location:
                {
                    @params.Add("Characters", new List<IElement>());
                    @params.Add("Items", new List<IElement>());
                    @params.Add("Events", new List<IElement>());
                }
                break;
            case ElemType.Event:
                {
                    @params.Add("Characters", new List<IElement>());
                    @params.Add("Items", new List<IElement>());
                    @params.Add("Locations", new List<IElement>());
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        var element = new Element(type, @params:@params);
        return element;
    }
    
    public static Element CreateFullElement(ElemType type, string name)
    {
        var element = CreateFullElement(type);
        element.Name = name;
        return element;
    }
}