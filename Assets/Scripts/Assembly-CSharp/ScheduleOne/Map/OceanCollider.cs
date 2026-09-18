using System.Collections;
using System.Collections.Generic;
using FishNet;
using Pathfinding;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class OceanCollider : MonoBehaviour
	{
		private bool localPlayerBeingWarped;

		private List<LandVehicle> warpedVehicles = new List<LandVehicle>();

		public AudioSourceController SplashSound;

		private void OnTriggerEnter(Collider other)
		{
			Player componentInParent = other.GetComponentInParent<Player>();
			if (componentInParent != null && componentInParent == Player.Local && componentInParent.Health.IsAlive && componentInParent.CurrentVehicle == null && !localPlayerBeingWarped)
			{
				localPlayerBeingWarped = true;
				StartCoroutine(WarpPlayer());
			}
			LandVehicle componentInParent2 = other.GetComponentInParent<LandVehicle>();
			if (componentInParent2 != null)
			{
				Debug.Log("Vehicle entered ocean");
				if ((componentInParent2.DriverPlayer == Player.Local || (componentInParent2.DriverPlayer == null && InstanceFinder.IsHost)) && !warpedVehicles.Contains(componentInParent2))
				{
					warpedVehicles.Add(componentInParent2);
					StartCoroutine(WarpVehicle(componentInParent2));
				}
			}
		}

		private IEnumerator WarpPlayer()
		{
			SplashSound.transform.SetParent(Player.Local.gameObject.transform);
			SplashSound.transform.localPosition = Vector3.zero;
			SplashSound.Play();
			Singleton<BlackOverlay>.Instance.Open(0.05f);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			yield return new WaitForSeconds(0.12f);
			PlayerSingleton<PlayerMovement>.Instance.WarpToNavMesh();
			yield return new WaitForSeconds(0.2f);
			Singleton<BlackOverlay>.Instance.Close(0.3f);
			localPlayerBeingWarped = false;
			SplashSound.transform.SetParent(base.transform);
			yield return new WaitForSeconds(0.2f);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
		}

		private IEnumerator WarpVehicle(LandVehicle veh)
		{
			bool faded = false;
			if (veh.localPlayerIsDriver)
			{
				faded = true;
				Singleton<BlackOverlay>.Instance.Open(0.15f);
			}
			yield return new WaitForSeconds(0.16f);
			NNConstraint nNConstraint = new NNConstraint();
			nNConstraint.graphMask = GraphMask.FromGraphName("Road Nodes");
			NNInfo nearest = AstarPath.active.GetNearest(veh.transform.position, nNConstraint);
			veh.transform.position = nearest.position + base.transform.up * veh.boundingBoxDimensions.y / 2f;
			veh.transform.rotation = Quaternion.identity;
			veh.Rb.velocity = Vector3.zero;
			veh.Rb.angularVelocity = Vector3.zero;
			yield return new WaitForSeconds(0.2f);
			if (faded)
			{
				Singleton<BlackOverlay>.Instance.Close(0.3f);
			}
			warpedVehicles.Remove(veh);
		}
	}
}
