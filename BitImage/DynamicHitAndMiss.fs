#light

namespace BitImage
module DynamicHitAndMiss

open System.Reflection.Emit

open Microsoft.FSharp
open Utility
open StructuringElement

// Optimisation using runtime bytecode generation as in Petzold's example in beautiful code
let isHitAndMissMatchILGenerator (edgeSuccess:bool) generateRelationOpCodes (sElem:StructuringElement<System.Nullable<bool>>) (image:BinaryImage) i j =
    // Produce a list of (p, q) structureing element co-ordinates which contain values
    let nonNullCoordList = [ for (p, q) in sElem.CoordList do
                               if sElem.[p, q].HasValue then 
                                 yield (p, q) ]
    let dynameth = new DynamicMethod("DynamicHitAndMiss", typeof<unit>,
                                     [| typeof<BinaryImage>; // image
                                        typeof<int>;         // i
                                        typeof<int>;         // j
                                     |] ) 
    let generator = dynameth.GetILGenerator()
    // Declare locals
    generator.DeclareLocal(typeof<int>) |> ignore // Local 0 : image index s = i + p
    generator.DeclareLocal(typeof<int>) |> ignore // Local 1 : image index t = j + q
    // Initialize locals
    generator.Emit(OpCodes.Ldc_I4_0) |> ignore
    generator.Emit(OpCodes.Stloc_0) |> ignore
    let labelTop = generator.DefineLabel()
    generator.MarkLabel(labelTop);
        
    let returnFalseMarker = generator.DefineLabel()    
        
    /// Generate the IL for processing one cell of the structuring element
    let elementCellIL (coord:int * int) =
      let (p, q) = coord
      let nextIterationMarker = generator.DefineLabel()
              
      generator.Emit(OpCodes.Ldarg_0)   |> ignore // Load image
      generator.Emit(OpCodes.Ldc_I4, p) |> ignore // Load p
      generator.Emit(OpCodes.Ldarg_1)   |> ignore // Load i
      generator.Emit(OpCodes.Add)       |> ignore // Add
      generator.Emit(OpCodes.Dup)       |> ignore // Duplicate    
      generator.Emit(OpCodes.Stloc_0)   |> ignore // Store s
      generator.Emit(OpCodes.Ldc_I4, q) |> ignore // Load q
      generator.Emit(OpCodes.Ldarg_2)   |> ignore // Load j
      generator.Emit(OpCodes.Add)       |> ignore // Add
      generator.Emit(OpCodes.Dup)       |> ignore // Duplicate
      generator.Emit(OpCodes.Stloc_1)   |> ignore // Store t
      generator.Emit(OpCodes.Call, typeof<BinaryImage>.GetMethod("InRange")) |> ignore
      let labelOutOfRange = generator.DefineLabel()
      generator.Emit(OpCodes.Brfalse_S,
                     if edgeSuccess then
                       nextIterationMarker
                     else
                       returnFalseMarker)
      generator.Emit(OpCodes.Ldarg_0)   |> ignore // Load image
      generator.Emit(OpCodes.Ldloc_0)   |> ignore // Load s
      generator.Emit(OpCodes.Ldloc_1)   |> ignore // Load t
      // Test image pixel [s, t] against sElem[p, q] using relation
      generator.Emit(OpCodes.Call, typeof<BinaryImage>.GetMethod("Item")) |> ignore
      generator.Emit(OpCodes.Ldc_I4, sElem.[p, q].Value |> boolToInt) |> ignore
      generateRelationOpCodes generator
      generator.Emit(OpCodes.Brfalse, returnFalseMarker)               
      generator.MarkLabel(nextIterationMarker) |> ignore
       
    List.iter elementCellIL nonNullCoordList
       
    // Return true
    generator.Emit(OpCodes.Ldc_I4_1) |> ignore // true
    generator.Emit(OpCodes.Ret)      |> ignore // return
    
    // Return false
    generator.MarkLabel(returnFalseMarker) |> ignore
    generator.Emit(OpCodes.Ldc_I4_0) |> ignore  // false
    generator.Emit(OpCodes.Ret)      |> ignore  // return
     
    let generatedIL (image:BinaryImage) i j =
      dynameth.Invoke(null,  