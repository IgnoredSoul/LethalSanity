using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

using static LethalSanity.NumberUtils;

namespace LethalSanity
{
	internal class PostProcessing : MonoBehaviour
	{
		public static PostProcessing Component { get; private set; }
		private GameObject VolumeObject { get; set; }
		private VolumeProfile VolumeProfile { get; set; }

		// =============================================================[ Creating Profile And Effects ]============================================================= \\
		private void Start()
		{
			Main.mls.LogWarning($"Starting to do Post Shit");
			GameObject origVol = GameObject.Find("Systems/Rendering/VolumeMain");           // Get the original volume objec to make it easier

			VolumeObject = Instantiate(origVol, origVol.transform.parent);                  // Instantiate it
			VolumeObject.name = "LS-Volume";                                                // Rename it

			VolumeProfile = new();                                                          // New profile
			VolumeProfile.name = "LS-VolumeProfile";                                        // Name it

			VolumeObject.GetComponentInChildren<Volume>().priority = Config.P_PP;           // Setting its priority
			VolumeObject.GetComponentInChildren<Volume>().profile = VolumeProfile;          // Setting its profile

			MakeDOF();
			MakeChrAb();
			MakeVignette();
			MakeLensDist();
			MakeFilmGrain();
			MakeSaturation();

			Patching.SanityChanged += Prnt;
		}

		private void Prnt(float v)
		{
			Main.mls.LogMessage($"Sanity: {v}");
		}

		/// <summary>
		/// Prepares the Vignette effect.
		/// </summary>
		private void MakeVignette()
		{
			if (!Config.Vignette.Enabled) return;

			float insanitylvl = NextO(Config.Vignette.Kickin, Config.Vignette.Offset, 65);

			Vignette VignetteComp = VolumeProfile.Add<Vignette>();
			VignetteComp.smoothness.value = 1;
			VignetteComp.roundness.value = 0.842f;
			VignetteComp.name = "LS-VignetteComp";

			SanityEventManager.Instance.AddEvent(insanitylvl, () =>
			{
				SmoothIncrementValue("Vignette", VignetteComp.intensity.Override, VignetteComp.intensity.value, NextF(0.45f, 0.6f), NextF(10f, 25f));
			}, () =>
			{
				SmoothIncrementValue("Vignette", VignetteComp.intensity.Override, VignetteComp.intensity.value, 0, NextF(0.5f, 2));
			});
		}

		/// <summary>
		/// Prepares the Film Grain effect.
		/// </summary>
		private void MakeFilmGrain()
		{
			if (!Config.FilmGrain.Enabled) return;

			float insanitylvl = NextO(Config.FilmGrain.Kickin, Config.FilmGrain.Offset, 65);

			FilmGrain FilmGrainComp = VolumeProfile.Add<FilmGrain>();
			FilmGrainComp.name = "LS-FilmGrainComp";

			SanityEventManager.Instance.AddEvent(insanitylvl, () =>
			{
				SmoothIncrementValue("FilmGrain", FilmGrainComp.intensity.Override, FilmGrainComp.intensity.value, NextF(0.5f, 1f), NextF(10, 20));
			}, () =>
			{
				SmoothIncrementValue("FilmGrain", FilmGrainComp.intensity.Override, FilmGrainComp.intensity.value, 0f, NextF(0.5f, 2));
			});
		}

		/// <summary>
		/// Prepares the Chromatic Aberration effect.
		/// </summary>
		private void MakeChrAb()
		{
			if (!Config.ChromaticAberation.Enabled) return;

			float insanitylvl = NextO(Config.ChromaticAberation.Kickin, Config.ChromaticAberation.Offset, 65);

			ChromaticAberration ChrAbComp = VolumeProfile.Add<ChromaticAberration>();
			ChrAbComp.name = "LS-ChrAbComp";

			SanityEventManager.Instance.AddEvent(insanitylvl, () =>
			{
				SmoothIncrementValue("ChrAb", ChrAbComp.intensity.Override, ChrAbComp.intensity.value, NumberUtils.NextF(0.5f, 1.5f), NumberUtils.NextF(10, 20));
			}, () =>
			{
				SmoothIncrementValue("ChrAb", ChrAbComp.intensity.Override, ChrAbComp.intensity.value, 0f, NumberUtils.NextF(0.5f, 2));
			});
		}

		/// <summary>
		/// Prepares the Lens Distortion effect.
		/// </summary>
		private void MakeLensDist()
		{
			if (!Config.LensDistortion.Enabled) return;

			float insanitylvl = NextO(Config.LensDistortion.Kickin, Config.LensDistortion.Offset, 65);

			LensDistortion LensDistComp = VolumeProfile.Add<LensDistortion>();
			LensDistComp.name = "LS-LensDistComp";

			SanityEventManager.Instance.AddEvent(insanitylvl, () =>
			{
				SmoothIncrementValue("LensDist", LensDistComp.intensity.Override, LensDistComp.intensity.value, NextF(0.4f, 0.6f), NextF(20, 30));
			}, () =>
			{
				SmoothIncrementValue("LensDist", LensDistComp.intensity.Override, LensDistComp.intensity.value, 0, NextF(0.5f, 2));
			});
		}

		/// <summary>
		/// Prepares the Depth of Field effect.
		/// </summary>
		private void MakeDOF()
		{
			Main.mls.LogWarning("1");
			if (!Config.DOF.Enabled) return;
			Main.mls.LogWarning("2");

			float insanitylvl = NextO(Config.DOF.Kickin, Config.DOF.Offset, 65);
			Main.mls.LogWarning("3");

			DepthOfField DOFComp = VolumeProfile.Add<DepthOfField>();
			Main.mls.LogWarning("4");
			DOFComp.farFocusStart.Override(2000);
			Main.mls.LogWarning("5");
			DOFComp.farFocusEnd.Override(2000);
			Main.mls.LogWarning("6");
			DOFComp.name = "LS-DOFComp";
			Main.mls.LogWarning("7");

			SanityEventManager.Instance.AddEvent(insanitylvl, () =>
			{
				SmoothIncrementValue("DOFStart", DOFComp.farFocusStart.Override, DOFComp.farFocusStart.value, NextF(3, 8), NextF(10, 17));
				SmoothIncrementValue("DOFEnd", DOFComp.farFocusEnd.Override, DOFComp.farFocusEnd.value, NextF(10, 15), NextF(18, 25));
			}, () =>
			{
				SmoothIncrementValue("DOFStart", DOFComp.farFocusStart.Override, DOFComp.farFocusStart.value, 2000f, NextF(10, 13));
				SmoothIncrementValue("DOFEnd", DOFComp.farFocusEnd.Override, DOFComp.farFocusEnd.value, 2000, NextF(13, 16));
			});
		}

		/// <summary>
		/// Prepares the Color Adjustments effect.
		/// </summary>
		private void MakeSaturation()
		{
			if (!Config.Saturation.Enabled) return;

			float insanitylvl = NextO(Config.Saturation.Kickin, Config.Saturation.Offset, 65);

			ColorAdjustments CAComp = VolumeProfile.Add<ColorAdjustments>();
			CAComp.name = "LS-CAComp";

			SanityEventManager.Instance.AddEvent(insanitylvl, () =>
			{
				SmoothIncrementValue("CA", CAComp.saturation.Override, CAComp.saturation.value, -NextF(50, 70), NextF(18, 28));
			}, () =>
			{
				SmoothIncrementValue("CA", CAComp.saturation.Override, CAComp.saturation.value, 0, NextF(0.5f, 2));
			});
		}
	}
}