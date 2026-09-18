using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.TV
{
	public class PongBall : MonoBehaviour
	{
		public Pong Game;

		public RectTransform Rect;

		public Rigidbody RB;

		public float RandomForce = 0.5f;

		public UnityEvent onHit;

		private void FixedUpdate()
		{
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.collider.gameObject.name == "LeftGoal")
			{
				Game.GoalHit(Pong.ESide.Left);
			}
			else if (collision.collider.gameObject.name == "RightGoal")
			{
				Game.GoalHit(Pong.ESide.Right);
			}
			if (RB.velocity.y < 0.1f && collision.collider.GetComponent<PongPaddle>() != null)
			{
				float magnitude = RB.velocity.magnitude;
				RB.AddForce(new Vector3(0f, Random.Range(0f - RandomForce, RandomForce), 0f), ForceMode.VelocityChange);
				RB.velocity = RB.velocity.normalized * magnitude;
			}
			if (onHit != null)
			{
				onHit.Invoke();
			}
		}
	}
}
