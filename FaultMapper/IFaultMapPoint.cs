using Numeric;

namespace FaultMapper
{
    public interface IFaultMapPoint : IPositionable2D
    {
        Point3D? RightCutoff { get; set; }
        Point3D? LeftCutoff { get; set; }
        double? Throw { get; }
        double? Heave { get; }
    }
}