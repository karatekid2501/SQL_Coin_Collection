using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoinCollection
{
    /// <summary>
    /// Base class for the JsonValue
    /// </summary>
    /// <param name="keyName">Name of the key</param>
    public abstract class JsonValueEditBase(string keyName)
    {
        public string KeyName { get; private set; } = keyName;

        /// <summary>
        /// Value to return for the child class
        /// </summary>
        /// <returns>Returns the value of the child class as an object</returns>
        public abstract object? GetValue();
    }

    /// <summary>
    /// Main class for storing values to use for the Json file
    /// </summary>
    /// <typeparam name="T">Type of value to store into the Json file</typeparam>
    /// <param name="keyName">Name of the key</param>
    /// <param name="keyValue">Name of the value</param>
    public class JsonValueEdit<T>(string keyName, T keyValue) : JsonValueEditBase(keyName) where T : notnull
    {
        public T KeyValue { get; private set; } = keyValue;

        public override object? GetValue()
        {
            return KeyValue;
        }
    }

    /// <summary>
    /// Group of the Json value classes
    /// </summary>
    /// <param name="name">Name of the group</param>
    /// <param name="values">Values to store with in the group</param>
    public class JsonValueEditGroup(string name, params JsonValueEditBase[] values)
    {
        public string Name { get; private set; } = name;
        public JsonValueEditBase[] JsonValues { get; private set; } = values;
    }

    /// <summary>
    /// Parentless group of single values
    /// </summary>
    /// <param name="values">Values to store with in the group</param>
    public class JsonValueEditGroupSingles(params JsonValueEditBase[] values) : JsonValueEditGroup(string.Empty, values) { }

    /// <summary>
    /// Json configerator editor
    /// </summary>
    public class JsonConfigEditor
    {
        public bool CheckJsonConfig { get { return _jsonConfig != null; } }

        public bool ConfigFileExist { get { return File.Exists(ConfigPath); } }

        public string ConfigPath { get; private set; }

        private readonly string _jsonConfigName;

        private readonly JsonSerializerOptions _options;

        private JsonObject? _jsonConfig;

        public JsonConfigEditor(string jsonFileName = "", JsonSerializerOptions options = null!)
        {
            if(string.IsNullOrEmpty(jsonFileName))
            {
                jsonFileName = "appsettings";
            }

            _jsonConfigName = jsonFileName;

            ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), $"{jsonFileName}.json");

            UpdateJsonFileReference();

            if(_jsonConfig == null)
            {
                Debug.WriteLine("JsonConfig file not found!!!");
            }

            _options = options;
        }

        /// <summary>
        /// Updates the JsonObject from the saved Json file
        /// </summary>
        public bool UpdateJsonFileReference()
        {
            if(File.Exists(ConfigPath))
            {
                _jsonConfig = (JsonObject?)JsonObject.Parse(File.ReadAllText(ConfigPath));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the JsonObject settings file
        /// </summary>
        /// <param name="save">Should the created JsonObject be saved to the Json file</param>
        /// <param name="groups">Values to create the new JsonObject</param>
        public void Create(bool save, params JsonValueEditGroup[] groups)
        {
            JsonObject parent = [];

            foreach (JsonValueEditGroup group in groups)
            {
                if(string.IsNullOrEmpty(group.Name))
                {
                    CreateChildren(ref parent, group.JsonValues);
                }
                else
                {
                    parent.Add(group.Name, CreateChildren(group.JsonValues));
                }
            }

            _jsonConfig = parent;

            if (save)
            {
                UpdateConfigFile();
            }
        }

        /// <summary>
        /// Sets a value with in the Json file
        /// </summary>
        /// <param name="value">Value to find</param>
        /// <param name="parents">Parent hierarchy</param>
        /// <returns>True if the value was foun and was the correct type</returns>
        public bool Set(JsonValueEditBase value, params string[] parents)
        {
            return Set(value.KeyName, value.GetValue()!, parents);
        }

        /// <summary>
        /// Sets a value with in the Json file
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="keyName">Name of the key</param>
        /// <param name="keyValue">Value to set</param>
        /// <param name="parents">Parent hierarchy</param>
        /// <returns>True if the value was foun and was the correct type</returns>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Set<T>(string keyName, T keyValue, params string[] parents) where T : notnull
        {
            JsonObject foundParent = FindParent(_jsonConfig!, parents);

            if (foundParent!.ContainsKey(keyName))
            {
                //https://stackoverflow.com/questions/71587944/how-to-find-out-what-type-a-jsonvalue-is-in-system-text-json
                if (foundParent[keyName]!.AsValue().TryGetValue(out T? _))
                {
                    foundParent[keyName] = JsonValue.Create(keyValue);

                    return true;
                }
                else
                {
                    throw new InvalidCastException($"{keyName} is not type of {typeof(T)}!!!");
                }
            }
            else
            {
                throw new ArgumentNullException($"No value by the name of {keyName}, is in {_jsonConfigName} json file!!!");
            }
        }

        /// <summary>
        /// Gets a value stored in the JsonObject config variable
        /// </summary>
        /// <typeparam name="T">Type to convert found value to</typeparam>
        /// <param name="keyName">Name of the value to get</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">Unable to convert a value from Json config file to type</exception>
        /// <exception cref="ArgumentNullException">Unable to find the value with in the Json config file</exception>
        public T Get<T>(string keyName, params string[] parents)
        {
            JsonObject foundParent = FindParent(_jsonConfig!, parents);

            if (foundParent!.ContainsKey(keyName))
            {
                if (foundParent[keyName]!.AsValue().TryGetValue(out T? value))
                {
                    return value;
                }
                else
                {
                    throw new InvalidCastException($"Unable to convert {keyName} to {nameof(T)}!!!");
                }
            }
            else
            {
                throw new ArgumentNullException($"No value by the name of {keyName}, is in {_jsonConfigName} json file!!!");
            }
        }

        /// <summary>
        /// Saves the JSON file
        /// </summary>
        /// <param name="options">Formate options for the JSON file</param>
        public void UpdateConfigFile(JsonSerializerOptions options = null!)
        {
            if(options == null && _options != null)
            {
                options = _options;
            }

            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(_jsonConfig, options));
        }

        /// <summary>
        /// Creates children with in a JsonObject
        /// </summary>
        /// <param name="values">The values to set within the JsonObject</param>
        /// <returns>JsonObject that has children in it</returns>
        private static JsonObject CreateChildren(JsonValueEditBase[] values)
        {
            JsonObject jObject = [];

            CreateChildren(ref jObject, values);

            return jObject;
        }

        /// <summary>
        /// Creates children with in a JsonObject
        /// </summary>
        /// <param name="parent">Reference to the JsonObject that the children will be saved in</param>
        /// <param name="values">The values to set within the JsonObject</param>
        /// <returns>JsonObject that has children in it</returns>
        private static void CreateChildren(ref JsonObject parent, JsonValueEditBase[] values)
        {
            foreach (JsonValueEditBase jsonValueEditBase in values)
            {
                parent[jsonValueEditBase.KeyName] = JsonValue.Create(jsonValueEditBase.GetValue());
            }
        }

        /// <summary>
        /// Finds the parent of the JsonObject
        /// </summary>
        /// <param name="jsonObject">JsonObject to find</param>
        /// <param name="parents">Parent hierarchy</param>
        /// <returns>The found JsonObject</returns>
        /// <exception cref="Exception"></exception>
        private static JsonObject FindParent(JsonObject jsonObject, params string[] parents)
        {
            if(parents.Length == 0)
            {
                return jsonObject;
            }

            JsonObject currentObject = jsonObject;

            foreach (string name in parents)
            { 
                if (currentObject!.ContainsKey(name))
                {
                    currentObject = (JsonObject)currentObject[name]!;
                }
                else
                {
                    throw new Exception($"Unable to find {name} in {currentObject.GetPropertyName()}!!!");
                }
            }

            return currentObject;
        }
    }
}
