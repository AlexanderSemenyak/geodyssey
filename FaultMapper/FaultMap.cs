using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geometry.HalfEdge;

namespace FaultMapper
{
    /// <summary>
    /// Representation of a fault centerline map
    /// </summary>
    public class FaultMap
    {

        /// <summary>
        /// The half-edge data structure storing the positions and connectivity
        /// of the fault center lines. Each vertex carries additional fault information
        /// such as the location of cutoff points.
        /// </summary>
        private Mesh<HalfEdge.FaultVertex, EdgeBase, FaceBase> centerMesh;

        /// <summary>
        /// A list of the connected components of faults in the mesh. Each entry
        /// is a connected component of of the map.
        /// </summary>
        private List<HalfEdge.FaultVertex> components;

        /// <summary>
        /// A sequence of vertices providing access to each connected component
        /// of the fault map
        /// </summary>
        public IEnumerable<HalfEdge.FaultVertex> Components
        {
            get { return components; }
        }
    }
}
