using DbNetSuiteCore.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DbNetSuiteCore.Extensions;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class JsonUpdateRequest
    {
        private string _primaryKey;
        private Dictionary<string, object> _changes;
        public string? PrimaryKey
        {
            set => _primaryKey = value;
        }
        public string PrimaryKeyName
        {
            get => primaryKey().Keys.First();
        }
        public object PrimaryKeyValue
        {
            get => ((JsonElement)primaryKey().Values.First()).Value();
        }
        public string? EditMode { get; set; }
        public Dictionary<string, object> Changes
        {
            get => convertChanges();
            set => _changes = value;
        }
        public object? FormData { get; set; }

        private Dictionary<string, object> primaryKey()
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(EncodingHelper.Decode(_primaryKey));
        }

        private Dictionary<string, object> convertChanges()
        {
            Dictionary<string, object> convertedChanges = new Dictionary<string, object>();
            foreach (string key in _changes.Keys)
            {
                convertedChanges[key] = ((JsonElement)_changes[key]).Value();
            }

            return convertedChanges;
        }
    };
}