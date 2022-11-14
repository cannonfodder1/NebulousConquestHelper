using System;
using System.Drawing;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("Belt")]
    [Serializable]
    public class Belt : IComparable<Belt>
    {
        public string Name;
        public float NearEdgeDistanceAU;
        public float FarEdgeDistanceAU;

        public int CompareTo(Belt compareBelt)
        {
            if (compareBelt == null)
                return 1;
            else
                return compareBelt.NearEdgeDistanceAU.CompareTo(NearEdgeDistanceAU);
        }

        public PointF GetEdgeCoordinates(int degrees)
        {
            double radians = degrees * (Math.PI / 180);
            double beltX = Math.Sin(radians) * FarEdgeDistanceAU;
            double beltY = Math.Cos(radians) * FarEdgeDistanceAU * -1;
            return new PointF((float)beltX, (float)beltY);
        }

        public bool TraversingAsteroidBelt(Location src, Location dst, int daysFromStart = 0)
        {
            bool srcWithinBelt = src.OrbitalDistanceAU < NearEdgeDistanceAU;
            bool dstWithinBelt = dst.OrbitalDistanceAU < NearEdgeDistanceAU;
            
            if (srcWithinBelt && dstWithinBelt)
            {
                return false;
            }
            if (srcWithinBelt != dstWithinBelt)
            {
                return true;
            }

            PointF startT = src.GetCoordinates(daysFromStart);
            PointF endT = dst.GetCoordinates(daysFromStart);

            if (DoLinesIntersect(startT, endT, GetEdgeCoordinates(0), GetEdgeCoordinates(180))) return true;
            if (DoLinesIntersect(startT, endT, GetEdgeCoordinates(45), GetEdgeCoordinates(225))) return true;
            if (DoLinesIntersect(startT, endT, GetEdgeCoordinates(90), GetEdgeCoordinates(270))) return true;
            if (DoLinesIntersect(startT, endT, GetEdgeCoordinates(135), GetEdgeCoordinates(315))) return true;

            return false;
        }

        private static bool DoLinesIntersect(PointF startA, PointF endA, PointF startB, PointF endB)
        {
            float uA = ((endB.X - startB.X) * (startA.Y - startB.Y) - (endB.Y - startB.Y) * (startA.X - startB.X)) / ((endB.Y - startB.Y) * (endA.X - startA.X) - (endB.X - startB.X) * (endA.Y - startA.Y));
            float uB = ((endA.X - startA.X) * (startA.Y - startB.Y) - (endA.Y - startA.Y) * (startA.X - startB.X)) / ((endB.Y - startB.Y) * (endA.X - startA.X) - (endB.X - startB.X) * (endA.Y - startA.Y));

            return uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1;
        }
    }
}