using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Utility.Collections;

namespace Geometry.RedBlue
{
    internal class BundleTree
    {
        private Bundle top = null;
        private Bundle bottom = null;
        public SplayTree<ISpatiallyComparable> bTree;

        public BundleTree()
        {
            bTree = new SplayTree<ISpatiallyComparable>();
        }

        public BundleTree(SplayTree<ISpatiallyComparable> bTree, Bundle top, Bundle bottom)
        {
            this.bTree = bTree;
            this.top = top;
            this.bottom = bottom;
        }

        public BundleTree Split(ISpatiallyComparable splitPos, bool keep)
        {
            BundleTree upperPart = null;
            Segment splitSeg;
            Bundle splitB;
            if (keep)
            {
                splitB = FindSmBAbove(splitPos);
                if (splitB.PurpleDown != null && splitB.PurpleDown.CompareTo(splitPos) > 0)
                {
                    splitB = splitB.PurpleDown;
                }

                if (splitB.PurpleDown != null)
                {
                    splitB = splitB.PurpleDown;
                    bool found = splitB.TryFindSmallestSegmentAbove(splitPos, out splitSeg);
                    Debug.Assert(found == (splitSeg != null));
                }
                else
                {
                    upperPart = new BundleTree(bTree, top, bottom);
                    bTree = null;
                    top = bottom = null;
                    return upperPart;
                }
            }
            else
            {
                splitB = FindLgBBelow(splitPos);
                if (splitB.PurpleUp != null && splitB.PurpleUp.CompareTo(splitPos) < 0)
                {
                    splitB = splitB.PurpleUp;
                }

                if (splitB.PurpleUp != null)
                {
                    bool found = splitB.PurpleUp.TryFindLargestSegmentBelow(splitPos, out splitSeg);
                    if (found)
                    {
                        splitB = splitB.PurpleUp;
                    }
                }
                else
                {
                    return new BundleTree();
                }
            }

            if (splitSeg != null)
            {
                splitB.Split(splitSeg, keep ^ true);
                PlainInsert(splitB.PurpleUp);
            }
            SplayTree<ISpatiallyComparable> s = splitB.IsRed ? bTree.SplitAt(splitB, true) : bTree.SplitAt(splitB.PurpleDown, true);
            upperPart = new BundleTree(s, top, splitB.PurpleUp);
            top = splitB;
            top.PurpleUp = upperPart.bottom.PurpleDown = null;
            return upperPart;
        }

        public void Merge(BundleTree rhs, bool mergeUp)
        {
            if (rhs == null)
            {
                return;
            }
            if (rhs.IsEmpty)
            {
                return;
            }

            if(IsEmpty)
            {
                bTree = rhs.bTree;
                top = rhs.top;
                bottom = rhs.bottom;
            }
            else if(mergeUp)
            {
                if (top.IsRed)
                {
                    bTree.Merge(top, rhs.bTree);
                }
                else
                {
                    bTree.Merge(top.PurpleDown, rhs.bTree);
                }
                top.PurpleUp = rhs.bottom;
                rhs.bottom.PurpleDown = top;
                if (top.IsRed == rhs.bottom.IsRed)
                {
                    GroupBundles(top, true);
                }
                top = rhs.top;
            }
            else
            {
                if (rhs.top.IsRed)
                {
                    rhs.bTree.Merge(rhs.top, bTree);
                }
                else
                {
                    rhs.bTree.Merge(rhs.top.PurpleDown, bTree);
                }
                bTree = rhs.bTree;
                rhs.top.PurpleUp = bottom;
                bottom.PurpleDown = rhs.top;
                if (rhs.top.IsRed == bottom.IsRed)
                {
                    GroupBundles(rhs.top, true);
                }
                bottom = rhs.bottom;
            }
        }

        public void InsertBAtTop(Bundle b)
        {
            if (b.IsRed)
            {
                bTree.Add(b);
            }
            b.PurpleUp = null;
            b.PurpleDown = top;
            if (top != null)
            {
                top.PurpleUp = b;
            }
            top = b;
            if (bottom == null)
            {
                bottom = b;
            }
        }

        public void PlainInsert(Bundle b)
        {
            if (b.IsRed)
            {
                bTree.Add(b);
            }
            if (top == b.PurpleDown)
            {
                top = b;
            }
            if (bottom == b.PurpleUp)
            {
                bottom = b;
            }
        }

        public void GroupBundles(Bundle b, bool mergeUp)
        {
            Bundle other = mergeUp ? b.PurpleUp : b.PurpleDown;
            if (other.IsRed)
            {
                bTree.Remove(other);
            }
            if (top == other)
            {
                top = top.PurpleDown;
            }
            if (bottom == other)
            {
                bottom = bottom.PurpleUp;
            }
            b.Merge(other, mergeUp);
        }

        public Bundle separateEq(Bundle b, FPoint ev, bool keep, bool retUpper)
        {
            Segment splitSeg;
            bool found = keep ? b.TryFindSmallestSegmentAbove(ev, out splitSeg)
                              : b.TryFindLargestSegmentBelow(ev, out splitSeg);
            if(!found)
            {
                Debug.Assert(splitSeg == null);
                return b;
            }

            if(keep && splitSeg == b.Bottom || !keep && splitSeg == b.Top)
            {
                return b;
            } 
            Bundle upperB = b.Split(splitSeg, keep ^ true);
            PlainInsert(upperB);
            return retUpper ? upperB : b;
        }

        public bool IsEmpty
        {
            get { return top == null; }
        }

        public Bundle Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }

        public Bundle Top
        {
            get { return top; }
            set { top = value; }
        }

        public Bundle FindSmBAbove(ISpatiallyComparable x)
        {
            Bundle result;
            bool success = TryFindSmBAbove(x, out result);
            if (!success)
            {
                throw new IndexOutOfRangeException();
            }
            return result;
        }

        public Bundle FindLgBBelow(ISpatiallyComparable x)
        {
            Bundle result;
            bool success = TryFindLgBBelow(x, out result);
            if (!success)
            {
                throw new IndexOutOfRangeException();
            }
            return result;
        }

        public bool TryFindSmBAbove(ISpatiallyComparable x, out Bundle result)
        {
            ISpatiallyComparable bundle;
            bool success = bTree.TryGetSmallestAbove(x, out bundle);
            result = (Bundle) bundle;
            return success;
        }

        public bool TryFindLgBBelow(ISpatiallyComparable x, out Bundle result)
        {
            ISpatiallyComparable bundle;
            bool success = bTree.TryGetLargestBelow(x, out bundle);
            result = (Bundle) bundle;
            return success;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Top = {0}\n", Top);
            sb.AppendFormat("Bottom = {0}\n", Bottom);
            sb.AppendFormat("bTree = {0}\n", bTree);
            return sb.ToString();
        }
    }
}
