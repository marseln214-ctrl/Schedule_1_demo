using UnityEngine;
using UnityEngine.UI;

namespace MTAssets.UltimateLODSystem
{
	public class RuntimeScanDemo : MonoBehaviour
	{
		public UltimateLevelOfDetail ulodOfScene;

		public Text buttonText;

		public GameObject buttonObj;

		public Text scanStatus;

		public Animator cameraAnimator;

		private void Start()
		{
			ulodOfScene.onDoneScan.AddListener(delegate
			{
				scanStatus.text = "Scan Done! Showing LOD Demo";
				cameraAnimator.SetBool("runLoop", value: true);
				buttonObj.SetActive(value: true);
			});
			ulodOfScene.onUndoScan.AddListener(delegate
			{
				scanStatus.text = "No Scan Performed Yet";
				cameraAnimator.SetBool("runLoop", value: false);
				buttonObj.SetActive(value: true);
			});
		}

		private void Update()
		{
			if (ulodOfScene.isMeshesCurrentScannedAndLodsWorkingInThisComponent())
			{
				buttonText.text = "Undo Current Scan And Delete Generated LODs";
			}
			if (!ulodOfScene.isMeshesCurrentScannedAndLodsWorkingInThisComponent())
			{
				buttonText.text = "Do Scan And Generete LOD Groups";
			}
		}

		public void StartUndoScan()
		{
			if (ulodOfScene.isMeshesCurrentScannedAndLodsWorkingInThisComponent())
			{
				scanStatus.text = "Undoing Scan...";
				buttonObj.SetActive(value: false);
				ulodOfScene.UndoCurrentScanWorkingAndDeleteGeneratedMeshes(runMonoIl2CppGc: true, runUnityGc: true);
			}
			else if (!ulodOfScene.isMeshesCurrentScannedAndLodsWorkingInThisComponent())
			{
				scanStatus.text = "Scanning...";
				buttonObj.SetActive(value: false);
				ulodOfScene.ScanAllMeshesAndGenerateLodsGroups();
			}
		}
	}
}
