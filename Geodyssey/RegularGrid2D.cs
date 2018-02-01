/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-22
 * Time: 12:33
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

using Numeric;
using Model;

namespace Geodyssey
{
	/// <summary>
	/// Description of RegularGrid2D.
	/// </summary>
	public class RegularGrid2D : IRegularGrid2D
	{
        private const float goldenNullValue = 1.70141e38f;

	    private Point2D origin;
	    private Vector2D spacing;
	    private readonly double?[,] zs;
	    #region Construction

        public RegularGrid2D(double originX, double originY, double spacingX, double spacingY, int sizeI, int sizeJ, double? initalValue):
            this(new Point2D(originX, originY), new Vector2D(spacingX, spacingY), sizeI, sizeJ, initalValue)
        {
        }

	    public RegularGrid2D(double originX, double originY, double spacingX, double spacingY, int sizeI, int sizeJ) :
	        this(new Point2D(originX, originY), new Vector2D(spacingX, spacingY), sizeI, sizeJ, null)
		{
		}

        public RegularGrid2D(Point2D origin, Vector2D spacing, int sizeI, int sizeJ) :
            this(origin, spacing, sizeI, sizeJ, null)
        {
        }
 
	    public RegularGrid2D(Point2D origin, Vector2D spacing, int sizeI, int sizeJ, double? initialValue)
	    {
	        this.origin = origin;
	        this.spacing = spacing;
	        this.zs = new double?[sizeI, sizeJ];
            for (int i = 0 ; i < sizeI ; ++i)
            {
                for (int j = 0 ; j < sizeJ ; ++j)
                {
                    this.zs[i, j] = initialValue;
                }
            }
	    }

        public RegularGrid2D(RegularGrid2D other)
        {
            this.origin = other.origin;
            this.spacing = other.spacing;
            this.zs = (double?[,]) other.zs.Clone();
        }

        public static RegularGrid2D Create(StreamReader reader)
        {
            RegularGrid2D grid = null;
            try
            {
                // Line 1 - the identifier
                string header1 = reader.ReadLine();
                if (!header1.StartsWith("DSAA"))
                {
                    return null;
                }
                // Line 2 - the number of points along the I and J axes 
                string header2 = reader.ReadLine();
                string[] fields2 = header2.Split();
                int sizeI = int.Parse(fields2[0]);
                int sizeJ = int.Parse(fields2[1]);

                // Line 3 - the extent in the I direction
                string header3 = reader.ReadLine();
                string[] fields3 = header3.Split();
                double minX = double.Parse(fields3[0]);
                double maxX = double.Parse(fields3[1]);

                // Line 4 - the extent in the J direction
                string header4 = reader.ReadLine();
                string[] fields4 = header4.Split();
                double minY = double.Parse(fields4[0]);
                double maxY = double.Parse(fields4[1]);

                // Line 4 - the extent in the J direction
                string header5 = reader.ReadLine();
                string[] fields5 = header5.Split();
                double minZ = double.Parse(fields5[0]);
                double maxZ = double.Parse(fields5[1]);

                double spacingX = (maxX - minX) / (sizeI - 1);
                double spacingY = (maxY - minY) / (sizeJ - 1);

                grid = new RegularGrid2D(minX, minY, spacingX, spacingY, sizeI, sizeJ);

                string line;
                int counter = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split();
                    foreach (string field in fields)
                    {
                        double z;
                        if (double.TryParse(field, out z))
                        {   
                            int i = counter % sizeI;
                            int j = counter / sizeI;
                            if (z == goldenNullValue || z < minZ || z > maxZ)
                            {
                                grid[i, j] = null;
                            }
                            else
                            {
                                grid[i, j] = z;
                            }
                            ++counter;
                        }
                    }
                }
            }
            catch (System.FormatException)
            {
                grid = null;
            }
            return grid;
        }

        #endregion

        #region Properties

        public Point2D Origin
	    {
	        get { return origin; }
	    }
	    
	    public Vector2D Spacing
	    {
	        get { return spacing; }
	    }
	    
	    public int SizeI
	    {
	        get { return zs.GetLength(0); }
	    }
	    
	    public int SizeJ
	    {
	        get { return zs.GetLength(1); }
	    }
	    	    
	    public double? this[int i, int j]
	    {
	        get { return zs[i, j];  }
	        set { zs[i, j] = value; }
	    }
	    	    
	    public double MinX
	    {
	        get { return origin.X; }
	    }
	    
	    public double MinY
	    {
	        get { return origin.Y; }
	    }
	    
	    public double MaxX
	    {
	        get { return NodeX(SizeI - 1); }
	    }
	    
	    public double MaxY
	    {
	        get { return NodeY(SizeJ - 1); }
	    }
	    
	    public double? MinZ
	    {
	        get
	        {
	            double? zValue = null;
	            foreach (double? z in zs)
	            {
                    if (z.HasValue)
                    {
                        zValue = z;
                        break;
                    }
                }

                if (!zValue.HasValue)
                {
                    return null;
                }

                foreach (double? z in zs)
                {
	                if (z.HasValue && z < zValue)
	                {
	                    zValue = z;
	                }
	            }
	            return zValue;
	        }
	    }
	    
	    public double? MaxZ
	    {
	        get
	        {
                double? zValue = null;
                foreach (double? z in zs)
                {
                    if (z.HasValue)
                    {
                        zValue = z;
                        break;
                    }
                }

                if (!zValue.HasValue)
                {
                    return null;
                }

                foreach (double? z in zs)
                {
                    if (z.HasValue && z > zValue)
                    {
                        zValue = z;
                    }
                }
                return zValue;
	        }
	    }
	    
	    #endregion
	    
	    #region Methods
	    
	    public double NodeX(int i)
	    {
	        return Origin.X + Spacing.DeltaX * i;     
	    }
	    
	    public double NodeY(int j)
	    {
	        return Origin.Y + Spacing.DeltaY * j;
	    }
	    
	    public double? NodeZ(int i, int j)
	    {
	        return zs[i, j];
	    }
	    
        /// <summary>
        /// Compute the Root Mean Square difference betweeen two grids of the same dimensions.
        /// Nodes which are null in either grid will be ignored.
        /// </summary>
        /// <param name="otherGrid">The grid against which this grid will be compared.</param>
        /// <returns>The Root Mean Square difference of the z values.</returns>
	    public double? RmsDifference(IRegularGrid2D otherGrid)
	    {
            return RmsDifference(otherGrid, 0, SizeI, 0, SizeJ);
	    }

        /// <summary>
        /// Compute the Root Mean Square difference betweeen two grids of the same dimensions.
        /// Nodes which are null in either grid will be ignored.
        /// </summary>
        /// <param name="otherGrid">The grid against which this grid will be compared.</param>
        /// <param name="beginI">The minimumI</param>
        /// <param name="endI">One beyond the maximumI (half-open range)</param>
        /// <param name="beginJ">The minimumJ</param>
        /// <param name="endJ">One beyond the maximumJ (half-open range)</param>
        /// <returns>The Root Mean Square difference of the z values.</returns>
        public double? RmsDifference(IRegularGrid2D otherGrid, int beginI, int endI, int beginJ, int endJ)
        {
            Debug.Assert(this.SizeI == otherGrid.SizeI);
            Debug.Assert(this.SizeJ == otherGrid.SizeJ);
            Debug.Assert(beginI >= 0 && beginI < SizeI);
            Debug.Assert(beginJ >= 0 && beginJ < SizeJ);
            Debug.Assert(endI > 0 && endI <= SizeI);
            Debug.Assert(endJ > 0 && endJ <= SizeJ);
            double total = 0.0;
            int count = 0;
            for (int i = beginI; i < endI; ++i)
            {
                for (int j = beginJ; j < endJ; ++j)
                {
                    if (this[i, j].HasValue && otherGrid[i, j].HasValue)
                    {
                        double diff = this[i, j].Value - otherGrid[i, j].Value;
                        Debug.Assert(!double.IsNaN(diff));
                        Debug.Assert(!double.IsInfinity(diff));
                        total += diff * diff;
                        ++count;
                    }
                }
            }
            if (count == 0)
            {
                return null;
            }
            double mean = total / count;
            return Math.Sqrt(mean);
        }

        /// <summary>
        /// Compute a normalized correspondence value between two grids of the same dimensions.
        /// </summary>
        /// <param name="otherGrid">The grid against which this grid will be compared.</param>
        /// <returns>
        /// A number between 0.0 and 1.0 indicating the proportion of corresponding
        /// grid nodes which either both contain a value, or both contain a null.
        /// </returns>
        public double Correspondence(IRegularGrid2D otherGrid)
        {
            Debug.Assert(this.SizeI == otherGrid.SizeI);
            Debug.Assert(this.SizeJ == otherGrid.SizeJ);
            int correspondence = 0;
            for (int i = 0 ; i < SizeI ; ++i)
            {
                for (int j = 0 ; j < SizeJ ; ++j)
                {
                    if (this[i, j].HasValue == otherGrid[i, j].HasValue)
                    {
                        correspondence += 1;
                    }                   
                }
            }
            return (double) correspondence / (double) zs.Length;
        }

        public void MaskFrom(IRegularGrid2D maskGrid)
        {
            Debug.Assert(this.SizeI == maskGrid.SizeI);
            Debug.Assert(this.SizeJ == maskGrid.SizeJ);
            for (int i = 0 ; i < SizeI ; ++i)
            {
                for (int j = 0 ; j < SizeJ ; ++j)
                {
                    if (!maskGrid[i, j].HasValue)
                    {
                        this[i, j] = null;
                    }
                }
            }
        }

        public double Mean()
        {
            double total = 0.0;
            int count = 0;
            foreach (double? z in zs)
            {
                if (z.HasValue)
                {
                    total += z.Value;
                    ++count;
                }
            }
            double mean = total / count;
            return mean;
        }

	    public IEnumerator<double?> GetEnumerator()
	    {
	        for (int i = 0 ; i < SizeI ; ++i)
	        {
	            for (int j = 0; j < SizeJ ; ++j)
	            {
	                yield return zs[i, j];
	            }
	        }
	    }

	    public override string ToString()
        {
            // TODO Could do this using a TextWriter 
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DSAA");
            sb.AppendFormat("{0} {1}\n", SizeI, SizeJ);
            sb.AppendFormat("{0} {1}\n", MinX, MaxX);
            sb.AppendFormat("{0} {1}\n", MinY, MaxY);
            sb.AppendFormat("{0} {1}\n", MinZ, MaxZ);
            
            for (int j = 0; j < SizeJ; ++j)
            {
                int counter = 0;
                for (int i = 0; i < SizeI; ++i)
                {
                    if (counter == 10)
                    {
                        sb.AppendLine();
                        counter = 0;
                    }
                    if (this[i, j].HasValue)
                    {
                        sb.Append(this[i, j].Value);
                    }
                    else
                    {
                        sb.Append(goldenNullValue);
                    }
                    if (i != SizeI - 1)
                    {
                        sb.Append(' ');
                    }
                    ++counter;
                }
                sb.AppendLine();
                sb.AppendLine();
            }
            return sb.ToString();
        }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }

	    public void WriteSurfer6BinaryFile(string filename)
        {
            using (FileStream file = File.Create(filename))
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                writer.Write((int) 0x42425344);
                writer.Write((short) SizeI);
                writer.Write((short) SizeJ);
                writer.Write((double) MinX);
                writer.Write((double) MaxX);
                writer.Write((double) MinY);
                writer.Write((double) MaxY);
                writer.Write((double) MinZ);
                writer.Write((double) MaxZ);
                for (int j = 0; j < SizeJ; ++j)
                {
                    for (int i = 0; i < SizeI; ++i)
                    {
                        if (this[i, j].HasValue)
                        {
                            writer.Write((float) this[i, j].Value);
                        }
                        else
                        {
                            writer.Write((float) goldenNullValue);
                        }
                    }
                }
            }
        }

        public IRegularGrid2D CreatePartialDerivativeIGrid()
        {
            RegularGrid2D result = new RegularGrid2D(Origin, Spacing, SizeI, SizeJ);
            for (int i = 0 ; i < SizeI ; ++i)
	        {
	            for(int j = 0 ; j < SizeJ ; ++j)
	            {
                    //int trailing = Math.Max(i - 1, 0);
                    int trailing = i;
                    int leading  = Math.Min(i + 1, SizeI - 1);
                    double h = (leading - trailing) * spacing.DeltaX;
                    if (h > 0.0)
                    {
                        result[i, j] = (this[leading, j] - this[trailing, j]) / h;
                    }
                    else
                    {
                        result[i, j] = null;
                    }
                }
            }
            return result;
        }

        public IRegularGrid2D CreatePartialDerivativeJGrid()
        {
            RegularGrid2D result = new RegularGrid2D(Origin, Spacing, SizeI, SizeJ);
            for (int i = 0 ; i < SizeI ; ++i)
	        {
	            for(int j = 0 ; j < SizeJ ; ++j)
	            {
                    //int trailing = Math.Max(j - 1, 0);
                    int trailing = j;
                    int leading  = Math.Min(j + 1, SizeJ - 1);
                    double h = (leading - trailing) * spacing.DeltaY;
                    if (h > 0.0)
                    {
                        result[i, j] = (this[i, leading] - this[i, trailing]) / h;
                    }
                    else
                    {
                        result[i, j] = null;
                    }
                }
            }
            return result;
        }

        public IGridCoordinateTransformation CreateHypsometricGrid()
        {
            Debug.Assert(MinZ.HasValue);
            Debug.Assert(MaxZ.HasValue);
            // Build a histogram of the point elevations
            const int numClasses = 100;
            List<int> histogram = new List<int>(numClasses);
            for (int i = 0 ; i < numClasses ; ++i)
            {
                histogram.Add(0);
            }
            Debug.Assert(histogram.Count == numClasses);
            double minZ = MinZ.Value;
            double maxZ = MaxZ.Value;
            double classInterval = (maxZ - minZ) / numClasses;
            
            // Now accumulate the histogram
            foreach (double? z in zs)
            {
                if (z.HasValue)
                {
                    // Compute the bin number from the z value
                    double offset = z.Value - minZ;
                    int index = (int) Math.Floor(offset / classInterval);
                    // Put the maxZ values into the top class
                    if (index == histogram.Count)
                    {
                        Debug.Assert(z == maxZ);
                        --index;
                    }
                    Debug.Assert(index >= 0 && index < histogram.Count);
                    ++histogram[index];
                }
            }
            
            // Now convert to a cumulative histogram
            for (int i = 1 ; i < histogram.Count ; ++i)
            {
                histogram[i] += histogram[i-1];
            }
            
            // Create the result grid
            RegularGrid2D result = new RegularGrid2D(Origin, Spacing, SizeI, SizeJ);
            for (int i = 0 ; i < SizeI ; ++i)
	        {
	            for(int j = 0 ; j < SizeJ ; ++j)
	            {
	                double? z = zs[i, j];
                    if (z.HasValue)
                    {
                        double offset = z.Value - minZ;
                        int index = (int) Math.Floor(offset / classInterval);
                        // Put the maxZ values into the top class
                        if (index == histogram.Count)
                        {
                            Debug.Assert(z == maxZ);
                            --index;
                        }
                        int lower = (index > 0) ? histogram[index - 1] : 0;
                        int higher = histogram[index];
                        double classOffset = offset - (index * classInterval);
                        double classOffsetProportion = classOffset / classInterval;
                        result[i, j] = lower + classOffsetProportion * (higher - lower);
                    }
                    else
                    {
                        result[i, j] = null;
                    }
	            }
            }
            return result;
        }

        public Histogram CreateDipAzimuthHistogram(int numberOfClasses)
        {
            Histogram histogram = new Histogram(numberOfClasses, -Math.PI, +Math.PI);

            IRegularGrid2D dZdX = CreatePartialDerivativeIGrid();
            IRegularGrid2D dZdY = CreatePartialDerivativeJGrid();
            for (int i = 0 ; i < SizeI ; ++i)
            {
                for (int j = 0 ; j < SizeJ ; ++j)
                {
                    double? x = dZdX[i, j];
                    double? y = dZdY[i, j];
                    if (x.HasValue && y.HasValue)
                    {
                        if (x != 0.0 && y != 0.0)
                        {
                            
                            double dipAzimuth = Math.Atan2(-y.Value, -x.Value); // Negate x and y to give down-dip rather than up-dip
                            histogram.Accumulate(dipAzimuth);
                        } 
                    }
                }
            }
            return histogram;
        }

        /// <summary>
        /// Use bilinear interpolation to estimate the value of the grid surface at the
        /// specified point
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public double? Bilinear(Point2D position)
        {
            // TODO: Many opportunities for simplification.
            Point2D gridPosition = GeographicToGrid(position);
            double x = gridPosition.X;
            double y = gridPosition.Y;
      
            // TODO: The crop the result to the region with data.  This should be handled by the
            //       calling code, and only asserted on here.
            int x1 = Range.Clip((int) Math.Floor(x), 0, SizeI - 1);
            int y1 = Range.Clip((int) Math.Floor(y), 0, SizeJ - 1);
            int x2 = Range.Clip((int) Math.Ceiling(x), 0, SizeI - 1);
            int y2 = Range.Clip((int) Math.Ceiling(y), 0, SizeJ - 1);
            double? q;
            if (x1 == x2)
            {
                if (y1 == y2)
                {
                    q = this[x1, y1];
                }
                else
                {
                    double? q1 = this[x1, y1];
                    double? q2 = this[x1, y2];
                    double t = (y - y1) / (y2 - y1);
                    q = q1 + t * (q2 - q1);

                }
            }
            else
            {
                if (y1 == y2)
                {
                    double? q1 = this[x1, y1];
                    double? q2 = this[x2, y1];
                    double t = (x - x1) / (x2 - x1);
                    q = q1 + t * (q2 - q1);
                }
                else
                {
                    double? q11 = this[x1, y1];
                    double? q21 = this[x2, y1];
                    double? q12 = this[x1, y2];
                    double? q22 = this[x2, y2];
                    double denom = (x2 - x1) * (y2 - y1);
                    double? t11 = (x2 - x) * (y2 - y) * q11 / denom;
                    double? t21 = (x - x1) * (y2 - y) * q21 / denom;
                    double? t12 = (x2 - x) * (y - y1) * q12 / denom;
                    double? t22 = (x - x1) * (y - y1) * q22 / denom;
                    q = t11 + t21 + t12 + t22;
                }
            }
            return q;
        }

        public Point2D GridToGeographic(double i, double j)
        {
            return new Point2D(MinX + i * Spacing.DeltaX, MinY + j * Spacing.DeltaY);
        }

        public Point2D GridToGeographic(Point2D gridPosition)
        {
            return GridToGeographic(gridPosition.X, gridPosition.Y);
        }

        public Point2D GeographicToGrid(Point2D geoPosition)
        {
            return new Point2D((geoPosition.X - MinX) / Spacing.DeltaX, (geoPosition.Y - MinY) / Spacing.DeltaY);
        }

        public IRegularGrid2D CloneSize()
        {
            return new RegularGrid2D(Origin, Spacing, SizeI, SizeJ);
        }

        /// <summary>
        /// Drovides a different number of cells occupinging
        /// the same region as the original grid.
        /// </summary>
        /// <param name="newSizeI">The number of points in the i direction</param>
        /// <param name="newSizeJ">The number of points in the j direction</param>
        /// <returns></returns>
	    public IRegularGrid2D CloneSize(int newSizeI, int newSizeJ)
	    {
	        Vector2D newSpacing = new Vector2D((float) SizeI / newSizeI, (float) SizeJ / newSizeJ);
	        return new RegularGrid2D(Origin, newSpacing,  newSizeI, newSizeJ);
	    }

	    #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new RegularGrid2D(this);
        }

        #endregion
    }
}
