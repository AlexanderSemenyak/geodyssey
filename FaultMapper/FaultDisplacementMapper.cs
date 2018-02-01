using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FaultMapper.HalfEdge;
using Geometry.HalfEdge;
using Geometry.PolygonPartitioning;
using Numeric;
using Utility.Extensions.System.Collections.Generic;

namespace FaultMapper
{
    /// <summary>
    /// Given adds fault displacement information to a FaultNetwork from the appropriate
    /// horizon map
    /// </summary>
    public class FaultDisplacementMapper
    {
        /// <summary>
        /// The source fault network
        /// </summary>
        private readonly FaultNetwork network;

        /// <summary>
        /// A mesh representation of the fault centerline network
        /// </summary>
        private Mesh<PositionedVertexBase, EdgeBase, FaceBase> result;

        public FaultDisplacementMapper(FaultNetwork network)
        {
            this.network = network;
        }


        // TODO: Temporary

        public Mesh<PositionedVertexBase, EdgeBase, FaceBase> GetResult()
        {
            if (result == null)
            {
                result = Process();
            }
            return result;
        }

        /// <summary>
        /// Execute the algorithm
        /// </summary>
        private Mesh<PositionedVertexBase, EdgeBase, FaceBase> Process()
        {
            var mesh = CreateMesh(); // TODO: This should return the mesh

            // Add a bounding box to the mesh
            var points = mesh.Vertices.Select(v => v.Position);
            BBox2D bbox = BBox2D.FromPoints(points, 1.0);
            PositionedVertexBase bb0 = mesh.Add(new BoundingVertex(new Point2D(bbox.XMin, bbox.YMin)));
            PositionedVertexBase bb1 = mesh.Add(new BoundingVertex(new Point2D(bbox.XMax, bbox.YMin)));
            PositionedVertexBase bb2 = mesh.Add(new BoundingVertex(new Point2D(bbox.XMax, bbox.YMax)));
            PositionedVertexBase bb3 = mesh.Add(new BoundingVertex(new Point2D(bbox.XMin, bbox.YMax)));
            mesh.AddEdge(bb0, bb1, new BoundingEdge());
            mesh.AddEdge(bb1, bb2, new BoundingEdge());
            mesh.AddEdge(bb2, bb3, new BoundingEdge());
            mesh.AddEdge(bb3, bb0, new BoundingEdge());

            // Monotonize the mesh
            var partitioner = new MonotoneYPartitioner<PositionedVertexBase, EdgeBase, FaceBase>(mesh, InsertMonotonizingDiagonal);
            mesh = partitioner.GetResult();
            return mesh;
        }

        /// <summary>
        /// Create a mesh representing the fault centerlines
        /// </summary>
        private Mesh<PositionedVertexBase, EdgeBase, FaceBase> CreateMesh()
        {
            // 1a. Add the vertices
            IEnumerable<PositionedVertexBase> vs = network.Points.Select(fp => (PositionedVertexBase) new FaultVertex(fp));
            var mesh = new Mesh<PositionedVertexBase, EdgeBase, FaceBase>(vs);
            
            // 1b. Connect vertices
            foreach (SegmentNode segment in network.Segments)
            {
                SegmentNode s = segment; // Prevents access to modified closure
                segment.InclusivePoints.ForEachPair((p1, p2) => AddFaultEdge(s, mesh, p1, p2));
            }

            return mesh;
        }

        /// <summary>
        /// Insert a FaultEdge edge into the mesh between the two specified positions
        /// </summary>
        /// <param name="segment">The equivalent segment in the FaultNetwork</param>
        /// <param name="m">The mesh to which the edge is to be added</param>
        /// <param name="p1">A position at the start of the edge</param>
        /// <param name="p2">A position at the end of the edge</param>
        private static void AddFaultEdge(SegmentNode segment, Mesh<PositionedVertexBase, EdgeBase, FaceBase> m, IPositionable2D p1, IPositionable2D p2)
        {
            var v1 = m.Find(new FaultVertex(p1));
            Debug.Assert(v1 != null);
            var v2 = m.Find(new FaultVertex(p2));
            Debug.Assert(v2 != null);
            m.AddEdge(v1, v2, new FaultEdge(segment));
        }

        /// <summary>
        /// Used for inserting monotonizing diagonals into the mesh by the monotone partitioning algorithm.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="fromVertex"></param>
        /// <param name="toVertex"></param>
        private static void InsertMonotonizingDiagonal(Mesh<PositionedVertexBase, EdgeBase, FaceBase> m, PositionedVertexBase fromVertex, PositionedVertexBase toVertex)
        {
            m.AddEdge(fromVertex, toVertex, new MonotonizingEdge());
        }
    }
}
