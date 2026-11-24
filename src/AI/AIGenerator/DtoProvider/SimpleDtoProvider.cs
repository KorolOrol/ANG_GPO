using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using BaseClasses.Services;

namespace AIGenerator.DtoProvider
{
    public class SimpleDtoProvider : IDtoProvider
    {
        private readonly Dictionary<string, IParamKey> _paramKeys;
        private readonly Dictionary<string, IParamKey> _relationKeys;

        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public SimpleDtoProvider()
        {
            _paramKeys = new Dictionary<string, IParamKey>();
            _relationKeys = new Dictionary<string, IParamKey>();
        }
        
        public string ToDto(Plot plot)
        {
            foreach (var key in plot.Elements.SelectMany(element => element.Params.Keys)
                         .Where(key => !_paramKeys.ContainsKey(key.Name)))
            {
                _paramKeys[key.Name] = key;
            }
            foreach (var key in plot.Relations.Select(rel => rel.Param)
                         .Where(key => !_relationKeys.ContainsKey(key.Name)))
            {
                _relationKeys[key.Name] = key;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Param Keys:");
            foreach (var kv in _paramKeys)
            {
                sb.AppendLine($"- {kv.Key}: {TypeNameHelper.GetTypeName(kv.Value.ValueType)}");
            }
            sb.AppendLine("Relation Keys:");
            foreach (var kv in _relationKeys)
            {
                sb.AppendLine($"- {kv.Key}: {TypeNameHelper.GetTypeName(kv.Value.ValueType)}");
            }
            foreach (var element in plot.Elements)
            {
                sb.AppendLine(JsonSerializer.Serialize(new SimpleElementDto(element, plot), Options));
            }
            return sb.ToString();
        }

        public string ToDto(IElement element)
        {
            return JsonSerializer.Serialize(new SimpleElementDto(element), Options);
        }

        public IElement FromDto(string dto, Plot plot)
        {
            var dtoElement = JsonSerializer.Deserialize<SimpleElementDto>(dto, Options);
            if (dtoElement == null) throw new JsonException("Failed to deserialize SimpleElementDto.");
            var element = dtoElement.ToElement(plot, _paramKeys, _relationKeys);
            return element;
        }

        public string GetSchema()
        {
            return @"{
  ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
  ""$id"": ""SimpleElementDto.schema.json"",
  ""title"": ""SimpleElementDto"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
    ""Type"": {
      ""type"": ""string"",
      ""enum"": [""Character"", ""Item"", ""Location"", ""Event""],
      ""description"": ""Тип элемента, соответствующий BaseClasses.Enum.ElemType.""
    },
    ""Name"": {
      ""type"": ""string"",
      ""description"": ""Имя элемента.""
    },
    ""Description"": {
      ""type"": ""string"",
      ""description"": ""Описание элемента.""
    },
    ""Params"": {
      ""type"": ""object"",
      ""description"": ""Словарь параметров элемента, где ключ — имя параметра, значение — JSON-значение произвольного типа."",
      ""additionalProperties"": true
    },
    ""Relations"": {
      ""type"": ""array"",
      ""description"": ""Список связей элемента."",
      ""items"": {
        ""type"": ""object"",
        ""additionalProperties"": false,
        ""properties"": {
          ""Target"": {
            ""type"": ""string"",
            ""description"": ""Имя целевого элемента.""
          },
          ""Param"": {
            ""type"": ""string"",
            ""description"": ""Имя параметра связи.""
          },
          ""Value"": {
            ""description"": ""Значение связи."",
            ""oneOf"": [
              { ""type"": ""string"" },
              { ""type"": ""number"" },
              { ""type"": ""integer"" },
              { ""type"": ""boolean"" },
              { ""type"": ""null"" },
              { ""type"": ""object"" },
              { ""type"": ""array"", ""items"": {} }
            ]
          }
        },
        ""required"": [""Target"", ""Param"", ""Value""]
      }
    }
  },
  ""required"": [""Type"", ""Name"", ""Description"", ""Params"", ""Relations""]
}";
        }

        public List<(IElement, IParamKey, object)> GetNewElements(string dto, Plot plot)
        {
            var dtoElement = JsonSerializer.Deserialize<SimpleElementDto>(dto, Options);
            if (dtoElement == null) throw new JsonException("Failed to deserialize SimpleElementDto.");
            return dtoElement.Relations
                .Where(relation => plot.Elements.All(e => e.Name != relation.Target))
                .Select(relation =>
                {
                    var paramKey = _relationKeys.TryGetValue(relation.Param, out var foundKey) && foundKey != null
                        ? foundKey
                        : new ParamKey<JsonElement>(relation.Param, "AI.Raw");
                    try
                    {
                        var expectedType = plot.Binder.RegisteredRoutes
                            .Where(r => Equals(r.ParamKey, paramKey) &&
                                        r.SourceType == Enum.Parse<ElemType>(dtoElement.Type))
                            .Select(r => r.TargetType)
                            .First();
                        return ((IElement)new Element(expectedType, relation.Target, string.Empty), paramKey, 
                            JsonSerializer.Deserialize(relation.Value.GetRawText(), paramKey.ValueType, Options));
                    }
                    catch (InvalidOperationException)
                    {
                        return (null, null, null);
                    }
                })
                .Where(tuple => tuple.Item1 != null && tuple.Item2 != null)
                .ToList()!;
        }

        private class SimpleElementDto
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public Dictionary<string, JsonElement> Params { get; set; }
            public List<SimpleRelationDto> Relations { get; set; }

            public SimpleElementDto()
            {
                Type = string.Empty;
                Name = string.Empty;
                Description = string.Empty;
                Params = new Dictionary<string, JsonElement>();
                Relations = new List<SimpleRelationDto>();
            }

            public SimpleElementDto(IElement element, Plot? plot = null)
                : this()
            {
                Type = element.Type.ToString();
                Name = element.Name;
                Description = element.Description;
                Params = element.Params.Enumerate()
                    .ToDictionary(p => p.Key.Name,
                        p => JsonSerializer.SerializeToElement(p.Value, Options));
                if (plot != null)
                    Relations = plot.Relations.Where(r => Equals(r.Source, element))
                        .Select(r => new SimpleRelationDto
                        {
                            Target = r.Target.Name,
                            Param = r.Param.Name,
                            Value = JsonSerializer.SerializeToElement(r.Value, Options)
                        }).ToList();
                else
                    Relations = new List<SimpleRelationDto>();
            }

            public IElement ToElement(Plot plot,
                Dictionary<string, IParamKey> paramKeys,
                Dictionary<string, IParamKey> relationKeys)
            {
                var type = Enum.TryParse<ElemType>(Type, true, out var parsed)
                    ? parsed
                    : throw new JsonException("Invalid element type value.");
                var element = new Element(type, Name, Description);
                plot.Add(element);
                foreach (var param in Params)
                {
                    var key = paramKeys.TryGetValue(param.Key, out var foundKey) && foundKey != null
                        ? foundKey
                        : new ParamKey<JsonElement>(param.Key, "AI.Raw");
                    element.Params.Add(key, JsonSerializer.Deserialize(param.Value.GetRawText(), key.ValueType, Options));
                }
                foreach (var relation in Relations)
                {
                    var target = plot.Elements.FirstOrDefault(e => e.Name == relation.Target);
                    if  (target == null) continue;
                    var paramKey = relationKeys.TryGetValue(relation.Param, out var foundKey) && foundKey != null
                        ? foundKey
                        : new ParamKey<JsonElement>(relation.Param, "AI.Raw");
                    plot.Bind(element, target, paramKey,
                        JsonSerializer.Deserialize(relation.Value.GetRawText(), paramKey.ValueType, Options));
                }
                return element;
            }
        }

        private class SimpleRelationDto
        {
            public string Target { get; set; }
            public string Param { get; set; }
            public JsonElement Value { get; set; }

            public SimpleRelationDto()
            {
                Target = string.Empty;
                Param = string.Empty;
            }
        }
    }
}