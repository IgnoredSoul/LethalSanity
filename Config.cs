using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace LethalSanity
{
    internal class Config
    {
        internal static JsonData JsonData;
        protected private readonly string jsonPath = Path.Combine(Paths.ConfigPath, "LethalSanity.json");
        protected private readonly string cfgPath = Path.Combine(Paths.ConfigPath, "LethalSanity.cfg");

        // Do not touch, leave it.
        protected private readonly Dictionary<string, EffectData> defaultEffects = new()
        {
            { "Vignette", new EffectData { KickinMin = 23, KickinMax = 30, KickoutMin = -1, KickoutMax = -1, EaseInTimeMin = 35, EaseInTimeMax = 40, EaseOutTimeMin = 5, EaseOutTimeMax = 10, EaseInIntensityMin = 0.5f, EaseInIntensityMax = 0.6f, EaseOutIntensityMin = 0.15f, EaseOutIntensityMax = 0.15f } },
            { "Film Grain", new EffectData { KickinMin = 24, KickinMax = 28, KickoutMin = -1, KickoutMax = -1, EaseInTimeMin = 45, EaseInTimeMax = 60, EaseOutTimeMin = 8, EaseOutTimeMax = 16, EaseInIntensityMin = 0.7f, EaseInIntensityMax = 0.89f, EaseOutIntensityMin = 0, EaseOutIntensityMax = 0 } },
            { "Chromatic Aberation", new EffectData { KickinMin = 40, KickinMax = 60, KickoutMin = -1, KickoutMax = -1, EaseInTimeMin = 55, EaseInTimeMax = 60, EaseOutTimeMin = 8, EaseOutTimeMax = 16, EaseInIntensityMin = 0.9f, EaseInIntensityMax = 1.5f, EaseOutIntensityMin = 0, EaseOutIntensityMax = 0 } },
            { "Lens Distortion", new EffectData { KickinMin = 30, KickinMax = 50, KickoutMin = -1, KickoutMax = -1, EaseInTimeMin = 25, EaseInTimeMax = 30, EaseOutTimeMin = 8, EaseOutTimeMax = 16, EaseInIntensityMin = 0.4f, EaseInIntensityMax = 0.6f, EaseOutIntensityMin = 0, EaseOutIntensityMax = 0} },
            { "DepthOfField Start", new EffectData { KickinMin = 30, KickinMax = 35, KickoutMin = -1, KickoutMax = -1, EaseInTimeMin = 60, EaseInTimeMax = 70, EaseOutTimeMin = 8, EaseOutTimeMax = 16, EaseInIntensityMin = 3, EaseInIntensityMax = 8, EaseOutIntensityMin = 1000, EaseOutIntensityMax = 1000} },
            { "DepthOfField End", new EffectData { KickinMin = 30, KickinMax = 35, KickoutMin = -1, KickoutMax = -1, EaseInTimeMin = 60, EaseInTimeMax = 70, EaseOutTimeMin = 13, EaseOutTimeMax = 20, EaseInIntensityMin = 15, EaseInIntensityMax = 30, EaseOutIntensityMin = 1000, EaseOutIntensityMax = 1000} },
            { "Saturation", new EffectData { KickinMin = 25, KickinMax = 40, KickoutMin = -1, KickoutMax = -1, EaseInTimeMin = 25, EaseInTimeMax = 30, EaseOutTimeMin = 2, EaseOutTimeMax = 5, EaseInIntensityMin = -50, EaseInIntensityMax = -70, EaseOutIntensityMin = 0, EaseOutIntensityMax = 0} }
        };

        internal Config()
        {
            // Setup BepInEx config items
            ConfigFile configFile = new(cfgPath, true);
            JsonData = new() { Effects = defaultEffects, Key = string.Empty };
            ConfigEntry<string> ExKey = configFile.Bind("_Config Base64 Key", "ExKey", string.Empty, "This is used for loading and or sharing config.\nI'd suggest you keep your copy before you lose it.");

            // Does the config file exist?
            if (!File.Exists(jsonPath))
            {
                // Save the Json file with its default effects applied
                SaveJSON();
            }
            
            // Load just cause
            LoadJSON();

            // If the JsonData key is not set, set it
            if(!string.IsNullOrEmpty(JsonData.Key))
            {
                JsonData.Key = CompressEffects(JsonData.Effects);
            }

            // If the ExKey is not set, set it to the JsonData.key
            if(!string.IsNullOrEmpty(ExKey.Value))
            {
                ExKey.Value = JsonData.Key;
            }

            // Next, we need to check if the ExKey differs from the stored JsonData.key
            // Also check if it's a valid Base64 key and is decompressable
            if ((IsB64(ExKey.Value) && IsED(ExKey.Value)) && ExKey.Value.Equals(JsonData.Key, StringComparison.CurrentCultureIgnoreCase))
            {
                // Decompress the ExKey and write it to file.
                Dictionary<string, EffectData> dict = DecompressEffects(ExKey.Value);

                // Rewrite JsonData data
                JsonData.Effects = dict;
                SaveJSON();
                LoadJSON();
                return;
            }

            // Make sure values are good.
            VerifyEffects();

            // Effects that are controlled through the launched config editor.
            BindEffect(configFile, "Vignette");
            BindEffect(configFile, "Film Grain");
            BindEffect(configFile, "Saturation");
            BindEffect(configFile, "Lens Distortion");
            BindEffect(configFile, "Chromatic Aberation");

            JsonData.Effects["DepthOfField End"].Enabled = JsonData.Effects["DepthOfField Start"].Enabled = configFile.Bind("Depth of Field", "Enabled", true, "").Value;
            JsonData.Effects["DepthOfField End"].KickinMin = JsonData.Effects["DepthOfField Start"].KickinMin = configFile.Bind("Depth of Field", "KickinMin", defaultEffects["DepthOfField Start"].KickinMin, "").Value;
            JsonData.Effects["DepthOfField End"].KickinMax = JsonData.Effects["DepthOfField Start"].KickinMax = configFile.Bind("Depth of Field", "KickinMax", defaultEffects["DepthOfField Start"].KickinMax, "").Value;

            ExKey.Value = CompressEffects(JsonData.Effects);
        }

        /// <summary>
        /// Im lazy
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="effectName"></param>
        private void BindEffect(ConfigFile configFile, string effectName)
        {
            var effect = JsonData.Effects[effectName];
            effect.Enabled = configFile.Bind(effectName, "Enabled", true, "").Value;
            effect.KickinMin = configFile.Bind(effectName, "KickinMin", defaultEffects[effectName].KickinMin, "").Value;
            effect.KickinMax = configFile.Bind(effectName, "KickinMax", defaultEffects[effectName].KickinMax, "").Value;
        }

        /// <summary>
        /// Checks if any of the values are set to -1, resseting it to it's default value
        /// </summary>
        protected private void VerifyEffects()
        {
            foreach (KeyValuePair<string, EffectData> kvp in defaultEffects)
            {
                if (!JsonData.Effects.TryGetValue(kvp.Key, out var effect))
                {
                    JsonData.Effects[kvp.Key] = new EffectData(); // Add missing key with default
                    effect = JsonData.Effects[kvp.Key];
                }

                foreach (System.Reflection.PropertyInfo prop in typeof(EffectData).GetProperties().Where(prop => prop.PropertyType == typeof(float) && (float)prop.GetValue(effect) == -1))
                {
                    prop.SetValue(effect, prop.GetValue(kvp.Value));
                }
            }
        }

        /// <summary>
        /// Checks if the string is of base64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected private bool IsB64(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;

            if (str.Length % 4 != 0) return false;

            try { Convert.FromBase64String(str); return true; }
            catch (FormatException) { return false; }
        }

        /// <summary>
        /// Checks if the string can deserialise into a JsonData object
        /// </summary>
        /// <param name="js"></param>
        /// <returns></returns>
        protected private bool IsED(string js)
        {
            try
            {
                var s = DecompressEffects(js);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Write's the JsonData to the json file
        /// </summary>
        protected private void SaveJSON()
        {
            // Get the compressed key of the Effect's dictionary
            string effectsDict = CompressEffects(JsonData.Effects);

            // Put the key in the JsonData
            JsonData.Key = effectsDict;

            // Serialize JsonData class and write to file
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(JsonData, Formatting.Indented));
        }

        /// <summary>
        /// Loads the JsonData from the json file
        /// </summary>
        /// <exception cref="Exception"></exception>
        protected private void LoadJSON()
        {
            // If the file somehow does not exist... idfk it should?
            if (!File.Exists(jsonPath)) throw new Exception("What...?");

            // Deserialize the json file and store it
            JsonData = JsonConvert.DeserializeObject<JsonData>(File.ReadAllText(jsonPath));
        }

        /// <summary>
        /// Compress's a <see cref="EffectData"/> dictionary to <see cref="string"/> base64.
        /// </summary>
        protected private string CompressEffects(Dictionary<string, EffectData> data)
        {
            try
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                using MemoryStream outputStream = new();
                using (DeflateStream deflateStream = new(outputStream, CompressionLevel.Optimal))
                    deflateStream.Write(inputBytes, 0, inputBytes.Length);
                return Convert.ToBase64String(outputStream.ToArray());
            }
            catch (Exception ex) { Main.mls.LogError($"Failed to convert Effect dictionary to compressed base 64:\n{ex}\n"); }
            return string.Empty;
        }

        /// <summary>
        /// Decompress's a compressed base64 <see cref="string"/> to a <see cref="EffectData"/> dictionary.
        /// </summary>
        protected private Dictionary<string, EffectData> DecompressEffects(string compressedData)
        {
            try
            {
                using MemoryStream inputStream = new(Convert.FromBase64String(compressedData));
                using DeflateStream deflateStream = new(inputStream, CompressionMode.Decompress);
                using MemoryStream outputStream = new(); deflateStream.CopyTo(outputStream);
                return JsonConvert.DeserializeObject<Dictionary<string, EffectData>>(Encoding.UTF8.GetString(outputStream.ToArray()));
            }
            catch (Exception ex) { Main.mls.LogError($"Failed to convert compressed base 64 to Effect dictionary:\n{ex}\n"); }
            return null;
        }
    }

    /// <summary>
    /// Compiled effect data
    /// </summary>
    public class Effect
    {
        public bool Enabled { get; set; }
        public float OnSanity { get; set; }
        public float OffSanity { get; set; }
        public float EaseInTime { get; set; }
        public float EaseOutTime { get; set; }
        public float EaseInIntensity { get; set; }
        public float EaseOutIntensity { get; set; }

        public Effect(EffectData data)
        {
            Enabled = data.Enabled;
            OnSanity = NumberUtils.Next(data.KickinMin, data.KickinMax);
            OffSanity = (data.KickoutMin == -1 || data.KickoutMax == -1) ? OnSanity : NumberUtils.Next(data.KickoutMin, data.KickoutMax);
            EaseInTime = NumberUtils.Next(data.EaseInTimeMin, data.EaseInTimeMax);
            EaseOutTime = NumberUtils.Next(data.EaseOutTimeMin, data.EaseOutTimeMax);
            EaseInIntensity = NumberUtils.Next(data.EaseInIntensityMin, data.EaseInIntensityMax);
            EaseOutIntensity = NumberUtils.Next(data.EaseOutIntensityMin, data.EaseOutIntensityMax);
        }
    }

    /// <summary>
    /// Holding data with key for easy rewrites
    /// </summary>
    public class JsonData
    {
        public string Key { get; set; }
        public Dictionary<string, EffectData> Effects { get; set; }
    }

    /// <summary>
    /// Data for the json file
    /// </summary>
    public class EffectData
    {
        [JsonProperty("Enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("KickinMin")]
        public float KickinMin { get; set; } = -1;

        [JsonProperty("KickinMax")]
        public float KickinMax { get; set; } = -1;

        [JsonProperty("KickoutMin")]
        public float KickoutMin { get; set; } = -1;

        [JsonProperty("KickoutMax")]
        public float KickoutMax { get; set; } = -1;

        [JsonProperty("EaseInTimeMin")]
        public float EaseInTimeMin { get; set; } = -1;

        [JsonProperty("EaseInTimeMax")]
        public float EaseInTimeMax { get; set; } = -1;

        [JsonProperty("EaseOutTimeMin")]
        public float EaseOutTimeMin { get; set; } = -1;

        [JsonProperty("EaseOutTimeMax")]
        public float EaseOutTimeMax { get; set; } = -1;

        [JsonProperty("EaseInIntensityMin")]
        public float EaseInIntensityMin { get; set; } = -1;

        [JsonProperty("EaseInIntensityMax")]
        public float EaseInIntensityMax { get; set; } = -1;

        [JsonProperty("EaseOutIntensityMin")]
        public float EaseOutIntensityMin { get; set; } = -1;

        [JsonProperty("EaseOutIntensityMax")]
        public float EaseOutIntensityMax { get; set; } = -1;
    }
}