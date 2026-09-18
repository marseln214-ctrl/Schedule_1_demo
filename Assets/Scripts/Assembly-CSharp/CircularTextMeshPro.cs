using TMPro;
using UnityEngine;

[ExecuteAlways]
public class CircularTextMeshPro : MonoBehaviour
{
	[SerializeField]
	private TMP_Text text;

	public AnimationCurve vertexCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f, 0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f, 1f, 0f, 0f, 0f));

	public float yCurveScaling = 50f;

	private bool isForceUpdatingMesh;

	private void Reset()
	{
		text = base.gameObject.GetComponent<TMP_Text>();
	}

	private void Awake()
	{
		if (!text)
		{
			text = base.gameObject.GetComponent<TMP_Text>();
		}
	}

	private void OnEnable()
	{
		WarpText();
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ReactToTextChanged);
	}

	private void OnDisable()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ReactToTextChanged);
		text.ForceMeshUpdate();
	}

	private void OnValidate()
	{
		WarpText();
	}

	private void ReactToTextChanged(Object obj)
	{
		TMP_Text tMP_Text = obj as TMP_Text;
		if ((bool)tMP_Text && (bool)text && tMP_Text == text && !isForceUpdatingMesh)
		{
			WarpText();
		}
	}

	private void WarpText()
	{
		if (!text)
		{
			return;
		}
		isForceUpdatingMesh = true;
		vertexCurve.preWrapMode = WrapMode.Once;
		vertexCurve.postWrapMode = WrapMode.Once;
		text.havePropertiesChanged = true;
		text.ForceMeshUpdate();
		TMP_TextInfo textInfo = text.textInfo;
		if (textInfo == null)
		{
			return;
		}
		int characterCount = textInfo.characterCount;
		if (characterCount == 0)
		{
			return;
		}
		float x = text.bounds.min.x;
		float x2 = text.bounds.max.x;
		for (int i = 0; i < characterCount; i++)
		{
			if (textInfo.characterInfo[i].isVisible)
			{
				int vertexIndex = textInfo.characterInfo[i].vertexIndex;
				int materialReferenceIndex = textInfo.characterInfo[i].materialReferenceIndex;
				Vector3[] vertices = textInfo.meshInfo[materialReferenceIndex].vertices;
				Vector3 vector = new Vector2((vertices[vertexIndex].x + vertices[vertexIndex + 2].x) / 2f, textInfo.characterInfo[i].baseLine);
				vertices[vertexIndex] += -vector;
				vertices[vertexIndex + 1] += -vector;
				vertices[vertexIndex + 2] += -vector;
				vertices[vertexIndex + 3] += -vector;
				float num = (vector.x - x) / (x2 - x);
				float num2 = num + 0.0001f;
				float y = vertexCurve.Evaluate(num) * yCurveScaling;
				float y2 = vertexCurve.Evaluate(num2) * yCurveScaling;
				Vector3 lhs = new Vector3(1f, 0f, 0f);
				Vector3 rhs = new Vector3(num2 * (x2 - x) + x, y2) - new Vector3(vector.x, y);
				float num3 = Mathf.Acos(Vector3.Dot(lhs, rhs.normalized)) * 57.29578f;
				float z = ((Vector3.Cross(lhs, rhs).z > 0f) ? num3 : (360f - num3));
				Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(0f, y, 0f), Quaternion.Euler(0f, 0f, z), Vector3.one);
				vertices[vertexIndex] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex]);
				vertices[vertexIndex + 1] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 1]);
				vertices[vertexIndex + 2] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 2]);
				vertices[vertexIndex + 3] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 3]);
				vertices[vertexIndex] += vector;
				vertices[vertexIndex + 1] += vector;
				vertices[vertexIndex + 2] += vector;
				vertices[vertexIndex + 3] += vector;
				text.UpdateVertexData();
			}
		}
		isForceUpdatingMesh = false;
	}
}
