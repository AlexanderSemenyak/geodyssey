#light

module BitImage.Utility

// -- Utility functions --

let bitsSet u =
  let v = u - ((u >>> 1) &&& 0x55555555)
  let v = (v &&& 0x33333333) + ((v >>> 2) &&& 0x33333333)
  ((v + (v >>> 4) &&& 0xF0F0F0F) * 0x1010101) >>> 24

let anyTrue s = 
  Seq.fold (fun acc x -> acc || x) false s

let rec interleave xs ys =
  match xs, ys with
  |  x::xs, y::ys -> x::y::interleave xs ys
  |  []   , ys    -> ys
  |  xs   , []    -> xs

let charToNullableBool (s:char) =
  match s with
  | '0' -> new System.Nullable<bool>(false)
  | '1' -> new System.Nullable<bool>(true)
  |  s  -> new System.Nullable<bool>()

let stringToNullableBools (strs:string) =
  let chars = strs.ToCharArray() |> Array.to_list
  List.map charToNullableBool chars

let stringListToNullableBools x =
  List.map stringToNullableBools x

let stringToBools (strs:string) =
  let chars = strs.ToCharArray() |> Array.to_list
  List.map (fun x -> x <> '0') chars

let stringListToBools x =
  List.map stringToBools x

let intToBool n i =
  [ for b in 0 .. n - 1 -> 1 <<< b &&& i <> 0]

let intsToBools n x =
  List.map (intToBool n) x

let array2FromListOfLists lol =
  let nRows = List.length lol in
  let rowLengths = List.map List.length lol in
  let nCols = List.fold1_left max rowLengths in
  let arr = Array2.zero_create nCols nRows
  let setRow j rowVals =
    let setElem i elem = Array2.set arr i j elem in
    List.iteri setElem rowVals
  List.iteri setRow lol;
  arr
  
/// Integer spiral - generate the co-ordinates of
/// numbers along the integer spiral
let spiral i =
  match i with
  | 0 -> (0, 0)
  | x -> 
    let n  = truncate (0.5 * (sqrt (float i) + 1.0)) in
    let d  = i - (2*n - 1) * (2*n - 1) in
    let k  = d / (2*n)
    and o  = d % (2*n) in
    let sx = 1 -  (k    &&& 2)
    and sy = 1 - ((k+3) &&& 2)
    and dx =      k    &&& 1
    and dy =     (k+3) &&& 1 in
    (sx * (n - dx * (o + 1)), sy * (n - dy * (o + 1)))