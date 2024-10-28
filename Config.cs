using System;
using BepInEx;
using System.IO;
using BepInEx.Configuration;

namespace LethalSanity
{
	internal class Config
	{
		internal static int P_PP { get; private set; }
		internal static ConfigItem Vignette { get; private set; }
		internal static ConfigItem FilmGrain { get; private set; }
		internal static ConfigItem ChromaticAberation { get; private set; }
		internal static ConfigItem LensDistortion { get; private set; }
		internal static ConfigItem DOF { get; private set; }
		internal static ConfigItem Saturation { get; private set; }

		internal Config()
		{
			// ====================================================================[ Create / Read ]==================================================================== \\
			ConfigFile _config = new(Path.Combine(Paths.ConfigPath, "LethalSanity.cfg"), true, new(Main.modGUID, Main.modName, Main.modVer));

			// ===================================================================[ Post Processing ]=================================================================== \\
			P_PP = _config.Bind("", "Priority", 1, "In Unity, the post-processing priority value determines which volume's effects are applied first when multiple volumes overlap.\nHigher priority values take precedence, allowing for specific area effects to override global ones.\nSet the value higher if effects are being wacky.").Value;
			Vignette = ConvertInput(_config.Bind("", "Vignette activation", "true, 25, 3", "Toggle, insanity level, offset"));
			FilmGrain = ConvertInput(_config.Bind("", "Film Grain activation", "true, 30, 5", "Toggle, insanity level, offset"));
			ChromaticAberation = ConvertInput(_config.Bind("", "Chromatic Aberation activation", "true, 40, 5", "Toggle, insanity level, offset"));
			LensDistortion = ConvertInput(_config.Bind("", "Lens Distortion activation", "true, 35, 4", "Toggle, insanity level, offset"));
			DOF = ConvertInput(_config.Bind("", "Depth of Field activation", "true, 50, 5", "Toggle, insanity level, offset"));
			Saturation = ConvertInput(_config.Bind("", "Saturation activation", "true, 50, 3", "Toggle, insanity level, offset"));
		}

		private ConfigItem ConvertInput(ConfigEntry<string> input)
		{
			// Try to use the users values
			try
			{
				// First, if the input is valid
				string[] rtrn = input.Value?.Replace(" ", "").Split(',');
				if (rtrn?.Length != 3) throw new ArgumentException("Invalid amount. Defaulting.");

				// Second, run TryParses
				if (!bool.TryParse(rtrn[0].ToLower().Trim(), out _)) throw new ArgumentException($"({input.Definition.Key}) Item 1: {rtrn[0]}, does not parse into a bool.");
				if (!float.TryParse(rtrn[1].Trim(), out _)) throw new ArgumentException($"({input.Definition.Key}) Item 2: {rtrn[1]}, does not parse into a float.");
				if (!float.TryParse(rtrn[2].Trim(), out _)) throw new ArgumentException($"({input.Definition.Key}) Item 3: {rtrn[2]}, does not parse into a float.");

				// Return tuple with converted values
				return new(bool.Parse(rtrn[0]), float.Parse(rtrn[1]), float.Parse(rtrn[2]));
			}
			catch (ArgumentException e) { Main.mls.LogFatal(e); }

			// If we cannot, just use default
			string[] rtrn2 = input.DefaultValue.ToString().Replace(" ", "").Split(',');
			return new(bool.Parse(rtrn2[0]), float.Parse(rtrn2[1]), float.Parse(rtrn2[2]));
		}
	}

	internal struct ConfigItem
	{
		public bool Enabled { get; }
		public float Kickin { get; }
		public float Offset { get; }

		public ConfigItem(bool enabled, float kickin, float offset)
		{
			Enabled = enabled;
			Kickin = kickin;
			Offset = offset;
		}
	}
}