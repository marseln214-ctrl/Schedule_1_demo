using FishNet.Object;

namespace ScheduleOne.Management
{
	public interface IUsable
	{
		bool IsInUse
		{
			get
			{
				if (!(NPCUserObject != null))
				{
					return PlayerUserObject != null;
				}
				return true;
			}
		}

		NetworkObject NPCUserObject { get; set; }

		NetworkObject PlayerUserObject { get; set; }

		void SetPlayerUser(NetworkObject playerObject);

		void SetNPCUser(NetworkObject playerObject);
	}
}
