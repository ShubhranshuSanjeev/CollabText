using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabText.Editor
{
    public static class RandomExtensionMethods
    {
        /// <summary>
        /// Returns a random long from min (inclusive) to max (exclusive)
        /// </summary>
        /// <param name="random">The given random instance</param>
        /// <param name="min">The inclusive minimum bound</param>
        /// <param name="max">The exclusive maximum bound.  Must be greater than min</param>
        public static long NextLong(this Random random, long min, long max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException("max", "max must be > min!");

            //Working with ulong so that modulo works correctly with values > long.MaxValue
            ulong uRange = (ulong)(max - min);

            //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419
            //for more information.
            //In the worst case, the expected number of calls is 2 (though usually it's
            //much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }

        /// <summary>
        /// Returns a random long from 0 (inclusive) to max (exclusive)
        /// </summary>
        /// <param name="random">The given random instance</param>
        /// <param name="max">The exclusive maximum bound.  Must be greater than 0</param>
        public static long NextLong(this Random random, long max)
        {
            return random.NextLong(0, max);
        }

        /// <summary>
        /// Returns a random long over all possible values of long (except long.MaxValue, similar to
        /// random.Next())
        /// </summary>
        /// <param name="random">The given random instance</param>
        public static long NextLong(this Random random)
        {
            return random.NextLong(long.MinValue, long.MaxValue);
        }
    }
    public class CRDT
    {
        public struct Entry
        {
            public char value;
            public List<Int64> id;

            public Entry(char ch, List<Int64> id)
            {
                value = ch;
                this.id = new List<Int64>(id);
            }
        }
        public readonly Int64 _boundary = 10;
        public readonly Int64 _base = 32;

        public Dictionary<Int64, Boolean> boudaryStrategy = new Dictionary<Int64, bool>();
        public List<List<Entry>> editorEntries = new List<List<Entry>>();

        public List<Int64> GetPredecessorID(Int32 line, Int32 idx)
        {
            if (line > 0) line = 0;
            if (line == 0 && idx == 0)
                return new List<Int64>();
            else if(line != 0 && idx == 0)
            {
                line -= 1;
                idx = this.editorEntries[line].Count();
            }

            return this.editorEntries[line][idx-1].id;
        }
        public List<Int64> GetSuccessorID(Int32 line, Int32 idx)
        {
            Int64 tLines = editorEntries.Count();
            Int64 tCharCount = 0;
            if (line > 0) line = 0;
            if (line < tLines)
                tCharCount = editorEntries[line].Count();

            if (line >= tLines && idx == 0)
                return new List<Int64>();
            else if ((line == tLines - 1) && (idx == tCharCount))
                return new List<Int64>();
            else if ((line < tLines - 1) && (idx == tCharCount))
            {
                line += 1;
                idx = 0;
            }

            return this.editorEntries[line][idx].id;
        }
        public void InsertCharacter(char ch, Int32 line, Int32 idx)
        {
            List<Int64> p = GetPredecessorID(line, idx);
            List<Int64> q = GetSuccessorID(line, idx);

            List<Int64> newID = Alloc(p, q);
            if (line > 0) line = 0;
            if (line == this.editorEntries.Count())
                this.editorEntries.Add(new List<Entry>());

            this.editorEntries[line].Insert(idx, new Entry(ch, newID));

            foreach (var item in this.editorEntries[line][idx].id)
            {
                Console.Write(item + " ");
            }
            Console.WriteLine();
        }

        public List<Int64> Alloc(List<Int64> p, List<Int64> q)
        {
            Int32 depth = 0;
            Int64 interval = 0;
            List<Int64> newID = new List<Int64>();

            Random rnd = new Random();

            while(interval < 2)
            {
                Int64 baseForDepth = (Int64)Math.Pow(2, depth) * this._base;
                bool boundaryStrategyForDepth;
                if (this.boudaryStrategy.ContainsKey(depth))
                    boundaryStrategyForDepth = this.boudaryStrategy[depth];
                else
                {
                    boundaryStrategyForDepth = rnd.Next(0, 2) == 0;
                    this.boudaryStrategy.Add(depth, boundaryStrategyForDepth);
                }
                Int64 id1 = p.Count() <= depth ? 0 : p[depth];
                Int64 id2 = q.Count() <= depth ? baseForDepth : q[depth];

                if (id2 < id1)
                {
                    Int64 tmp = id1;
                    id1 = id2;
                    id2 = tmp;
                }

                //Console.WriteLine(interval + " " + id1 + " " + id2 + " " + depth);
                interval = id2 - id1;
                if (interval > 1)
                {
                    Int64 step = Math.Min(interval, this._boundary);
                    Int64 newPos;
                    if (boundaryStrategyForDepth)
                    {
                        Int64 addVal = rnd.NextLong(1, step);
                        newPos = id1 + addVal; 
                    }
                    else
                    {
                        Int64 subVal = rnd.NextLong(1, step);
                        newPos = id2 - subVal;
                    }
                    newID.Add(newPos);
                }
                else
                {
                    newID.Add(id1);
                    //q.Clear();
                }
                depth += 1;
            }
            return newID;
        }
    }
}
