using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class VolumetricFire : MonoBehaviour
{
	private Mesh mesh;

	private Material material;

	[SerializeField]
	[Range(1f, 20f)]
	[Tooltip("Controls the number of additional meshes to render in front of and behind the original mesh")]
	private int thickness = 1;

	[SerializeField]
	[Range(0.01f, 1f)]
	[Tooltip("Controls the total distance between the frontmost mesh and the backmost mesh")]
	private float spread = 0.2f;

	[SerializeField]
	private bool billboard = true;

	private MaterialPropertyBlock materialPropertyBlock;

	private int internalCount;

	private float randomStatic;

	private Collider boundaryCollider;

	private void Start()
	{
		materialPropertyBlock = new MaterialPropertyBlock();
		MeshRenderer component = GetComponent<MeshRenderer>();
		component.enabled = false;
		material = component.sharedMaterial;
		mesh = GetComponent<MeshFilter>().sharedMesh;
		boundaryCollider = GetComponent<Collider>();
		randomStatic = Random.Range(0f, 1f);
	}

	private void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += RenderFlames;
	}

	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= RenderFlames;
	}

	private static bool IsVisible(Camera camera, Bounds bounds)
	{
		return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), bounds);
	}

	private void RenderFlames(ScriptableRenderContext context, Camera camera)
	{
		IsVisible(camera, boundaryCollider.bounds);
		internalCount = (thickness - 1) * 2;
		float spacing = 0f;
		if (internalCount > 0)
		{
			spacing = spread / (float)internalCount;
		}
		for (int i = 0; i <= internalCount; i++)
		{
			float item = (float)i - (float)internalCount * 0.5f;
			SetupMaterialPropertyBlock(item);
			CreateItem(spacing, item, camera);
		}
	}

	private void SetupMaterialPropertyBlock(float item)
	{
		if (materialPropertyBlock != null)
		{
			materialPropertyBlock.SetFloat("_ITEMNUMBER", item);
			materialPropertyBlock.SetFloat("_INTERNALCOUNT", internalCount);
			materialPropertyBlock.SetFloat("_INITIALPOSITIONINT", randomStatic);
		}
	}

	private void CreateItem(float spacing, float item, Camera camera)
	{
		Quaternion q = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		if (billboard)
		{
			q *= camera.transform.rotation;
			Vector3 normalized = (base.transform.position - camera.transform.position).normalized;
			zero = base.transform.position - normalized * item * spacing;
		}
		else
		{
			q = base.transform.rotation;
			zero = base.transform.position - base.transform.forward * item * spacing;
		}
		Matrix4x4 matrix = Matrix4x4.TRS(zero, q, base.transform.localScale);
		Graphics.DrawMesh(mesh, matrix, material, 0, camera, 0, materialPropertyBlock, castShadows: false, receiveShadows: false, useLightProbes: false);
	}
}
