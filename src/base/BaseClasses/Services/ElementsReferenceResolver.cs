using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseClasses.Services
{
    internal class ElementsReferenceResolver : ReferenceResolver
    {
        private uint _referenceCount;
        private readonly Dictionary<string, object> _referenceIdToObjectMap = 
            new Dictionary<string, object> { };
        private readonly Dictionary<object, string> _objectToReferenceIdMap = 
            new Dictionary<object, string>(ReferenceEqualityComparerSingleton.Instance);
        
        private sealed class ReferenceEqualityComparerSingleton : IEqualityComparer<object>
        {
            public readonly static ReferenceEqualityComparerSingleton Instance = new ReferenceEqualityComparerSingleton();
            private ReferenceEqualityComparerSingleton() { }
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        public override void AddReference(string referenceId, object value)
        {
            if (!_referenceIdToObjectMap.TryAdd(referenceId, value))
            {
                throw new JsonException($"Reference ID already exists: {referenceId}");
            }
        }

        public override string GetReference(object value, out bool alreadyExists)
        {
            if (_objectToReferenceIdMap.TryGetValue(value, out string? referenceId))
            {
                alreadyExists = true;
            }
            else
            {
                _referenceCount++;
                referenceId = _referenceCount.ToString();
                _objectToReferenceIdMap.Add(value, referenceId);
                alreadyExists = false;
            }

            return referenceId;
        }

        public override object ResolveReference(string referenceId)
        {
            if (!_referenceIdToObjectMap.TryGetValue(referenceId, out object? value))
            {
                throw new JsonException($"Reference ID '{referenceId}' not found.");
            }

            return value;
        }
    }
}
