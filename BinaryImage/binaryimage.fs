#light

namespace BitImage

open System.Text;
open Microsoft.FSharp.Collections.Array2

// Class for storing binary images
[<System.Reflection.DefaultMember("Item")>]
type BinaryImage = class
  val elements: bool[,]
  member obj.SizeI
    with get () = Array2.length1 obj.elements
  member obj.SizeJ
    with get () = Array2.length2 obj.elements
  member obj.Item
    with get (i, j) = obj.elements.[i, j]
    and set (i, j, value) = obj.elements.[i, j] <- value
  member obj.Elements  // TODO: This breaks encapsulation
    with get () = obj.elements
  member obj.InRange(i, j) =
    i >= 0 && i < obj.SizeI && j >= 0 && j < obj.SizeJ
  member lhs.IsEqual(rhs:BinaryImage) =
    let equalElements rhsArr =
      let mutable res = true in
      let mutable i = 0 in
      while res && i < lhs.SizeI do
        let mutable j = 0 in
        while res && j < lhs.SizeJ do
          res <- lhs.elements.[i, j] = rhs.elements.[i, j]
          j <- j + 1
        done;
        i <- i + 1
      done;
      res 
    lhs.SizeI = rhs.SizeI && lhs.SizeJ = rhs.SizeJ && (equalElements rhs.elements)
  member obj.Copy() =
    new BinaryImage(Array2.copy obj.elements)
  override obj.ToString() =
    let sb = new StringBuilder()
    for j = obj.SizeJ - 1 downto 0 do 
      for i = 0 to obj.SizeI - 1 do 
        if obj.[i, j] then
          sb.Append('1') |> ignore
        else
          sb.Append('0') |> ignore
      done;
      sb.AppendLine() |> ignore
    done;
    sb.ToString()
  new(elem) = { elements = elem }
end
