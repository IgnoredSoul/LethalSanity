using GameNetcodeStuff;

using HarmonyLib;
using System;
using UnityEngine.InputSystem;

namespace LethalSanity
{
	public class Patching
	{
		// ===========================================================[ Connect Client To Player Object ]=========================================================== \\
		[HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
		[HarmonyPostfix]
		private static void _CONNECT(PlayerControllerB __instance)
		{
			if (GameNetworkManager.Instance.localPlayerController == __instance)
			{
				Connect?.Invoke(__instance);
			}
		}

		internal static event Action<PlayerControllerB> Connect;

		// ===============================================================[ Set Player Sanity Level ]================================================================ \\
		[HarmonyPatch(typeof(PlayerControllerB), "SetPlayerSanityLevel")]
		[HarmonyPostfix]
		private static void _SANITY_SET()

		{
			// If the player does not exist or is dead.
			if (!LocalPlayer.Player || LocalPlayer.PlayerController.isPlayerDead) return;

			// if the previous value is not the same as the current value
			float parsed = float.Parse(LocalPlayer.Insanity.ToString("0.0"));
			if (_prev_san != parsed)
			{
				// Set prev value to new
				_prev_san = parsed;

				// Invoke virtual
				SanityChanged?.Invoke(parsed);
			}
		}

		private static float _prev_san { get; set; } = -3.141f;

		internal static event Action<float> SanityChanged;
	}
}