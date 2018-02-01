#light

namespace BitImage
module Analysis

// Signature files may open namespaces.  The namespaces Microsoft.FSharp
// and Microsoft.FSharp.Compatibility.OCaml are implicitly opened.

open Image

// Signatures for functions and simple values are given using 'val':

val Neighbourhood : IImage<bool> * int * int -> int
val ToEight : int -> int
val ToFourCross : int -> int
val ToFourDiagonal : int -> int
val EightConnectivity : int -> int
val FourConnectivity : int -> int
val DegreeEight : IImage<bool> -> IImage<int>
val DegreeFour : IImage<bool> -> IImage<int>
val Connectivity : IImage<bool> -> IImage<int>
val ConnectedComponents : IImage<bool> -> IImage<int>
val ConnectedRelativeCoordinates : int -> seq<Numeric.Discrete2D>
val Threshold : IImage<double> * float -> IImage<bool>
