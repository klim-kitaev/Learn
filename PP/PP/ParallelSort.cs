using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PP
{
    public class ParallelSort<T>
    {

        internal static void doSort(T[]data, int startIndex,int endIndex,IComparer<T> comparer, int depth, int maxDepth,int minBlockSize)
        {
            if (startIndex < endIndex)
            {
                if(depth>maxDepth || endIndex - startIndex < minBlockSize)
                {
                    Array.Sort(data, startIndex, endIndex - startIndex + 1, comparer);
                }
                else
                {
                    
                }
            }
        }

        internal static int partitionBlock(T[]data,int startIndex,int endIndex,IComparer<T> comparer)
        {
            // get the pivot value - we will be comparing all
            // of the other items against this value
            T pivot = data[startIndex];
            // put the pivot value at the end of block
            swapValues(data, startIndex, endIndex);
            // index used to store values smaller than the pivot
            int storeIndex = startIndex;
            // iterate through the items in the block
            for (int i = startIndex; i < endIndex; i++)
            {
                // look for items that are smaller or equal to the pivot
                if (comparer.Compare(data[i], pivot) <= 0)
                {
                    // move the value and increment the index
                    swapValues(data, i, storeIndex);
                    storeIndex++;
                }
            }
            swapValues(data, storeIndex, endIndex);
            return storeIndex;
        }

        internal static void swapValues(T[] data, int firstIndex, int secondIndex)
        {
            T holder = data[firstIndex];
            data[firstIndex] = data[secondIndex];
            data[secondIndex] = holder;
        }
    }
}
