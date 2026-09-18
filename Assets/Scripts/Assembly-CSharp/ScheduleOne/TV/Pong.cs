using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.TV
{
	public class Pong : TVApp
	{
		public enum EGameMode
		{
			SinglePlayer = 0,
			MultiPlayer = 1
		}

		public enum ESide
		{
			Left = 0,
			Right = 1
		}

		public enum EState
		{
			Ready = 0,
			Playing = 1,
			GameOver = 2
		}

		public RectTransform Rect;

		public PongPaddle LeftPaddle;

		public PongPaddle RightPaddle;

		public PongBall Ball;

		public TextMeshProUGUI LeftScoreLabel;

		public TextMeshProUGUI RightScoreLabel;

		public TextMeshProUGUI WinnerLabel;

		[Header("Settings")]
		public float InitialVelocity = 0.8f;

		public float VelocityGainPerSecond = 0.05f;

		public float MaxVelocity = 2f;

		public int GoalsToWin = 10;

		[Header("AI")]
		public float ReactionTime = 0.1f;

		public float TargetRandomization = 10f;

		public float SpeedMultiplier = 0.5f;

		public UnityEvent onServe;

		public UnityEvent onLeftScore;

		public UnityEvent onRightScore;

		public UnityEvent onGameOver;

		public UnityEvent onLocalPlayerWin;

		public UnityEvent onReset;

		private ESide nextBallSide;

		private Vector3 ballVelocity = Vector3.zero;

		private float reactionTimer;

		public EGameMode GameMode { get; set; }

		public EState State { get; set; }

		public int LeftScore { get; set; }

		public int RightScore { get; set; }

		private void Update()
		{
			if (base.IsOpen && !base.IsPaused)
			{
				UpdateInputs();
				if (GameMode == EGameMode.SinglePlayer)
				{
					UpdateAI();
				}
			}
		}

		private void FixedUpdate()
		{
			if (!base.IsOpen || base.IsPaused)
			{
				Ball.RB.isKinematic = true;
				return;
			}
			ballVelocity = Ball.RB.velocity;
			Ball.RB.velocity += Ball.RB.velocity.normalized * VelocityGainPerSecond * Time.deltaTime;
			if (Ball.RB.velocity.magnitude > MaxVelocity)
			{
				Ball.RB.velocity = Ball.RB.velocity.normalized * MaxVelocity;
			}
		}

		protected override void TryPause()
		{
			Ball.RB.isKinematic = true;
			if (State == EState.Ready || State == EState.GameOver)
			{
				Close();
			}
			else
			{
				base.TryPause();
			}
		}

		public void UpdateInputs()
		{
			if (State == EState.Playing)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, Input.mousePosition, PlayerSingleton<PlayerCamera>.Instance.Camera, out var localPoint);
				if (GameMode == EGameMode.SinglePlayer)
				{
					SetPaddleTargetY(ESide.Left, localPoint.y);
				}
			}
			else if (State == EState.Ready)
			{
				if (GameInput.GetButtonDown(GameInput.ButtonCode.Jump))
				{
					ServeBall();
				}
			}
			else if (State == EState.GameOver && GameInput.GetButtonDown(GameInput.ButtonCode.Jump))
			{
				ResetGame();
			}
		}

		private void UpdateAI()
		{
			if (State == EState.Playing)
			{
				reactionTimer += Time.deltaTime;
				if (reactionTimer >= ReactionTime)
				{
					float t = (Mathf.Clamp01(Ball.Rect.anchoredPosition.x / 300f) + 1f) / 2f;
					reactionTimer = 0f;
					float num = TargetRandomization * Mathf.Lerp(3f, 1f, t);
					float targetY = Ball.Rect.anchoredPosition.y + Random.Range(0f - num, num);
					RightPaddle.SetTargetY(targetY);
					RightPaddle.SpeedMultiplier = Mathf.Lerp(0.1f, 1f, t) * SpeedMultiplier;
				}
			}
		}

		public void GoalHit(ESide side)
		{
			if (State != EState.Playing)
			{
				return;
			}
			if (side == ESide.Left)
			{
				RightScore++;
				if (onRightScore != null)
				{
					onRightScore.Invoke();
				}
			}
			else
			{
				LeftScore++;
				if (onLeftScore != null)
				{
					onLeftScore.Invoke();
				}
			}
			LeftScoreLabel.text = LeftScore.ToString();
			RightScoreLabel.text = RightScore.ToString();
			Ball.RB.velocity = Vector3.zero;
			Ball.RB.isKinematic = true;
			State = EState.Ready;
			if (LeftScore >= GoalsToWin)
			{
				Win(ESide.Left);
			}
			else if (RightScore >= GoalsToWin)
			{
				Win(ESide.Right);
			}
		}

		private void Win(ESide winner)
		{
			if (winner == ESide.Left)
			{
				WinnerLabel.text = "Player 1 Wins!";
				WinnerLabel.color = LeftPaddle.GetComponent<Image>().color;
				if (onLocalPlayerWin != null)
				{
					onLocalPlayerWin.Invoke();
				}
			}
			else
			{
				WinnerLabel.text = "Player 2 Wins!";
				WinnerLabel.color = RightPaddle.GetComponent<Image>().color;
			}
			State = EState.GameOver;
			if (onGameOver != null)
			{
				onGameOver.Invoke();
			}
		}

		private void ResetBall()
		{
			Ball.Rect.anchoredPosition = Vector2.zero;
			Ball.RB.velocity = Vector3.zero;
		}

		private void ServeBall()
		{
			ResetBall();
			Ball.RB.isKinematic = false;
			if (nextBallSide == ESide.Left)
			{
				Vector2 normalized = new Vector2(-1f, Random.Range(-0.5f, 0.5f)).normalized;
				Ball.RB.AddRelativeForce(normalized * InitialVelocity, ForceMode.VelocityChange);
			}
			else
			{
				Vector2 normalized2 = new Vector2(1f, Random.Range(-0.5f, 0.5f)).normalized;
				Ball.RB.AddRelativeForce(normalized2 * InitialVelocity, ForceMode.VelocityChange);
			}
			State = EState.Playing;
			nextBallSide = ((nextBallSide == ESide.Left) ? ESide.Right : ESide.Left);
			if (onServe != null)
			{
				onServe.Invoke();
			}
		}

		private void ResetGame()
		{
			State = EState.Ready;
			LeftScore = 0;
			RightScore = 0;
			LeftScoreLabel.text = LeftScore.ToString();
			RightScoreLabel.text = RightScore.ToString();
			ResetBall();
			nextBallSide = ESide.Left;
			ballVelocity = Vector3.zero;
			if (onReset != null)
			{
				onReset.Invoke();
			}
		}

		public void SetPaddleTargetY(ESide player, float y)
		{
			if (player == ESide.Left)
			{
				LeftPaddle.SetTargetY(y);
			}
			else
			{
				RightPaddle.SetTargetY(y);
			}
		}

		public override void Resume()
		{
			base.Resume();
			Ball.RB.isKinematic = false;
			Ball.RB.velocity = ballVelocity;
		}
	}
}
