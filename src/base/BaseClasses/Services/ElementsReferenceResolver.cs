using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseClasses.Services
{
    internal class ElementsReferenceResolver : ReferenceResolver
    {
        private uint _referenceCount;
        private readonly Dictionary<string, object> _referenceIdToObjectMap = [];
        private readonly Dictionary<object, string> _objectToReferenceIdMap = new(ReferenceEqualityComparer.Instance);

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
