using BepInEx;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace LethalSanity
{
	internal class Config
	{
		// ======================================================================[ Properties ]====================================================================== \\
		internal static int P_PP { get; private set; }
		internal static ItemData Vignette { get; private set; }
		internal static ItemData FilmGrain { get; private set; }
		internal static ItemData ChromaticAberation { get; private set; }
		internal static ItemData LensDistortion { get; private set; }
		internal static ItemData DOF_Start { get; private set; }
		internal static ItemData DOF_End { get; private set; }
		internal static ItemData Saturation { get; private set; }

		private readonly string xmlPath = Path.Combine(Paths.ConfigPath, "LethalSanity.xml");
		private readonly string cfgPath = Path.Combine(Paths.ConfigPath, "LethalSanity.cfg");

		// =======================================================================[ Methods ]======================================================================= \\
		internal Config()
		{
			// =============================================================[ Create new XML if not exist ]============================================================= \\
			if (!File.Exists(xmlPath))
			{
				Main.mls.LogMessage("Creating XML");
				ApplyXmlData(null);
				SaveXML();
				Main.mls.LogMessage("Created XML");
			}

			// ===================================================================[ Load XML config ]=================================================================== \\
			XmlSerializer serializer = new(typeof(ConfigData));
			using (StreamReader reader = new(xmlPath))
			{
				ConfigData configData = (ConfigData)serializer.Deserialize(reader);
				ApplyXmlData(configData);
			}

			// =============================[ After the XML config has loaded, we then load the BepInEx config to override default values. ]============================= \\
			ConfigFile _config = new(cfgPath, true, new(Main.modGUID, Main.modName, Main.modVer));

			P_PP = _config.Bind("", "Priority", 1, "In Unity, the post-processing priority value determines which volume's effects are applied first when multiple volumes overlap.\nHigher priority values take precedence, allowing for specific area effects to override global ones.\nSet the value higher if effects are being wacky.").Value;

			Vignette.Enabled = _config.Bind("Vignette", "Enabled", true, "Should this effect be enabled?").Value;
			Vignette.Kickin = _config.Bind("Vignette", "Kickin", 25, "At what insanity level should this effect kick in at?").Value;
			Vignette.Offset = _config.Bind("Vignette", "Offset", 3, "This applies a slight randomizaton to the kickin value. (Kickin ± Offset)").Value;

			FilmGrain.Enabled = _config.Bind("FilmGrain", "Enabled", true, "Should this effect be enabled?").Value;
			FilmGrain.Kickin = _config.Bind("FilmGrain", "Kickin", 30, "At what insanity level should this effect kick in at?").Value;
			FilmGrain.Offset = _config.Bind("FilmGrain", "Offset", 5, "This applies a slight randomizaton to the kickin value. (Kickin ± Offset)").Value;

			ChromaticAberation.Enabled = _config.Bind("ChromaticAberation", "Enabled", true, "Should this effect be enabled?").Value;
			ChromaticAberation.Kickin = _config.Bind("ChromaticAberation", "Kickin", 40, "At what insanity level should this effect kick in at?").Value;
			ChromaticAberation.Offset = _config.Bind("ChromaticAberation", "Offset", 5, "This applies a slight randomizaton to the kickin value. (Kickin ± Offset)").Value;

			LensDistortion.Enabled = _config.Bind("LensDistortion", "Enabled", true, "Should this effect be enabled?").Value;
			LensDistortion.Kickin = _config.Bind("LensDistortion", "Kickin", 35, "At what insanity level should this effect kick in at?").Value;
			LensDistortion.Offset = _config.Bind("LensDistortion", "Offset", 4, "This applies a slight randomizaton to the kickin value. (Kickin ± Offset)").Value;

			DOF_Start.Enabled = DOF_End.Enabled = _config.Bind("DepthOfField", "Enabled", true, "Should this effect be enabled?").Value;
			DOF_Start.Kickin = DOF_End.Kickin = _config.Bind("DepthOfField", "Kickin", 45, "At what insanity level should this effect kick in at?").Value;
			DOF_Start.Offset = DOF_End.Offset = _config.Bind("DepthOfField", "Offset", 5, "This applies a slight randomizaton to the kickin value. (Kickin ± Offset)").Value;

			Saturation.Enabled = _config.Bind("Saturation", "Enabled", true, "Should this effect be enabled?").Value;
			Saturation.Kickin = _config.Bind("Saturation", "Kickin", 35, "At what insanity level should this effect kick in at?").Value;
			Saturation.Offset = _config.Bind("Saturation", "Offset", 4, "This applies a slight randomizaton to the kickin value. (Kickin ± Offset)").Value;

			// ================================[ Then we save the XML once again because we modified the values with the BepInEx values ]================================ \\
			SaveXML();

			// =================================================[ Then just print simple info about each one to verify ]================================================= \\
			foreach (ItemData item in new[] { Vignette, FilmGrain, ChromaticAberation, LensDistortion, DOF_Start, DOF_End, Saturation }.ToList())
			{
				Main.mls.LogMessage($"	[{item.Name}]\n - {item.Enabled}\n - {item.Kickin}\n - {item.Offset}\n - {item.EaseInTimeMin}\n - {item.EaseInTimeMax}\n - {item.EaseOutTimeMin}\n - {item.EaseOutTimeMax}\n - {item.EaseInIntensityMin}\n - {item.EaseInIntensityMax}\n - {item.EaseOutIntensityMin}\n - {item.EaseOutIntensityMax}");
			}
		}

		/// <summary>
		/// Hahaha spagetti :3
		/// </summary>
		/// <param name="configData"></param>
		private void ApplyXmlData(ConfigData configData)
		{
			// Assign loaded XML data to individual items
			Vignette = configData?.Items.Find(i => i.Name == "Vignette") ?? new ItemData
			{
				Name = "Vignette",
				Enabled = true,
				Kickin = 20,
				Offset = 3,
				EaseInTimeMin = 20,
				EaseInTimeMax = 40,
				EaseOutTimeMin = 5,
				EaseOutTimeMax = 10,
				EaseInIntensityMin = 0.45f,
				EaseInIntensityMax = 0.6f,
				EaseOutIntensityMin = 0.15f,
				EaseOutIntensityMax = 0.15f
			};
			FilmGrain = configData?.Items.Find(i => i.Name == "Film Grain") ?? new ItemData
			{
				Name = "Film Grain",
				Enabled = true,
				Kickin = 20,
				Offset = 5,
				EaseInTimeMin = 30,
				EaseInTimeMax = 60,
				EaseOutTimeMin = 8,
				EaseOutTimeMax = 16,
				EaseInIntensityMin = 0.4f,
				EaseInIntensityMax = 0.8f,
				EaseOutIntensityMin = 0,
				EaseOutIntensityMax = 0
			};
			ChromaticAberation = configData?.Items.Find(i => i.Name == "Chromatic Aberation") ?? new ItemData
			{
				Name = "Chromatic Aberation",
				Enabled = true,
				Kickin = 30,
				Offset = 5,
				EaseInTimeMin = 40,
				EaseInTimeMax = 60,
				EaseOutTimeMin = 8,
				EaseOutTimeMax = 16,
				EaseInIntensityMin = 0.9f,
				EaseInIntensityMax = 1.5f,
				EaseOutIntensityMin = 0,
				EaseOutIntensityMax = 0
			};
			LensDistortion = configData?.Items.Find(i => i.Name == "Lens Distortion") ?? new ItemData
			{
				Name = "Lens Distortion",
				Enabled = true,
				Kickin = 30,
				Offset = 10,
				EaseInTimeMin = 15,
				EaseInTimeMax = 30,
				EaseOutTimeMin = 8,
				EaseOutTimeMax = 16,
				EaseInIntensityMin = 0.4f,
				EaseInIntensityMax = 0.6f,
				EaseOutIntensityMin = 0,
				EaseOutIntensityMax = 0
			};
			DOF_Start = configData?.Items.Find(i => i.Name == "DepthOfField Far") ?? new ItemData
			{
				Name = "DepthOfField Far",
				Enabled = true,
				Kickin = 40,
				Offset = 5,
				EaseInTimeMin = 15,
				EaseInTimeMax = 30,
				EaseOutTimeMin = 8,
				EaseOutTimeMax = 16,
				EaseInIntensityMin = 3,
				EaseInIntensityMax = 8,
				EaseOutIntensityMin = 2000,
				EaseOutIntensityMax = 2000
			};
			DOF_End = configData?.Items.Find(i => i.Name == "DepthOfField Near") ?? new ItemData
			{
				Name = "DepthOfField Near",
				Enabled = true,
				Kickin = 40,
				Offset = 5,
				EaseInTimeMin = 18,
				EaseInTimeMax = 38,
				EaseOutTimeMin = 13,
				EaseOutTimeMax = 20,
				EaseInIntensityMin = 15,
				EaseInIntensityMax = 30,
				EaseOutIntensityMin = 2000,
				EaseOutIntensityMax = 2000
			};
			Saturation = configData?.Items.Find(i => i.Name == "Saturation") ?? new ItemData
			{
				Name = "Saturation",
				Enabled = true,
				Kickin = 50,
				Offset = 10,
				EaseInTimeMin = 20,
				EaseInTimeMax = 30,
				EaseOutTimeMin = 2,
				EaseOutTimeMax = 5,
				EaseInIntensityMin = 50,
				EaseInIntensityMax = 70,
				EaseOutIntensityMin = 0,
				EaseOutIntensityMax = 0
			};
		}

		/// <summary>
		/// Write XML data do the xmlPath
		/// </summary>
		private void SaveXML()
		{
			ConfigData configData = new() { Items = [Vignette, FilmGrain, ChromaticAberation, LensDistortion, DOF_Start, DOF_End, Saturation] };

			XmlSerializer serializer = new(typeof(ConfigData));
			using StreamWriter writer = new(xmlPath);
			serializer.Serialize(writer, configData);
		}
	}

	[XmlRoot("Items")]
	public class ConfigData
	{
		[XmlElement("Item")]
		public List<ItemData> Items { get; set; } = [];
	}

	[XmlRoot("Items")]
	public class ItemData
	{
		[XmlAttribute("Name")]
		public string Name { get; set; }

		[XmlElement("Enabled")]
		public bool Enabled { get; set; }

		[XmlElement("Kickin")]
		public float Kickin { get; set; }

		[XmlElement("Offset")]
		public float Offset { get; set; }

		[XmlElement("EaseInTimeMin")]
		public float EaseInTimeMin { get; set; }

		[XmlElement("EaseInTimeMax")]
		public float EaseInTimeMax { get; set; }

		[XmlElement("EaseOutTimeMin")]
		public float EaseOutTimeMin { get; set; }

		[XmlElement("EaseOutTimeMax")]
		public float EaseOutTimeMax { get; set; }

		[XmlElement("EaseInIntensityMin")]
		public float EaseInIntensityMin { get; set; }

		[XmlElement("EaseInIntensityMax")]
		public float EaseInIntensityMax { get; set; }

		[XmlElement("EaseOutIntensityMin")]
		public float EaseOutIntensityMin { get; set; }

		[XmlElement("EaseOutIntensityMax")]
		public float EaseOutIntensityMax { get; set; }
	}
}