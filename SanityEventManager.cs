using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalSanity
{
	internal class SanityEventManager
	{
		// ======================================================================[ Properties ]====================================================================== \\
		internal static SanityEventManager Instance { get; private set; }

		internal List<SanityEvent> Events { get; private set; } = [];

		// =======================================================================[ Methods ]======================================================================= \\
		internal SanityEventManager()
		{ Instance = this; Patching.SanityChanged += SanityEventCheck; }

		internal void AddEvent(SanityEvent @event) => Events.Add(@event);

		private void SanityEventCheck(float sanity)
		{
			// Events that are below the sanity
			foreach (var item in Events.Where(i => i.Sanity < sanity).ToArray())
			{
				// Get index position
				int index = Events.IndexOf(item);

				// If valid and has not been applied
				if (index != -1 && !Events[index].Applied)
				{
					// Copy
					var modifiedEvent = Events[index];

					// Set as applied
					modifiedEvent.Applied = true;

					// Set
					Events[index] = modifiedEvent;

					// Invoke
					modifiedEvent.OnAction?.Invoke();
				}
			}

			// Events that are above the sanity
			foreach (var item in Events.Where(i => i.Sanity >= sanity).ToArray())
			{
				// Get index position
				int index = Events.IndexOf(item);

				// If valid and has been applied
				if (index != -1 && Events[index].Applied)
				{
					// Copy
					var modifiedEvent = Events[index];

					// Set as applied
					modifiedEvent.Applied = false;

					// Set
					Events[index] = modifiedEvent;

					// Invoke
					modifiedEvent.OffAction?.Invoke();
				}
			}
		}

		private readonly Dictionary<string, CancellationTokenSource> ctsDict = [];

		internal async void SmoothIncrementValue(SMVConfig config)
		{
			// Cancel and dispose any existing token, then create a new one
			if (ctsDict.TryGetValue(config.ActionName, out var existingCts))
			{
				existingCts.Cancel();
				existingCts.Dispose();
				Main.mls.LogWarning($"Cancelling smooth increment task: {config.ActionName}");
			}

			CancellationTokenSource cts = new();
			ctsDict[config.ActionName] = cts;

			float elapsedTime = 0f;

			try
			{
				while (elapsedTime < config.Duration)
				{
					cts.Token.ThrowIfCancellationRequested();

					config.Action(Mathf.Lerp(config.StartValue, config.TargetValue, elapsedTime / config.Duration));
					elapsedTime += Time.deltaTime;
					await Task.Yield();
				}

				config.Action(config.TargetValue);
			}
			catch (OperationCanceledException) { /* Task was cancelled */ }
			finally
			{
				ctsDict.Remove(config.ActionName);
				cts.Dispose();
			}
		}
	}

	public struct SMVConfig
	{
		public string ActionName { get; set; }
		public Action<float> Action { get; set; }
		public float StartValue { get; set; }
		public float TargetValue { get; set; }
		public float Duration { get; set; }
	}

	public struct SanityEvent
	{
		public float Sanity { get; set; }
		public Action OnAction { get; set; }
		public Action OffAction { get; set; }
		public bool Applied { get; set; }
	}
}