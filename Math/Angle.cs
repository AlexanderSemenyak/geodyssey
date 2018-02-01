/*
 * Created by SharpDevelop.
 * User: rjs
 * Date: 2007-04-29
 * Time: 00:18
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Numeric
{
	/// <summary>
	/// Description of Angle.
	/// </summary>
	public class Angle
	{
	    public static double DegreesToRadians(double degrees)
	    {
	        return (Math.PI / 180.0) * degrees;
	    }
	    
	    public static double RadiansToDegrees(double radians)
	    {
	        return (180.0 / Math.PI) * radians;
	    }
	}
}
