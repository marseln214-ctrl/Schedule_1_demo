using ScheduleOne.PlayerTasks;

namespace ScheduleOne.StationFramework
{
	public class IngredientModule : ItemModule
	{
		public IngredientPiece[] Pieces;

		public override void ActivateModule(StationItem item)
		{
			base.ActivateModule(item);
			for (int i = 0; i < Pieces.Length; i++)
			{
				Pieces[i].GetComponent<DraggableConstraint>().SetContainer(item.transform.parent);
			}
		}
	}
}
