using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Wintellect.PowerCollections;

using Numeric;

namespace Geometry.RedBlue
{
    public class RedBlueIntersector
    {
        private readonly double[] redMinMax  = new double[4];
        private readonly double[] blueMinMax = new double[4];
        private readonly List<Segment> mergedSegments = new List<Segment>();
        private readonly Dictionary<Segment2D, Segment> inToOut = new Dictionary<Segment2D, Segment>();

        /// <summary>
        /// Create a RedBlueIntersector from two sequences of line segments - one red and one blue.
        /// The red segments should not self-intersect except at segment ends.  The blue segments
        /// should not intersect except at segment ends.
        /// </summary>
        /// <param name="red">A sequence of red segments.</param>
        /// <param name="blue">A sequence of blue segments.</param>
        /// <exception cref="ArgumentException">Thrown if the input segment sets contain same-colour self intersections.</exception>
        public RedBlueIntersector(IEnumerable<Segment2D> red, IEnumerable<Segment2D> blue)
        {
            SetMinMax(red.FirstOrDefault(), redMinMax);
            foreach (Segment2D segment2d in red)
            {
                UpdateMinMax(segment2d, redMinMax);
                Segment segment = new Segment(segment2d, true);
                mergedSegments.Add(segment);
                inToOut.Add(segment2d, segment);
            }

            SetMinMax(blue.FirstOrDefault(), blueMinMax);
            foreach (Segment2D segment2d in blue)
            {
                UpdateMinMax(segment2d, blueMinMax);
                Segment segment = new Segment(segment2d, false);
                mergedSegments.Add(segment);
                inToOut.Add(segment2d, segment);
            }

            mergedSegments.Sort(new SegmentXComparer());
            try
            {
                CalcIntersections();
            }
            catch (SelfIntersectingLineSegmentsException e)
            {
                throw new ArgumentException("Line segments contain same-colour self intersections", e);
            }
        }

        // TODO: Need to handle segments with MidDegen set correctly in here

        /// <summary>
        /// Returns a sequence of all of the segments, of either colour, which are involved in 
        /// polchromatic intersections - those which intersect at purple intersection points.
        /// </summary>
        public IEnumerable<Segment2D> AllIntersectingSegments
        {
            get { return inToOut.Keys.SelectMany(s => IntersectingSegments(s)).Distinct(); }
        }

        /// <summary>
        /// Returns a sequence of all of the red segments, which are involved in 
        /// polchromatic intersections - those which intersect at purple intersection points.
        /// </summary>
        public IEnumerable<Segment2D> AllRedIntersectingSegments
        {
            get { return inToOut.Keys.SelectMany(s => RedIntersectingSegments(s)).Distinct(); }
        }

        /// <summary>
        /// Returns a sequence of all of the blue segments, which are involved in 
        /// polchromatic intersections - those which intersect at purple intersection points.
        /// </summary>
        public IEnumerable<Segment2D> AllBlueIntersectingSegments
        {
            get { return inToOut.Keys.SelectMany(s => BlueIntersectingSegments(s)).Distinct(); }
        }

        /// <summary>
        /// Returns all of the segments with the opposite colour to the supplied segment which
        /// have the opposite colour - those which intersect at purple intersection points.
        /// </summary>
        /// <param name="segment">The query segment</param>
        /// <returns>A sequence of segments which intersect with the query segment</returns>
        public IEnumerable<Segment2D> IntersectingSegments(Segment2D segment)
        {
            Debug.Assert(inToOut.ContainsKey(segment));
            Segment seg = inToOut[segment];
            return IntersectingSegments(seg, s => (s.IsRed != seg.IsRed));
        }

        /// <summary>
        /// Find all of the red segments which intersect the supplied blue segment.
        /// If a red segment is supplied an empty sequence is returned.
        /// </summary>
        /// <param name="blueSegment">A blue segment</param>
        /// <returns>All of the red segments which intersect the blue segment.</returns>
        public IEnumerable<Segment2D> RedIntersectingSegments(Segment2D blueSegment)
        {
            Debug.Assert(inToOut.ContainsKey(blueSegment));
            Segment segment = inToOut[blueSegment];
            return !segment.IsRed ? IntersectingSegments(segment, s => s.IsRed) : Enumerable.Empty<Segment2D>();
        }

        /// <summary>
        /// Find all of the red segments which intersect the supplied blue segment.
        /// If a blue segment is supplied an empty sequence is returned.
        /// </summary>
        /// <param name="redSegment">A red segment</param>
        /// <returns>All of the blue segments which intersect the red segment.</returns>
        public IEnumerable<Segment2D> BlueIntersectingSegments(Segment2D redSegment)
        {
            Debug.Assert(inToOut.ContainsKey(redSegment));
            Segment segment = inToOut[redSegment];
            return segment.IsRed ? IntersectingSegments(segment, s => !s.IsRed) : Enumerable.Empty<Segment2D>();
        }

        /// <summary>
        /// Find all of the segments which intersect the supplied segment and match the supplied predicate function.
        /// </summary>
        /// <param name="segment">The query segment</param>
        /// <param name="predicate">A predicate filter function</param>
        /// <returns>A sequence of Segmet2D objects which intersect the segment and meet the predicate condition</returns>
        private static IEnumerable<Segment2D> IntersectingSegments(Segment segment, Func<Segment, bool> predicate)
        {
            return IntersectingSegments(segment).Where(predicate)
                .Select(s => FullSegmentExtent(s));
        }

        /// <summary>
        /// Given a Segment, trace its continuation forwards and backwards to locate its original
        /// endpoints. Return a Segment2D with these endpoints.
        /// </summary>
        /// <param name="segment"></param>
        private static Segment2D FullSegmentExtent(Segment segment)
        {
            Segment forward = segment;
            while (forward.ContNext != null)
            {
                forward = forward.ContNext;
            }

            Segment backward = segment;
            while (backward.ContPrev != null)
            {
                backward = backward.ContPrev;
            }

            return new Segment2D(backward.startPt.Position, forward.endPt.Position);
        }

        /// <summary>
        /// Find all of the segments which intersect the supplied segment.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        private static IEnumerable<Segment> IntersectingSegments(Segment segment)
        {
            while (segment != null)
            {
                foreach (Segment intersector in segment.IntersectingSegments)
                {
                    yield return intersector;
                }
                segment = segment.ContNext;
            }
        }

        private void CalcIntersections()
        {
            Debug.Assert(mergedSegments != null);
            Segment[] contSegs = new Segment[2];
            ClearIntersections();
            IEnumerable<FPoint> eventList = CreateEventList(mergedSegments);
            BundleTree rBundleTree = InitSentinels();
            int imerge = 0;
            foreach (FPoint sweepEvent in eventList)
            {
                Bundle bClose = rBundleTree.FindSmBAbove(sweepEvent);
                Bundle bRedTeq = bClose.PurpleDown.PurpleDown;
                if (bRedTeq.PurpleDown != null)
                {
                    for (bRedTeq = rBundleTree.separateEq(bRedTeq, sweepEvent, true, false); Witnessed(bRedTeq.PurpleDown, bRedTeq, sweepEvent); SwapMerge(bRedTeq.PurpleDown, bRedTeq, rBundleTree))
                    {
                    }
                    rBundleTree.separateEq(bRedTeq.PurpleDown, sweepEvent, true, true);
                    if (Witnessed(bRedTeq.PurpleDown, bRedTeq, sweepEvent))
                    {
                        SwapMerge(bRedTeq.PurpleDown, bRedTeq, rBundleTree);
                    }
                    if (bRedTeq.PurpleUp.IsRed)
                    {
                        bClose = bRedTeq.PurpleUp;
                    }
                    if (bClose.PurpleUp != null)
                    {
                        for (; Witnessed(bClose, bClose.PurpleUp, sweepEvent); SwapMerge(bClose, bClose.PurpleUp, rBundleTree)) { }
                        rBundleTree.separateEq(bClose.PurpleUp, sweepEvent, true, false);
                        if (Witnessed(bClose, bClose.PurpleUp, sweepEvent))
                        {
                            SwapMerge(bClose, bClose.PurpleUp, rBundleTree);
                        }
                    }
                    if (bRedTeq.PurpleUp.IsRed)
                    {
                        rBundleTree.GroupBundles(bRedTeq, true);
                    }
                    for (bRedTeq = rBundleTree.separateEq(bRedTeq, sweepEvent, false, false); Witnessed(bRedTeq.PurpleDown, bRedTeq, sweepEvent); SwapMerge(bRedTeq.PurpleDown, bRedTeq, rBundleTree))
                    {
                    }
                    rBundleTree.separateEq(bRedTeq.PurpleDown, sweepEvent, false, true);
                    if (Witnessed(bRedTeq.PurpleDown, bRedTeq, sweepEvent))
                    {
                        SwapMerge(bRedTeq.PurpleDown, bRedTeq, rBundleTree);
                    }
                    if (bRedTeq.PurpleUp.IsRed)
                    {
                        rBundleTree.GroupBundles(bRedTeq, true);
                    }
                }
                bClose = rBundleTree.FindLgBBelow(sweepEvent);
                Bundle bRedBeq = bClose.PurpleUp.PurpleUp;
                if (bRedBeq.PurpleUp != null)
                {
                    for (bRedBeq = rBundleTree.separateEq(bRedBeq, sweepEvent, false, true); Witnessed(bRedBeq, bRedBeq.PurpleUp, sweepEvent); SwapMerge(bRedBeq, bRedBeq.PurpleUp, rBundleTree)) { }
                    rBundleTree.separateEq(bRedBeq.PurpleUp, sweepEvent, false, false);
                    if (Witnessed(bRedBeq, bRedBeq.PurpleUp, sweepEvent))
                    {
                        SwapMerge(bRedBeq, bRedBeq.PurpleUp, rBundleTree);
                    }
                    if (bRedBeq.PurpleDown.IsRed)
                    {
                        bClose = bRedBeq.PurpleDown;
                    }
                    if (bClose.PurpleDown != null)
                    {
                        for (; Witnessed(bClose.PurpleDown, bClose, sweepEvent); SwapMerge(bClose.PurpleDown, bClose, rBundleTree)) { }
                        rBundleTree.separateEq(bClose.PurpleDown, sweepEvent, false, true);
                        if (Witnessed(bClose.PurpleDown, bClose, sweepEvent))
                        {
                            SwapMerge(bClose.PurpleDown, bClose, rBundleTree);
                        }
                    }
                    if (bRedBeq.PurpleDown.IsRed)
                    {
                        rBundleTree.GroupBundles(bRedBeq, false);
                    }
                    for (bRedBeq = rBundleTree.separateEq(bRedBeq, sweepEvent, true, true); Witnessed(bRedBeq, bRedBeq.PurpleUp, sweepEvent); SwapMerge(bRedBeq, bRedBeq.PurpleUp, rBundleTree)) { }
                    rBundleTree.separateEq(bRedBeq.PurpleUp, sweepEvent, true, false);
                    if (Witnessed(bRedBeq, bRedBeq.PurpleUp, sweepEvent))
                    {
                        SwapMerge(bRedBeq, bRedBeq.PurpleUp, rBundleTree);
                    }
                    if (bRedBeq.PurpleDown.IsRed)
                    {
                        rBundleTree.GroupBundles(bRedBeq, false);
                    }
                }
                BundleTree plusTree = rBundleTree.Split(sweepEvent, true);
                BundleTree minusTree = rBundleTree;
                BundleTree oldEqTree = minusTree.Split(sweepEvent, false);
                FindContSegs(oldEqTree, contSegs, sweepEvent);
                rBundleTree = new BundleTree();
                imerge = MakeNewEqTree(rBundleTree, mergedSegments, imerge, contSegs, sweepEvent);
                RecordEndIntersections(oldEqTree, rBundleTree);
                rBundleTree.Merge(plusTree, true);
                rBundleTree.Merge(minusTree, false);
            }
        }

        private static void RecordEndIntersections(BundleTree oldEqTree, BundleTree newEqTree)
        {
            int intCount = -1;
            SingleLL intptr = null;
            Segment prev = null;
            for (Bundle bptr = oldEqTree.Top; bptr != null; bptr = bptr.PurpleDown)
            {
                for (Segment sptr = bptr.Top; sptr != null; sptr = sptr.Down)
                {
                    intCount++;
                    if (intptr == null)
                    {
                        intptr = new SingleLL(sptr, null);
                        intptr.next = intptr;
                    }
                    else
                    {
                        intptr.next = new SingleLL(sptr, intptr.next);
                        intptr = intptr.next;
                    }
                }

            }

            for (Bundle bptr = newEqTree.Bottom; bptr != null; bptr = bptr.PurpleUp)
            {
                for (Segment sptr = bptr.Bottom; sptr != null; sptr = sptr.Up)
                {
                    intCount++;
                    if (intptr == null)
                    {
                        intptr = new SingleLL(sptr, null);
                        intptr.next = intptr;
                    }
                    else
                    {
                        intptr.next = new SingleLL(sptr, intptr.next);
                        intptr = intptr.next;
                    }
                }

            }

            if (intCount <= 0)
            {
                return;
            }
            Debug.Assert(intptr != null);
            intptr = intptr.next;
            for (Bundle bptr = oldEqTree.Top; bptr != null; bptr = bptr.PurpleDown)
            {
                for (Segment sptr = bptr.Top; sptr != null; sptr = sptr.Down)
                {
                    sptr.AddEndIntersections(intptr, intCount);
                    if (prev != null && prev.IsSameSeg(sptr))
                    {
                        prev.AddLIntersection(sptr);
                        sptr.AddLIntersection(prev);
                    }
                    intptr = intptr.next;
                    prev = sptr;
                }

            }

            for (Bundle bptr = newEqTree.Bottom; bptr != null; bptr = bptr.PurpleUp)
            {
                for (Segment sptr = bptr.Bottom; sptr != null; sptr = sptr.Up)
                {
                    sptr.AddStartIntersections(intptr, intCount);
                    intptr = intptr.next;
                }
            }
        }

        private static void FindContSegs(BundleTree oldEqTree, Segment[] contSegs, FPoint sweepEvent)
        {
            int i = 0;
            contSegs[0] = null;
            contSegs[1] = null;
            for (Bundle bptr = oldEqTree.Top; bptr != null; bptr = bptr.PurpleDown)
            {
                for(Segment sptr = bptr.Top; sptr != null; sptr = sptr.Down)
                {
                    if (sptr.EndsAfter(sweepEvent))
                    {
                        contSegs[i++] = sptr.BreakSeg(sweepEvent);
                    }
                }
            }
        }

        private static int MakeNewEqTree(BundleTree tree, IList<Segment> slist, int sIdx, Segment[] contS, ISpatiallyComparable sweepEvent)
        {
            // TODO: Refactor this horrible method!
            Bundle currB = null;
            int nc = 0;
            int ic = 0;
            while (nc < 2 && contS[nc] != null)
            {
                ++nc;
            }

            int iss = sIdx;
            while (sIdx < slist.Count && slist[sIdx].CompareTo(sweepEvent) <= 0)
            {
                ++sIdx;
            }

            if (ic >= nc && iss >= sIdx)
            {
                return sIdx;
            }

            Segment currS = ic < nc && (iss >= sIdx || contS[ic].CompareTo(slist[iss]) <= 0) ? contS[ic++] : slist[iss++];

            bool currBIsRed = currS.IsRed ^ true;
            do
            {
                if(currBIsRed == currS.IsRed)
                {
                    Debug.Assert(currB != null);
                    currB.insertSegAtTop(currS);
                }
                else
                {
                    currBIsRed = currS.IsRed;
                    currB = new Bundle(currBIsRed, null, currB, currS);
                    tree.InsertBAtTop(currB);
                }
                currS.useStart = false;
                if (ic < nc || iss < sIdx)
                {
                    if (ic < nc && (iss >= sIdx || contS[ic].CompareTo(slist[iss]) <= 0))
                    {
                        currS = contS[ic++];
                    }
                    else
                    {
                        currS = slist[iss++];
                    }
                }
                else
                {
                    return sIdx;
                }
            }
            while(true);
        }

        private static void SwapMerge(Bundle lower, Bundle upper, BundleTree rBundleTree)
        {
            lower.RemovePurpleL();
            RecordIntersections(lower, upper);
            lower.AddAbovePurpleL(upper);
            if (lower.IsRed == lower.PurpleUp.IsRed)
            {
                rBundleTree.GroupBundles(lower, true);
            }
            if (upper.IsRed == upper.PurpleDown.IsRed)
            {
                rBundleTree.GroupBundles(upper, false);
            }
        }

        private static void RecordIntersections(Bundle lower, Bundle upper)
        {
            Segment lowerSegment = lower.Top;
            for (Segment upperSegment = upper.Bottom; lowerSegment != null ; upperSegment = upper.Bottom)
            {
                while (upperSegment != null)
                {
                    lowerSegment.AddNonDegenerateIntersection(upperSegment);
                    upperSegment.AddNonDegenerateIntersection(lowerSegment);
                    upperSegment = upperSegment.Up;
                }
                lowerSegment = lowerSegment.Down;
            }
        }

        private static bool Witnessed(Bundle lower, Bundle upper, ISpatiallyComparable sweepEvent)
        {
            int lComp = lower.Bottom.CompareTo(sweepEvent);
            int uComp = upper.Top.CompareTo(sweepEvent);
            bool result = lComp > uComp;
            return result;
        }

        private BundleTree InitSentinels()
        {
            BundleTree rBundleTree = new BundleTree();
            double lx = (redMinMax[0] >= blueMinMax[0] ? blueMinMax[0] : redMinMax[0]) - 1.0D;
            double ly = (redMinMax[2] >= blueMinMax[2] ? blueMinMax[2] : redMinMax[2]) - 1.0D;
            double ux = (redMinMax[1] <= blueMinMax[1] ? blueMinMax[1] : redMinMax[1]) + 1.0D;
            double uy = (redMinMax[3] <= blueMinMax[3] ? blueMinMax[3] : redMinMax[3]) + 1.0D;
            Segment[] boundary = {
                                new Segment(new FPoint(lx, ly - 1.0D), new FPoint(ux, ly - 1.0D), true),
                                new Segment(new FPoint(lx, ly), new FPoint(ux, ly), false),
                                new Segment(new FPoint(lx, uy), new FPoint(ux, uy), false),
                                new Segment(new FPoint(lx, uy + 1.0D), new FPoint(ux, uy + 1.0D), true)
                            };

            MakeNewEqTree(rBundleTree, boundary, 0, new Segment[1], new FPoint(ux + 1.0D, uy + 1.0D));
            return rBundleTree;
        }

        private static IEnumerable<FPoint> CreateEventList(IEnumerable<Segment> mergedSegments)
        {
            OrderedSet<FPoint> eventList = new OrderedSet<FPoint>();
            foreach (Segment segment in mergedSegments)
            {
                eventList.Add(segment.startPt);
                eventList.Add(segment.endPt);
            }
            return eventList;
        }

        private static void SetMinMax(Segment2D segment2d, double[] minmax)
        {
            SetMinMax(segment2d.Source, minmax);
            UpdateMinMax(segment2d.Target, minmax);
        }

        private static void SetMinMax(Point2D point, double[] minmax)
        {
            minmax[0] = point.X;
            minmax[1] = point.X;
            minmax[2] = point.Y;
            minmax[3] = point.Y;
        }

        private static void UpdateMinMax(Segment2D segment2d, double[] minmax)
        {
            UpdateMinMax(segment2d.Source, minmax);
            UpdateMinMax(segment2d.Target, minmax);
        }

        private static void UpdateMinMax(Point2D point, double[] minmax)
        {
            if (point.X < minmax[0])
            {
                minmax[0] = point.X;
            }
            if (point.X > minmax[1])
            {
                minmax[1] = point.X;
            }
            if (point.Y < minmax[2])
            {
                minmax[2] = point.Y;
            }
            if (point.Y > minmax[3])
            {
                minmax[3] = point.Y;
            }
        }

        private void ClearIntersections()
        {
            mergedSegments.ForEach(s => s.Reset());
        }
    }
}
