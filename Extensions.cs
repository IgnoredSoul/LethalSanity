using GameNetcodeStuff;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

namespace LethalSanity
{
	internal static class NumberUtils
	{
		/// <summary>
		/// Represents a shared instance of a random number generator.
		/// </summary>
		internal static readonly System.Random random = new(GenerateTrulyRandomNumber());

		/// <summary>
		/// Generates a truly random number using cryptographic random number generation.
		/// </summary>
		/// <returns>A truly random number within a specified range.</returns>
		internal static int GenerateTrulyRandomNumber()
		{
			using RNGCryptoServiceProvider rng = new();
			byte[] bytes = new byte[4]; // 32 bits
			rng.GetBytes(bytes);

			// Convert the random bytes to an integer and ensure it falls within the specified range
			int randomInt = BitConverter.ToInt32(bytes, 0);
			return Math.Abs(randomInt % (50 - 10)) + 10;
		}

		/// <summary>
		/// Returns a random integer within the specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound of the random number to be generated.</param>
		/// <param name="max">The exclusive upper bound of the random number to be generated.</param>
		/// <returns>A random integer within the specified range.</returns>
		internal static int Next(int min, int max) => random.Next(min, max + 1);

		internal static float Next(float input, float offset, float max) => Mathf.Clamp(Next(input - offset, input + offset), 0, max);

		/// <summary>
		/// Returns a random float number within the specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound of the random float number to be generated.</param>
		/// <param name="max">The exclusive upper bound of the random float number to be generated.</param>
		/// <returns>A random float number within the specified range.</returns>
		internal static float Next(float min, float max) => (float)((NextD() * (max - min)) + min);

		/// <summary>
		/// Returns a random double number between 0.0 and 1.0.
		/// </summary>
		/// <returns>A random double number between 0.0 and 1.0.</returns>
		internal static double NextD() => random.NextDouble();
	}

	internal static class LocalPlayer
	{
		/// <summary>
		/// Gets the <see cref="PlayerControllerB"/> instance representing the local player controller.
		/// </summary>
		internal static PlayerControllerB? PlayerController
		{
			get => GameNetworkManager.Instance?.localPlayerController ?? null;
		}

		/// <summary>
		/// Gets the <see cref="GameObject"/> representing the local player.
		/// </summary>
		internal static GameObject? Player => PlayerController?.gameObject;

		/// <summary>
		/// Gets the insanity level of the local player.
		/// </summary>
		internal static float Insanity
		{
			get => PlayerController?.insanityLevel ?? -1;
			set => PlayerController.insanityLevel = value;
		}

		/// <summary>
		/// Gets the maximum insanity level of the local player.
		/// </summary>
		internal static float MaxInsanity
		{
			get => PlayerController?.maxInsanityLevel ?? -1;
			set => PlayerController.maxInsanityLevel = value;
		}

		internal static bool ShouldQuickReset()
		{
			if (!PlayerController) return false;
			return !(PlayerController.isInHangarShipRoom || PlayerController.isInsideFactory);
		}
    }
}