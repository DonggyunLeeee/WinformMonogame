using ClipperLib;
using System.Collections.Generic;
using Poly2Tri.Triangulation.Polygon;

namespace WinformMonoGame
{
    public class ShapeClipper
    {
        private List<IntPoint> ConvertToClipperPath(List<PolygonPoint> polygon)
        {
            int precisionFactor = 1000;
            List<IntPoint> path = new List<IntPoint>();
            foreach (var vertex in polygon)
            {
                path.Add(new IntPoint((long)vertex.X * precisionFactor, (long)vertex.Y * precisionFactor));
            }
            return path;
        }

        private List<PolygonPoint> ConvertToVectorPath(List<IntPoint> polygon)
        {
            float precisionFactor = 0.001f;
            List<PolygonPoint> path = new List<PolygonPoint>();
            foreach (var point in polygon)
            {
                path.Add(new PolygonPoint(point.X * precisionFactor, point.Y * precisionFactor));
            }
            return path;
        }

        public List<List<PolygonPoint>> DifferencePolygons(List<PolygonPoint> subject, List<PolygonPoint> clip)
        {
            var clipper = new Clipper();

            var subjectPath = ConvertToClipperPath(subject);
            var clipPath = ConvertToClipperPath(clip);

            clipper.AddPolygon(subjectPath, PolyType.ptSubject);
            clipper.AddPolygon(clipPath, PolyType.ptClip);

            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctDifference, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            
            List<List<PolygonPoint>> result = new List<List<PolygonPoint>>();
            foreach (var poly in solution)
            {
                result.Add(ConvertToVectorPath(poly));
            }
            
            return result;
        }

        public List<List<PolygonPoint>> UnionPolygons(List<PolygonPoint> subject, List<PolygonPoint> clip)
        {
            var clipper = new Clipper();

            var subjectPath = ConvertToClipperPath(subject);
            var clipPath = ConvertToClipperPath(clip);

            clipper.AddPolygon(subjectPath, PolyType.ptSubject);
            clipper.AddPolygon(clipPath, PolyType.ptClip);

            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            List<List<PolygonPoint>> result = new List<List<PolygonPoint>>();
            foreach (var poly in solution)
            {
                result.Add(ConvertToVectorPath(poly));
            }

            return result;
        }
    }
}
