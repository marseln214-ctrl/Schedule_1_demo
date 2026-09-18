using UnityEngine;
using UnityEngine.UI;

namespace MTAssets.UltimateLODSystem
{
	public class ChangeLodAutoManagement : MonoBehaviour
	{
		public Text buttonText;

		public Text explanation;

		private void Update()
		{
			if (UltimateLevelOfDetailGlobal.isGlobalULodSystemEnabled())
			{
				buttonText.text = "Disable Global Ultimate LOD System";
				explanation.text = "ULOD System is Enabled. Consult the documentation for more details on the feature of enabling/disabling the ULOD system while the game is running.";
			}
			if (!UltimateLevelOfDetailGlobal.isGlobalULodSystemEnabled())
			{
				buttonText.text = "Enable Global Ultimate LOD System";
				explanation.text = "ULOD System is Disabled. Consult the documentation for more details on the feature of enabling/disabling the ULOD system while the game is running.";
			}
		}

		public void buttonEnableAutoManagement()
		{
			UltimateLevelOfDetailGlobal.EnableGlobalULodSystem(!UltimateLevelOfDetailGlobal.isGlobalULodSystemEnabled());
		}
	}
}
