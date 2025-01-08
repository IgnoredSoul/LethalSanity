using System;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
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

        // ===============================================================[ Set Player Sanity Level ]=============================================================== \\

        [HarmonyPatch(typeof(PlayerControllerB), "SetPlayerSanityLevel")]
        [HarmonyPostfix]
        private static void _SANITY_SET()
        {
            // Exit if the player does not exist or is dead.
            if (!LocalPlayer.Player || LocalPlayer.PlayerController.isPlayerDead) return;

            // Parse the player's current sanity level.
            if (float.TryParse(LocalPlayer.Insanity.ToString("0.0"), out float parsed))
            {
                if (_prevSan != parsed)
                {
                    bool isDecreasing = _prevSan > parsed;
                    _prevSan = parsed;

                    // Invoke the sanity changed event.
                    SanityChanged?.Invoke(parsed, isDecreasing);
                }
            }
        }

        private static float _prevSan { get; set; }

        // Event to notify about sanity changes.
        internal static event Action<float, bool> SanityChanged;
        // =======================================================================[ Effects ]======================================================================= \\

        [HarmonyPatch(typeof(StartOfRound), "StartGame")]
		[HarmonyPostfix]
		private static void _START_GAME() { SanityEventManager.events.Clear(); PostProcessing.Component?.RegenerateEffects(); }

		[HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
		[HarmonyPostfix]
		private static void _KILL_PLAYER(Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, int deathAnimation, Vector3 positionOffset)
        {
			SanityEventManager.events.Clear();
			if(PostProcessing.Component != null) UnityEngine.Object.Destroy(PostProcessing.Component);
		}

		[HarmonyPatch(typeof(PlayerControllerB), "Update")]
		[HarmonyPostfix]
		private static void _TMP_()
		{
			if(Keyboard.current.qKey.wasPressedThisFrame)
			{
				LocalPlayer.PlayerController.insanityLevel += 5;
			}
		}
    }
}