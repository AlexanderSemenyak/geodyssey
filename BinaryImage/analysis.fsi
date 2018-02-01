module BitImage.Analysis

// Signature files may open namespaces.  The namespaces Microsoft.FSharp
// and Microsoft.FSharp.Compatibility.OCaml are implicitly opened.

open BitImage

// Signatures for functions and simple values are given using 'val':

val Neighbourhood : BinaryImage * int * int -> int
val ToEight : int -> int
val ToFourCross : int -> int
val ToFourDiagonal : int -> int
val EightConnectivity : int -> int
val FourConnectivity : int -> int
val DegreeEight : BinaryImage -> IntegerImage
val DegreeFour : BinaryImage -> IntegerImage
val Connectivity : BinaryImage -> IntegerImage
val ConnectedComponents : BinaryImage -> IntegerImage
val ConnectedRelativeCoordinates : int -> seq<Numeric.Discrete2D>