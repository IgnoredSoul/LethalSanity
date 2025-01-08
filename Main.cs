using BepInEx;
using HarmonyLib;
using BepInEx.Logging;

namespace LethalSanity
{
	[BepInPlugin(modGUID, modName, modVer)]
	[BepInProcess("Lethal Company.exe")]
	public class Main : BaseUnityPlugin
	{
		// ========================================================================[ Properties ]======================================================================== \\
		internal const string modVer = "3.0";

		internal const string modName = "LethalSanity";
		internal const string modGUID = "com.Github.IGNOREDSOUL.LethalSanity";

		internal static Main Instance { get; private set; }
		internal static ManualLogSource mls { get; private set; }
		internal Harmony harmony { get; private set; }

		// ===========================================================================[ Methods ]======================================================================== \\
		private void Awake()
		{
			// ==============================================================[ Setting the main instance ]=============================================================== \\
			while (!Instance) Instance = this;

			// ============================================================[ Setting up the BepInEx logging ]============================================================ \\
			mls = BepInEx.Logging.Logger.CreateLogSource(modName);

			// ===============================================================[ Create Harmony Instance ]================================================================ \\
			harmony = new(modGUID);

			// ======================================================================[ Patch yall ]====================================================================== \\
			harmony.PatchAll(typeof(Patching));

			// ================================================================[ Setting up the config ]================================================================= \\
			new Config();

			// =================================[ Set to when the HUDManager calls its Awake method, add the PostProcessing component ]================================== \\
			Patching.Connect += (GameNetcodeStuff.PlayerControllerB __instance) => __instance.gameObject.AddComponent<PostProcessing>();
            Patching.Connect += (GameNetcodeStuff.PlayerControllerB __instance) => __instance.gameObject.AddComponent<TextManager>();
            Patching.SanityChanged += SanityEventManager.OnSanityChange;
			Patching.SanityChanged += (float v, bool d) => TextManager.instance?.UpdateSanity(v, d);
        }
	}
}