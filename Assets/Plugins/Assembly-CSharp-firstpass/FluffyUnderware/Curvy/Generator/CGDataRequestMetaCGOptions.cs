using System;
using JetBrains.Annotations;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGDataRequestMetaCGOptions : CGDataRequestParameter
	{
		[UsedImplicitly]
		[Obsolete("This option is now always assumed to be true")]
		public bool CheckHardEdges;

		[UsedImplicitly]
		[Obsolete("This option is now always assumed to be true")]
		public bool CheckMaterialID;

		public bool IncludeControlPoints;

		[UsedImplicitly]
		[Obsolete("This option is now always assumed to be true")]
		public bool CheckExtendedUV;

		public CGDataRequestMetaCGOptions(bool checkEdges, bool checkMaterials, bool includeCP, bool extendedUV)
		{
			CheckHardEdges = checkEdges;
			CheckMaterialID = checkMaterials;
			IncludeControlPoints = includeCP;
			CheckExtendedUV = extendedUV;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CGDataRequestMetaCGOptions cGDataRequestMetaCGOptions))
			{
				return false;
			}
			if (CheckHardEdges == cGDataRequestMetaCGOptions.CheckHardEdges && CheckMaterialID == cGDataRequestMetaCGOptions.CheckMaterialID && IncludeControlPoints == cGDataRequestMetaCGOptions.IncludeControlPoints)
			{
				return CheckExtendedUV == cGDataRequestMetaCGOptions.CheckExtendedUV;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return new
			{
				A = CheckHardEdges,
				B = CheckMaterialID,
				C = IncludeControlPoints,
				D = CheckExtendedUV
			}.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", "CheckHardEdges", CheckHardEdges, "CheckMaterialID", CheckMaterialID, "IncludeControlPoints", IncludeControlPoints, "CheckExtendedUV", CheckExtendedUV);
		}
	}
}
