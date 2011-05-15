using System;

namespace AlgebraTree
{
    public class Popularity : Tuple<int, int>
    {
        public Popularity(int item1, int item2) : base(item1, item2)
        {
        }

        public static Popularity Unknown
        {
            get { return new Popularity(0, 0);}
        }
    }
}
