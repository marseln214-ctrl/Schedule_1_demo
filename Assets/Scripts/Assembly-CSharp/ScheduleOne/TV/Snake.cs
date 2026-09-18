using System.Collections.Generic;
using EasyButtons;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.TV
{
	public class Snake : TVApp
	{
		public enum EGameState
		{
			Ready = 0,
			Playing = 1
		}

		public const int SIZE_X = 20;

		public const int SIZE_Y = 12;

		[Header("Settings")]
		public SnakeTile TilePrefab;

		public float TimePerTile = 0.4f;

		[Header("References")]
		public RectTransform PlaySpace;

		public SnakeTile[] Tiles;

		public TextMeshProUGUI ScoreText;

		private Vector2 lastFoodPosition = Vector2.zero;

		private float _timeSinceLastMove;

		private float _timeOnGameOver;

		public UnityEvent onStart;

		public UnityEvent onEat;

		public UnityEvent onGameOver;

		public UnityEvent onWin;

		public Vector2 HeadPosition { get; private set; } = new Vector2(10f, 6f);


		public List<Vector2> Tail { get; private set; } = new List<Vector2>();


		public Vector2 LastTailPosition { get; private set; } = Vector2.zero;


		public Vector2 Direction { get; private set; } = Vector2.right;


		public Vector2 QueuedDirection { get; private set; } = Vector2.right;


		public Vector2 NextDirection { get; private set; } = Vector2.zero;


		public EGameState GameState { get; private set; }

		protected override void Awake()
		{
			base.Awake();
		}

		private void Update()
		{
			if (!base.IsPaused && base.IsOpen)
			{
				UpdateInput();
				UpdateMovement();
				_timeOnGameOver += Time.deltaTime;
				ScoreText.text = Tail.Count.ToString();
			}
		}

		private void UpdateInput()
		{
			if (_timeOnGameOver < 0.3f)
			{
				return;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Forward) || Input.GetKeyDown(KeyCode.UpArrow))
			{
				if (Direction != Vector2.down)
				{
					QueuedDirection = Vector2.up;
				}
				NextDirection = Vector2.up;
				if (GameState == EGameState.Ready)
				{
					StartGame(Vector2.up);
				}
			}
			else if (GameInput.GetButtonDown(GameInput.ButtonCode.Backward) || Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (Direction != Vector2.up)
				{
					QueuedDirection = Vector2.down;
				}
				NextDirection = Vector2.down;
				if (GameState == EGameState.Ready)
				{
					StartGame(Vector2.down);
				}
			}
			else if (GameInput.GetButtonDown(GameInput.ButtonCode.Left) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				if (Direction != Vector2.right)
				{
					QueuedDirection = Vector2.left;
				}
				NextDirection = Vector2.left;
				if (GameState == EGameState.Ready)
				{
					StartGame(Vector2.left);
				}
			}
			else if (GameInput.GetButtonDown(GameInput.ButtonCode.Right) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				if (Direction != Vector2.left)
				{
					QueuedDirection = Vector2.right;
				}
				NextDirection = Vector2.right;
				if (GameState == EGameState.Ready)
				{
					StartGame(Vector2.right);
				}
			}
		}

		private void UpdateMovement()
		{
			if (GameState == EGameState.Playing)
			{
				_timeSinceLastMove += Time.deltaTime;
				if (_timeSinceLastMove >= TimePerTile)
				{
					_timeSinceLastMove -= TimePerTile;
					MoveSnake();
				}
			}
		}

		private void MoveSnake()
		{
			Direction = QueuedDirection;
			Vector2 vector = HeadPosition + Direction;
			SnakeTile tile = GetTile(vector);
			if (tile == null)
			{
				GameOver();
				return;
			}
			if (tile.Type == SnakeTile.TileType.Snake && Tail.Count > 0 && tile.Position != Tail[Tail.Count - 1])
			{
				GameOver();
				return;
			}
			bool flag = false;
			if (tile.Type == SnakeTile.TileType.Food)
			{
				Eat();
				flag = true;
				if (GameState != EGameState.Playing)
				{
					return;
				}
			}
			GetTile(vector).SetType(SnakeTile.TileType.Snake);
			Vector2 vector2 = HeadPosition;
			HeadPosition = vector;
			for (int i = 0; i < Tail.Count; i++)
			{
				if (i == Tail.Count - 1)
				{
					LastTailPosition = Tail[i];
				}
				Vector2 vector3 = Tail[i];
				Tail[i] = vector2;
				GetTile(Tail[i]).SetType(SnakeTile.TileType.Snake, 1 + i);
				vector2 = vector3;
			}
			GetTile(vector2).SetType(SnakeTile.TileType.Empty);
			LastTailPosition = vector2;
			if (NextDirection != Vector2.zero && NextDirection != -Direction)
			{
				QueuedDirection = NextDirection;
			}
			if (flag)
			{
				SpawnFood();
			}
		}

		private SnakeTile GetTile(Vector2 position)
		{
			if (position.x < 0f || position.x >= 20f || position.y < 0f || position.y >= 12f)
			{
				return null;
			}
			return Tiles[(int)position.y * 20 + (int)position.x];
		}

		private void StartGame(Vector2 initialDir)
		{
			SnakeTile tile = GetTile(lastFoodPosition);
			if (tile != null)
			{
				tile.SetType(SnakeTile.TileType.Empty);
			}
			SpawnFood();
			GetTile(HeadPosition)?.SetType(SnakeTile.TileType.Empty);
			HeadPosition = new Vector2(10f, 6f);
			for (int i = 0; i < Tail.Count; i++)
			{
				GetTile(Tail[i]).SetType(SnakeTile.TileType.Empty);
			}
			Tail.Clear();
			QueuedDirection = initialDir;
			NextDirection = Vector2.zero;
			_timeSinceLastMove = 0f;
			MoveSnake();
			GameState = EGameState.Playing;
			if (onStart != null)
			{
				onStart.Invoke();
			}
		}

		private void Eat()
		{
			Tail.Add(LastTailPosition);
			if (onEat != null)
			{
				onEat.Invoke();
			}
		}

		private void SpawnFood()
		{
			List<SnakeTile> list = new List<SnakeTile>();
			SnakeTile[] tiles = Tiles;
			foreach (SnakeTile snakeTile in tiles)
			{
				if (snakeTile.Type == SnakeTile.TileType.Empty)
				{
					list.Add(snakeTile);
				}
			}
			if (list.Count == 0)
			{
				Win();
				return;
			}
			SnakeTile snakeTile2 = list[Random.Range(0, list.Count)];
			snakeTile2.SetType(SnakeTile.TileType.Food);
			lastFoodPosition = snakeTile2.Position;
		}

		private void GameOver()
		{
			GameState = EGameState.Ready;
			_timeOnGameOver = 0f;
			if (onGameOver != null)
			{
				onGameOver.Invoke();
			}
		}

		private void Win()
		{
			GameState = EGameState.Ready;
			_timeOnGameOver = 0f;
			if (onWin != null)
			{
				onWin.Invoke();
			}
		}

		protected override void TryPause()
		{
			if (GameState == EGameState.Ready)
			{
				Close();
			}
			else
			{
				base.TryPause();
			}
		}

		[Button]
		public void CreateTiles()
		{
			SnakeTile[] tiles = Tiles;
			for (int i = 0; i < tiles.Length; i++)
			{
				Object.DestroyImmediate(tiles[i].gameObject);
			}
			Tiles = new SnakeTile[240];
			float tileSize = PlaySpace.rect.width / 20f;
			for (int j = 0; j < 12; j++)
			{
				for (int k = 0; k < 20; k++)
				{
					SnakeTile snakeTile = Object.Instantiate(TilePrefab, PlaySpace);
					snakeTile.SetType(SnakeTile.TileType.Empty);
					snakeTile.SetPosition(new Vector2(k, j), tileSize);
					Tiles[j * 20 + k] = snakeTile;
				}
			}
		}
	}
}
