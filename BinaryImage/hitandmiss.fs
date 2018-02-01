#light

module BitImage.HitAndMiss

open BitImage

// TODO: Use this general hit and miss operator elsewhere for refactoring
// Edge success determines whether structing elements can ever match at edge pixels
// of the image. If edgeSuccess is true, structuring elements can match, if edgeSuccess
// is false, structuringElements cannot match.
let isHitAndMissMatch edgeSuccess relation (sElem:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) i j =
  // TODO: Create some sort of iterator until true. Is this
  // or try to use List.first for this.
  let rec testPixels coords =
    if List.nonempty coords then
      let (p, q) = coords.Head in
      let s = i + p in 
      let t = j + q in
      if sElem.[p, q].HasValue then 
        if image.InRange(s, t) then
          // TODO: Need to be able to pass in the relational operator on the next line
          //if sElem.[p, q].Value = image.[s, t] then // Check for exact match
          if (relation (sElem.[p, q].Value) (image.[s, t])) then
            testPixels coords.Tail // true - Structuring element matches, check next pixel
          else
            false // Structuring element does not match
        else // TODO: Could put an elif here
          if edgeSuccess then 
            testPixels coords.Tail // No test - test more pixels - controls behaviour at edge
          else
            false // Structuring element does not match at edge
      else
        testPixels coords.Tail // Skip this test pixel
    else
      true // Structuring element matches
  sElem.CoordList |> testPixels

let isHitAndMissAny edgeSuccess relation (sElems:StructuringElement<System.Nullable<bool>> list) (image:BinaryImage) i j =
  let isMatch sElem =
    isHitAndMissMatch edgeSuccess relation sElem image i j
  // TODO: Create some sort of iterator until true. Is this
  // or try to use List.first for this.
  let rec applyJunctionElements matched es =
      if matched then
        true
      else
        if List.nonempty es then
          applyJunctionElements (isMatch es.Head) es.Tail
        else
          matched
  applyJunctionElements false sElems