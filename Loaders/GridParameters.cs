using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Builders;

namespace Loaders.plugins
{
    class GridParameters
    {
        public class ParameterException : OpenException
        {
            public ParameterException(string message) :
                base(message)
            {
            }
        }

        #region Fields

        private int? iNum = null; // number of grid nodes on the minor axis - parallel to x for zero rotation
        private int? jNum = null; // number of grid nodes on the major axis  - parallel to y for zero rotation
        private double? xOrigin = null; // x origin of grid - node [i=0, j=0]
        private double? yOrigin = null; // y origin of grid - node [i=0, j=0]
        private double? xMax = null; // maximum x value - only used with axis aligned grids
        private double? yMax = null; // maximum y value - only used with axis aligned grids
        private double? zMin = null; // minimum z value - only used for identifying nulls
        private double? zMax = null; // maximum z value - only used for identifying nulls
        private double? zNull = null; // The z null value
        private double? iInc = null; // iInc - the spacing between grid nodes on the i axis
        private double? jInc = null; // jInc - the spacing between grid nodes on the j axis
        private double orientation = 0.0; // The angle in degrees clockwise from North to the j axis

        #endregion

        #region Properties

        public int? INum
        {
            get { return iNum; }
            set
            {
                if (value < 1)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Number of grid columns given ({0}) is not positive", value);
                    throw new ArgumentOutOfRangeException(message.ToString());
                }
                iNum = value;
            }
        }


        public int? JNum
        {
            get { return jNum; }
            set
            {
                if (value < 1)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Number of grid rows given ({0}) is not positive", value);
                    throw new ArgumentOutOfRangeException(message.ToString());
                }
                jNum = value;
            }
        }


        public double? XOrigin
        {
            get { return xOrigin; }
            set { xOrigin = value; }
        }

        public double? YOrigin
        {
            get { return yOrigin; }
            set { yOrigin = value; }
        }

        public double? XMax
        {
            get { return xMax; }
            set {
                if (xOrigin.HasValue && value <= xOrigin)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Grid x dimension is not a positive value. Minimum x = {0}, Maximum x = {1}", xOrigin, xMax);
                    throw new ArgumentOutOfRangeException(message.ToString());
                }
                xMax = value;
            }
        }

        public double? YMax
        {
            get { return yMax; }
            set
            {
                if (yOrigin.HasValue && value <= yOrigin)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Grid y dimension is not a positive value. Minimum x = {0}, Maximum x = {1}", yOrigin, yMax);
                    throw new ArgumentOutOfRangeException(message.ToString());
                }
                yMax = value;
            }
        }

        public double? ZMin
        {
            get { return zMin; }
            set {
                if (zMax.HasValue && value > zMax)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Minimum z is greater than maximum z. Minimum z = {0}, Maximum z = {1}", value, zMax);
                    throw new ArgumentOutOfRangeException(message.ToString());
                }
                zMin = value;
            }
        }

        public double? ZMax
        {
            get { return zMax; }
            set
            {
                if (zMin.HasValue && value < zMin)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Maximum z is less-than than minimum z. Minimum z = {0}, Maximum z = {1}", value, zMax);
                    throw new ArgumentOutOfRangeException(message.ToString());
                }
                zMax = value;
            }
        }

        public double? ZNull
        {
            get { return zNull; }
            set {
                if (zMin.HasValue && zMax.HasValue
                    && value >= zMin && value <= zMax)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Null z value is within the allowed range of values. Null = {0}, Minimum z = {1}, Maximum z = {2}", value, zMin, zMax);
                    throw new ArgumentOutOfRangeException(message.ToString());
                }
                zNull = value; 
            }
        }

        public double? IInc
        {
            get { return iInc; }
            set
            {
                if (value <= 0.0)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Grid node spacing given ({0}) is not positive.", value);
                    throw new ArgumentOutOfRangeException(message.ToString());
                }
                iInc = value;
            }
        }

        public double? JInc
        {
            get { return jInc; }
            set {
                if (value <= 0.0)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Grid node spacing given ({0}) is not positive.", value);
                    throw new ArgumentOutOfRangeException(message.ToString());
                }
                jInc = value;
            }
        }

        public double Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        #endregion

        #region Methods

        public void VerifyAndCompleteParameters()
        {
            // We may be given various combinations of values,
            // possibly containing redundant information. Here we piece them
            // together to make a complete and consistent set, or we raise an
            // exception.

            // First of all, try to determine the number of rows and the number
            // of columns - both integers of course.
            if (orientation == 0.0)
            {
                // If the rotation angle is zero we can use the extent of the grid
                // X-axis : (xMax - xOrigin) / iInc = iNum
                int numXUnknowns = 0;
                StringBuilder xMessage = new StringBuilder();
                xMessage.Append("Too many unknown grid parameters: ");
                if (!xMax.HasValue)
                {
                    xMessage.Append("Grid x maximum value is unknown. ");
                    xMax = (iNum - 1) * iInc + xOrigin;
                    ++numXUnknowns; 
                }
                if (!xOrigin.HasValue)
                {
                    xMessage.Append("Grid x origin value is unknown. ");
                    xOrigin = xMax - (iNum - 1) * iInc;
                    ++numXUnknowns;
                }
                if (!iInc.HasValue)
                {
                    xMessage.Append("Grid x increment value is unknown. ");
                    iInc = (xMax - xOrigin) / (iNum - 1);
                    ++numXUnknowns;                }
                if (!iNum.HasValue)
                {
                    xMessage.Append("Number of grid nodes in x direction is unknown. ");
                    // TODO: Check the rounding used here.
                    double? unrounded = (xMax - xOrigin) / iInc;
                    if (unrounded.HasValue)
                    {
                        iNum = 1 + (int) Math.Round(unrounded.Value, MidpointRounding.AwayFromZero);
                    }
                    ++numXUnknowns;
                }
                if (numXUnknowns > 1)
                {
                    throw new ParameterException(xMessage.ToString().Trim());
                }

                // Y-axis : (yMax - yOrigin) / jInc = jNum
                int numYUnknowns = 0;
                StringBuilder yMessage = new StringBuilder();
                yMessage.Append("Too many unknown grid parameters. ");
                if (!yMax.HasValue)
                {
                    yMessage.Append("Grid y maximum value is unknown. ");
                    yMax = (jNum - 1) * jInc + yOrigin;
                    ++numYUnknowns;
                }
                if (!yOrigin.HasValue)
                {
                    yMessage.Append("Grid y origin value is unknown. ");
                    yOrigin = yMax - (jNum - 1) * jInc;
                    ++numYUnknowns;
                }
                if (!jInc.HasValue)
                {
                    yMessage.Append("Grid y increment value is unknown. ");
                    jInc = (yMax - yOrigin) / (jNum - 1);
                    ++numYUnknowns;
                }
                if (!jNum.HasValue)
                {
                    yMessage.Append("Number of grid nodes in y direction is unknown. ");
                    // TODO: Check the rounding used here
                    double? unrounded = (yMax - yOrigin) / jInc;
                    if (unrounded.HasValue)
                    {
                        jNum = 1 + (int) Math.Round(unrounded.Value, MidpointRounding.AwayFromZero);
                    }
                    ++numYUnknowns;
                }
                if (numXUnknowns > 1)
                {
                    throw new ParameterException(yMessage.ToString().Trim());
                }
            }
            else
            {
                // If the rotation angle is non-zero, we choose to depend
                // on having the origin defined, and the number of rows
                // and columns defined explicity
                if (!(xOrigin.HasValue && yOrigin.HasValue
                    && iNum.HasValue && jNum.HasValue && (iInc.HasValue || jInc.HasValue)))
                {
                    throw new ParameterException("Rotated grids must have both origin, number of columns ,number of rows and a grid increment defined.");
                }
                if (!jInc.HasValue)
                {
                    jInc = iInc;
                    // TODO: Issue warning
                }
                if (!iInc.HasValue)
                {
                    iInc = jInc;
                    // TODO: Issue warning
                }
            }
        }

        public void Build(IGrid2DBuilder builder)
        {
            Debug.Assert(builder != null);
            Debug.Assert(XOrigin.HasValue && YOrigin.HasValue
                      && INum.HasValue && JNum.HasValue
                      && IInc.HasValue && JInc.HasValue);
            builder.XOrigin = XOrigin.Value;
            builder.YOrigin = YOrigin.Value;
            builder.INum = INum.Value;
            builder.JNum = JNum.Value;
            builder.IInc = IInc.Value;
            builder.JInc = JInc.Value;
            builder.Orientation = Orientation;
        }


        #endregion 
    }
}
