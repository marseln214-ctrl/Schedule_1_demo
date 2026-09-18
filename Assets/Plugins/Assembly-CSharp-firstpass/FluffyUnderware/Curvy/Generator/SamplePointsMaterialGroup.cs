using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Generator
{
	public class SamplePointsMaterialGroup
	{
		public int MaterialID;

		public List<SamplePointsPatch> Patches;

		public int TriangleCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < Patches.Count; i++)
				{
					num += Patches[i].TriangleCount;
				}
				return num;
			}
		}

		public int StartVertex => Patches[0].Start;

		public int EndVertex => Patches[Patches.Count - 1].End;

		public int VertexCount => EndVertex - StartVertex + 1;

		public SamplePointsMaterialGroup(int materialID)
			: this(materialID, new List<SamplePointsPatch>())
		{
		}

		public SamplePointsMaterialGroup(int materialID, List<SamplePointsPatch> patches)
		{
			MaterialID = materialID;
			Patches = patches;
		}

		public void GetLengths(CGVolume volume, out float worldLength, out float uLength)
		{
			worldLength = 0f;
			for (int i = StartVertex; i < EndVertex; i++)
			{
				worldLength += (volume.Vertices.Array[i + 1] - volume.Vertices.Array[i]).magnitude;
			}
			uLength = volume.CrossCustomValues.Array[EndVertex] - volume.CrossCustomValues.Array[StartVertex];
		}

		public SamplePointsMaterialGroup Clone()
		{
			return new SamplePointsMaterialGroup(MaterialID, new List<SamplePointsPatch>(Patches));
		}
	}
}
