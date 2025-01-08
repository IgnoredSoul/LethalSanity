using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using BepInEx;
using Newtonsoft.Json.Linq;
using System.Text;

namespace LethalSanity
{
	internal class JsonHandler
	{
		private static readonly string jsonPath = Path.Combine(Paths.ConfigPath, "LethalSanity.json");

		/// <summary>
		/// Loads the <see cref="ConfigData"/> from file.
		/// </summary>
		/// <returns><see cref="ConfigData"/></returns>
		internal static ConfigData LoadJsonFromFile()
		{
			try
			{
				if (File.Exists(jsonPath))
				{
					string json = File.ReadAllText(jsonPath);
					return JsonConvert.DeserializeObject<ConfigData>(json, new ConfigDataConverter());
				}
			}
			catch { }

			return null;
		}

		internal static ConfigData LoadJsonFromBase64(string Compressed64)
		{
			try
			{
				return JsonConvert.DeserializeObject<ConfigData>(Compressed64, new ConfigDataConverter());
			}
			catch { }
			return null;
		}

		/// <summary>
		/// Saves the <see cref="ConfigData"/> to file.
		/// </summary>
		/// <returns>Compressed Base64 of the <see cref="EffectData"></see></returns>
		internal static string SaveJsonToFile(ref ConfigData data)
		{
			// Compress and store the Effects data in JsonKey
			data.JsonKey = CompressString(JsonConvert.SerializeObject(data.Effects));

			// Serialize the complete ConfigData
			string json = JsonConvert.SerializeObject(data, Formatting.Indented, new ConfigDataConverter());

			// Write JSON to file
			File.WriteAllText(jsonPath, json);

			// Return the compressed effects data
			return data.JsonKey;
		}
	}

	internal class ConfigDataConverter : JsonConverter<ConfigData>
	{
		public override ConfigData ReadJson(JsonReader reader, Type objectType, ConfigData existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);
			ConfigData configData = new ConfigData();

			// Deserialize JsonKey with default fallback
			configData.JsonKey = jsonObject.TryGetValue("JsonKey", out JToken jsonKeyToken)
				? jsonKeyToken.ToString()
				: "default_key";

			// Decompress Effects if JsonKey is present
			if (!string.IsNullOrEmpty(configData.JsonKey))
			{
				try
				{
					configData.Effects = JsonConvert.DeserializeObject<List<EffectData>>(JsonHandler.DecompressString(configData.JsonKey));
				}
				catch
				{
					configData.Effects = new List<EffectData>(); // Default empty list if decompression fails
				}
			}

			// If Effects field is present, use it
			if (jsonObject.TryGetValue("Effects", out JToken effectsToken) && configData.Effects.Count == 0)
			{
				configData.Effects = effectsToken.ToObject<List<EffectData>>();
			}

			return configData;
		}

		public override void WriteJson(JsonWriter writer, ConfigData value, JsonSerializer serializer)
		{
			JObject jsonObject = new() { { "JsonKey", value.JsonKey }, { "Effects", JArray.FromObject(value.Effects) } }; jsonObject.WriteTo(writer);
		}
	}

	public class ConfigData
	{
		[JsonProperty("JsonKey")]
		public string JsonKey { get; set; } = "default_key";

		[JsonProperty("Effects")]
		public List<EffectData> Effects { get; set; } = new();
	}

	public class EffectData
	{
		[JsonProperty("Name")]
		public string Name { get; set; } = "Unknown";

		[JsonProperty("Enabled")]
		public bool Enabled { get; set; } = false;

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