using System.Collections.Generic;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Lighting;
using ScheduleOne.Misc;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class GrowLight : ProceduralGridItem
	{
		[Header("References")]
		public ToggleableLight Light;

		public UsableLightSource usableLightSource;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EGrowLightAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EGrowLightAssembly_002DCSharp_002Edll_Excuted;

		public override void InitializeProceduralGridItem(ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
		{
			base.InitializeProceduralGridItem(instance, _rotation, _footprintTileMatches, GUID);
			if (isGhost)
			{
				return;
			}
			SetIsOn(isOn: true);
			foreach (CoordinateProceduralTilePair item in base.SyncAccessor_footprintTileMatches)
			{
				if (item.tile.MatchedFootprintTile != null)
				{
					item.tile.MatchedFootprintTile.MatchedStandardTile.LightExposureNode.AddSource(usableLightSource, 1f);
				}
			}
		}

		public void SetIsOn(bool isOn)
		{
			usableLightSource.isEmitting = isOn;
			Light.isOn = isOn;
		}

		public override void DestroyItem(bool callOnServer = true)
		{
			foreach (CoordinateProceduralTilePair item in base.SyncAccessor_footprintTileMatches)
			{
				if (item.tile.MatchedFootprintTile != null)
				{
					item.tile.MatchedFootprintTile.MatchedStandardTile.LightExposureNode.RemoveSource(usableLightSource);
				}
			}
			base.DestroyItem(callOnServer);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EGrowLightAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EGrowLightAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EGrowLightAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EGrowLightAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
