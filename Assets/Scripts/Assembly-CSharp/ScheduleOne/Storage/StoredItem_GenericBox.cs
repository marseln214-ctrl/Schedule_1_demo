using UnityEngine;

namespace ScheduleOne.Storage
{
	public class StoredItem_GenericBox : StoredItem
	{
		private const float ReferenceIconWidth = 1024f;

		[Header("References")]
		[SerializeField]
		protected SpriteRenderer icon1;

		[SerializeField]
		protected SpriteRenderer icon2;

		[Header("Settings")]
		public float IconScale = 0.5f;

		public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
		{
			base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
			icon1.sprite = _item.Icon;
			icon2.sprite = _item.Icon;
			float num = 0.025f / (_item.Icon.rect.width / 1024f) * IconScale;
			icon1.transform.localScale = new Vector3(num, num, 1f);
			icon2.transform.localScale = new Vector3(num, num, 1f);
		}
	}
}
