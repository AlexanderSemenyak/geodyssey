using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utility.Collections;

namespace Geometry.RedBlue
{
    internal class Bundle : ISpatiallyComparable
    {
        private Bundle purpleUp;
        private Bundle purpleDown;
        private Segment top;
        private Segment bottom;
        private SplayTree<ISpatiallyComparable> segBundle = new SplayTree<ISpatiallyComparable>();
        private readonly bool isRed = true;

        public Bundle()
        {
        }

        public Bundle(bool isRed, Bundle purpleUp, Bundle purpleDown, Segment segment)
        {
            this.isRed = isRed;
            PurpleUp = purpleUp;
            if (purpleUp != null)
            {
                purpleUp.PurpleDown = this;
            }
            PurpleDown = purpleDown;
            if (purpleDown != null)
            {
                purpleDown.PurpleUp = this;
            }
            if (segment != null)
            {
                segBundle.Add(segment);
            }
            Top = Bottom = segment;
        }

        public Bundle PurpleDown
        {
            get { return purpleDown; }
            set { purpleDown = value; }
        }

        public Bundle PurpleUp
        {
            get { return purpleUp; }
            set { purpleUp = value; }
        }

        public Segment Top
        {
            get { return top; }
            set
            {
                top = value;
            }
        }

        public Segment Bottom
        {
            get { return bottom; }
            set
            {
                bottom = value;
            }
        }

        public bool IsRed
        {
            get { return isRed; }
        }

        public Bundle Split(Segment seg, bool keepWSm)
        {
            Bundle upperBundle = new Bundle(IsRed, PurpleUp, this, null);
            upperBundle.segBundle = segBundle.SplitAt(seg, keepWSm);
            if (upperBundle.segBundle.Root == null)
            {
                return upperBundle;
            }

            if (segBundle.Root == null)
            {
                upperBundle.Top = Top;
                upperBundle.Bottom = Bottom;
                Top = null;
                Bottom = null;
                return upperBundle;
            }

            upperBundle.Top = Top;
            if (segBundle.Right == null && ((Segment) segBundle.Root).Up != null)
            {
                Top = (Segment) segBundle.Root;
            }
            else
            {
                Top = ((Segment) upperBundle.segBundle.Root).Down;
            }
            upperBundle.Bottom = Top.Up;
            upperBundle.Bottom.Down = null;
            Top.Up = null;
            return upperBundle;
        }

        public void Merge(Bundle rhs, bool mergeUp)
        {
            if (rhs == null)
            {
                return;
            }
            rhs.RemovePurpleL();
            if (rhs.IsEmpty)
            {
                return;
            }
            if (IsEmpty)
            {
                segBundle = rhs.segBundle;
                Top = rhs.Top;
                Bottom = rhs.Bottom;
            }
            else if (mergeUp)
            {
                try
                {
                    segBundle.Merge(Top, rhs.segBundle);
                }
                catch(ArgumentOutOfRangeException e)
                {
                    throw new SelfIntersectingLineSegmentsException("Bundles out of order when merging", e);
                }
                Top.Up = rhs.Bottom;
                rhs.Bottom.Down = Top;
                Top = rhs.Top;
            }
            else
            {
                try
                {
                    rhs.segBundle.Merge(rhs.Top, segBundle);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    throw new SelfIntersectingLineSegmentsException("Bundles out of order when merging", e);
                }
                segBundle = rhs.segBundle;
                rhs.Top.Up = Bottom;
                Bottom.Down = rhs.Top;
                Bottom = rhs.Bottom;
            }
        }

        public void RemovePurpleL()
        {
            if (PurpleDown != null)
            {
                PurpleDown.PurpleUp = PurpleUp;
            }
            if (PurpleUp != null)
            {
                PurpleUp.PurpleDown = PurpleDown;
            }
        }

        public void AddAbovePurpleL(Bundle lower)
        {
            PurpleUp = lower.PurpleUp;
            PurpleDown = lower;
            if (PurpleUp != null)
            {
                PurpleUp.PurpleDown = this;
            }
            lower.PurpleUp = this;
        }

        public void insertSegAtTop(Segment segment)
        {
            segBundle.Add(segment);
            segment.Up = null;
            segment.Down = Top;
            Top.Up = segment;
            Top = segment;
            if (Bottom == null)
            {
                Bottom = segment;
            }
        }

        public int CompareTo(ISpatiallyComparable rhs)
        {
            if(rhs is Bundle)
            {
                return Top.CompareTo(((Bundle) rhs).Top);
            }

            int comp = Top.CompareTo(rhs);
            if(comp <= 0)
            {
                return comp;
            }

            comp = Bottom.CompareTo(rhs);
            return comp < 0 ? 0 : comp;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is ISpatiallyComparable))
            {
                throw new ArgumentException("Both arguments must be ISpatiallyComparable");
            }
            // TODO: Could we fall back on default comparer here?
            return CompareTo((ISpatiallyComparable) obj);
        }

        public bool IsEmpty
        {
            get { return Top == null; }
        }

        public Segment FindSmallestSegmentAbove(ISpatiallyComparable x)
        {
            return (Segment) segBundle.SmallestAbove(x);
        }

        public Segment FindLargestSegmentBelow(ISpatiallyComparable x)
        {
            return (Segment) segBundle.LargestBelow(x);
        }

        public bool TryFindSmallestSegmentAbove(ISpatiallyComparable x, out Segment result)
        {
            ISpatiallyComparable segment;
            bool success = segBundle.TryGetSmallestAbove(x, out segment);
            result = (Segment) segment;
            return success;
        }

        public bool TryFindLargestSegmentBelow(ISpatiallyComparable x, out Segment result)
        {
            ISpatiallyComparable segment;
            bool success = segBundle.TryGetLargestBelow(x, out segment);
            result = (Segment) segment;
            return success;
        }
    }
}
