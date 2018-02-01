#light

module Memoize

open System.Collections.Generic

// See Expert F# page 188 and
// http://weblogs.asp.net/podwysocki/archive/2008/08/01/recursing-into-recursion-memoization.aspx

type ITable<'a, 'b> =
  inherit System.IDisposable
  abstract Item : 'a -> 'b with get
  
let memoize f =
  let outerTable = new Dictionary<_,_>()
  { new ITable<_,_> with
      member t.Item
        with get(n) =
          if outerTable.ContainsKey(n) then
            outerTable.[n]
          else
            let res = f n
            outerTable.Add(n, res)
            res
      
      member t.Dispose() =
        outerTable.Clear()
  }
  