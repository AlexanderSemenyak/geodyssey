using Numeric;

namespace Model
{
    public interface IGridCoordinateTransformation
    {
        /// <summary>
        /// Transform a grid node co-ordinate into a geographical co-ordinate
        /// </summary>
        /// <param name="i">The i grid node co-ordinate</param>
        /// <param name="j">The j grid node co-ordinate</param>
        /// <returns>The geographical position of the grid node</returns>
        Point2D GridToGeographic(double i, double j);
        Point2D GridToGeographic(Point2D gridPosition);
        Point2D GeographicToGrid(Point2D geoPosition);
    }
}