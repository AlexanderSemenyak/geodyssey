using System;
using System.Collections.Generic;
using System.Text;

using Numeric;

namespace Model
{
    public interface IRegularGrid2D : ICloneable, IGridCoordinateTransformation, IEnumerable<double?>
    {
        double Correspondence(IRegularGrid2D otherGrid);
        Histogram CreateDipAzimuthHistogram(int numberOfClasses);
        IGridCoordinateTransformation CreateHypsometricGrid();
        IRegularGrid2D CreatePartialDerivativeIGrid();
        IRegularGrid2D CreatePartialDerivativeJGrid();
        void MaskFrom(IRegularGrid2D maskGrid);
        double MaxX { get; }
        double MaxY { get; }
        double? MaxZ { get; }
        double Mean();
        double MinX { get; }
        double MinY { get; }
        double? MinZ { get; }
        double NodeX(int i);
        double NodeY(int j);
        double? NodeZ(int i, int j);
        Point2D Origin { get; }
        double? RmsDifference(IRegularGrid2D otherGrid, int beginI, int endI, int beginJ, int endJ);
        double? RmsDifference(IRegularGrid2D otherGrid);
        int SizeI { get; }
        int SizeJ { get; }
        Vector2D Spacing { get; }
        double? this[int i, int j] { get; set; }
        double? Bilinear(Point2D position);
        string ToString();
        void WriteSurfer6BinaryFile(string filename);

        IRegularGrid2D CloneSize();
        IRegularGrid2D CloneSize(int newSizeI, int newSizeJ);
    }
}
