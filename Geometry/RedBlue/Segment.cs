using System;
using System.Collections.Generic;
using Numeric;

namespace Geometry.RedBlue
{
    internal class Segment : ISpatiallyComparable, IComparable<Segment>
    {
        //private static int totalInt; // TODO: This should be removed.
        public FPoint startPt;
        public FPoint endPt;
        private Segment up;
        private Segment down;
        private Segment contPrev;
        private Segment contNext; // This seems to be used for split segments - and may need to be followed when iterating through the segments.
        private SingleLL start_int;
        private SingleLL mid_int;
        private SingleLL end_int;
        private bool midDegen; // Used to mark degenerate segments
        private readonly bool isRed;
        public bool useStart = true;

#if DEBUG
        private static int counter = -1;
        private readonly int id;

        public int Id
        {
            get { return id; }
        }
#endif
        public bool MidDegen
        {
            get { return midDegen; }
        }

        public Segment ContPrev
        {
            get { return contPrev; }
        }

        public Segment ContNext
        {
            get { return contNext; }
        }

        public Segment Up
        {
            get { return up; }
            set
            {
                up = value;
            }
        }

        public Segment Down
        {
            get { return down; }
            set
            {
                down = value;
            }
        }

        public bool IsRed
        {
            get { return isRed; }
        }

        public Segment(Segment2D segment, bool isRed) :
            this(new FPoint(segment.Source), new FPoint(segment.Target), isRed)
        {
        }

        public Segment(FPoint a, FPoint b, bool isRed)
        {
            if (a.X < b.X || a.X == b.X && a.Y < b.Y)
            {
                startPt = a;
                endPt = b;
            }
            else
            {
                startPt = b;
                endPt = a;
            }
            this.isRed = isRed;
#if DEBUG
            ++counter;
            this.id = counter;
#endif
        }

        public Segment BreakSeg(FPoint pt)
        {
            // TODO: Make this list doubly linked...
            contNext = new Segment(pt, endPt, IsRed);
            contNext.contPrev = this;
            endPt = pt;
            return ContNext;
        }

        public double CrossProduct(FPoint pt)
        {
            return (endPt.X - startPt.X) * (pt.Y - startPt.Y) - (endPt.Y - startPt.Y) * (pt.X - startPt.X);
        }

        public int CompareTo(ISpatiallyComparable rhs)
        {
            if (rhs is Bundle)
            {
                return -((rhs as Bundle).CompareTo(this));
            }
            if (rhs is FPoint)
            {
                return CompareTo(rhs as FPoint);
            }
            if (rhs is Segment)
            {
                return CompareTo(rhs as Segment);
            }
            throw new ArgumentException();
        }

        private int CompareTo(FPoint point)
        {
            if (startPt.X > point.X)
            {
                return 1;
            }
            if (endPt.X < point.X)
            {
                return -1;
            }
            if (startPt.Y > point.Y && endPt.Y > point.Y)
            {
                return 1;
            }
            if (startPt.Y < point.Y && endPt.Y < point.Y)
            {
                return -1;
            }
            double crossp = CrossProduct(point);
            if (crossp == 0.0)
            {
                return 0;
            }
            return crossp >= 0.0 ? -1 : 1;
        }

        public int CompareTo(Segment segment)
        {
            if ((startPt.Y <= endPt.Y ? endPt.Y : startPt.Y) < (segment.startPt.Y >= segment.endPt.Y ? segment.endPt.Y : segment.startPt.Y))
            {
                return -1;
            }
            if ((startPt.Y >= endPt.Y ? endPt.Y : startPt.Y) > (segment.startPt.Y <= segment.endPt.Y ? segment.endPt.Y : segment.startPt.Y))
            {
                return 1;
            }
            double crossp;
            if (segment.useStart || useStart || IsRed == segment.IsRed)
            {
                if (startPt.X <= segment.startPt.X)
                {
                    if ((crossp = CrossProduct(segment.startPt)) == 0.0 && (crossp = CrossProduct(segment.endPt)) == 0.0)
                    {
                        return 0;
                    }
                    return crossp >= 0.0 ? -1 : 1;
                }
                if ((crossp = segment.CrossProduct(startPt)) == 0.0 && (crossp = segment.CrossProduct(endPt)) == 0.0)
                {
                    return 0;
                }
                return crossp >= 0.0 ? 1 : -1;
            }
            if (endPt.X >= segment.endPt.X)
            {
                if ((crossp = CrossProduct(segment.endPt)) == 0.0 && (crossp = CrossProduct(segment.startPt)) == 0.0)
                {
                    return 0;
                }
                return crossp >= 0.0 ? -1 : 1;
            }
            if ((crossp = segment.CrossProduct(endPt)) == 0.0 && (crossp = segment.CrossProduct(startPt)) == 0.0)
            {
                return 0;
            }
            return crossp >= 0.0 ? 1 : -1;
        }
    
        public bool IsSameSeg(Segment rhs)
        {
            return startPt.X == rhs.startPt.X && startPt.Y == rhs.startPt.Y && endPt.X == rhs.endPt.X && endPt.Y == rhs.endPt.Y;
        }

        public bool EndsAfter(FPoint ev)
        {
            return endPt.X > ev.X || endPt.X == ev.X && endPt.Y > ev.Y;
        }

        public void AddNonDegenerateIntersection(Segment seg)
        {
            mid_int = new SingleLL(seg, mid_int);
            midDegen = false;
        }
        
        public void AddEndIntersections(SingleLL intList, int numint)
        {
            end_int = intList;
        }

        public void AddStartIntersections(SingleLL intList, int numint)
        {
            start_int = intList;
        }

        public void AddLIntersection(Segment seg)
        {
            mid_int = new SingleLL(seg, mid_int);
            midDegen = true;
        }

        public void Reset()
        {
            start_int = null;
            mid_int = null;
            end_int = null;
            midDegen = false;
            Up = null;
            Down = null;
            useStart = true;
            // TODO: Reset contPrev here too
            for (; contNext != null; contNext = contNext.contNext)
            {
                endPt = contNext.endPt;
            }
        }

        /// <summary>
        /// Enumerate all of the Segments which intersect with this segment
        /// </summary>
        public IEnumerable<Segment> IntersectingSegments
        {
            get
            {
                if (start_int != null)
                {
                    for (SingleLL tmp = start_int.next; tmp != start_int; tmp = tmp.next)
                    {
                        yield return tmp.item;
                    }
                }
                if (end_int != null)
                {
                    for (SingleLL tmp = end_int.next; tmp != end_int; tmp = tmp.next)
                    {
                        yield return tmp.item;
                    }
                }
                for (SingleLL tmp = mid_int; tmp != null; tmp = tmp.next)
                {
                    yield return tmp.item;
                }
            }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj" />. Zero This instance is equal to <paramref name="obj" />. Greater than zero This instance is greater than <paramref name="obj" />. 
        /// </returns>
        /// <param name="obj">An object to compare with this instance. </param>
        /// <exception cref="T:System.ArgumentException"><paramref name="obj" /> is not the same type as this instance. </exception><filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            if (!(obj is ISpatiallyComparable))
            {
                throw new ArgumentException("Both arguments must be ISpatiallyComparable");
            }
            // TODO: Could we fall back on default comparer here?
            return CompareTo((ISpatiallyComparable) obj);
        }


    }
}
