using ScheduleOne.Audio;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.StationFramework
{
	public class BoilingFlask : Fillable
	{
		public const float TEMPERATURE_MAX = 500f;

		public float TEMPERATURE_MAX_VELOCITY = 200f;

		public float TEMPERATURE_ACCELERATION = 50f;

		public bool LockTemperature;

		public AnimationCurve BoilSoundPitchCurve;

		[Header("References")]
		public BunsenBurner Burner;

		public Canvas TemperatureCanvas;

		public TextMeshProUGUI TemperatureLabel;

		public Slider TemperatureSlider;

		public RectTransform TemperatureRangeIndicator;

		public ParticleSystem SmokeParticles;

		public AudioSourceController BoilSound;

		public float CurrentTemperature { get; private set; }

		public float CurrentTemperatureVelocity { get; private set; }

		public bool IsTemperatureInRange
		{
			get
			{
				if (CurrentTemperature >= Recipe.CookTemperatureLowerBound)
				{
					return CurrentTemperature <= Recipe.CookTemperatureUpperBound;
				}
				return false;
			}
		}

		public StationRecipe Recipe { get; private set; }

		public void Update()
		{
			if (!LockTemperature)
			{
				float num = Burner.CurrentHeat - CurrentTemperature / 500f;
				CurrentTemperatureVelocity = Mathf.MoveTowards(CurrentTemperatureVelocity, num * TEMPERATURE_MAX_VELOCITY, TEMPERATURE_ACCELERATION * Time.deltaTime);
				CurrentTemperature = Mathf.Clamp(CurrentTemperature + CurrentTemperatureVelocity * Time.deltaTime, 0f, 500f);
			}
			if (CurrentTemperature > 0f)
			{
				BoilSound.VolumeMultiplier = Mathf.Clamp01(CurrentTemperature / 500f);
				BoilSound.AudioSource.pitch = BoilSoundPitchCurve.Evaluate(Mathf.Clamp01(CurrentTemperature / 500f));
				if (!BoilSound.AudioSource.isPlaying)
				{
					BoilSound.AudioSource.Play();
				}
			}
			else
			{
				BoilSound.AudioSource.Stop();
			}
		}

		private void FixedUpdate()
		{
			UpdateCanvas();
			UpdateSmoke();
		}

		private void UpdateCanvas()
		{
			if (TemperatureCanvas.gameObject.activeSelf)
			{
				TemperatureLabel.text = Mathf.RoundToInt(CurrentTemperature) + "°C";
				if (CurrentTemperature < Recipe.CookTemperatureLowerBound)
				{
					TemperatureLabel.color = Color.white;
				}
				else if (CurrentTemperature > Recipe.CookTemperatureUpperBound)
				{
					TemperatureLabel.color = new Color32(byte.MaxValue, 90, 90, byte.MaxValue);
				}
				else
				{
					TemperatureLabel.color = Color.green;
				}
				TemperatureSlider.value = CurrentTemperature / 500f;
			}
		}

		private void UpdateSmoke()
		{
			if (CurrentTemperature < 1f)
			{
				if (SmokeParticles.isPlaying)
				{
					SmokeParticles.Stop();
				}
				return;
			}
			ParticleSystem.MainModule main = SmokeParticles.main;
			main.simulationSpeed = Mathf.Lerp(1f, 3f, CurrentTemperature / 500f);
			main.startColor = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, CurrentTemperature / 500f));
			if (!SmokeParticles.isPlaying)
			{
				SmokeParticles.Play();
			}
		}

		public void SetCanvasVisible(bool visible)
		{
			TemperatureCanvas.gameObject.SetActive(visible);
		}

		public void SetTemperature(float temp)
		{
			CurrentTemperature = temp;
		}

		public void SetRecipe(StationRecipe recipe)
		{
			Recipe = recipe;
			if (!(recipe == null))
			{
				float x = Recipe.CookTemperatureLowerBound / 500f;
				float x2 = Recipe.CookTemperatureUpperBound / 500f;
				TemperatureRangeIndicator.anchorMin = new Vector2(x, TemperatureRangeIndicator.anchorMin.y);
				TemperatureRangeIndicator.anchorMax = new Vector2(x2, TemperatureRangeIndicator.anchorMax.y);
			}
		}
	}
}
