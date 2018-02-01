using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace Image
{
    public class HAM
    {
        public FastImage<bool> HitAndMissTransform1(bool edgeSuccess, FastImage<bool> source, FastImage<bool?> sElem, Func<bool, bool, bool> relation)
        {
            FastImage<bool> target = (FastImage<bool>) source.CloneSize();
            for (int j = 0 ; j < source.Height; ++j)
            {
                for (int i = 0 ; i < source.Width; ++i)
                {
                    target[i, j] = ProcessPixel1(edgeSuccess, source, sElem, relation, i, j);
                }
            }
            return target;
        }

        private bool ProcessPixel1(bool edgeSuccess, FastImage<bool> source, FastImage<bool?> sElem, Func<bool, bool, bool> relation, int i, int j)
        {
            for (int q = 0; q < sElem.Height; ++q)
            {
                for (int p = 0; p < sElem.Width; ++p)
                {
                    if (sElem[p, q].HasValue)
                    {
                        int s = i + p;
                        int t = j + q;
                        if (source.IsInRange(s, t))
                        {
                            if (!relation(sElem[p, q].Value, source[s, t]))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (!edgeSuccess)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool ProcessPixel2(bool edgeSuccess, FastImage<bool> source, FastImage<bool?> sElem, Func<bool, bool, bool> relation, int i, int j)
        {
            for (int q = 0; q < sElem.Height; ++q) // compile time
            {
                for (int p = 0; p < sElem.Width; ++p) // compile time
                {
                    if (sElem[p, q].HasValue) // compile time
                    {
                        int s = i + p;
                        int t = j + q;
                        int source_index = t * source.Height + s;
                        if (source_index >= 0 && source_index < source.Buffer.Length)
                        {
                            // Determine whether the source image value affects the result for this
                            // structuring element value
                            if (relation(sElem[p, q].Value, true) == relation(sElem[p, q].Value, false)) // compile time
                            {
                                if (!relation(sElem[p, q].Value, true)) // Can be evaluated at 'compile time'
                                {
                                    return false;
                                }
                            }
                            else // compile time 
                            {
                                if (!relation(sElem[p, q].Value, source.Buffer[source_index])) // Must be evaluated at 'run time'
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (!edgeSuccess) // Can be evaluated at compile time
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        

        private bool ProcessPixel2IL(bool edgeSuccess, FastImage<bool> source, FastImage<bool?> sElem, Func<bool, bool, bool> relation, int i, int j)
        {
            DynamicMethod dynameth = new DynamicMethod("Go", typeof (bool), new Type[] {typeof (bool[])}, GetType());
            ILGenerator generator = dynameth.GetILGenerator();

            // Define local variables
            generator.DeclareLocal(typeof(int)); // Index 0 : source_index
            for (int q = 0; q < sElem.Height; ++q) // compile time
            {
                for (int p = 0; p < sElem.Width; ++p) // compile time
                {
                    if (sElem[p, q].HasValue) // compile time
                    {
                        Label nextIteration = generator.DefineLabel();
                        int offset = q * source.Height + p;
                        
                        // Put source array on the stack
                        generator.Emit(OpCodes.Ldarg_0);

                        // Define labels for branching
                        Label labelLessThanZero = generator.DefineLabel();
                        Label labelGreaterThan = generator.DefineLabel();
                        Label labelLoopBottom = generator.DefineLabel();

                        generator.Emit(OpCodes.Ldloc_0);        // source index on stack
                        generator.Emit(OpCodes.Ldc_I4, offset); // offset on stack
                        generator.Emit(OpCodes.Add);            // add the two
                        generator.Emit(OpCodes.Dup);            // duplicate twice
                        generator.Emit(OpCodes.Dup);            

                        // TODO: Remove these checks for internal pixels
                        // Check if less than zero (pops first index)
                        generator.Emit(OpCodes.Ldc_I4_0);
                        generator.Emit(OpCodes.Blt_S, labelLessThanZero);

                        // Check if greater than zero (pops second index)
                        generator.Emit(OpCodes.Ldc_I4, source.Buffer.Length);
                        generator.Emit(OpCodes.Bge_S, labelGreaterThan);

                        // Determine whether the source image value affects the result for this
                        // structuring element value
                        if (relation.Evaluate(sElem[p, q].Value, true) == relation.Evaluate(sElem[p, q].Value, false)) // compile time
                        {
                            if (!relation.Evaluate(sElem[p, q].Value, true)) // Can be evaluated at 'compile time'
                            {
                                // We can compute the result of the relation at compile time
                                generator.Emit(OpCodes.Ldc_I4_0); // return false
                                generator.Emit(OpCodes.Ret);
                            }
                            else
                            {
                                // TODO: Go to next iteration - continue
                            }
                        }
                        else // compile time 
                        {
                            generator.Emit(OpCodes.Ldelem_I4);                 // Access array
                            generator.Emit(OpCodes.Ldc_I4, sElem[p, q].Value ? 1 : 0); // Structuring element value on stack
                            relation.GenerateIL(generator);
                            generator.Emit(OpCodes.Brtrue_S, nextIteration);
                            // We can compute the result of the relation at compile time
                            generator.Emit(OpCodes.Ldc_I4_0); // return false
                            generator.Emit(OpCodes.Ret);
                        }
                        generator.MarkLabel(nextIteration);
                    }
                }
            }

        }
    }
}
