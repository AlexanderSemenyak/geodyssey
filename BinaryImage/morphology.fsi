
module BitImage.Morphology

// Signature files may open namespaces.  The namespaces Microsoft.FSharp
// and Microsoft.FSharp.Compatibility.OCaml are implicitly opened.

open BitImage

// Signatures for functions and simple values are given using 'val':

val Erosion   : BinaryImage -> BinaryImage
val Dilation  : BinaryImage -> BinaryImage
val Opening   : BinaryImage -> BinaryImage
val Closing   : BinaryImage -> BinaryImage
val Thin      : BinaryImage -> BinaryImage
val ThinBlock : BinaryImage -> BinaryImage
val Fill      : BinaryImage -> BinaryImage
val ThinUntilConvergence : BinaryImage -> BinaryImage
val ThinBlockUntilConvergence : BinaryImage -> BinaryImage
val PepperFiltering : int * BinaryImage -> BinaryImage

