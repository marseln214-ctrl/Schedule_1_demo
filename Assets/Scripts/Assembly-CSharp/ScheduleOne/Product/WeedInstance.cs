using System;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.Packaging;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product.Packaging;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	public class WeedInstance : ProductItemInstance
	{
		public WeedInstance()
		{
		}

		public WeedInstance(ItemDefinition definition, int quantity, EQuality quality, PackagingDefinition packaging = null)
			: base(definition, quantity, quality, packaging)
		{
		}

		public override ItemInstance GetCopy(int overrideQuantity = -1)
		{
			int quantity = Quantity;
			if (overrideQuantity != -1)
			{
				quantity = overrideQuantity;
			}
			return new WeedInstance(base.Definition, quantity, Quality, base.AppliedPackaging);
		}

		public override void SetupPackagingVisuals(FilledPackagingVisuals visuals)
		{
			base.SetupPackagingVisuals(visuals);
			if (visuals == null)
			{
				Console.LogError("WeedInstance: visuals is null!");
				return;
			}
			WeedDefinition weedDefinition = base.Definition as WeedDefinition;
			if (weedDefinition == null)
			{
				Console.LogError("WeedInstance: definition is null! Type: " + base.Definition);
				return;
			}
			FilledPackagingVisuals.MeshIndexPair[] mainMeshes = visuals.weedVisuals.MainMeshes;
			foreach (FilledPackagingVisuals.MeshIndexPair meshIndexPair in mainMeshes)
			{
				Material[] materials = meshIndexPair.Mesh.materials;
				materials[meshIndexPair.MaterialIndex] = weedDefinition.MainMat;
				meshIndexPair.Mesh.materials = materials;
			}
			mainMeshes = visuals.weedVisuals.SecondaryMeshes;
			foreach (FilledPackagingVisuals.MeshIndexPair meshIndexPair2 in mainMeshes)
			{
				Material[] materials2 = meshIndexPair2.Mesh.materials;
				materials2[meshIndexPair2.MaterialIndex] = weedDefinition.SecondaryMat;
				meshIndexPair2.Mesh.materials = materials2;
			}
			mainMeshes = visuals.weedVisuals.LeafMeshes;
			foreach (FilledPackagingVisuals.MeshIndexPair meshIndexPair3 in mainMeshes)
			{
				Material[] materials3 = meshIndexPair3.Mesh.materials;
				materials3[meshIndexPair3.MaterialIndex] = weedDefinition.LeafMat;
				meshIndexPair3.Mesh.materials = materials3;
			}
			mainMeshes = visuals.weedVisuals.StemMeshes;
			foreach (FilledPackagingVisuals.MeshIndexPair meshIndexPair4 in mainMeshes)
			{
				Material[] materials4 = meshIndexPair4.Mesh.materials;
				materials4[meshIndexPair4.MaterialIndex] = weedDefinition.StemMat;
				meshIndexPair4.Mesh.materials = materials4;
			}
			visuals.weedVisuals.Container.gameObject.SetActive(value: true);
		}

		public override ItemData GetItemData()
		{
			return new WeedData(base.Definition.ID, Quantity, Quality.ToString(), PackagingID);
		}

		public override void ApplyEffectsToNPC(NPC npc)
		{
			npc.Avatar.Eyes.OverrideEyeballTint(new Color32(byte.MaxValue, 170, 170, byte.MaxValue));
			npc.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0.3f,
				topLidOpen = 0.3f
			});
			npc.Avatar.Eyes.ForceBlink();
			base.ApplyEffectsToNPC(npc);
		}

		public override void ClearEffectsFromNPC(NPC npc)
		{
			npc.Avatar.Eyes.ResetEyeballTint();
			npc.Avatar.Eyes.ResetEyeLids();
			npc.Avatar.Eyes.ForceBlink();
			base.ClearEffectsFromNPC(npc);
		}

		public override void ApplyEffectsToPlayer(Player player)
		{
			player.Avatar.Eyes.OverrideEyeballTint(new Color32(byte.MaxValue, 170, 170, byte.MaxValue));
			player.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0.3f,
				topLidOpen = 0.3f
			});
			if (player.IsOwner)
			{
				Singleton<PostProcessingManager>.Instance.ChromaticAberrationController.AddOverride(0.2f, 5, "weed");
				Singleton<PostProcessingManager>.Instance.SaturationController.AddOverride(70f, 5, "weed");
				Singleton<PostProcessingManager>.Instance.BloomController.AddOverride(3f, 5, "weed");
				Singleton<MusicPlayer>.Instance.SetMusicDistorted(distorted: true);
				Singleton<AudioManager>.Instance.SetDistorted(distorted: true);
			}
			base.ApplyEffectsToPlayer(player);
		}

		public override void ClearEffectsFromPlayer(Player Player)
		{
			Player.Avatar.Eyes.ResetEyeballTint();
			Player.Avatar.Eyes.ResetEyeLids();
			Player.Avatar.Eyes.ForceBlink();
			if (Player.IsOwner)
			{
				Singleton<PostProcessingManager>.Instance.ChromaticAberrationController.RemoveOverride("weed");
				Singleton<PostProcessingManager>.Instance.SaturationController.RemoveOverride("weed");
				Singleton<PostProcessingManager>.Instance.BloomController.RemoveOverride("weed");
				Singleton<MusicPlayer>.Instance.SetMusicDistorted(distorted: false);
				Singleton<AudioManager>.Instance.SetDistorted(distorted: false);
			}
			base.ClearEffectsFromPlayer(Player);
		}
	}
}
