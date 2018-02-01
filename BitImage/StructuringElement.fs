#light

namespace BitImage
module StructuringElement

open Memoize

let lazyGet computeValue optionValue =
  match !optionValue with
  | Some value -> value
  | None ->
      let value = computeValue()
      optionValue := Some value
      value

[<System.Reflection.DefaultMember("Item")>]
type StructuringElement<'a>(originI : int, originJ : int, pattern : 'a[,]) =
  let coords = ref None : (int * int) list option ref
  member obj.Pattern
    with get () = pattern
  member obj.SizeI
    with get () = Array2D.length1 pattern
  member obj.SizeJ
    with get () = Array2D.length2 pattern
  member obj.Left
    with get () = -originI
  member obj.Right
    with get () = obj.SizeI - 1 - originI
  member obj.Bottom
    with get () = -originJ
  member obj.Top
    with get () = obj.SizeJ - 1 - originJ
  member obj.OriginI
    with get () = originI
  member obj.OriginJ
    with get () = originJ
  member obj.Item
    with get (i, j) = pattern.[originI + i, originJ + j]
    and set (i, j, value) = pattern.[originI + i, originJ + j] <- value
  member private obj.computeCoords () =
    [ for x in obj.Left .. obj.Right do
      for y in obj.Bottom .. obj.Top do yield x,y ]
  member obj.CoordList
    with get () = lazyGet obj.computeCoords coords




