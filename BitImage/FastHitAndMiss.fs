#light
#nowarn "57"
#nowarn "40"

// This module implements a highly specialised compiler for an image processing
// morphological hit-and-miss operators Domain Specific Language.
// The DSL expression are F# quotations of a specific
// form and with a very limited syntax

namespace BitImage
module FastHitAndMiss

open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.ExprShape
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Linq.QuotationEvaluation

open Image
open StructuringElement

// Evaluating a quoted expression
// of the form bool -> bool -> bool
// TODO: Use generic parameter
let eval (expr:Expr<(bool -> bool -> bool)>) =
  expr.Eval()


let generateIsHitAndMissMatch edgeSuccess (foo:Expr<(bool -> bool -> bool)>) (relation:Expr<(bool -> bool -> bool)>) (sElem:StructuringElement<System.Nullable<bool>>) (image:IImage<bool>) =
  let dynameth = new DynamicMethod("Go", typeof<bool>, [| typeof<IImage<bool>> ; typeof<int> ; typeof<int> |], MethodInfo.GetCurrentMethod().Module)
  let generator = dynameth.GetILGenerator()
  
  // Declare local variables used by the routine
  generator.DeclareLocal(typeof<int>)  |> ignore // s
  generator.DeclareLocal(typeof<int>)  |> ignore// t
  generator.DeclareLocal(typeof<bool>) |> ignore// el
  generator.DeclareLocal(typeof<bool>) |> ignore// im
  
  if (image :? FastImage<bool>) then
      generator.DeclareLocal(typeof<bool[]>) |> ignore // image.Buffer
      generator.DeclareLocal(typeof<int>)    |> ignore // image.Height
      
      
  // A mapping from the names of variables in the supplied quoted expressions
  // to local variable storage locations in the generated IL
  let locals = Dictionary<string, int>()
  locals.Add("s", 0)
  locals.Add("t", 1)
  locals.Add("el", 2)
  locals.Add("im", 3)
  
  if (image :? FastImage<bool>) then
    locals.Add("bf", 4)
    locals.Add("ht", 5)
   
  /// <summary>
  /// Create an emit an Ldloc instruction using the
  /// supplied generator to load a local from the
  /// specified index onto the execution stack.
  /// </summary>
  let compileLdloc index =
    match index with
    | 0     -> generator.Emit(OpCodes.Ldloc_0)
    | 1     -> generator.Emit(OpCodes.Ldloc_1)
    | 2     -> generator.Emit(OpCodes.Ldloc_2)
    | 3     -> generator.Emit(OpCodes.Ldloc_3)
    | index -> generator.Emit((if index < 256 then OpCodes.Ldloc_S else OpCodes.Ldloc), index)

  let compileLdcBool v =
    if v then
      generator.Emit(OpCodes.Ldc_I4_1)
    else
      generator.Emit(OpCodes.Ldc_I4_0) 

  let compileHitAndMiss compiler =
    let missLabel = generator.DefineLabel()
    let endHitAndMiss = generator.DefineLabel()
    
    if (image :? FastImage<bool>) then
       // Store the image.Buffer in local variable 4
       generator.Emit(OpCodes.Ldarg_0) // image on the stack
       generator.Emit(OpCodes.Call, typeof<FastImage<bool>>.GetMethod("get_Buffer")) // image.Buffer consuming image from the stack and returning the buffer
       generator.Emit(OpCodes.Stloc_S, 4)
       
       // Store the image.Height in local variable 5
       generator.Emit(OpCodes.Ldc_I4, image.Height) // image.Height on the stack -- TODO: Pull this out into a local variable
       generator.Emit(OpCodes.Stloc_S, 5) 
    
    let rec compileTestPixels coords =
      if not (List.isEmpty coords) then
        let (p, q) = coords.Head in
        
        if sElem.[p, q].HasValue then
          let nextPixelLabel = generator.DefineLabel()
          if (eval relation sElem.[p, q].Value false) <> (eval relation sElem.[p, q].Value true) then
            // TODO: Remember the else clause for this if
            // Call to IsInRange
            generator.Emit(OpCodes.Ldarg_0)   // image on the stack
            generator.Emit(OpCodes.Ldarg_1)   // i on the stack
            generator.Emit(OpCodes.Ldc_I4, p) // p on the stack
            generator.Emit(OpCodes.Add)       // s = i + p on the stack
            generator.Emit(OpCodes.Dup)       // duplicate s
            generator.Emit(OpCodes.Stloc_0)   // store s in loc 0
            generator.Emit(OpCodes.Ldarg_2)   // j on the stack
            generator.Emit(OpCodes.Ldc_I4, q) // q on the stack
            generator.Emit(OpCodes.Add)       // t = j + q on the stack
            generator.Emit(OpCodes.Dup)       // duplicate t
            generator.Emit(OpCodes.Stloc_1)   // store t in loc 1
            generator.Emit(OpCodes.Callvirt, typeof<FastImage<bool>>.GetMethod("IsInRange")) // call image.IsInRange(s, t) using consuming the duplicated values from the stack
            generator.Emit(OpCodes.Brfalse, if edgeSuccess then nextPixelLabel else missLabel) // TODO Could use short branch for first clause
            generator.Emit(OpCodes.Ldc_I4, if sElem.[p, q].Value then 1 else 0)                  // Structuring element pixel on the stack
            generator.Emit(OpCodes.Stloc_2)   // Store el here in loc_2 
          
            // Retrieve the underlying array containing the image data
            
           
            // Do do type check and generate the correct code for each type
            if (image :? FastImage<bool>) then
              // This block only works for FastImage. 
              
              // Load the reference to Buffer onto the stack
              generator.Emit(OpCodes.Ldloc_S, 4) // image.Buffer on the stack
              
              // Compute the index into the array = s * height + t
              generator.Emit(OpCodes.Ldloc_0)     // s on the stack
              generator.Emit(OpCodes.Ldloc_S, 5)  // image.Height on the stack
              generator.Emit(OpCodes.Mul)         // s * image.Height is on the stack
              generator.Emit(OpCodes.Ldloc_1)     // t on the stack
              generator.Emit(OpCodes.Add)         // s * image.Height + t is on the stack
              
              // Access the array
              generator.Emit(OpCodes.Ldelem_U1)            // Get the pixel value 'im'
              generator.Emit(OpCodes.Stloc_3)              // Store im here in loc_3
            else
              generator.Emit(OpCodes.Ldarg_0) // image on the stack
              generator.Emit(OpCodes.Ldloc_0) // s on the stack
              generator.Emit(OpCodes.Ldloc_1) // t on the stack
              generator.Emit(OpCodes.Callvirt, typeof<IImage<bool>>.GetMethod("get_Item")) |> ignore // image.[s, t] consuming values from the stack and returning the pixel
              generator.Emit(OpCodes.Stloc_3) // Store el here in loc_3
                        
            compiler relation
          
            generator.Emit(OpCodes.Brfalse, missLabel)
          else
            if not (eval relation sElem.[p, q].Value false) then
              generator.Emit(OpCodes.Br, missLabel)
                
          generator.MarkLabel(nextPixelLabel)
          
        compileTestPixels coords.Tail
          
    sElem.CoordList |> compileTestPixels

    // hit (true)
    generator.Emit(OpCodes.Ldc_I4_1)  // true on stack
    generator.Emit(OpCodes.Br_S, endHitAndMiss)       // return
    
    // miss (false)
    generator.MarkLabel(missLabel)
    generator.Emit(OpCodes.Ldc_I4_0)  // false on stack
    
    generator.MarkLabel(endHitAndMiss)
    //generator.Emit(OpCodes.Nop)

  /// <summary>
  /// Compile a quoted expression of the form and emit the
  /// generated byte codes using the supplied generator.
  /// bool -> bool -> bool.
  /// </summary>
  /// <param name="">
  /// An expression which takes the state of the current pixel 'px' and the result of the hit
  /// and miss operator for the current pixel 'op' and computes the state of the corresponding
  /// pixel in the result image.
  /// </param>
  /// <param name="expr">
  /// The boolean expression to be compiled; a function of the form (bool -> bool -> bool) used
  /// to compare each element in the structuring element with the corresponding pixel. The function
  /// should take two arguments: 'im' - the image pixel and 'el' the structuring element pixel.
  /// </param>
  /// <param name="locals">A map of names of parameters to local variable indices in the CIL code</param>
  let rec compile expr =
    match expr with
    | Lambda(var, body)         -> compile body
    | IfThenElse(cond, t, f)    -> compileIfToOperator cond t f 
    | Call(inst, meth, l)       -> compileCall inst meth l
    | Bool v                    -> compileLdcBool v
    | Var(v)                    -> compileVar v 
    | _                         -> failwith "Unknown"
  and compileIfToOperator cond t f =
    match (t, f) with
    | (Bool true , _) -> compileOr cond f 
    | (_, Bool false) -> compileAnd cond t 
    | _               -> compileIf cond t f
  and compileIf cond t f =
    compile cond
    let trueLabel = generator.DefineLabel()
    let elseLabel = generator.DefineLabel()
    generator.Emit(OpCodes.Brfalse, elseLabel)
    compile t
    generator.Emit(OpCodes.Br, trueLabel)
    generator.MarkLabel(elseLabel)
    compile f
    generator.MarkLabel(trueLabel)
  and compileCall inst (meth:MethodInfo) (args:Expr list) =
    match meth.Name with
    | "not"            -> compileNot (List.hd args)
    | "op_Equality"    -> compileEq (List.hd args) (List.hd (List.tl args))
    | "op_LessGreater" -> compileNe (List.hd args) (List.hd (List.tl args))
    | _                -> failwith "Unknown"
  and compileVar (v:Var) =
    // When compiling a 'variable' in the expression we either retrieve a known local variable
    // (if it is registered in the locals dictionary) or if not we insert inline code which
    // evaluates to the value of the pseudovariable.
    if locals.ContainsKey(v.Name) then
      compileLdloc locals.[v.Name]
    else
      match v.Name with
      | "op" -> compileHitAndMiss compile
      | "px" -> generator.Emit(OpCodes.Ldarg_0) // image on the stack // TODO: Use the fast technique here too
                generator.Emit(OpCodes.Ldarg_1) // i on the stack
                generator.Emit(OpCodes.Ldarg_2) // j on the stack
                generator.Emit(OpCodes.Callvirt, typeof<FastImage<bool>>.GetMethod("get_Item")) |> ignore // image.[i, j] consuming values from the stack and returning the pixel
      | _    -> failwith "Unknown var"
  and compileOr lhs rhs =
    compile lhs 
    compile rhs
    generator.Emit(OpCodes.Or)
  and compileAnd lhs rhs =
    compile lhs 
    compile rhs
    generator.Emit(OpCodes.And)
  and compileNot operand =
    compile operand
    generator.Emit(OpCodes.Not)
  and compileEq lhs rhs =
    compile lhs 
    compile rhs
    generator.Emit(OpCodes.Ceq)
  and compileNe lhs rhs =
    compileEq lhs rhs
    generator.Emit(OpCodes.Not)
  
  compile foo
  
  generator.Emit(OpCodes.Ret) // Target for final branch
   
  let del = dynameth.CreateDelegate(typeof<System.Func<IImage<bool>, int, int, bool>>)
  del :?> System.Func<IImage<bool>, int, int, bool>
