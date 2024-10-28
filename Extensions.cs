using System;
using System.Text;
using UnityEngine;
using GameNetcodeStuff;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading;

namespace LethalSanity
{
	public static class NumberUtils
	{
		/// <summary>
		/// Represents a shared instance of a random number generator.
		/// </summary>
		public static readonly System.Random random = new System.Random(GenerateTrulyRandomNumber());

		/// <summary>
		/// Generates a truly random number using cryptographic random number generation.
		/// </summary>
		/// <returns>A truly random number within a specified range.</returns>
		public static int GenerateTrulyRandomNumber()
		{
			using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
			{
				byte[] bytes = new byte[4]; // 32 bits
				rng.GetBytes(bytes);

				// Convert the random bytes to an integer and ensure it falls within the specified range
				int randomInt = BitConverter.ToInt32(bytes, 0);
				return Math.Abs(randomInt % (50 - 10)) + 10;
			}
		}

		/// <summary>
		/// Determines if an event with a given probability occurs.
		/// </summary>
		/// <param name="percentage">The probability of the event occurring, in percentage.</param>
		/// <returns>True if the event occurs, otherwise false.</returns>
		public static bool Chance(int percentage = 50)
		{
			if (percentage < 0 || percentage > 100) throw new Exception("Uh...?");
			return (Next(0, 100) < percentage);
		}

		/// <summary>
		/// Determines if an event with a given probability occurs.
		/// </summary>
		/// <param name="percentage">The probability of the event occurring, in percentage.</param>
		/// <returns>True if the event occurs, otherwise false.</returns>
		public static bool Chance(float percentage = 50f)
		{
			if (percentage < 0f || percentage > 100f) throw new Exception("Uh...?");
			return (NextF(0f, 100f) < percentage);
		}

		/// <summary>
		/// Generates a random 64-bit integer.
		/// </summary>
		/// <returns>A randomly generated 64-bit integer.</returns>
		public static long GenInt64()
		{
			byte[] buffer = new byte[8];
			random.NextBytes(buffer);
			long randomInt64 = BitConverter.ToInt64(buffer, 0);
			return randomInt64;
		}

		/// <summary>
		/// Returns a non-negative random integer.
		/// </summary>
		/// <returns>A random non-negative integer.</returns>
		public static int Next() => Next(0, int.MaxValue);

		/// <summary>
		/// Returns a random integer less than the specified maximum value. Mainly used for arrays / lists
		/// </summary>
		/// <param name="max">The exclusive upper bound of the random number to be generated.</param>
		/// <returns>A random integer less than <paramref name="max"/>.</returns>
		public static int NextL(int max) => Next(0, max - 1);

		/// <summary>
		/// Returns a random integer less than the specified maximum value.
		/// </summary>
		/// <param name="max">The exclusive upper bound of the random number to be generated.</param>
		/// <returns>A random integer less than <paramref name="max"/>.</returns>
		public static int Next(int max) => Next(0, max + 1);

		/// <summary>
		/// Returns a random integer within the specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound of the random number to be generated.</param>
		/// <param name="max">The exclusive upper bound of the random number to be generated.</param>
		/// <returns>A random integer within the specified range.</returns>
		public static int Next(int min, int max) => random.Next(min, max + 1);

		public static int NextO(int input, int offset) => Mathf.Clamp(Next(input - offset, input + offset), 0, int.MaxValue);

		public static int NextO(int input, int offset, int max) => Mathf.Clamp(Next(input - offset, input + offset), 0, max);

		public static int NextO(int input, int offset, int min, int max) => Mathf.Clamp(Next(input - offset, input + offset), min, max);

		public static float NextO(float input, float offset) => Mathf.Clamp(NextF(input - offset, input + offset), 0, float.MaxValue);

		public static float NextO(float input, float offset, float max) => Mathf.Clamp(NextF(input - offset, input + offset), 0, max);

		public static float NextO(float input, float offset, float min, float max) => Mathf.Clamp(NextF(input - offset, input + offset), min, max);

		/// <summary>
		/// Returns a random float number between 0.0 and the maximum value representable by a float (not inclusive).
		/// </summary>
		/// <returns>A random float number between 0.0 and the maximum value representable by a float (not inclusive).</returns>
		public static float NextF() => NextF(0, float.MaxValue);

		/// <summary>
		/// Returns a random float number less than the specified maximum value.
		/// </summary>
		/// <param name="max">The maximum value of the random float number to be generated.</param>
		/// <returns>A random float number less than <paramref name="max"/>.</returns>
		public static float NextF(float max) => NextF(0, max + 1);

		/// <summary>
		/// Returns a random float number within the specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound of the random float number to be generated.</param>
		/// <param name="max">The exclusive upper bound of the random float number to be generated.</param>
		/// <returns>A random float number within the specified range.</returns>
		public static float NextF(float min, float max) => (float)((NextD() * (max - min)) + min);

		/// <summary>
		/// Returns a random double number between 0.0 and 1.0.
		/// </summary>
		/// <returns>A random double number between 0.0 and 1.0.</returns>
		public static double NextD() => random.NextDouble();

		/// <summary>
		/// Generates a 128-bit string hash based on the given value.
		/// </summary>
		/// <param name="val">The value to be hashed.</param>
		/// <returns>A 128-bit string hash.</returns>
		public static string String128(object input)
		{
			Hash128 h = new();
			h.Append(input.ToString());
			return h.ToString();
		}

		/// <summary>
		/// Returns a random vector3 position between the desired bounds.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Vector3 NextV3(float x, float y, float z) => new Vector3(NextF(-x, x), NextF(-y, y), NextF(-z, z));

		public static string MD5(object input)
		{
			using (MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				byte[] inputBytes = Encoding.UTF8.GetBytes(input.ToString());
				byte[] hashBytes = md5.ComputeHash(inputBytes);
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
					sb.Append(hashBytes[i].ToString("x2"));
				return sb.ToString();
			}
		}

		/// <summary>
		/// I am too dumb to do this any other way so.... this is what I've god :3
		/// </summary>
		private static Dictionary<string, CancellationTokenSource> ctsDict = new Dictionary<string, CancellationTokenSource>();

		/// <summary>
		/// Smoothly changes a value to another value using asynchronous tasks.
		/// </summary>
		/// <param name="ActionName">The name of the action.</param>
		/// <param name="action">The action to perform during the transition.</param>
		/// <param name="start">The starting value.</param>
		/// <param name="target">The target value.</param>
		/// <param name="duration">The duration of the transition.</param>
		public static async void SmoothIncrementValue(string ActionName, Action<float> action, float start, float target, float duration)
		{
			// Cancel the task if it's already running
			if (ctsDict.ContainsKey(ActionName))
			{
				ctsDict[ActionName].Cancel();
				ctsDict[ActionName].Dispose();
				Main.mls.LogWarning($"Cancelling smooth increment task: {ActionName}");
				ctsDict.Remove(ActionName);
			}

			// Create a new cancellation token source for the task
			ctsDict.Add(ActionName, new CancellationTokenSource());

			// Start the task
			Task smoothIncrementTask = SmoothIncrementValueTask((value) =>
			{
				action(value);
			}, start, target, duration, ctsDict[ActionName].Token);

			// Wait for the task to complete (or be canceled)
			await smoothIncrementTask;

			// Clean up the CancellationTokenSource and remove action from dict
			ctsDict.Remove(ActionName);
		}

		private static async Task SmoothIncrementValueTask(Action<float> action, float start, float target, float duration, CancellationToken cancellationToken)
		{
			float elapsedTime = 0f;
			float currentValue;

			while (elapsedTime < duration)
			{
				// Check for cancellation
				if (cancellationToken.IsCancellationRequested)
				{
					// Perform cleanup or any necessary actions before exiting
					return;
				}

				currentValue = Mathf.Lerp(start, target, elapsedTime / duration);
				action(currentValue);

				elapsedTime += Time.deltaTime;

				await Task.Yield();
			}

			currentValue = target;
			action(currentValue);
		}
	}

	public static class LocalPlayer
	{
		/// <summary>
		/// Gets the <see cref="PlayerControllerB"/> instance representing the local player controller.
		/// </summary>
		public static PlayerControllerB? PlayerController
		{
			get => GameNetworkManager.Instance?.localPlayerController ?? null;
		}

		/// <summary>
		/// Asynchronously retrieves the <see cref="PlayerControllerB"/> instance representing the local player controller.
		/// </summary>
		/// <param name="maxIter">Maximum number of iterations to wait for.</param>
		/// <param name="delay">Delay between iterations in milliseconds.</param>
		/// <returns>A task representing the asynchronous operation that yields the local player controller.</returns>
		public static async Task<PlayerControllerB> PlayerControllerAsync(int maxIter = 25, int delay = 250)
		{
			if (PlayerController != null) return PlayerController;
			int iterCount = 0;
			PlayerControllerB player = null;

			do
			{
				// If gameObject exists else null
				player = (GameNetworkManager.Instance?.localPlayerController);

				// wait 250ms | 0.25s if player doesnt exist
				if (!player)
					await Task.Delay(delay);
			} while (++iterCount < maxIter || !player); // Iter counter so it doesnt sit here for ever

			return player;
		}

		/// <summary>
		/// Gets the <see cref="GameObject"/> representing the local player.
		/// </summary>
		public static GameObject? Player => PlayerController?.gameObject;

		/// <summary>
		/// Asynchronously retrieves the <see cref="GameObject"/> representing the local player.
		/// </summary>
		/// <param name="maxIter">Maximum number of iterations to wait for.</param>
		/// <param name="delay">Delay between iterations in milliseconds.</param>
		/// <returns>A task representing the asynchronous operation that yields the local player.</returns>
		public static async Task<GameObject> PlayerAsync(int maxIter = 25, int delay = 250)
		{
			int iterCount = 0;
			GameObject player = null;

			do
			{
				// If gameObject exists else null
				player = (StartOfRound.Instance?.localPlayerController?.gameObject);

				// wait 250ms | 0.25s if player doesnt exist
				if (!player)
					await Task.Delay(delay);
			} while (++iterCount < maxIter || !player); // Iter counter so it doesnt sit here for ever

			return player;
		}

		/// <summary>
		/// Gets the insanity level of the local player.
		/// </summary>
		public static float Insanity
		{
			get => PlayerController?.insanityLevel ?? -1;
			set => PlayerController.insanityLevel = value;
		}

		/// <summary>
		/// Asynchronously retrieves the insanity level of the local player.
		/// </summary>
		/// <param name="maxIter">Maximum number of iterations to wait for.</param>
		/// <param name="delay">Delay between iterations in milliseconds.</param>
		/// <returns>A task representing the asynchronous operation that yields the insanity level of the local player.</returns>
		public static async Task<float> InsanityAsync(int maxIter = 25, int delay = 250)
		{
			int iterCount = 0;
			float insanity;
			do
			{
				// If sanity exists else -1
				insanity = StartOfRound.Instance?.localPlayerController?.insanityLevel ?? -1;

				// wait 250ms | 0.25s if sanity is below 0
				if (insanity < 0)
					await Task.Delay(delay);
			} while (++iterCount < maxIter || insanity < 0); // Iter counter so it doesnt sit here for ever

			return insanity;
		}

		/// <summary>
		/// Gets the maximum insanity level of the local player.
		/// </summary>
		public static float MaxInsanity
		{
			get => PlayerController?.maxInsanityLevel ?? -1;
			set => PlayerController.maxInsanityLevel = value;
		}

		/// <summary>
		/// Asynchronously retrieves the maximum insanity level of the local player.
		/// </summary>
		/// <param name="maxIter">Maximum number of iterations to wait for.</param>
		/// <param name="delay">Delay between iterations in milliseconds.</param>
		/// <returns>A task representing the asynchronous operation that yields the maximum insanity level of the local player.</returns>
		public static async Task<float> MaxInsanityAsync(int maxIter = 25, int delay = 250)
		{
			int iterCount = 0;
			float insanity;
			do
			{
				// If sanity exists else -1
				insanity = StartOfRound.Instance?.localPlayerController?.maxInsanityLevel ?? -1;

				// wait 250ms | 0.25s if sanity is below 0
				if (insanity < 0)
					await Task.Delay(delay);
			} while (++iterCount < maxIter || insanity < 0); // Iter counter so it doesnt sit here for ever

			return insanity;
		}

		/// <summary>
		/// Gets the camera attached to the local player.
		/// </summary>
		public static Camera Camera
		{
			get => Player?.GetComponentInChildren<Camera>();
		}

		/// <summary>
		/// Asynchronously retrieves the camera attached to the local player.
		/// </summary>
		/// <param name="maxIter">Maximum number of iterations to wait for.</param>
		/// <param name="delay">Delay between iterations in milliseconds.</param>
		/// <returns>A task representing the asynchronous operation that yields the camera attached to the local player.</returns>
		public static async Task<Camera> CameraAsync(int maxIter = 25, int delay = 250)
		{
			int iterCount = 0;
			Camera cam = null;

			do
			{
				// If camera exists else null
				cam = (Player?.GetComponentInChildren<Camera>());

				// wait 250ms | 0.25s if player doesnt exist
				if (!cam)
					await Task.Delay(delay);
			} while (++iterCount < maxIter || !cam); // Iter counter so it doesnt sit here for ever

			return cam;
		}

		/// <summary>
		/// Indicates whether the local player is near other players within a specified radius.
		/// </summary>
		public static bool IsNearOtherPlayers => PlayersNearMe().Length > 0;

		/// <summary>
		/// Retrieves an array of player controllers representing other players near the local player within a specified radius.
		/// </summary>
		/// <param name="rad">The radius within which to search for other players.</param>
		/// <returns>An array of player controllers representing other players near the local player.</returns>
		public static PlayerControllerB[] PlayersNearMe(float rad = 10f)
		{
			PlayerControllerB[] l = null;

			foreach (PlayerControllerB player in Resources.FindObjectsOfTypeAll<PlayerControllerB>())
				if (Vector3.Distance(player.transform.position, Player.transform.position) <= rad)
					l[l.Length] = player;

			return l;
		}

		/// <summary>
		/// Retireves all the input's the player performs.
		/// </summary>
		public static InputActionAsset Actions = IngamePlayerSettings.Instance?.playerInput?.actions;

		/// <summary>
		/// Indicates whether the quick menu is open for the local player.
		/// </summary>
		public static bool IsMenuOpen => PlayerController.quickMenuManager.isMenuOpen;

		/// <summary>
		/// Indicates whether the terminal menu is open for the local player.
		/// </summary>
		public static bool IsTermOpen => PlayerController.inTerminalMenu;

		/// <summary>
		/// Indicated whether the player pressed their 'interact' button.
		/// </summary>
		public static bool Interected => Actions.FindAction("Interact", false).WasPressedThisFrame();
	}
}