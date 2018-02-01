using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

//using Wintellect.PowerCollections;
using Geometry.HalfEdge;
using Utility;
using Utility.Collections;
using Numeric;

namespace Geometry
{
    public class AngularBisectorNetwork
    {
        private class Vertex
        {
            // TODO: Combine position and bisector into a Ray2D
            private Ray2D bisector;
            public Edge inEdge;
            public Edge outEdge;
            public bool processed = false;

            public Vertex(Point2D position)
            {
                this.bisector = new Ray2D(position, new Direction2D());
            }

            public Ray2D Bisector
            {
                get { return bisector; }
                set { bisector = value; }
            }

            public Point2D Position
            {
                get { return bisector.Source; }
                set { bisector.Source = value; }
            }

            public Direction2D Direction
            {
                get { return bisector.Direction; }
                set { bisector.Direction = value; }
            }
        }

        private class Edge
        {
            public readonly Vertex source;
            public readonly Vertex target;

            public Edge(Vertex source, Vertex target)
            {
                this.source = source;
                this.target = target;
            }

            public Vector2D Vector
            {
                get { return target.Position - source.Position; }
            }
        }

        private abstract class Intersection
        {
            private readonly Point2D position;
            
            protected Intersection(Point2D position)
            {
                this.position = position;
            }

            public abstract bool Processed
            {
                get;
                set;
            }

            public Point2D Position
            {
                get { return position; }
            }

            public abstract bool CreateArcs(AngularBisectorNetwork network);
            public abstract void ModifyLav(AngularBisectorNetwork network);
        }

        private class EdgeEvent : Intersection
        {
            public readonly CircularLinkedListNode<Vertex> nodeA;
            public readonly CircularLinkedListNode<Vertex> nodeB;

            public EdgeEvent(Point2D position, CircularLinkedListNode<Vertex> node1, CircularLinkedListNode<Vertex> node2) :
                base(position)
            {
                this.nodeA = node1;
                this.nodeB = node2;
            }

            public override bool Processed
            {
                get
                {
                    Vertex vA = nodeA.Value;
                    Vertex vB = nodeB.Value;

                    //  b. If the vertices Va and Vb, pointed to by I are marked as processed, the continue on the step 2
                    //     else the edge between vertices Va, Vb shrinks to zero (edge event)
                    bool processedA = vA.processed;
                    bool processedB = vB.processed;
                    return processedA || processedB;
                }

                set
                {
                    Vertex vA = nodeA.Value;
                    Vertex vB = nodeB.Value;

                    vA.processed = value;
                    vB.processed = value;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="network"></param>
            /// <returns>true if the skeleton output is the peak of the 'roof'</returns>
            public override bool CreateArcs(AngularBisectorNetwork network)
            {
                Vertex vA = nodeA.Value;
                Vertex vB = nodeB.Value;

                //  c. if the predecessor of Va is equal to Vb (peak of the roof), then output three
                //     straight skeleton arcs VaI, VbI and VcI where Vc is the predecessor of Va and
                //     the successor of Vb in the LAV simultaneously, and continue on the step 2
                //     NOTE: Cannot occur without circular LAV (i.e. polygon)
                 if (nodeA.Previous == nodeB)
                {
                    Vertex vC = nodeA.Previous.Value;
                    network.AddArc(vA, this);
                    network.AddArc(vB, this);
                    network.AddArc(vC, this);
                    return true;
                }

                //  Convex: d. Output two skeleton arcs of the straight skeleton VaI and VbI
                network.AddArc(vA, this);
                network.AddArc(vB, this);
                return false;
            }

            public override void ModifyLav(AngularBisectorNetwork network)
            {
                Debug.Assert(nodeA.List == nodeB.List);
                CircularLinkedList<Vertex> lav = nodeA.List;
                Vertex vA = nodeA.Value;
                Vertex vB = nodeB.Value;

                //     * Create a new Vertex V with the coordinates of the intersection I
                Vertex v = new Vertex(this.Position);

                //     * insert the new vertex into the LAV. That means connect it with the predecessor
                //       of Va and the successor of Vb in the LAV
                CircularLinkedListNode<Vertex> nodeV = new CircularLinkedListNode<Vertex>(v);
                lav.AddAfter(nodeA, nodeV);
                lav.Remove(nodeA);
                lav.Remove(nodeB);

                //     * link the new node V with the appropriate edges ea and eb (pointed to by vertices
                //       Va and Vb
                // TODO: This needs some work for start and end nodes.  What should happen?
                v.inEdge = vA.inEdge;
                v.outEdge = vB.outEdge;

                // f. for the new node V, created from I, compute:
                //     * the new angle bisector b between the line segments ea and eb, and

                // TODO: This bisector sometimes needs to be revered! But under what circumstances?
                Direction2D bisector = AngularBisector(nodeV.Value.inEdge, nodeV.Value.outEdge);

                // Determine whether the triangle A B V has an acute or obtuse angle at V
                // this is used to determine the direction of the bisector
                Triangle2D triangle = new Triangle2D(vA.Position, vB.Position, v.Position);
                if (triangle.AngleC > (Math.PI / 2.0))
                {
                    bisector = -bisector;
                }

                nodeV.Value.Bisector = new Ray2D(nodeV.Value.Bisector.Source, bisector);

                //     * the intersections of this bisector with the bisectors starting from the neighbour vertices in
                //       the LAV in the same way as in the step 1c
                //     * store the nearer intersection (if it exists) in the priority queue
                network.EnqueueNearestBisectorIntersection(lav, nodeV);
            }
        }

        private class SplitEvent : Intersection
        {
            public readonly CircularLinkedListNode<Vertex> nodeV;

            public SplitEvent(Point2D position, CircularLinkedListNode<Vertex> node) :
                base(position)
            {
                this.nodeV = node;
            }

            private Vertex V
            {
                get { return nodeV.Value;  }
            }

            public override bool Processed
            {
                get
                {
                    return V.processed;
                }

                set
                {
                    V.processed = value;
                }
            }

            public override bool CreateArcs(AngularBisectorNetwork network)
            {
                // 2c. do the same as in the convex case, only the meaning is a bit different, because
                //     more local peaks of the roof exist.
                //  c. if the predecessor of Va is equal to Vb (peak of the roof), then output three
                //     straight skeleton arcs VaI, VbI and VcI where Vc is the predecessor of Va and
                //     the successor of Vb in the LAV simultaneously, and continue on the step 2
                //     NOTE: Cannot occur without circular LAV (i.e. polygon)
                //if (nodeA.Previous == nodeB)
                //{
                    //Vertex vC = nodeV.Next.Value;
                    //network.AddArc(vC, this);
                    //network.AddArc(vB, this);
                    //network.AddArc(vC, this);
                    //return true;
                //}

                // 2d. Output one arc VI of the straight skeleton, where vertex/node V is the one pointed
                //     to by the intersection point I. Intersections of the split event type point to one
                //     vertex in LAV/SLAV.
                network.AddArc(V, this);
                return false;
            }

            public override void ModifyLav(AngularBisectorNetwork network)
            {
                Debug.Assert(nodeV.List != null);
                CircularLinkedList<Vertex> lav1 = nodeV.List;
                //    * Create two new nodes V1 and V2 with the same co-ordinates as the intersection point I
                CircularLinkedListNode<Vertex> nodeV1 = new CircularLinkedListNode<Vertex>(new Vertex(this.Position));
                CircularLinkedListNode<Vertex> nodeV2 = new CircularLinkedListNode<Vertex>(new Vertex(this.Position));

                //    * Search the opposite edge in SLAV sequentially
                CircularLinkedListNode<Vertex> oppositeNode = lav1.FindPair(delegate(Vertex va, Vertex vb)
                {
                    // Ignore testing againt the in and out edges of V
                    if (va == V || vb == V)
                    {
                        return false;
                    }

                    // and the candiate "opposite" line
                    Line2D oppositeLine = new Line2D(va.outEdge.source.Position, va.outEdge.target.Position);

                    // TODO: Check which way round these lines are - so the the positive side defines our zone of interest
                    Line2D oppositeBoundary1 = va.Bisector.SupportingLine.Opposite;
                    Line2D oppositeBoundary2 = vb.Bisector.SupportingLine;

                    OrientedSide sideA = oppositeBoundary1.Side(this.Position);
                    OrientedSide sideB = oppositeLine.Side(this.Position);
                    OrientedSide sideC = oppositeBoundary2.Side(this.Position);
                    //return sideA == OrientedSide.Negative && sideB == OrientedSide.Negative && sideC == OrientedSide.Negative;
                    return sideA == OrientedSide.Negative && sideB == OrientedSide.Negative && sideC == OrientedSide.Negative;
                });

                // TODO: Is this legitimate?
                if (oppositeNode == null)
                {
                    Debug.Assert(false);
                    return;
                }

                // oppositeNode is Y in the paper
                Debug.Assert(oppositeNode != null);

                //    * insert both new nodes into the SLAV (break one LAV into two parts). Vertex V1 will be
                //      interconnected between the predecessor of V and the vertex/node which is an end point of
                //      the opposite line segment. V2 will be connected between the successor of V and the vertex/
                //      node which is a starting point of the opposite line segment.  This step actually splits the
                //      polygon shape into two parts.

                // Set up nodes according to the naming conventions in the Felkel et al paper.
                CircularLinkedListNode<Vertex> nodeY = oppositeNode;
                CircularLinkedListNode<Vertex> nodeX = oppositeNode.Next;
                CircularLinkedListNode<Vertex> nodeM = nodeV.Previous;
                CircularLinkedListNode<Vertex> nodeN = nodeV.Next;

                // The LinkedList is split into two parts.  The first part remains in the original list, 
                // the second part is placed into a new list.
                CircularLinkedList<Vertex> lav2 = network.CreateLav();

                // We search the list for either M or Y, whichever comes first. This tells us which
                // algorithm to use to split the lav in two.
                CircularLinkedListNode<Vertex> found = lav1.FindNode(node => node == nodeM || node == nodeY);

                lav1.Remove(nodeV);

                

                // Splice nodes from lav1 into lav2
                Debug.Assert(found != null);
                if (found == nodeY)
                {
                    // Splice two sections of lav1 into lav2 with V1 and V2
                    // TODO: We have four nodes in a linked list defining two ranges First--->Y and N--->Last
                    //       These ranges may be non-overlapping, or may be overlapping in some way. e.g. Y may come after N.
                    //       1) Find none overlapping range or ranges
                    //       2) Copy the range(s) to lav2
                    //       3) Insert nodeV2 into the correct place in lav2 after Y

                    // Another alternative 2

                    // 1. Is the break in the list between N and Y?
                    bool continuousNY = null != lav1.FindNodeFrom(nodeN, node => node == nodeY);
                    if (continuousNY)
                    {
                        lav2.AddLast(nodeV2);
                        lav2.SpliceLast(nodeN, nodeY);
                        lav2.IsCircular = true;
                    }
                    else // !continuousNY
                    {
                        lav2.SpliceLast(lav1.First, nodeY);
                        lav2.AddLast(nodeV2);
                        lav2.SpliceLast(nodeN, lav1.Last);
                        lav2.IsCircular = lav1.IsCircular;
                    }

                    // Alternative approach

                    //// 1. Remember whether lav1 is circular
                    //bool lav1Circularity = lav1.IsCircular;

                    //// 1a. Determine whether lav2 should be circular- by whether it is continuous between nodeN and nodeY
                    //bool continuousNY = null != lav1.FindNodeFrom(nodeN, node => node == nodeY);
                    //bool lav2Circularity = lav1Circularity || continuousNY;

                    //// 2. Make lav1 circular, so we can safely iterate from N to Y irrespective of the location of the list head
                    //lav1.IsCircular = true;

                    //// 3. Add nodeV2 into lav2
                    //lav2.AddLast(nodeV2);

                    //// 4. Splice from N to Y into Lav2
                    //lav2.SpliceLast(nodeN, nodeY);

                    //// 5. Restore the circularity of lav1 and lav2
                    //lav1.IsCircular = lav1Circularity;
                    //lav2.IsCircular = lav2Circularity;

                    // X--->M->V1
                    if (lav1.Count > 0) // <--- Is this needed?
                    {
                        Debug.Assert(lav1.First == nodeX);
                        Debug.Assert(lav1.Last == nodeM);
                        lav1.AddLast(nodeV1);
                        lav1.IsCircular = true;
                    }
                }
                else
                {
                    Debug.Assert(found == nodeM);
                    // Splice one section of lav1 into lav2
                    // N--->Y->V2
                    lav2.SpliceFirst(nodeN, nodeY);
                    lav2.AddLast(nodeV2);

                    // First--->M->V2->X--->Last
                    if (lav1.Count > 0)
                    {
                        Debug.Assert(nodeM.Next == nodeX);
                        lav1.AddAfter(nodeM, nodeV1);
                    }
                }

                //    * link the new nodes V1 and V2 with the appropriate edges
                nodeV1.Value.inEdge  = V.inEdge;
                nodeV1.Value.outEdge = nodeY.Value.outEdge;

                nodeV2.Value.inEdge = nodeY.Value.outEdge;
                nodeV2.Value.outEdge = V.outEdge;

                // f. for both nodes V1 and V2:

                //     * compute the new angle bisectors betwenne the line segment linked to them is step 2e
                nodeV1.Value.Bisector = new Ray2D(nodeV1.Value.Bisector.Source, AngularBisector(nodeV1.Value.inEdge, nodeV1.Value.outEdge));
                nodeV2.Value.Bisector = new Ray2D(nodeV2.Value.Bisector.Source, AngularBisector(nodeV2.Value.inEdge, nodeV2.Value.outEdge));

                //     * compute the intersections of these bisectors with the bisectors starting at their neighbour
                //       vertices according to the LAVs (e.g. at points N and Y and M and X in fig 6a.), the same
                //       way as in step 1c. New intersection points of both types may occur
                //     * store the nearest intersection into the priority queue
                //       TODO: [ Does this mean the nearest for V1 and the nearest for V2, or only the nearest of them both? ]
                network.EnqueueNearestBisectorIntersection(lav1, nodeV1);
                network.EnqueueNearestBisectorIntersection(lav2, nodeV2);
            }
        }

        public class BisectorVertex : VertexBase
        {
            private readonly Point2D position;
            private Direction2D? direction;

            public BisectorVertex(Point2D position)
            {
                this.position = position;
                this.direction = null;
            }

            public BisectorVertex(BisectorVertex other) :
                base(other)
            {
                this.position = other.position;
                this.direction = other.direction;
            }

            public Point2D Position
            {
                get { return position; }
            }

            public Direction2D? Direction
            {
                get { return direction; }
                set { direction = value;  }
            }

            public override object Clone()
            {
                return new BisectorVertex(this);
            }

            public override int GetHashCode()
            {
                // Deliberately does not use the direction field
                return position.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                // Deliberately does not use the direction field
                BisectorVertex vertex = obj as BisectorVertex;
                return vertex != null && position.Equals(vertex.position);
            }
        }

        private readonly List<CircularLinkedList<Vertex>> slav = new List<CircularLinkedList<Vertex>>();
        private readonly PriorityQueueDictionary<double, Intersection> queueDictionary;
        private readonly Mesh<BisectorVertex, EdgeBase, FaceBase> skeleton = new Mesh<BisectorVertex, EdgeBase, FaceBase>();

        public static Mesh<BisectorVertex, EdgeBase, FaceBase> CreateFromPolyline(IEnumerable<Point2D> points)
        {
            AngularBisectorNetwork abn = new AngularBisectorNetwork(points, false);
            return abn.skeleton;
        }

        public static Mesh<BisectorVertex, EdgeBase, FaceBase> CreateFromPolygon(IEnumerable<Point2D> points)
        {
            AngularBisectorNetwork abn = new AngularBisectorNetwork(points, true);
            return abn.skeleton;
        }

        public static Mesh<BisectorVertex, EdgeBase, FaceBase> CreateFromPolylineDoubleSided(IEnumerable<Point2D> points)
        {
            // Avoids zero length edges at the ends of the polyline
            IEnumerable<Point2D> doubleSidedPolyline = points.Concat(points.Reverse().Skip(1).Take(points.Count() - 2));
            AngularBisectorNetwork abn = new AngularBisectorNetwork(doubleSidedPolyline, true);
            return abn.skeleton;
        }

        private AngularBisectorNetwork(IEnumerable<Point2D> points, bool closed)
        {
            queueDictionary = PriorityQueueDictionary<double, Intersection>.CreateLowFirstOut();

            // TODO: Throw an exception on duplicate points
            foreach (Point2D point in points)
            {
                skeleton.Add(new BisectorVertex(point));
            }

            // 1. Initialization
            //  a. Organize the given vertices into one double connected list of active vertices (LAV)
            //     stored in the SLAV. The vertices in LAV are all active at the moment.
            CircularLinkedList<Vertex> lav = CreateLav(closed);
            foreach (Point2D point in points)
            {
                lav.AddLast(new Vertex(point));
            }

            // b. For each vertex Vi in LAV add the pointer to to incident edges ei-1 = Vi-1 Vi
            //    band ei = Vi Vi+1 and compute the vertex angle bisector (ray) bi
            InitializeEdges(lav);
            InitalizeBisectors(lav);

            // c. For each vertex Vi compute the nearer intersection of the bisector bi with the adjacent
            //    vertex bisectors bi-1 and bi+1 starting at the neighbouring vertices Vi-1 Vi+1 and (if
            //    it exists) store it into a priority queue according to the distance to the line L(ei) which
            //    holds edge ei. For each intersection point store references to Va and Vb, the two origins of
            //    the bisectors which have created the intersection point.
            FindFirstIntersections(lav);

            // 2. While the priority queue with the intersection points is not empty process the intersection
            //    points to find futher intersections, until all intersecting bisectors have been processed.
            ProcessIntersections();

            // Add infinite rays from any unprocesseed points remaining
            // TODO: foreach lav in slav
            foreach (Vertex v in lav)
            {
                if (!v.processed)
                {
                    AddTerminalRay(v);
                }
            }
        }

        private void ProcessIntersections()
        {
            while (queueDictionary.Count != 0)
            {
                //  a. Pop the intersection point I from the priority queue
                Intersection i = queueDictionary.Dequeue();

                if (i.Processed)
                {
                    continue;
                }

                //  c. if the predecessor of Va is equal to Vb (peak of the roof), then output three
                //     straight skeleton arcs VaI, VbI and VcI where Vc is the predecessor of Va and
                //     the successor of Vb in the LAV simultaneously, and continue on the step 2
                //     NOTE: Cannot occur without circular LAV (i.e. polygon)

                //  d. Output two skeleton arcs of the straight skeleton VaI and VbI
                bool peak = i.CreateArcs(this);
                if (peak)
                {
                    continue;
                }

                //  e. modify the list of active vertices
                //     * Mark the vertices Va, Vb (pointed to by I) as processed
                i.Processed = true;

                i.ModifyLav(this);

                RemoveDegenerateLists();
            }
        }

        /// <summary>
        /// Process any remaining circular LAVs containing only two vertices
        /// and generate an arc between the two remaining points
        /// </summary>
        private void RemoveDegenerateLists()
        {
            foreach (CircularLinkedList<Vertex> lav in slav)
            {
                //Debug.Assert(lav.Count >= 2);
                if (lav.IsCircular && lav.Count == 2)
                {
                    AddArc(lav.First.Value, lav.Last.Value);
                    foreach (Vertex vertex in lav)
                    {
                        vertex.processed = true;
                    }
                    lav.Clear();
                }
            }
        }

        private void FindFirstIntersections(CircularLinkedList<Vertex> lav)
        {
            lav.ForEachNode(node => EnqueueNearestBisectorIntersection(lav, node));
        }

        private static void InitializeEdges(CircularLinkedList<Vertex> lav)
        {
            // Create a dummy incident edge into the first node from a dummy vertex collinear with
            // the first line segment. This removes special casing for the first bisector computation.
            CircularLinkedListNode<Vertex> firstNode = lav.First;
            Debug.Assert(firstNode != null);
            Debug.Assert(firstNode.Next != null);
            Vector2D firstEdgeDirection = firstNode.Next.Value.Position - firstNode.Value.Position;
            Vertex prefixVertex = new Vertex(firstNode.Value.Position - firstEdgeDirection);
            firstNode.Value.inEdge = new Edge(prefixVertex, firstNode.Value);

            // Create a dummy indicent edge from the last node to a dummy vertex collinear with the last
            // line segment. This removes special casing for the last bisector computation.
            CircularLinkedListNode<Vertex> lastNode = lav.Last;
            Debug.Assert(lastNode != null);
            Debug.Assert(lastNode.Previous != null);
            Vector2D lastEdgeDirection = lastNode.Value.Position - lastNode.Previous.Value.Position;
            Vertex suffixVertex = new Vertex(lastNode.Value.Position + lastEdgeDirection);
            lastNode.Value.outEdge = new Edge(lastNode.Value, suffixVertex);

            // Create edges between successive pairs of the list
            lav.ForEachPair(delegate(Vertex va, Vertex vb)
            {
                va.outEdge = new Edge(va, vb);
                vb.inEdge = va.outEdge;
            });
        }

        private static void InitalizeBisectors(IEnumerable<Vertex> lav)
        {
            foreach (Vertex vertex in lav)
            {
                Direction2D bisectorDirection = AngularBisector(vertex.inEdge, vertex.outEdge);
                vertex.Bisector = new Ray2D(vertex.Bisector.Source, bisectorDirection);
            }
        }

        /// <summary>
        /// Compute the intersections of the bisector of the supplied node with the bisectors starting
        /// from the neightbour vertices in the LAV. Store the nearest intersection (if it exists) in the
        /// priority queue.
        /// </summary>
        /// <param name="lav">The List of Active Vertices</param>
        /// <param name="node">The node at the origin of the bisector to be intersected.</param>
        private void EnqueueNearestBisectorIntersection(CircularLinkedList<Vertex> lav, CircularLinkedListNode<Vertex> node)
        {
            // TODO: Retrieve the the lav using node.List
            // Find intersection with previous bisector
            // TODO: For some unknown reason the ternary operator does not work for the following expression
            Point2D? previousIntersection = null;
            if (node.Previous != null)
            {
                previousIntersection = Intersector.Intersect(node.Value.Bisector, node.Previous.Value.Bisector);
            }

            // Find intersection with next bisector
            // TODO: For some unknown reason the ternary operator does not work for the following expression
            Point2D? nextIntersection = null;
            if (node.Next != null)
            {
                nextIntersection = Intersector.Intersect(node.Value.Bisector, node.Next.Value.Bisector);
            }

            // Find intersection with "opposite" bisector at point B
            Point2D? oppositeIntersection = null;
            if (IsReflex(node.Value))
            {
                oppositeIntersection = OppositeIntersection(lav, node.Value);
            }

            double? previousDistance2 = previousIntersection.HasValue ? (previousIntersection.Value - node.Value.Bisector.Source).Magnitude2 : (double?) null;
            double? nextDistance2 = nextIntersection.HasValue ? (nextIntersection.Value - node.Value.Bisector.Source).Magnitude2 : (double?) null;
            double? oppDistance2 = oppositeIntersection.HasValue ? (oppositeIntersection.Value - node.Value.Bisector.Source).Magnitude2 : (double?)  null;


            if (IsMinimum(previousDistance2, nextDistance2, oppDistance2))
            {
                EnqueuePreviousIntersection(node, previousIntersection.Value);
            }
            else if (IsMinimum(nextDistance2, previousDistance2, oppDistance2))
            {
                EnqueueNextIntersection(node, nextIntersection.Value);
            }
            else if (IsMinimum(oppDistance2, previousDistance2, nextDistance2))
            {
                EnqueueOppIntersection(node, oppositeIntersection.Value);
            }
        }

        /// <summary>
        /// Is a less than or equal to both b and c. Null values are discounted.
        /// </summary>
        /// <param name="a">The value to be tested</param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsMinimum(double? a, double? b, double? c)
        {
            return a.HasValue && (!b.HasValue || (a.Value <= b.Value)) && (!c.HasValue || (a.Value <= c.Value));
        }

        /// <summary>
        /// Determination of the co-ordinates of point B
        ///
        /// Point B can be characterized as having the same perpendicular distance to the straight line carrying
        /// the "opposite" line segment to the vertex V and from both straight lines containing the line segments
        /// starting at the vertex V. We have to find such an "opposite" line segment.
        /// 
        /// We traverse all line segments e of the original polygon [polyline] and test them whether they can be
        /// "opposite" line segments. Unfortunately, a simple test of the intersection between a bisector starting
        /// at V and the currently tested line segment cannot be used.  We have to test the intersection with the
        /// line holding the edge ei and by the bisectors bi and bi+1 leading from vertices at both ends of this
        /// line segment.
        /// </summary>
        /// <param name="lav"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        private static Point2D? OppositeIntersection(CircularLinkedList<Vertex> lav, Vertex vertex)
        {
            double minDistance2 = Double.MaxValue;
            Point2D? minIncenter = null;

            // Project the supporting the edges starting at V (node) as rays
            Ray2D inRay = new Ray2D(vertex.inEdge.target.Position, new Direction2D(vertex.inEdge.Vector));
            Ray2D outRay = new Ray2D(vertex.outEdge.source.Position, new Direction2D(-vertex.outEdge.Vector));

            // Ignore cases where the in and out edges are collinear.
            if (!inRay.SupportingLine.IsParallelTo(outRay.SupportingLine))
            {
                lav.ForEachPair(delegate(Vertex oppA, Vertex oppB)
                {
                    // Ignore testing againt the in and out edges of V
                    if (oppA == vertex || oppB == vertex)
                    {
                        return;
                    }

                    // Simple intersection test between the bisector starting at V and the (whole) line containing the
                    // currently tested line segment ei rejects the line segments laying "behind" the vertex V. We then compute
                    // the co-ordinates of the candidate point Bi as the intersection between the bisector at V and the axis [bisector]
                    // of the angle between of the edges starting at V and the tested line segment ei. Simple check should be performed
                    // to exclude the case where one of the line segments starting at V us parallel to ei. The resulting point B
                    // is selected from all the candidates Bi as the nearest point to the vertex V

                    // TODO: Performance: How much code can be factored out of this loop?

                    // TODO: Do we need to prevent testing against the in and out edges of V?
                    // and the candiate "opposite" line
                    Line2D oppositeLine = new Line2D(oppA.outEdge.source.Position, oppA.outEdge.target.Position);

                    // TODO: Need to use a ray-line intersection here, where inLine and outLine are
                    //       replaced by rays which start at V and project in alignment with the two incident edges
                    Point2D? inOppIntersection = Intersector.Intersect(inRay, oppositeLine);
                    Point2D? outOppIntersection = Intersector.Intersect(outRay, oppositeLine);

                    if (inOppIntersection.HasValue && outOppIntersection.HasValue)
                    {
                        Triangle2D triangle = new Triangle2D(vertex.Position, inOppIntersection.Value, outOppIntersection.Value);
                        if (!triangle.IsDegenerate)
                        {
                            Point2D incenter = triangle.Incenter;

                            // Is the incenter in the zone bounded by the oppositeLine and bisectors at the beginning
                            // and end of the segment embedded within it?

                            // TODO: Check which way round these lines are - so the the positive side defines our zone of interest
                            Line2D oppositeBoundary1 = oppA.Bisector.SupportingLine.Opposite;
                            Line2D oppositeBoundary2 = oppB.Bisector.SupportingLine;

                            OrientedSide sideA = oppositeBoundary1.Side(incenter);
                            OrientedSide sideB = oppositeLine.Side(incenter);
                            OrientedSide sideC = oppositeBoundary2.Side(incenter);

                            if (sideA == OrientedSide.Negative && sideB == OrientedSide.Negative && sideC == OrientedSide.Negative)
                            {
                                double distance2 = (incenter - vertex.Position).Magnitude2;
                                if (distance2 > 0.0 && distance2 < minDistance2)
                                {
                                    minDistance2 = distance2;
                                    minIncenter = incenter;
                                }
                            }
                        }
                    }
                });
            }
            return minIncenter;
        }

        private void EnqueuePreviousIntersection(CircularLinkedListNode<Vertex> node, Point2D position)
        {
            double distanceToSupportingLine = DistanceToSupportingLine(node, position); // TODO: Refactor
            EdgeEvent intersection = new EdgeEvent(position, node.Previous, node);
            queueDictionary.Enqueue(distanceToSupportingLine, intersection);
        }

        private void EnqueueNextIntersection(CircularLinkedListNode<Vertex> node, Point2D position)
        {
            double distanceToSupportingLine = DistanceToSupportingLine(node, position); // TODO: Refactor
            EdgeEvent intersection = new EdgeEvent(position, node, node.Next);
            queueDictionary.Enqueue(distanceToSupportingLine, intersection);
        }

        private void EnqueueOppIntersection(CircularLinkedListNode<Vertex> node, Point2D position)
        {
            double distanceToSupportingLine = DistanceToSupportingLine(node, position); // TODO: Refactor
            SplitEvent intersection = new SplitEvent(position, node);
            queueDictionary.Enqueue(distanceToSupportingLine, intersection);
        }

        private static double DistanceToSupportingLine(CircularLinkedListNode<Vertex> node, Point2D position)
        {
            // Take the outEdge for all except the last point
            Edge edge = node.Value.outEdge ?? node.Value.inEdge;
            Debug.Assert(edge != null);
            Line2D supportingLine = new Line2D(edge.source.Bisector.Source, edge.target.Bisector.Source);
            double distanceToSupportingLine = Math.Abs(supportingLine.DistanceTo(position));
            return distanceToSupportingLine;
        }

        /// <summary>
        /// Return true if the vertex is reflex - that is if the angle between
        /// its in-edge and out-edge is greater than Pi (180°) on the side of interest
        /// </summary>
        /// <param name="vertex">The vertex to be tested for reflexivity</param>
        /// <returns>true if the vertex is reflex. false if the vertext is convex or straight.</returns>
        private static bool IsReflex(Vertex vertex)
        {
            Edge inEdge = vertex.inEdge;
            Edge outEdge = vertex.outEdge;
            // TODO: Refactor to use Edge.Vector
            Vector2D v1 = inEdge.target.Position - inEdge.source.Position;
            Vector2D v2 = outEdge.target.Position - outEdge.source.Position;
            double det = v1.Determinant(v2);
            return  det > 0.0; 
        }

        /// <summary>
        /// Given two Edges, determine the angular bisector direction between them that lies
        /// in the anticlockwise angle from edge1 to edge2.
        /// </summary>
        /// <param name="edge1">The first edge</param>
        /// <param name="edge2">The second edge</param>
        /// <returns>A Direction2D that bisects the angle between edge1 and edge2</returns>
        private static Direction2D AngularBisector(Edge edge1, Edge edge2)
        {
            // TODO: Refactor to use Edge.Vector
            Vector2D v1 = edge1.target.Position - edge1.source.Position;
            Vector2D v2 = edge2.target.Position - edge2.source.Position;
            return VectorGeometry.Bisector(ref v1, ref v2);
        }

        /// <summary>
        /// Create a single arc of the angular bisector network and store it in the representation
        /// of the bisector network
        /// </summary>
        /// <param name="source">The source Vertex of the arc</param>
        /// <param name="target">The target Intersection of the arc</param>
        private void AddArc(Vertex source, Intersection target)
        {
            BisectorVertex sourceVertex = new BisectorVertex(source.Position);
            BisectorVertex targetVertex = new BisectorVertex(target.Position);
            sourceVertex = skeleton.Add(sourceVertex);
            targetVertex = skeleton.Add(targetVertex);
            skeleton.AddEdge(sourceVertex, targetVertex);
        }

        private void AddArc(Vertex source, Vertex target)
        {
            BisectorVertex sourceVertex = new BisectorVertex(source.Position);
            BisectorVertex targetVertex = new BisectorVertex(target.Position);
            sourceVertex = skeleton.Add(sourceVertex);
            targetVertex = skeleton.Add(targetVertex);
            skeleton.AddEdge(sourceVertex, targetVertex);
        }

        private void AddTerminalRay(Vertex source)
        {
            BisectorVertex vertex = skeleton.Find(new BisectorVertex(source.Position));
            Debug.Assert(vertex != null);
            vertex.Direction = source.Direction;
        }

        private CircularLinkedList<Vertex> CreateLav()
        {
            return CreateLav(true);
        }

        private CircularLinkedList<Vertex> CreateLav(bool isCircular)
        {
            CircularLinkedList<Vertex> lav = new CircularLinkedList<Vertex> { IsCircular = isCircular } ;
            slav.Add(lav);
            return lav;
        }
    }
}
