using BepInEx;
using HarmonyLib;
using BepInEx.Logging;

namespace LethalSanity
{
	[BepInPlugin(modGUID, modName, modVer)]
	[BepInProcess("Lethal Company.exe")]
	public class Main : BaseUnityPlugin
	{
		internal const string modVer = "3.0";
		internal const string modName = "LethalSanity";
		internal const string modGUID = "com.Github.IGNOREDSOUL.LethalSanity";

		internal static Main Instance { get; private set; }
		internal static ManualLogSource mls { get; private set; }
		internal readonly Harmony harmony = new(modGUID);
		internal static Config config { get; private set; }

		private void Awake()
		{
			// ==============================================================[ Setting the main instance ]============================================================== \\
			while (!Instance) Instance = this;

			// ============================================================[ Setting up the BepInEx logging ]============================================================ \\
			mls = BepInEx.Logging.Logger.CreateLogSource(modName);

			// ======================================================================[ Patch yall ]====================================================================== \\
			harmony.PatchAll(typeof(Patching));

			// ================================================================[ Setting up the config ]================================================================ \\
			new Config();

			// ==========================================================[ Setting up the SanityEventManager ]========================================================== \\
			new SanityEventManager();

			// =================================[ Set to when the HUDManager calls its Awake method, add the PostProcessing component ]================================= \\
			Patching.Connect += ((GameNetcodeStuff.PlayerControllerB __instance) => __instance.gameObject.AddComponent<PostProcessing>());
		}
	}
}