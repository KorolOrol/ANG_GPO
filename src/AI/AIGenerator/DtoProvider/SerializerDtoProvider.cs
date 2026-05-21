using System.Collections.Generic;
using System.Text.Json;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services;

namespace AIGenerator.DtoProvider
{
    /// <summary>
    /// Провайдер для преобразования элементов и сюжета в DTO-формат и обратно, использующий сериализацию в JSON.
    /// </summary>
    public class SerializerDtoProvider : IDtoProvider
    {
        public string ToDto(Plot plot)
        {
            return Serializer.SerializeToString(plot);
        }

        public string ToDto(IElement element)
        {
            return Serializer.SerializeToString(element);
        }

        public IElement FromDto(string dto, Plot plot)
        {
            var raw = JsonDocument.Parse(dto).RootElement;
            var newElementJson = raw.GetProperty("NewElement");
            var newRelationsJson = raw.GetProperty("NewRelations");
            var newElement = Serializer.DeserializeString<Element>(newElementJson.GetRawText());
            plot.Add(newElement);
            foreach (var relationJson in newRelationsJson.EnumerateArray())
            {
                var relation = Serializer.DeserializeString<Relation>(relationJson.GetRawText(), plot);
                plot.Bind(relation.Source, relation.Target, relation.Param, relation.Value);
            }
            return newElement;
        }

        public string GetSchema()
        {
            return @"{
  ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
  ""$id"": ""https://example.com/schemas/llm-response.json"",
  ""title"": ""LlmResponse"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""required"": [""NewElement"", ""NewRelations""],
  ""properties"": {
    ""NewElement"": {
      ""title"": ""Element"",
      ""type"": ""object"",
      ""additionalProperties"": false,
      ""required"": [""Id"", ""Type"", ""Name"", ""Description"", ""Params"", ""Time""],
      ""properties"": {
        ""Id"": {
          ""type"": ""string"",
          ""format"": ""uuid"",
          ""description"": ""GUID элемента""
        },
        ""Type"": {
          ""description"": ""Имя enum ElemType или его числовое значение"",
          ""oneOf"": [
            { ""type"": ""string"", ""enum"": [""Character"", ""Item"", ""Location"", ""Event""] },
            { ""type"": ""integer"", ""enum"": [0, 1, 2, 3] }
          ]
        },
        ""Name"": {
          ""type"": ""string""
        },
        ""Description"": {
          ""type"": ""string""
        },
        ""Params"": {
          ""description"": ""Массив параметров"",
          ""type"": [""array"", ""null""],
          ""items"": {
            ""type"": ""object"",
            ""additionalProperties"": false,
            ""required"": [""Namespace"", ""Name"", ""Type"", ""Value""],
            ""properties"": {
              ""Namespace"": {
                ""type"": ""string"",
                ""description"": ""Пространство имен параметра""
              },
              ""Name"": {
                ""type"": ""string"",
                ""description"": ""Имя параметра""
              },
              ""Type"": {
                ""type"": ""string"",
                ""description"": ""Assembly-qualified или full name .NET-типа""
              },
              ""Value"": {
                ""description"": ""Значение параметра, сериализованное согласно Type"",
                ""type"": [""string"", ""number"", ""integer"", ""boolean"", ""object"", ""array"", ""null""]
              }
            }
          }
        },
        ""Time"": {
          ""type"": ""integer""
        }
      }
    },
    ""NewRelations"": {
      ""type"": ""array"",
      ""items"": {
        ""title"": ""Relation"",
        ""type"": ""object"",
        ""additionalProperties"": false,
        ""required"": [""Source"", ""Target"", ""Namespace"", ""Name"", ""Type"", ""Value""],
        ""properties"": {
          ""Source"": {
            ""type"": ""string"",
            ""format"": ""uuid"",
            ""description"": ""Id элемента-источника""
          },
          ""Target"": {
            ""type"": ""string"",
            ""format"": ""uuid"",
            ""description"": ""Id элемента-цели""
          },
          ""Namespace"": {
            ""type"": ""string""
          },
          ""Name"": {
            ""type"": ""string""
          },
          ""Type"": {
            ""type"": ""string"",
            ""description"": ""Assembly-qualified или full name .NET-типа""
          },
          ""Value"": {
            ""description"": ""Значение связи, сериализованное согласно Type"",
            ""type"": [""string"", ""number"", ""integer"", ""boolean"", ""object"", ""array"", ""null""]
          }
        }
      }
    }
  }
}";
        }

        public List<(IElement, IParamKey, object)> GetNewElements(string dto, Plot plot)
        {
            return new List<(IElement, IParamKey, object)>();
        }
    }
}