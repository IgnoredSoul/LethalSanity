using System;
using System.Linq;
using System.Collections.Generic;

namespace LethalSanity
{
	internal class SanityEventManager
	{
		internal SanityEventManager()
		{ Instance = this; Patching.SanityChanged += SanityEventCheck; }

		internal static SanityEventManager Instance { get; private set; }

		internal readonly List<SanityEvent> Events = new();

		public void AddEvent(float sanity, Action on, Action off) => Events.Add(new(sanity, on, off));

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
	}

	public struct SanityEvent
	{
		public float Sanity;
		public Action OnAction;
		public Action OffAction;
		public bool Applied;

		internal SanityEvent(float sanity, Action onAct = null, Action offAct = null)
		{
			Sanity = sanity;
			OnAction = onAct;
			OffAction = offAct;
			Applied = false;
		}
	}
}