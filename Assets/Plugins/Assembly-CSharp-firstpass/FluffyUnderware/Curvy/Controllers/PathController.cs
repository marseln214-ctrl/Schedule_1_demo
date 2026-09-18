using System;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Controllers
{
	[AddComponentMenu("Curvy/Controllers/CG Path Controller")]
	[HelpURL("https://curvyeditor.com/doclink/pathcontroller")]
	public class PathController : CurvyController
	{
		[Section("General", true, false, 100, Sort = 0)]
		[SerializeField]
		[CGDataReferenceSelector(typeof(CGPath), Label = "Path/Slot")]
		private CGDataReference m_Path = new CGDataReference();

		public CGDataReference Path
		{
			get
			{
				return m_Path;
			}
			set
			{
				m_Path = value;
			}
		}

		public CGPath PathData
		{
			get
			{
				if (!Path.HasValue)
				{
					return null;
				}
				return Path.GetData<CGPath>();
			}
		}

		public override float Length
		{
			get
			{
				if (PathData == null)
				{
					return 0f;
				}
				return PathData.Length;
			}
		}

		public override bool IsReady
		{
			get
			{
				if (Path != null && !Path.IsEmpty)
				{
					return Path.HasValue;
				}
				return false;
			}
		}

		protected override float RelativeToAbsolute(float relativeDistance)
		{
			if (PathData == null)
			{
				return 0f;
			}
			return PathData.FToDistance(relativeDistance);
		}

		protected override float AbsoluteToRelative(float worldUnitDistance)
		{
			if (PathData == null)
			{
				return 0f;
			}
			return PathData.DistanceToF(worldUnitDistance);
		}

		protected override Vector3 GetInterpolatedSourcePosition(float tf)
		{
			return Path.Module.Generator.transform.TransformPoint(PathData.InterpolatePosition(tf));
		}

		protected override void GetInterpolatedSourcePosition(float tf, out Vector3 interpolatedPosition, out Vector3 tangent, out Vector3 up)
		{
			PathData.Interpolate(tf, out interpolatedPosition, out tangent, out up);
			Transform transform = Path.Module.Generator.transform;
			interpolatedPosition = transform.TransformPoint(interpolatedPosition);
			tangent = transform.TransformDirection(tangent);
			up = transform.TransformDirection(up);
		}

		protected override Vector3 GetTangent(float tf)
		{
			return Path.Module.Generator.transform.TransformDirection(PathData.InterpolateDirection(tf));
		}

		protected override Vector3 GetOrientation(float tf)
		{
			return Path.Module.Generator.transform.TransformDirection(PathData.InterpolateUp(tf));
		}

		protected override void Advance(float speed, float deltaTime)
		{
			float tf = base.RelativePosition;
			MovementDirection direction = base.MovementDirection;
			SimulateAdvance(ref tf, ref direction, speed, deltaTime);
			base.MovementDirection = direction;
			base.RelativePosition = tf;
		}

		protected override void SimulateAdvance(ref float tf, ref MovementDirection direction, float speed, float deltaTime)
		{
			int direction2 = direction.ToInt();
			switch (base.MoveMode)
			{
			case MoveModeEnum.Relative:
				PathData.Move(ref tf, ref direction2, speed * deltaTime, base.Clamping);
				break;
			case MoveModeEnum.AbsolutePrecise:
				PathData.MoveBy(ref tf, ref direction2, speed * deltaTime, base.Clamping);
				break;
			default:
				throw new NotSupportedException();
			}
			direction = MovementDirectionMethods.FromInt(direction2);
		}
	}
}
