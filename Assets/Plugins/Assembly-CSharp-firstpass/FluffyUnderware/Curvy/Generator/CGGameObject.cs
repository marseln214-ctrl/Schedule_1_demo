using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo("#FFF59D")]
	public class CGGameObject : CGBounds
	{
		public GameObject Object;

		public Vector3 Translate;

		public Vector3 Rotate;

		public Vector3 Scale = Vector3.one;

		public Matrix4x4 Matrix => Matrix4x4.TRS(Translate, Quaternion.Euler(Rotate), Scale);

		public CGGameObject()
		{
		}

		public CGGameObject(CGGameObjectProperties properties)
			: this(properties.Object, properties.Translation, properties.Rotation, properties.Scale)
		{
		}

		public CGGameObject(GameObject obj)
			: this(obj, Vector3.zero, Vector3.zero, Vector3.one)
		{
		}

		public CGGameObject(GameObject obj, Vector3 translate, Vector3 rotate, Vector3 scale)
		{
			Object = obj;
			Translate = translate;
			Rotate = rotate;
			Scale = scale;
			if ((bool)Object)
			{
				Name = Object.name;
			}
		}

		public CGGameObject(CGGameObject source)
			: base(source)
		{
			Object = source.Object;
			Translate = source.Translate;
			Rotate = source.Rotate;
			Scale = source.Scale;
		}

		public override T Clone<T>()
		{
			return new CGGameObject(this) as T;
		}

		public override void RecalculateBounds()
		{
			if (Object == null)
			{
				mBounds = default(Bounds);
				return;
			}
			Renderer[] componentsInChildren = Object.GetComponentsInChildren<Renderer>(includeInactive: true);
			Collider[] componentsInChildren2 = Object.GetComponentsInChildren<Collider>(includeInactive: true);
			Bounds value;
			if (componentsInChildren.Length != 0)
			{
				value = componentsInChildren[0].bounds;
				for (int i = 1; i < componentsInChildren.Length; i++)
				{
					value.Encapsulate(componentsInChildren[i].bounds);
				}
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					value.Encapsulate(componentsInChildren2[j].bounds);
				}
			}
			else if (componentsInChildren2.Length != 0)
			{
				value = componentsInChildren2[0].bounds;
				for (int k = 1; k < componentsInChildren2.Length; k++)
				{
					value.Encapsulate(componentsInChildren2[k].bounds);
				}
			}
			else
			{
				value = default(Bounds);
			}
			Vector3 vector = Quaternion.Inverse(Object.transform.localRotation) * value.size;
			value.size = new Vector3(vector.x * Scale.x, vector.y * Scale.y, vector.z * Scale.z);
			mBounds = value;
		}
	}
}
