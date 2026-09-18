using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.TV
{
	public class RunnerGame : TVApp
	{
		public float GameSpeed = 1f;

		public float MinGameSpeed = 1.5f;

		public float MaxGameSpeed = 4f;

		public float SpeedIncreaseRate = 0.1f;

		public int ScoreRate = 50;

		public float Gravity = 9.8f;

		public float JumpForce = 10f;

		public float GlobalForceMultiplier = 20f;

		public float DropForce = 1f;

		public RectTransform Character;

		public Flipboard CharacterFlipboard;

		public SlidingRect Ground;

		public UISpawner CloudSpawner;

		public UISpawner ObstacleSpawner;

		public TextMeshProUGUI ScoreLabel;

		public TextMeshProUGUI HighScoreLabel;

		public GameObject StartScreen;

		public GameObject GameOverScreen;

		public Animation NewHighScoreAnimation;

		public Sprite JumpSprite;

		private bool isJumping;

		private bool isGrounded = true;

		private bool isReady;

		private float score;

		private float yVelocity;

		private float defaultCharacterY;

		private List<UIMover> clouds = new List<UIMover>();

		private List<UIMover> obstacles = new List<UIMover>();

		public UnityEvent onJump;

		public UnityEvent onHit;

		public UnityEvent onNewHighScore;

		protected override void Awake()
		{
			base.Awake();
			defaultCharacterY = Character.anchoredPosition.y;
			CloudSpawner.OnSpawn.AddListener(CloudSpawned);
			ObstacleSpawner.OnSpawn.AddListener(ObstacleSpawned);
			StartScreen.SetActive(value: true);
			isReady = true;
			GameSpeed = 0f;
		}

		public override void Open()
		{
			base.Open();
			RefreshHighScore();
		}

		protected override void TryPause()
		{
			if (isReady)
			{
				Close();
			}
			else
			{
				base.TryPause();
			}
		}

		public void Update()
		{
			if (!base.IsOpen)
			{
				return;
			}
			Ground.SpeedMultiplier = (base.IsPaused ? 0f : GameSpeed);
			CharacterFlipboard.SpeedMultiplier = (base.IsPaused ? 0f : GameSpeed);
			ScoreLabel.text = score.ToString("00000");
			for (int i = 0; i < clouds.Count; i++)
			{
				if (clouds[i] == null || clouds[i].gameObject == null)
				{
					clouds.RemoveAt(i);
					i--;
				}
				else
				{
					clouds[i].SpeedMultiplier = (base.IsPaused ? 0f : GameSpeed);
				}
			}
			for (int j = 0; j < obstacles.Count; j++)
			{
				if (obstacles[j] == null || obstacles[j].gameObject == null)
				{
					obstacles.RemoveAt(j);
					j--;
				}
				else
				{
					obstacles[j].SpeedMultiplier = (base.IsPaused ? 0f : GameSpeed);
				}
			}
			float spawnRateMultiplier = Mathf.Sqrt(GameSpeed);
			ObstacleSpawner.SpawnRateMultiplier = spawnRateMultiplier;
			CloudSpawner.SpawnRateMultiplier = spawnRateMultiplier;
			if (isReady && (GameInput.GetButtonDown(GameInput.ButtonCode.Jump) || GameInput.GetButtonDown(GameInput.ButtonCode.Forward)))
			{
				StartGame();
			}
			if (base.IsPaused || GameSpeed == 0f)
			{
				return;
			}
			score += (float)ScoreRate * Time.deltaTime;
			GameSpeed = Mathf.Clamp(GameSpeed + SpeedIncreaseRate * Time.deltaTime, MinGameSpeed, MaxGameSpeed);
			if (Character.anchoredPosition.y - defaultCharacterY > 10f)
			{
				CharacterFlipboard.Image.sprite = JumpSprite;
				CharacterFlipboard.enabled = false;
			}
			else
			{
				CharacterFlipboard.enabled = true;
			}
			yVelocity -= Gravity * GlobalForceMultiplier * Time.deltaTime;
			if (isJumping && (GameInput.GetButton(GameInput.ButtonCode.Crouch) || GameInput.GetButton(GameInput.ButtonCode.Backward)))
			{
				yVelocity -= DropForce * GlobalForceMultiplier * Time.deltaTime;
			}
			if (Character.anchoredPosition.y + yVelocity * Time.deltaTime <= defaultCharacterY)
			{
				if (isJumping)
				{
					CharacterFlipboard.SetIndex(0);
				}
				Character.anchoredPosition = new Vector2(Character.anchoredPosition.x, defaultCharacterY);
				yVelocity = 0f;
				isJumping = false;
				isGrounded = true;
			}
			else
			{
				Character.anchoredPosition = new Vector2(Character.anchoredPosition.x, Character.anchoredPosition.y + yVelocity * Time.deltaTime);
			}
			if ((GameInput.GetButtonDown(GameInput.ButtonCode.Jump) || GameInput.GetButtonDown(GameInput.ButtonCode.Forward)) && isGrounded)
			{
				Jump();
			}
		}

		private void Jump()
		{
			isGrounded = false;
			isJumping = true;
			yVelocity = JumpForce * GlobalForceMultiplier;
			if (onJump != null)
			{
				onJump.Invoke();
			}
		}

		private void CloudSpawned(GameObject cloud)
		{
			clouds.Add(cloud.GetComponent<UIMover>());
		}

		private void ObstacleSpawned(GameObject obstacle)
		{
			obstacles.Add(obstacle.GetComponent<UIMover>());
		}

		private void RefreshHighScore()
		{
			float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("RunGameHighScore");
			HighScoreLabel.text = value.ToString("00000");
		}

		public void PlayerCollided()
		{
			if (!isReady)
			{
				EndGame();
				if (onHit != null)
				{
					onHit.Invoke();
				}
			}
		}

		private void EndGame()
		{
			float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("RunGameHighScore");
			if (score > value)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("RunGameHighScore", score.ToString());
				NewHighScoreAnimation.Play();
				if (onNewHighScore != null)
				{
					onNewHighScore.Invoke();
				}
			}
			GameOverScreen.SetActive(value: true);
			RefreshHighScore();
			GameSpeed = 0f;
			isReady = true;
		}

		private void StartGame()
		{
			ResetGame();
			GameSpeed = MinGameSpeed;
			GameOverScreen.SetActive(value: false);
			StartScreen.SetActive(value: false);
		}

		private void ResetGame()
		{
			score = 0f;
			GameSpeed = MinGameSpeed;
			yVelocity = 0f;
			isJumping = false;
			isGrounded = true;
			isReady = false;
			Character.anchoredPosition = new Vector2(Character.anchoredPosition.x, defaultCharacterY);
			for (int i = 0; i < clouds.Count; i++)
			{
				if (clouds[i] == null || clouds[i].gameObject == null)
				{
					clouds.RemoveAt(i);
					i--;
				}
				else
				{
					Object.Destroy(clouds[i].gameObject);
				}
			}
			clouds.Clear();
			for (int j = 0; j < obstacles.Count; j++)
			{
				if (obstacles[j] == null || obstacles[j].gameObject == null)
				{
					obstacles.RemoveAt(j);
					j--;
				}
				else
				{
					Object.Destroy(obstacles[j].gameObject);
				}
			}
			obstacles.Clear();
		}
	}
}
