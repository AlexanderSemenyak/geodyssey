#light

namespace BitImage
module Morphology

// Signature files may open namespaces.  The namespaces Microsoft.FSharp
// and Microsoft.FSharp.Compatibility.OCaml are implicitly opened.

open Image

// Signatures for functions and simple values are given using 'val':

val Erosion   : IImage<bool> -> IImage<bool>
val Dilation  : IImage<bool> -> IImage<bool>
val Opening   : IImage<bool> -> IImage<bool>
val Closing   : IImage<bool> -> IImage<bool>
val Thin      : IImage<bool> -> IImage<bool>
val ThinBlock : IImage<bool> -> IImage<bool>
val Fill      : IImage<bool> -> IImage<bool>
val ThinUntilConvergence : IImage<bool> -> IImage<bool>
val ThinBlockUntilConvergence : IImage<bool> -> IImage<bool>
val PepperFiltering : int * IImage<bool> -> IImage<bool>
val Invert : IImage<bool> -> IImage<bool>


