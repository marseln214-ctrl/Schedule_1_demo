using System.Linq;
using EasyButtons;
using UnityEngine;

namespace ScheduleOne.Properties.MixMaps
{
	public class MixerMapGenerator : MonoBehaviour
	{
		public float MapRadius = 5f;

		public string MapName = "New Map";

		public Transform BasePlateMesh;

		public Effect EffectPrefab;

		private void OnValidate()
		{
			BasePlateMesh.localScale = Vector3.one * MapRadius * 2f * 0.01f;
			base.gameObject.name = MapName;
		}

		[Button]
		public void CreateEffectPrefabs()
		{
			Property[] array = Resources.LoadAll<Property>("Properties");
			foreach (Property property in array)
			{
				if (GetEffect(property) == null)
				{
					Effect effect = Object.Instantiate(EffectPrefab, base.transform);
					effect.Property = property;
					effect.Radius = 0.5f;
					effect.transform.position = new Vector3(Random.Range(0f - MapRadius, MapRadius), 0.1f, Random.Range(0f - MapRadius, MapRadius));
				}
			}
		}

		public Effect GetEffect(Property property)
		{
			return GetComponentsInChildren<Effect>().FirstOrDefault((Effect effect) => effect.Property == property);
		}
	}
}
