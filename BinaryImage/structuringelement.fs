
#light

namespace BitImage

open Microsoft.FSharp.Collections.Array2

[<System.Reflection.DefaultMember("Item")>]
type StructuringElement<'a> = class
  val pattern: 'a[,]
  val originI: int
  val originJ: int
  member obj.Pattern
    with get () = obj.pattern
  member obj.SizeI
    with get () = Array2.length1 obj.pattern
  member obj.SizeJ
    with get () = Array2.length2 obj.pattern
  member obj.Left
    with get () = -obj.originI
  member obj.Right
    with get () = obj.SizeI - 1 - obj.originI
  member obj.Bottom
    with get () = -obj.originJ
  member obj.Top
    with get () = obj.SizeJ - 1 - obj.originJ
  member obj.OriginI
    with get () = obj.originI
  member obj.OriginJ
    with get () = obj.originJ
  member obj.Item
    with get (i, j) = obj.pattern.[obj.originI + i, obj.originJ + j]
    and set (i, j, value) = obj.pattern.[obj.originI + i, obj.originJ + j] <- value
  member obj.CoordList =
    [ for x in obj.Left .. obj.Right
      for y in obj.Bottom .. obj.Top -> x,y ]
  new(originI, originJ, pattern) = {pattern = pattern ; originI = originI ; originJ = originJ}
end

