using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;

namespace LethalSanity
{
	internal class PostProcessing : MonoBehaviour
	{
		// ======================================================================[ Properties ]====================================================================== \\
		public static PostProcessing Component { get; private set; }

		// Effects
		protected private Vignette VignetteComp;
		protected private FilmGrain FilmGrainComp;
        protected private ChromaticAberration ChrAbComp;
        protected private LensDistortion LensDistComp;
        protected private DepthOfField DOFComp;
        protected private ColorAdjustments SaturationComp;


        private GameObject VolumeObject { get; set; }
		private VolumeProfile VolumeProfile { get; set; }
		private static Dictionary<string, Effect> EffectData { get; set; }

        private void Awake()
        {
            Component = this;
        }

		// =============================================================[ Creating Profile And Effects ]============================================================= \\
		private void Start()
		{
			GameObject origVol = GameObject.Find("Systems/Rendering/VolumeMain");           // Get the original volume objec to make it easier
			VolumeObject = ScriptableObject.Instantiate(origVol, origVol.transform.parent); // Instantiate it
			VolumeObject.name = "LS-Volume";                                                // Rename it

			VolumeProfile = new() { name = "LS-VolumeProfile" };                            // Create new profile and name it

			VolumeObject.GetComponentInChildren<Volume>().priority = 1;						// Setting its priority
			VolumeObject.GetComponentInChildren<Volume>().profile = VolumeProfile;          // Setting its profile
		}

        private void OnDestroy()
        {
            if (VolumeProfile != null) UnityEngine.Object.Destroy(VolumeProfile);
            if (VolumeObject != null) UnityEngine.Object.Destroy(VolumeObject);
        }

		/// <summary>
		/// Basically remakes each EffectData into a pre-calculated Effects
		/// So each time the player dies or something, we can randomize the effects so they're not as predictable
		/// </summary>
		protected internal void RegenerateEffects()
		{
            EffectData = Config.JsonData.Effects.ToDictionary(kvp => kvp.Key, kvp => new Effect(kvp.Value));

            // Depth of field start and end should kick in at the same time
            EffectData["DepthOfField Start"].OnSanity = EffectData["DepthOfField End"].OnSanity;
            EffectData["DepthOfField Start"].OffSanity = EffectData["DepthOfField End"].OffSanity;
            EffectData["DepthOfField Start"].EaseInTime = EffectData["DepthOfField End"].EaseInTime;
            EffectData["DepthOfField Start"].EaseOutTime = EffectData["DepthOfField End"].EaseOutTime;

            TextManager.instance.Clear();
            foreach (KeyValuePair<string, Effect> kvp in EffectData)
            {
                TextManager.instance.UpdateText($"{kvp.Key}: {kvp.Value.Enabled}, {kvp.Value.OnSanity}, {kvp.Value.OffSanity}, {kvp.Value.EaseInIntensity}");
                Main.mls.LogFatal($"{kvp.Key}: {kvp.Value.Enabled}, {kvp.Value.OnSanity}, {kvp.Value.OffSanity}, {kvp.Value.EaseInIntensity}");
            }



            // ==============================================================[ Setting Max Insanity Level ]============================================================== \\

            LocalPlayer.MaxInsanity = new[] { "Vignette", "Film Grain", "Lens Distortion", "Saturation", "Chromatic Aberation", "DepthOfField Start", "DepthOfField End" }.Max(e => EffectData[e].OnSanity) + 0.1f;

            Main.mls.LogWarning($"Player's max insanity has been set to: {LocalPlayer.MaxInsanity}");

            MakeVignette();
            MakeFilmGrain();
            MakeChrAb();
            MakeLensDist();
            MakeDOF();
            MakeSaturation();
        }

		protected private void MakeVignette()
		{
            if (!EffectData["Vignette"].Enabled) return;

			if(VignetteComp) VolumeProfile.Remove<Vignette>();

            VignetteComp = VolumeProfile.Add<Vignette>();
            VignetteComp.smoothness.value = 1;
			VignetteComp.roundness.value = 1;
            VignetteComp.rounded.value = true;
            VignetteComp.opacity.value = 0.85f;
            VignetteComp.name = "LS-VignetteComp";

            // We start with a Vignette cause it just looks better and kind of notice the encrotching little less.
            VignetteComp.intensity.Override(0.15f);

			SanityEventManager.AddEvent(new()
			{
				Effect = EffectData["Vignette"],
                OnEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "Vignette",
                    Action = VignetteComp.intensity.Override,
                    Start = VignetteComp.intensity.value,
                    Target = EffectData["Vignette"].EaseInIntensity,
                    Duration = EffectData["Vignette"].EaseInTime,
                }),
                OffEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "Vignette",
                    Action = VignetteComp.intensity.Override,
                    Start = VignetteComp.intensity.value,
                    Target = EffectData["Vignette"].EaseOutIntensity,
                    Duration = EffectData["Vignette"].EaseOutTime,
                }),
            });
        }
		protected private void MakeFilmGrain()
		{
			if (!EffectData["Film Grain"].Enabled) return;

			if(FilmGrainComp) VolumeProfile.Remove<FilmGrain>();

            FilmGrainComp = VolumeProfile.Add<FilmGrain>();
            FilmGrainComp.name = "LS-FilmGrainComp";

            SanityEventManager.AddEvent(new()
            {
                Effect = EffectData["Film Grain"],
                OnEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "FilmGrain",
                    Action = FilmGrainComp.intensity.Override,
                    Start = FilmGrainComp.intensity.value,
                    Target = EffectData["Film Grain"].EaseInIntensity,
                    Duration = EffectData["Film Grain"].EaseInTime,
                }),
                OffEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "FilmGrain",
                    Action = FilmGrainComp.intensity.Override,
                    Start = FilmGrainComp.intensity.value,
                    Target = EffectData["Film Grain"].EaseOutIntensity,
                    Duration = EffectData["Film Grain"].EaseOutTime,
                }),
            });
        }
        protected private void MakeChrAb()
        {
            if (!EffectData["Chromatic Aberation"].Enabled) return;

            if (ChrAbComp) VolumeProfile.Remove<ChromaticAberration>();

            ChrAbComp = VolumeProfile.Add<ChromaticAberration>();
            ChrAbComp.name = "LS-ChrAbComp";

            SanityEventManager.AddEvent(new()
            {
                Effect = EffectData["Chromatic Aberation"],
                OnEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "ChrAb",
                    Action = ChrAbComp.intensity.Override,
                    Start = ChrAbComp.intensity.value,
                    Target = EffectData["Chromatic Aberation"].EaseInIntensity,
                    Duration = EffectData["Chromatic Aberation"].EaseInTime,
                }),
                OffEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "ChrAb",
                    Action = ChrAbComp.intensity.Override,
                    Start = ChrAbComp.intensity.value,
                    Target = EffectData["Chromatic Aberation"].EaseOutIntensity,
                    Duration = EffectData["Chromatic Aberation"].EaseOutTime,
                }),
            });
        }
        protected private void MakeLensDist()
        {
            if (!EffectData["Lens Distortion"].Enabled) return;

            if (LensDistComp) VolumeProfile.Remove<LensDistortion>();

            LensDistComp = VolumeProfile.Add<LensDistortion>();
            LensDistComp.name = "LS-LensDistComp";

            SanityEventManager.AddEvent(new()
            {
                Effect = EffectData["Lens Distortion"],
                OnEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "LensDist",
                    Action = LensDistComp.intensity.Override,
                    Start = LensDistComp.intensity.value,
                    Target = EffectData["Lens Distortion"].EaseInIntensity,
                    Duration = EffectData["Lens Distortion"].EaseInTime,
                }),
                OffEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "LensDist",
                    Action = LensDistComp.intensity.Override,
                    Start = LensDistComp.intensity.value,
                    Target = EffectData["Lens Distortion"].EaseOutIntensity,
                    Duration = EffectData["Lens Distortion"].EaseOutTime,
                }),
            });
        }
        protected private void MakeDOF()
        {
            if (!EffectData["DepthOfField Start"].Enabled || !EffectData["DepthOfField End"].Enabled) return;

            if (VignetteComp) VolumeProfile.Remove<DepthOfField>();

            DOFComp = VolumeProfile.Add<DepthOfField>();
            DOFComp.farFocusStart.Override(2000);
            DOFComp.farFocusEnd.Override(2000);
            DOFComp.name = "LS-DOFComp";

            SanityEventManager.AddEvent(new()
            {
                Effect = EffectData["DepthOfField Start"],
                OnEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "DOF_Start",
                    Action = DOFComp.farFocusStart.Override,
                    Start = DOFComp.farFocusStart.value,
                    Target = EffectData["DepthOfField Start"].EaseInIntensity,
                    Duration = EffectData["DepthOfField Start"].EaseInTime,
                }),
                OffEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "DOF_Start",
                    Action = DOFComp.farFocusStart.Override,
                    Start = DOFComp.farFocusStart.value,
                    Target = EffectData["DepthOfField Start"].EaseOutIntensity,
                    Duration = EffectData["DepthOfField Start"].EaseOutTime,
                }),
            });

            SanityEventManager.AddEvent(new()
            {
                Effect = EffectData["DepthOfField End"],
                OnEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "DOF_End",
                    Action = DOFComp.farFocusEnd.Override,
                    Start = DOFComp.farFocusEnd.value,
                    Target = EffectData["DepthOfField End"].EaseInIntensity,
                    Duration = EffectData["DepthOfField End"].EaseInTime,
                }),
                OffEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "DOF_End",
                    Action = DOFComp.farFocusEnd.Override,
                    Start = DOFComp.farFocusEnd.value,
                    Target = EffectData["DepthOfField End"].EaseOutIntensity,
                    Duration = EffectData["DepthOfField End"].EaseOutTime,
                }),
            });
        }
        protected private void MakeSaturation()
        {
            if (!EffectData["Saturation"].Enabled) return;

            if (SaturationComp) VolumeProfile.Remove<ColorAdjustments>();

            SaturationComp = VolumeProfile.Add<ColorAdjustments>();
            SaturationComp.name = "LS-SaturationComp";

            SanityEventManager.AddEvent(new()
            {
                Effect = EffectData["Saturation"],
                OnEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "Saturation",
                    Action = SaturationComp.saturation.Override,
                    Start = SaturationComp.saturation.value,
                    Target = EffectData["Saturation"].EaseInIntensity,
                    Duration = EffectData["Saturation"].EaseInTime,
                }),
                OffEffect = () => SanityEventManager.SIVHandler(new()
                {
                    ActionName = "Saturation",
                    Action = SaturationComp.saturation.Override,
                    Start = SaturationComp.saturation.value,
                    Target = EffectData["Saturation"].EaseOutIntensity,
                    Duration = EffectData["Saturation"].EaseOutTime,
                }),
            });
        }

    }
}