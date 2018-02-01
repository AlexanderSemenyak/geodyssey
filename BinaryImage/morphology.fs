
#light

module BitImage.Morphology

open System.Text;
open System.Collections.Generic;
open Microsoft.FSharp.Collections.Array2

open BitImage
open BitImage.Utility
open BitImage.HitAndMiss
open BitImage.StructuringElementUtils
 
// -- Erosion --
  
/// Erode the pixel at i, j in image using structuringElement
let erodePixel (sElem:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) i j =
  // In erosion we look for foreground pixels
  if image.[i, j] = true then
    isHitAndMissMatch true (fun el im -> (not el) || im) sElem image i j
  else
    false

/// Erode the image using the supplied structing element  
let Erode (structuringElement:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) =
  let erode = erodePixel structuringElement image
  new BinaryImage(Array2.init image.SizeI image.SizeJ erode)

/// Default Erosion operator using a square 3x3 structuring element
let Erosion (image) =
  Erode (SquareStructuringElement 3) image

// -- Dilation --

/// Dilate the pixel at i, j in image using structuringElement
let dilatePixel (sElem:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) i j =
  // In dilation we look for background pixels
  if image.[i, j] = false then
    not (isHitAndMissMatch true (fun el im -> not (el && im)) sElem image i j)
  else
    true

/// Dilate the image using the supplied structuring element
let Dilate (structuringElement:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) =
  let dilate = dilatePixel structuringElement image
  new BinaryImage(Array2.init image.SizeI image.SizeJ dilate)

/// Default dilation operator using a square 3x3 structuring element
let Dilation (image) =
  Dilate (SquareStructuringElement 3) image

// -- Opening --

/// Open the image using the supplied structuring element
let Open (structuringElement:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) =
  image |> Erode structuringElement |> Dilate structuringElement

/// Default opening operator using a square 3x3 structuring element
let Opening (image) =
  let structuringElement = SquareStructuringElement 3
  image |> Erode structuringElement |> Dilate structuringElement

// -- Closing --

/// Close the image using the supplied structuring element
let Close (structuringElement:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) =
  image |>  Dilate structuringElement |> Erode structuringElement

/// Default closing operator using a square 3x3 structuring element
let Closing (image) =
  let structuringElement = SquareStructuringElement 3
  image |>  Dilate structuringElement |> Erode structuringElement

// -- Pepper Filter --

let removePepperWithStructuringElement (sElem:StructuringElement<int>) (image:BinaryImage) i j =
  (neighbourScore sElem image i j) > 1
  
let removePepperPixel (sElem:StructuringElement<int>) (image:BinaryImage) i j =
  // We test the pixel against out structuring element
  if image.[i,j] = true then
    removePepperWithStructuringElement sElem image i j
  else
    false

/// Remove pepper noise using a square filter with width 'size' where
/// size if an odd number greater than two.
let PepperFilter size (image:BinaryImage) =
  assert ((size >= 3) && (size % 2 = 1))
  let structuringElement = SpiralStructuringElement size
  let pepper = removePepperPixel structuringElement image
  new BinaryImage(Array2.init image.SizeI image.SizeJ pepper)

/// Apply a square pepper filter of the given width where
/// size if an odd number greater than two.
let PepperFiltering (size, image) =
  PepperFilter size image

// -- Hit and Miss Transform --

let ThinningElementT =
  StructuringElementFromStrings [ "000" ;
                                  " 1 " ;
                                  "111" ]
let ThinningElementL =
  StructuringElementFromStrings [ " 00" ;
                                  "110" ;
                                  " 1 " ]
  
let thinningElements =
  let tElements = fourRotatedElements ThinningElementT
  let lElements = fourRotatedElements ThinningElementL
  interleave tElements lElements

let applyThinning (sElem:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) =
  //let thin = thinPixel sElem image
  let thin i j =
    if (isHitAndMissMatch true (fun el im -> el = im) sElem image i j) then
      false
    else
      image.[i, j]
  new BinaryImage(Array2.init image.SizeI image.SizeJ thin)

/// Apply one iteration of thinning. 
let Thin (image:BinaryImage) =
  let rec applyThinningElements accIm es =
    match es with
    | []     -> accIm
    | e::es  -> applyThinningElements (applyThinning e accIm) es 
  applyThinningElements image thinningElements

/// Repeatedly apply Thin to the image until there is
/// no further effect.
let ThinUntilConvergence (image:BinaryImage) =
  let rec thinUntil (prevIm:BinaryImage) =
    let nextIm = Thin prevIm
    match nextIm.IsEqual(prevIm) with
    |  true -> nextIm
    |  false -> thinUntil nextIm
  thinUntil image

// -- Block Thinning --

let BlockThinningElementSe =
  StructuringElementFromStrings [ " 0 " ;
                                  "111" ;
                                  " 11" ]

let BlockThinningElementSw =
  StructuringElementFromStrings [ " 0 " ;
                                  "111" ;
                                  "11 " ]
let blockThinningElements =
  let tElements = fourRotatedElements BlockThinningElementSe
  let lElements = fourRotatedElements BlockThinningElementSw
  interleave tElements lElements

/// Apply one iteration of block thinning to the image. Block
/// thinning deals with cases not dealt with by Thinning, and
/// is only recommended for application to images to which
/// ThinToConvergence has already been applied. 
let ThinBlock (image:BinaryImage) =
  let rec applyThinningElements accIm es =
    match es with
    | []     -> accIm
    | e::es  -> applyThinningElements (applyThinning e accIm) es 
  applyThinningElements image blockThinningElements

/// Repeatedly apply ThinBlock to the image until there is
/// no further effect. Only recommended for application to
/// images to which ThinToConvergence has already been applied.
let ThinBlockUntilConvergence (image:BinaryImage) =
  let rec thinUntil (prevIm:BinaryImage) =
    let nextIm = ThinBlock prevIm
    match nextIm.IsEqual(prevIm) with
    |  true -> nextIm
    |  false -> thinUntil nextIm
  thinUntil image
    
// -- Gap Filling --

let FillingElementNS =
  StructuringElementFromStrings [ " 1 " ;
                                  "000" ;
                                  " 1 " ]
let FillingElementNwSe =
  StructuringElementFromStrings [ "1 0" ;
                                  "000" ;
                                  "0 1" ]
let FillingElementNwS =
  StructuringElementFromStrings [ "1 0" ;
                                  "000" ;
                                  "010" ]
let FillingElementNeS =
  StructuringElementFromStrings [ "0 1" ;
                                  "000" ;
                                  "010" ]
let FillingElementNeSw =
  StructuringElementFromStrings [ "0 1" ;
                                  "000" ;
                                  "1 0" ]
let fillingElements =
  let nsElements   = twoRotatedElements FillingElementNS
  let nwseElements = twoRotatedElements FillingElementNwSe
  let neseElements = twoRotatedElements FillingElementNeSw
  let nesElements  = fourRotatedElements FillingElementNeS
  let nwsElements  = fourRotatedElements FillingElementNwS 
  List.concat [nsElements ; nwseElements ; neseElements ; nesElements ; nwsElements]

let fillPixel (sElem:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) i j =
  if (isHitAndMissMatch true (fun el im -> el = im) sElem image i j) then
    true
  else
    image.[i, j]

let applyFilling (sElem:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) =
  for i = sElem.OriginI to image.SizeI - sElem.Right - 1 do 
    for j = sElem.OriginJ to image.SizeJ - sElem.Top - 1 do 
      if image.[i, j] = false  then
        image.[i, j] <- fillPixel sElem image i j
    done;
  done
  image

/// Fill one pixel gaps in one pixel thick lines.  Only recommended
/// for use on images which have been thinned with ThinToConvergence
let Fill (image:BinaryImage) =
  let copy = new BinaryImage(Array2.copy image.Elements) // Modify this - implement IClonable on image
  let rec applyFillingElements accIm es =
    match es with
    | []     -> accIm
    | e::es  -> applyFillingElements (applyFilling e accIm) es 
  applyFillingElements copy fillingElements
