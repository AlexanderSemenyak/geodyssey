#light

namespace BitImage
module Analysis

open Image

open Utility
open StructuringElementUtils
open DisjointSet
open HitAndMiss
open Morphology

// -- Neighbourhood

/// Return a integer for the the specified pixel where the
/// least significant 9 bits of the integer reflect the state
/// of the query pixel and its eight neightbours in the order
///   5 4 3
///   6 1 2
///   7 8 9
let Neighbourhood (image:IImage<bool>, i, j) =
  let structuringElement = SpiralStructuringElement 3
  neighbourScore structuringElement image i j

/// Given an integer representing the state of the nine
/// pixels (obtained by calling Neighbourhood), restrict
/// to the eight adjacent to the center pixel, ignoring the
/// center pixel
let ToEight (neighbourhood) =
  neighbourhood &&& 0b111111110

/// Given an integer representing the state of the nine
/// pixels (obtained by calling Neighbourhood), restrict
/// to the four adjacent to the center pixel, ignoring the
/// center pixel and the diagonal pixels
let ToFourCross (neighbourhood) =
  neighbourhood &&& 0b010101010
  
/// Given an integer representing the state of the nine
/// pixels (obtained by calling Neighbourhood), restrict
/// to the four diagonally adjacent to the center pixel,
/// ignoring the center pixel and the upright cross pixels
let ToFourDiagonal (neighbourhood) =
  neighbourhood &&& 0b101010100

/// Given an integer representing the state of the eight adjacent
/// pixels (obtained by calling Neighbourhood), determine the
/// number of 8-connected neightbours.
let EightConnectivity (neighbourhood) =
  bitsSet (ToEight neighbourhood)

/// Given an integer representing the state of the eight adjacent
/// pixels (obtained by calling Neighbourhood), determine the
/// number of 4-connected neightbours.
let FourConnectivity (neighbourhood) =
  bitsSet (ToFourCross neighbourhood)

/// Return a sequence of Discrete2D relative co-ordinates e.g. [(+1, -1), (0, -1)] to
/// connected cells based upon the supplied neighbourhood. Never returns
/// (0,0) for the center cell, even if it is set.
let ConnectedRelativeCoordinates (neighbourhood) =
  let bits =   [for b in 0 .. 8 -> (neighbourhood &&& (1 <<< b)) <> 0]
  let coords = [for b in 0 .. 8 -> spiral b]
  let foo bit coord =
    if bit then coord else (0, 0)
  let tupleToDiscrete2D (t:int * int) =
    let (i, j) = t
    new Numeric.Discrete2D(i, j)
  List.map2 foo bits coords |> List.filter (fun x -> x <> (0, 0)) |> Seq.map tupleToDiscrete2D
  
// -- Degree Map --

/// Given a (int -> int) function capable of converting a Neighbourhood
/// description into a measure of connectivity, compute the connectivity
/// for each pixel in image, and place the result in an IntegerImage
let Degree neighbourhoodToConnectivity (image:IImage<bool>) =
  let pixelDegree i j =
    if image.[i,j] then
      Neighbourhood (image, i, j) |> neighbourhoodToConnectivity
    else
      0
  new FastImage<int>(image.Width, image.Height, (fun i j -> pixelDegree i j)) :> IImage<int>

/// For set (foreground) pixels returns the 8-connectivity
/// degree of the pixel. For unset (background pixels)
/// return 0
let DegreeEight (image:IImage<bool>) =
  Degree EightConnectivity image

/// For set (foreground) pixels returns the 8-connectivity
/// degree of the pixel. For unset (background pixels)
/// return 0
let DegreeFour (image:IImage<bool>) =
  Degree FourConnectivity image

// -- Junction Map --

let JunctionElementX =
  StructuringElementFromStrings [ "101" ;
                                  "010" ;
                                  "101" ]
let JunctionElementPlus = 
  StructuringElementFromStrings [ "010" ;
                                  "111" ;
                                  "010" ]

let junction4Elements = [ JunctionElementX ; JunctionElementPlus ]

let JunctionElementT =
  StructuringElementFromStrings [ "   " ;
                                  "111" ;
                                  " 1 " ]
let JunctionElementY =
  StructuringElementFromStrings [ "1 1" ;
                                  " 1 " ;
                                  "1  " ]
let JunctionElementA =
  StructuringElementFromStrings [ " 1 " ;
                                  " 1 " ;
                                  "1 1" ]
let JunctionElement4 =
  StructuringElementFromStrings [ "  1" ;
                                  "11 " ;
                                  " 1 " ]
let junction3Elements =
  let tElements     = fourRotatedElements JunctionElementT
  let yElements     = fourRotatedElements JunctionElementY
  let aElements     = fourRotatedElements JunctionElementA
  let fourElements  = fourRotatedElements JunctionElement4
  List.concat [tElements ; yElements ; aElements ; fourElements]

// -- False Junctions --
let RejectJunction3Element =
  StructuringElementFromStrings [ "   " ;
                                  "010" ;
                                  "111" ]
let rejectJunction3Elements =
  fourRotatedElements RejectJunction3Element
      
// -- Free End Detection --
let EndElement1 =
  StructuringElementFromStrings [ "000" ;
                                  "010" ;
                                  "010" ]
let EndElement2 =
  StructuringElementFromStrings [ "000" ;
                                  "010" ;
                                  "001" ]
let EndElement3 =
  StructuringElementFromStrings [ "001" ;
                                  "011" ;
                                  "000" ]
let EndElement4 =
  StructuringElementFromStrings [ "000" ;
                                  "011" ;
                                  "001" ]
let EndElement5 =
  StructuringElementFromStrings [ "000" ;
                                  "010" ;
                                  "111" ]
let endElements =
  let oneElements   = fourRotatedElements EndElement1
  let twoElements   = fourRotatedElements EndElement2
  let threeElements = fourRotatedElements EndElement3
  let fourElements  = fourRotatedElements EndElement4
  let fiveElements  = fourRotatedElements EndElement5
  List.concat [oneElements ; twoElements ; threeElements ; fourElements ; fiveElements]

/// Create an integer image of pixel connectivity, based on 'common sense'
/// notion of connectivity, which takes into account false junctions from
/// multiple connections to adjacent neighbours. Background pixels = 0,
/// End points = 1, Mid points = 2, Triple junctions = 3, Cross junctions = 4
let Connectivity (image:IImage<bool>) = 
  let connectivity i j =
     if image.[i, j] then
       if  (isHitAndMissAny false (fun a b -> a = b) junction4Elements image i j) then
         4
       else
         if (isHitAndMissAny false (fun a b -> a = b) junction3Elements image i j) then
           if (isHitAndMissAny false (fun a b -> a = b) rejectJunction3Elements image i j) then
             2
           else
             3
         else
           if (isHitAndMissAny true (fun a b -> a = b) endElements image i j) then
             1
           else
             2
     else
       0
  new FastImage<int>(image.Width, image.Height, (fun i j -> connectivity i j)) :> IImage<int>

// -- Connected Components --

/// Determine an integer label for a pixel based upon the
/// labels of its neighbours to the E, NE, N and NW. Returns
/// a tuple containing the integer label assigned to the query
/// pixel and a list of equivalent labels from the same connected
/// component.
let labelPixel (labelMap:IImage<int>) labelCount i j =
  let labelValue p q =
    if labelMap.IsInRange(p, q) then labelMap.[p, q] else 0
  let e  = labelValue (i - 1)  j
  let ne = labelValue (i - 1) (j + 1)
  let n  = labelValue  i      (j + 1)
  let nw = labelValue (i + 1) (j + 1)
  let neighbourValues = [e ; ne ; n ; nw]
  let neighbourLabels = List.filter (fun x -> x <> 0) neighbourValues
  let num = List.length neighbourLabels
  match num with
  | 0   -> (labelCount + 1, [])
  | 1   -> (neighbourLabels.Head, [])
  | num -> (neighbourLabels.Head, neighbourLabels)

/// Create an IntegerImage where each foreground pixel in image is
/// assigned to a connected component according to its 8-connectivity
/// The resulting IntegerImage has zero for background pixels and
/// 1 .. numComponents for foreground pixels.
let ConnectedComponents (image:IImage<bool>) =
  // We use explicit loops in this operation because we must
  // have strict control over the traversal order, which is not guaranteed by Array2.iteri, etc.
  // First pass is to label each pixel
  let labelMap = new FastImage<int>(image.Width, image.Height)
  let mutable labelCount = 0
  let mutable equivalences = []
  for j = image.Height - 1 downto 0 do 
    for i = 0 to image.Width - 1 do
        if image.[i, j] then
          let (result, equiv) = labelPixel labelMap labelCount i j
          if result > labelCount then
            labelCount <- result
          if not (List.isEmpty equiv) then
            equivalences <- equiv :: equivalences
          labelMap.[i, j] <- result
      done;
  done;
  
  // Tidy up the equivelence relations into equivalance sets
  let equivalenceSets = List.map Set.of_list equivalences |> List.filter (fun s -> Set.count s > 1) |> Set.of_seq
 
  // Build the disjoint sets (one per connected component) from the equivalence sets
  let disjointSets = new DisjointSets(labelCount)
  let labelToElement label = label - 1 // Index is zero based, but first label is one
  let makeEquivalent equivalenceSet = disjointSets.EquivalenceSet (Seq.map labelToElement equivalenceSet)
  Seq.iter makeEquivalent equivalenceSets
  
  // Map from the label number to the component number to build the connected components map    
  let elementToSetMapping = disjointSets.RepresentativeElements
  let setToComponent set = set + 1  
  let labelToComponent label =
    match label with
    | 0     -> 0
    | label -> label |> labelToElement |> disjointSets.Find |> elementToSetMapping.Item |> setToComponent  
  new FastImage<int>(image.Width, image.Height, (fun i j -> labelToComponent labelMap.[i,j])) :> IImage<int>

let Threshold ((image:IImage<float>), threshold) =
  new FastImage<bool>(image.Width, image.Height, (fun i j -> image.[i, j] > threshold)) :> IImage<bool>
