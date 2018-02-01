
#light

namespace BitImage

open System.Collections.Generic;
open Microsoft.FSharp.Collections.Array

// Class for maintaining a disjoint set data structure
// http://en.wikipedia.org/wiki/Disjoint-set_data_structure
// using an array based implementation from
// http://www.cs.berkeley.edu/~jrs/61bf06/lec/33
type DisjointSets = class
  val sets: int[]
  
  /// The number of disjoint sets
  member obj.NumSets
    with get () = obj.sets |> Array.filter (fun x -> (x < 0)) |> Array.length
    
  /// The number of elements in total in all of the disjoint sets
  member obj.Size
    with get () = obj.sets.Length
    
  /// Combine the two sets specified by their representative elements
  member obj.Union elem1 elem2 =
    assert (elem1 < obj.Size)
    assert (elem2 < obj.Size)
    assert (obj.sets.[elem1] < 0)
    assert (obj.sets.[elem2] < 0)
    if (elem1 <> elem2) then 
      if (obj.sets.[elem2] < obj.sets.[elem1]) then
        obj.sets.[elem2] <- obj.sets.[elem2] + obj.sets.[elem1]
        obj.sets.[elem1] <- elem2
        assert (obj.sets.[elem2] < 0)
      else
        obj.sets.[elem1] <- obj.sets.[elem1] + obj.sets.[elem2]
        obj.sets.[elem2] <- elem1
        assert (obj.sets.[elem1] < 0)
    assert (obj.sets.[elem1] < obj.Size)
    assert (obj.sets.[elem2] < obj.Size)
        
  /// Return the representative element for the set containing index
  member obj.Find elem =
    assert (elem < obj.Size)
    let rec find i =
      if (obj.sets.[i] < 0) then
        i
      else
        //obj.sets.[i] <- find (obj.sets.[i])
        //obj.sets.[i]
        find (obj.sets.[i])
    find elem
  
  /// The two elements specified are in the same set, so
  /// combine sets if necessary.  
  member obj.SameSet elem1 elem2 =
    assert (elem1 < obj.Size)
    assert (elem2 < obj.Size)
    obj.Union (obj.Find elem1) (obj.Find elem2)
  
  /// Make unions as necessary for all the members of the
  /// supplied equivalenceSet to be members of the same disjoint set
  member obj.EquivalenceSet equivalenceSet =
    Seq.iter (obj.SameSet (Seq.hd equivalenceSet)) equivalenceSet
  
  /// Create a mapping from representative elements to an integer set identifer. The
  /// sets are numbered from zero to numSets - 1. A new mapping will be created each
  /// time this method is called. The returned mapping is not updated for further
  /// changes to the DisjointSets object
  member obj.RepresentativeElements =
    let elementToSet = new System.Collections.Generic.Dictionary<int, int>()
    let enumerateSet i value =
      if (value < 0) then
        elementToSet.Add(i, elementToSet.Count)
    Seq.iteri enumerateSet obj.sets
    elementToSet
  
  /// Create a new DisjointSets object containing size elements,
  /// initally in size disjoint sets. The elements are numbered
  /// from zero to size - 1.
  new(size) = { sets = Array.create size (-1) }
end