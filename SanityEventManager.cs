using System;
using System.Linq;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LethalSanity
{
    internal static class SanityEventManager
    {
        internal static readonly List<SanityEvent> events = new();
        private static readonly Dictionary<string, CancellationTokenSource> ctsDict = new();

        internal static void AddEvent(SanityEvent sanityEvent) => events.Add(sanityEvent);

        internal static void OnSanityChange(float sanity, bool decreasing)
        {
            // If the sanity is decreasing
            if (decreasing)
            {
                // If the current sanity is less than the RelativeSanity and has been applied already, then turn it off
                foreach(SanityEvent SE in events.Where(e => sanity < e.Effect.OffSanity && e.Applied).ToList())
                {
                    SE.OffEffect?.Invoke();
                    SE.Applied = false;
                }
            }
            else
            {
                // If the current sanity is greater than the effects OnSanity value condition and is not applied already, then turn it on
                foreach (SanityEvent SE in events.Where(e => sanity >= e.Effect.OnSanity && !e.Applied).ToList())
                {
                    SE.OnEffect?.Invoke();
                    SE.Applied = true;
                }
            }
        }

        internal static async void SIVHandler(SMV config)
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

            Main.mls.LogWarning($"Starting smooth increment task: {config.ActionName}");
            try
            {
                while (elapsedTime < config.Duration)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    config.Action(Mathf.Lerp(config.Start, config.Target, elapsedTime / config.Duration));
                    elapsedTime += Time.deltaTime;
                    await Task.Yield();
                }

                config.Action(config.Target);
                Main.mls.LogWarning($"Finished smooth increment task: {config.ActionName}");
            }
            catch (ObjectDisposedException) { Main.mls.LogWarning($"{config.ActionName} was canceled"); }
            finally
            {
                ctsDict.Remove(config.ActionName);
                cts.Dispose();
            }
        }

        public class SMV
        {
            public string ActionName { get; set; }
            public Action<float> Action { get; set; }
            public float Start { get; set; }
            public float Target { get; set; }
            public float Duration { get; set; }
        }

        public class SanityEvent
        {
            public Effect Effect { get; set; }
            public Action OnEffect { get; set; }
            public Action OffEffect { get; set; }
            public bool Applied { get; set; }
        }
    }
}
