using FishNet.Connection;
using FishNet.Object;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Management
{
	public interface IConfigurable
	{
		EntityConfiguration Configuration { get; }

		ConfigurationReplicator ConfigReplicator { get; }

		EConfigurableType ConfigurableType { get; }

		WorldspaceUIElement WorldspaceUI { get; set; }

		NetworkObject CurrentPlayerConfigurer { get; set; }

		bool IsBeingConfiguredByOtherPlayer
		{
			get
			{
				if (CurrentPlayerConfigurer != null)
				{
					return !CurrentPlayerConfigurer.IsOwner;
				}
				return false;
			}
		}

		Sprite TypeIcon { get; }

		Transform Transform { get; }

		Transform UIPoint { get; }

		bool IsDestroyed => Transform == null;

		bool CanBeSelected { get; }

		WorldspaceUIElement CreateWorldspaceUI();

		void DestroyWorldspaceUI();

		void ShowOutline(Color color);

		void HideOutline();

		void Selected()
		{
			Configuration.Selected();
		}

		void Deselected()
		{
			Configuration.Deselected();
		}

		void SetConfigurer(NetworkObject player);

		void SendConfigurationToClient(NetworkConnection conn);
	}
}
