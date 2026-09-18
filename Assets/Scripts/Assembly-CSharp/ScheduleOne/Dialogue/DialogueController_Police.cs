using ScheduleOne.Police;

namespace ScheduleOne.Dialogue
{
	public class DialogueController_Police : DialogueController
	{
		private PoliceOfficer officer;

		protected override void Start()
		{
			base.Start();
			officer = npc as PoliceOfficer;
		}

		public override bool CanStartDialogue()
		{
			if (officer.PursuitBehaviour.Active)
			{
				return false;
			}
			if (officer.VehiclePursuitBehaviour.Active)
			{
				return false;
			}
			if (officer.BodySearchBehaviour.Active)
			{
				return false;
			}
			if (officer.CheckpointBehaviour.Active && officer.CheckpointBehaviour.IsSearching)
			{
				return false;
			}
			return base.CanStartDialogue();
		}
	}
}
