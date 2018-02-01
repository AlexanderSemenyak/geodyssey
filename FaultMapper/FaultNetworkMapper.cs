using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using Wintellect.PowerCollections;

using BitImage;
using Image;
using Numeric;
using Model;

namespace FaultMapper
{
    /// <summary>
    /// Converts the image and grid representations of the fault map
    /// and horizon into geometric object representation of the FaultNetwork
    /// </summary>
    public class FaultNetworkMapper
    {
        /// <summary>
        /// Aggregates free ends and junction points of a particular connected-component
        /// during construction of the fault map
        /// </summary>
        private class Component
        {
            public readonly Set<Discrete2D> freeEnds = new Set<Discrete2D>(); // This should be temporary and disposed of when no longer needed
            public readonly Dictionary<Discrete2D, BranchNode> junctions = new Dictionary<Discrete2D, BranchNode>();
        }

        /// <summary>
        /// An integer valued map of connected-component ids. Non-zero values are on fault
        /// centerlines.  The value is the component id.
        /// </summary>
        private readonly IImage<int> componentsMap;

        /// <summary>
        /// An integer valued map of pixel connectivity, based on 'common sense'
        /// notion of connectivity, which takes into account false junctions from
        /// multiple connections to adjacent neighbours. Background pixels = 0,
        /// End points = 1, Mid points = 2, Triple junctions = 3, Cross junctions = 4
        /// </summary>
        private readonly IImage<int> connectivityMap;

        /// <summary>
        /// A boolean map used to indicate which map cells have been processed by the algorithm
        /// so far.
        /// </summary>
        private readonly IImage<bool> notVisitedMap;

        /// <summary>
        /// A coordinate transformation for transformations between grid cell and
        /// geographical coordinates.
        /// </summary>
        private readonly IGridCoordinateTransformation transformation;

        /// <summary>
        /// The generated FaultNetwork object model of the fault centerline map
        /// </summary>
        private readonly FaultNetwork network;

        /// <summary>
        /// A map of connected-component ids (the same as stored in componentsMap)
        /// to Components which store the locations of end points and junction points of
        /// each connected component.
        /// </summary>
        private readonly Dictionary<int, Component> components;

        /// <summary>
        /// Convert a pixel fault centerline map into a geometric object model
        /// </summary>
        /// <param name="faultMap">A pixel map of fault centerlines</param>
        /// <param name="transformation">A transformation from grid to geographic co-ordinates</param>
        /// <returns>A FaultNetwork representing the faultMap</returns>
        public static FaultNetwork MapFaultNetwork(IImage<bool> faultMap, IGridCoordinateTransformation transformation)
        {
            FaultNetworkMapper mapper = new FaultNetworkMapper(faultMap, transformation);
            return mapper.Network;
        }

        private FaultNetworkMapper(IImage<bool> faultMap, IGridCoordinateTransformation transformation)
        {
            notVisitedMap = (IImage<bool>) faultMap.Clone();
            this.transformation = transformation; 
            componentsMap = BitImage.Analysis.ConnectedComponents(faultMap);
            connectivityMap = BitImage.Analysis.Connectivity(faultMap);
            Debug.Assert(componentsMap.Width == connectivityMap.Width);
            Debug.Assert(componentsMap.Height == connectivityMap.Height);
            components = new Dictionary<int, Component>();
            network = new FaultNetwork();
            BuildComponents();
            TraceComponents();
        }

        private FaultNetwork Network
        {
            get { return network;  }
        }

        /// <summary>
        /// Build a dictionary of connected components for the fault network by scanning
        /// the connected component map to determine the number of components, and the
        /// connectivity map to register the free ends and junction points of each connected
        /// component, which will be used as starting points when tracing the components.
        /// </summary>
        private void BuildComponents()
        {
            // Build a List of the starting points for each componentId
            for (int i = 0; i < componentsMap.Width; ++i)
            {
                for (int j = 0; j < componentsMap.Height; ++j)
                {
                    int componentId = componentsMap[i, j];
                    if (componentId != 0)
                    {
                        Component component = ComponentFromId(componentId);

                        int connectivity = connectivityMap[i, j];
                        Debug.Assert(connectivity > 0);
                        switch (connectivity)
                        {
                            case 1:
                                component.freeEnds.Add(new Discrete2D(i, j));
                                break;
                            case 3:
                            case 4:
                                Point2D geographicPosition = GridToGeographic(i, j);
                                BranchNode branchNode = network.CreateBranchNode(geographicPosition);
                                component.junctions.Add(new Discrete2D(i, j), branchNode);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private Component ComponentFromId(int componentId)
        {
            if (!components.ContainsKey(componentId))
            {
                Component component = new Component();
                components.Add(componentId, component);
                return component;
            }
            return components[componentId];
        }

        /// <summary>
        /// Convert a grid cell coordinate to a geographical location.
        /// </summary>
        /// <param name="i">The easting cell coordinate</param>
        /// <param name="j">The northing cell coordinate</param>
        /// <returns>The geographic coordinate</returns>
        private Point2D GridToGeographic(int i, int j)
        {
            // TODO: Does this return the cell center or a cell corner?
            return transformation.GridToGeographic(i, j);
        }

        /// <summary>
        /// Trace each connected component.
        /// </summary>
        private void TraceComponents()
        {
            // Start tracing at the endpoint of each component
            foreach (Component component in components.Values)
            {
                TraceFromFreeEnds(component);
                TraceFourConnectedFromJunctions(component);
                TraceEightConnectedFromJunctions(component);
                TraceContiguousJunctions(component);
            }
        }

        private void TraceFromFreeEnds(Component component)
        {
            Set<Discrete2D> freeEndPoints = component.freeEnds;
            foreach (Discrete2D startingPoint in freeEndPoints)
            {
                // Check that the end point hasn't already been consumed by a
                // previous TraceSegment operation starting from the other end
                if (notVisitedMap[startingPoint.I, startingPoint.J])
                {
                    TraceSegment(component, startingPoint);
                }
                else
                {
                    // Free end points should only have been visited already if
                    // the connected component has only one segment (i.e. no junctions)
                    Debug.Assert(component.junctions.Count == 0);
                }
            }
        }

        private void TraceFourConnectedFromJunctions(Component component)
        {
            Dictionary<Discrete2D, BranchNode> junctionPoints = component.junctions;
            // Trace line segments from each junction point's 4-connected neighbours
            foreach (KeyValuePair<Discrete2D, BranchNode> starting in junctionPoints)
            {
                Debug.Assert(notVisitedMap[starting.Key.I, starting.Key.J]);
                MarkVisited(starting.Key); // Prevent tracings immediately returning to their origin
                // Determine the 4-connected neighbours
                int neighbourhood = BitImage.Analysis.Neighbourhood(notVisitedMap, starting.Key.I, starting.Key.J);
                int fourNeighbourhood = BitImage.Analysis.ToFourCross(neighbourhood);
                int filteredNeighbourhood = RemoveJunctions(starting.Key, fourNeighbourhood);
                List<Discrete2D> fourNeighbours = new List<Discrete2D>(BitImage.Analysis.ConnectedRelativeCoordinates(filteredNeighbourhood));
                foreach (Discrete2D relativeNeighbour in fourNeighbours)
                {
                    Discrete2D neighbour = starting.Key + relativeNeighbour;
                    // Check that the starting point hasn't already been consumed, for example,
                    // by a single line looping back so the same branch point it originated from
                    if (notVisitedMap[neighbour.I, neighbour.J])
                    {
                        SegmentNode segment = TraceSegment(component, neighbour);
                        segment.PreviousBranch = starting.Value;
                    }
                    else
                    {
                        Console.WriteLine("Model contains single segment loops!"); // TODO: Test this
                    }
                }
                UnmarkVisited(starting.Key); // Unmark, so that this junction point can be found for other segment ends
            }
        }

        private void TraceEightConnectedFromJunctions(Component component)
        {
            // TODO: This case only occurs if there are segments which are 8-connected
            // at BOTH ends to branch points. NOT YET TESTED.
            Dictionary<Discrete2D, BranchNode> junctionPoints = component.junctions;
            // Trace line segments from each junction point's 8-connected neighbours
            foreach (KeyValuePair<Discrete2D, BranchNode> starting in junctionPoints)
            {
                Debug.Assert(notVisitedMap[starting.Key.I, starting.Key.J]);
                MarkVisited(starting.Key); // Prevent tracings immediately returning to their origin
                // Determine the 8-connected neighbours
                int neighbourhood = BitImage.Analysis.Neighbourhood(notVisitedMap, starting.Key.I, starting.Key.J);
                int fourNeighbourhood = BitImage.Analysis.ToFourDiagonal(neighbourhood); // One line different
                int filteredNeighbourhood = RemoveJunctions(starting.Key, fourNeighbourhood); // NOT NEEDED??
                List<Discrete2D> fourNeighbours = new List<Discrete2D>(BitImage.Analysis.ConnectedRelativeCoordinates(filteredNeighbourhood));
                foreach (Discrete2D relativeNeighbour in fourNeighbours)
                {
                    Discrete2D neighbour = starting.Key + relativeNeighbour;
                    // Check that the starting point hasn't already been consumed, for example,
                    // by a single line looping back so the same branch point it originated from
                    if (notVisitedMap[neighbour.I, neighbour.J])
                    {
                        SegmentNode segment = TraceSegment(component, neighbour);
                        segment.PreviousBranch = starting.Value;
                    }
                    else
                    {
                        Console.WriteLine("Model contains single segment loops!"); // TODO: Test this
                    }
                }
                UnmarkVisited(starting.Key); // Unmark, so that this junction point can be found for other segment ends
            }
        }

        private void TraceContiguousJunctions(Component component)
        {
            Dictionary<Discrete2D, BranchNode> junctionPoints = component.junctions;
            foreach (KeyValuePair<Discrete2D, BranchNode> starting in junctionPoints)
            {
                Debug.Assert(notVisitedMap[starting.Key.I, starting.Key.J]);
                MarkVisited(starting.Key); // n.b. not unmarked later, to prevent marking contiguous segments twice
                // Determine the 8-connected neighbours
                int neighbourhood = BitImage.Analysis.Neighbourhood(notVisitedMap, starting.Key.I, starting.Key.J);
                // TODO: Is it possible to have diagonally connected junctions?
                List<Discrete2D> relativeNeighbours = new List<Discrete2D>(BitImage.Analysis.ConnectedRelativeCoordinates(neighbourhood));
                foreach (Discrete2D relativeNeighbour in relativeNeighbours)
                {
                    Discrete2D neighbour = starting.Key + relativeNeighbour;
                    // Check that the starting point hasn't already been consumed by the other branchpoint
                    if (IsJunction(neighbour))
                    {
                        SegmentNode segment = new SegmentNode();
                        segment.PreviousBranch = starting.Value;
                        segment.NextBranch = junctionPoints[neighbour];
                    }
                }
            }
        }

        private SegmentNode TraceSegment(Component component, Discrete2D start)
        {
            Discrete2D? current = start;
            Debug.Assert(connectivityMap[current.Value.I, current.Value.J] != 0);
            SegmentNode segment = network.CreateSegmentNode();
            while (current.HasValue)
            {
                current = Follow(component, current, segment);
            }
            return segment;
        }

        private Discrete2D? Follow(Component component, Discrete2D? current, SegmentNode segment)
        {
            Debug.Assert(current.HasValue);
            int neighbourhood = BitImage.Analysis.Neighbourhood(notVisitedMap, current.Value.I, current.Value.J);

            neighbourhood = AccountForJunctions(current, neighbourhood);

            int eightConnectivity = BitImage.Analysis.EightConnectivity(neighbourhood);
            Debug.Assert(eightConnectivity > 0);

            if (eightConnectivity == 1)
            {
                List<Discrete2D> neighbours = new List<Discrete2D>(BitImage.Analysis.ConnectedRelativeCoordinates(neighbourhood));
                Debug.Assert(eightConnectivity == neighbours.Count);

                Discrete2D neighbour = current.Value + neighbours[0];
                if (IsJunction(neighbour))
                {
                    current = EndSegmentAtJunction(component, current, segment, neighbour);
                }
                else if (IsFreeEnd(neighbour))
                {
                    current = EndSegmentAtFreeEnd(current, segment, neighbour);
                }
                else // neighbour is segment stem
                {
                    current = AppendToSegment(current, segment, neighbour);
                }
            }
            else // eightConnectivity > 1
            {
                // Determine numFour - the number of 4-connected neighbours
                int fourConnectivity = BitImage.Analysis.FourConnectivity(neighbourhood);
                Debug.Assert(fourConnectivity == 1);

                int fourNeighbourhood = BitImage.Analysis.ToFourCross(neighbourhood);
                List<Discrete2D> fourNeighbours = new List<Discrete2D>(BitImage.Analysis.ConnectedRelativeCoordinates(fourNeighbourhood));
                Debug.Assert(fourConnectivity == fourNeighbours.Count);

                Discrete2D neighbour = current.Value + fourNeighbours[0];
                if (IsJunction(neighbour))
                {
                    current = EndSegmentAtJunction(component, current, segment, neighbour);
                }
                else // Extend to neighbour
                {
                    Debug.Assert(IsStem(neighbour));
                    current = AppendToSegment(current, segment, neighbour);
                }
            }
            return current;
        }

        /// <summary>
        /// Modify the neighbourhood to remove any eight-connected junctions. Useful
        /// when processing contiguous junction points
        /// </summary>
        /// <param name="current">The pixel whose neighbourhood should be modified</param>
        /// <param name="neighbourhood">The neighbourhood to be modified</param>
        /// <returns>The modified neighbourhood</returns>
        private int RemoveJunctions(Discrete2D? current, int neighbourhood)
        {
            Debug.Assert(current.HasValue);
            const int allExceptCenter = 510; // TODO: Make this and the next line static
            List<Discrete2D> allNeighbours = new List<Discrete2D>(BitImage.Analysis.ConnectedRelativeCoordinates(allExceptCenter));
            Int32 bits = 1;
            foreach (Discrete2D relativeCoord in allNeighbours)
            {
                bits <<= 1;
                Discrete2D neighbour = current.Value + relativeCoord;
                if (connectivityMap.IsInRange(neighbour.I, neighbour.J) && IsJunction(neighbour))
                {
                    Int32 mask = ~bits;
                    neighbourhood &= mask;
                    Debug.Assert(neighbourhood >= 0 && neighbourhood < 512);
                }
            }
            return neighbourhood;
        }

        /// <summary>
        /// Modify the neighbourhood to remove 8-neighbours which could be reached via
        /// 4-neighbour junction points, which have been temporarily removed.
        /// </summary>
        /// <param name="current">The pixel whose neighbourhood should be modified</param>
        /// <param name="neighbourhood">The neighbourhood to be modified</param>
        /// <returns>The modified neighbourhood</returns>
        private int AccountForJunctions(Discrete2D? current, int neighbourhood)
        {
            // Could use the same technique with a rotating bit pattern...
            Discrete2D west = current.Value.West();
            Discrete2D north = current.Value.North();
            Discrete2D south = current.Value.South();
            Discrete2D east = current.Value.East();
            if (connectivityMap.IsInRange(west.I, west.J) && IsJunction(west))
            {
                neighbourhood &= 431;
            }
            if (connectivityMap.IsInRange(north.I, north.J) && IsJunction(north))
            {
                neighbourhood &= 491;
            }
            if (connectivityMap.IsInRange(south.I, south.J) && IsJunction(south))
            {
                neighbourhood &= 191;
            }
            if (connectivityMap.IsInRange(east.I, east.J) && IsJunction(east))
            {
                neighbourhood &= 251;
            }
            return neighbourhood;
        }

        private Discrete2D? EndSegmentAtJunction(Component component, Discrete2D? current, SegmentNode segment, Discrete2D junction)
        {
            // End segment here with edge to junction
            Debug.Assert(current.HasValue);
            BranchNode branchPoint = component.junctions[junction];
            Point2D geographic = GridToGeographic(current.Value.I, current.Value.J);
            segment.Add(geographic);
            segment.NextBranch = branchPoint;
            MarkVisited(current.Value);
            return null;
        }

        private Discrete2D? EndSegmentAtFreeEnd(Discrete2D? current, SegmentNode segment, Discrete2D neighbour)
        {
            // End segment here by adding last node
            Debug.Assert(current.HasValue);
            Point2D currentGeographic = GridToGeographic(current.Value.I, current.Value.J);
            segment.Add(currentGeographic);
            Point2D neighbourGeographic = GridToGeographic(neighbour.I, neighbour.J);
            segment.Add(neighbourGeographic);
            segment.NextBranch = null;
            MarkVisited(current.Value);
            MarkVisited(neighbour);
            return null;
        }

        private Discrete2D? AppendToSegment(Discrete2D? current, SegmentNode segment, Discrete2D neighbour)
        {
            // Add a point to the segment, and move on
            Debug.Assert(current.HasValue);
            Debug.Assert(IsStem(neighbour));
            Point2D currentGeographic = GridToGeographic(current.Value.I, current.Value.J);
            segment.Add(currentGeographic);
            MarkVisited(current.Value);
            return neighbour;
        }

        private void MarkVisited(Discrete2D point)
        {
            notVisitedMap[point.I, point.J] = false;
        }

        private void UnmarkVisited(Discrete2D point)
        {
            notVisitedMap[point.I, point.J] = true;
        }

        private bool IsFreeEnd(Discrete2D coord)
        {
            Debug.Assert(connectivityMap != null);
            return connectivityMap[coord.I, coord.J] == 1;
        }

        private bool IsStem(Discrete2D coord)
        {
            Debug.Assert(connectivityMap != null);
            return connectivityMap[coord.I, coord.J] == 2;
        }

        private bool IsJunction(Discrete2D coord)
        {
            Debug.Assert(connectivityMap != null);
            return connectivityMap[coord.I, coord.J] >= 3;
        }
    }
}
