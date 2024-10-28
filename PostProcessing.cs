using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

using static LethalSanity.Config;
using static LethalSanity.NumberUtils;

namespace LethalSanity
{
	internal class PostProcessing : MonoBehaviour
	{
		// ======================================================================[ Properties ]====================================================================== \\
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

			VolumeProfile = new() { name = "LS-VolumeProfile" };                            // Create new profile and name it

			VolumeObject.GetComponentInChildren<Volume>().priority = Config.P_PP;           // Setting its priority
			VolumeObject.GetComponentInChildren<Volume>().profile = VolumeProfile;          // Setting its profile

			MakeDOF();
			MakeChrAb();
			MakeVignette();
			MakeLensDist();
			MakeFilmGrain();
			MakeSaturation();

			Patching.SanityChanged += Prnt;

			// ==============================================================[ Setting Max Insanity Level ]============================================================== \\
			LocalPlayer.MaxInsanity = (new[] { Config.Vignette, Config.FilmGrain, Config.ChromaticAberation, Config.LensDistortion, Config.DOF_Start, Config.DOF_End, Config.Saturation }.ToList()).Max(c => c.Kickin + c.Offset);
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

			Vignette VignetteComp = VolumeProfile.Add<Vignette>();
			VignetteComp.smoothness.value = 1;
			VignetteComp.roundness.value = 0.842f;
			VignetteComp.name = "LS-VignetteComp";

			// We start with a Vignette cause it just looks better and kind of notice the encrotching little less.
			VignetteComp.intensity.Override(0.15f);
			SanityEventManager.Instance.AddEvent(new()
			{
				Sanity = Next(Config.Vignette.Kickin, Config.Vignette.Offset),
				OnAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "Vignette",
					Action = VignetteComp.intensity.Override,
					StartValue = VignetteComp.intensity.value,
					TargetValue = Next(Config.Vignette.EaseInIntensityMin, Config.Vignette.EaseInIntensityMax),
					Duration = Next(Config.Vignette.EaseInTimeMin, Config.Vignette.EaseInTimeMax)
				}),
				OffAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "Vignette",
					Action = VignetteComp.intensity.Override,
					StartValue = VignetteComp.intensity.value,
					TargetValue = Next(Config.Vignette.EaseOutIntensityMin, Config.Vignette.EaseOutIntensityMax),
					Duration = Next(Config.Vignette.EaseOutTimeMin, Config.Vignette.EaseOutTimeMax)
				}),
			});
		}

		/// <summary>
		/// Prepares the Film Grain effect.
		/// </summary>
		private void MakeFilmGrain()
		{
			if (!Config.FilmGrain.Enabled) return;

			FilmGrain FilmGrainComp = VolumeProfile.Add<FilmGrain>();
			FilmGrainComp.name = "LS-FilmGrainComp";

			SanityEventManager.Instance.AddEvent(new()
			{
				Sanity = Next(Config.FilmGrain.Kickin, Config.FilmGrain.Offset),
				OnAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "FilmGrain",
					Action = FilmGrainComp.intensity.Override,
					StartValue = FilmGrainComp.intensity.value,
					TargetValue = Next(Config.FilmGrain.EaseInIntensityMin, Config.FilmGrain.EaseInIntensityMax),
					Duration = Next(Config.FilmGrain.EaseInTimeMin, Config.FilmGrain.EaseInTimeMax)
				}),
				OffAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "FilmGrain",
					Action = FilmGrainComp.intensity.Override,
					StartValue = FilmGrainComp.intensity.value,
					TargetValue = Next(Config.FilmGrain.EaseOutIntensityMin, Config.FilmGrain.EaseOutIntensityMax),
					Duration = Next(Config.FilmGrain.EaseOutTimeMin, Config.FilmGrain.EaseOutTimeMax)
				}),
			});
		}

		/// <summary>
		/// Prepares the Chromatic Aberration effect.
		/// </summary>
		private void MakeChrAb()
		{
			if (!Config.ChromaticAberation.Enabled) return;

			ChromaticAberration ChrAbComp = VolumeProfile.Add<ChromaticAberration>();
			ChrAbComp.name = "LS-ChrAbComp";

			SanityEventManager.Instance.AddEvent(new()
			{
				Sanity = Next(Config.ChromaticAberation.Kickin, Config.ChromaticAberation.Offset),
				OnAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "ChrAb",
					Action = ChrAbComp.intensity.Override,
					StartValue = ChrAbComp.intensity.value,
					TargetValue = Next(Config.ChromaticAberation.EaseInIntensityMin, Config.ChromaticAberation.EaseInIntensityMax),
					Duration = Next(Config.ChromaticAberation.EaseInTimeMin, Config.ChromaticAberation.EaseInTimeMax)
				}),
				OffAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "ChrAb",
					Action = ChrAbComp.intensity.Override,
					StartValue = ChrAbComp.intensity.value,
					TargetValue = Next(Config.ChromaticAberation.EaseOutIntensityMin, Config.ChromaticAberation.EaseOutIntensityMax),
					Duration = Next(Config.ChromaticAberation.EaseOutTimeMin, Config.ChromaticAberation.EaseOutTimeMax)
				}),
			});
		}

		/// <summary>
		/// Prepares the Lens Distortion effect.
		/// </summary>
		private void MakeLensDist()
		{
			if (!Config.LensDistortion.Enabled) return;

			float insanitylvl = Next(Config.LensDistortion.Kickin, Config.LensDistortion.Offset, 65);

			LensDistortion LensDistComp = VolumeProfile.Add<LensDistortion>();
			LensDistComp.name = "LS-LensDistComp";

			SanityEventManager.Instance.AddEvent(new()
			{
				Sanity = Next(Config.LensDistortion.Kickin, Config.LensDistortion.Offset),
				OnAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "LensDist",
					Action = LensDistComp.intensity.Override,
					StartValue = LensDistComp.intensity.value,
					TargetValue = Next(Config.LensDistortion.EaseInIntensityMin, Config.LensDistortion.EaseInIntensityMax),
					Duration = Next(Config.LensDistortion.EaseInTimeMin, Config.LensDistortion.EaseInTimeMax)
				}),
				OffAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "LensDist",
					Action = LensDistComp.intensity.Override,
					StartValue = LensDistComp.intensity.value,
					TargetValue = Next(Config.LensDistortion.EaseOutIntensityMin, Config.LensDistortion.EaseOutIntensityMax),
					Duration = Next(Config.LensDistortion.EaseOutTimeMin, Config.LensDistortion.EaseOutTimeMax)
				}),
			});
		}

		/// <summary>
		/// Prepares the Depth of Field effect.
		/// </summary>
		private void MakeDOF()
		{
			if (!DOF_Start.Enabled || DOF_End.Enabled) return;

			// Really should be the same time
			float insanitylvl = Next(Config.DOF_Start.Kickin, Config.DOF_Start.Offset, 65);

			DepthOfField DOFComp = VolumeProfile.Add<DepthOfField>();
			DOFComp.farFocusStart.Override(2000);
			DOFComp.farFocusEnd.Override(2000);
			DOFComp.name = "LS-DOFComp";

			SanityEventManager.Instance.AddEvent(new()
			{
				Sanity = Next(Config.DOF_Start.Kickin, Config.DOF_Start.Offset), // Doesnt matter since both are the same value.
				OnAction = () =>
				{
					SanityEventManager.Instance.SmoothIncrementValue(new()
					{
						ActionName = "DOFStart",
						Action = DOFComp.farFocusStart.Override,
						StartValue = DOFComp.farFocusStart.value,
						TargetValue = Next(Config.DOF_Start.EaseInIntensityMin, Config.DOF_Start.EaseInIntensityMax),
						Duration = Next(Config.DOF_Start.EaseInTimeMin, Config.DOF_Start.EaseInTimeMax)
					});
					SanityEventManager.Instance.SmoothIncrementValue(new()
					{
						ActionName = "DOFEnd",
						Action = DOFComp.farFocusEnd.Override,
						StartValue = DOFComp.farFocusEnd.value,
						TargetValue = Next(Config.DOF_End.EaseInIntensityMin, Config.DOF_End.EaseInIntensityMax),
						Duration = Next(Config.DOF_End.EaseInTimeMin, Config.DOF_End.EaseInTimeMax)
					});
				},
				OffAction = () =>
				{
					SanityEventManager.Instance.SmoothIncrementValue(new()
					{
						ActionName = "DOFStart",
						Action = DOFComp.farFocusStart.Override,
						StartValue = DOFComp.farFocusStart.value,
						TargetValue = Next(Config.DOF_Start.EaseOutIntensityMin, Config.DOF_Start.EaseOutIntensityMax),
						Duration = Next(Config.DOF_Start.EaseOutTimeMin, Config.DOF_Start.EaseOutTimeMax)
					});
					SanityEventManager.Instance.SmoothIncrementValue(new()
					{
						ActionName = "DOFEnd",
						Action = DOFComp.farFocusEnd.Override,
						StartValue = DOFComp.farFocusEnd.value,
						TargetValue = Next(Config.DOF_End.EaseOutIntensityMin, Config.DOF_End.EaseOutIntensityMax),
						Duration = Next(Config.DOF_End.EaseOutTimeMin, Config.DOF_End.EaseOutTimeMax)
					});
				},
			});
		}

		/// <summary>
		/// Prepares the Color Adjustments effect.
		/// </summary>
		private void MakeSaturation()
		{
			if (!Config.Saturation.Enabled) return;

			ColorAdjustments CAComp = VolumeProfile.Add<ColorAdjustments>();
			CAComp.name = "LS-CAComp";

			SanityEventManager.Instance.AddEvent(new()
			{
				Sanity = Next(Config.Saturation.Kickin, Config.Saturation.Offset),
				OnAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "CA",
					Action = CAComp.saturation.Override,
					StartValue = CAComp.saturation.value,
					TargetValue = -Next(Config.Saturation.EaseInIntensityMin, Config.Saturation.EaseInIntensityMax),
					Duration = Next(Config.Saturation.EaseInTimeMin, Config.Saturation.EaseInTimeMax)
				}),
				OffAction = () => SanityEventManager.Instance.SmoothIncrementValue(new()
				{
					ActionName = "CA",
					Action = CAComp.saturation.Override,
					StartValue = CAComp.saturation.value,
					TargetValue = Next(Config.Saturation.EaseOutIntensityMin, Config.Saturation.EaseOutIntensityMax),
					Duration = Next(Config.Saturation.EaseOutTimeMin, Config.Saturation.EaseOutTimeMax)
				}),
			});
		}
	}
}