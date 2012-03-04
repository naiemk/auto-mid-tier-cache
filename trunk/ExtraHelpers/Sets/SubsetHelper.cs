using System;
using System.Collections.Generic;

namespace ExtraHelpers.Sets
{
    public static class SubsetHelper
    {
        /// <summary>
        /// Returns all subsets for the selected members
        /// </summary>
        public static IEnumerable<string[]> GetAllSubsets(string[] members)
        {
            if (members.Length>32)
                throw new ArgumentOutOfRangeException("members", @"Too many members");
            uint i = ((uint)1 << members.Length) - 1;
            while (i>0)
            {
                yield return GetBitmapMembers(i, members);
                i--;
            }
        }

        private static string[] GetBitmapMembers(uint flags, string[] members)
        {
            var rv = new List<string>();
            for (int i = 0; i < members.Length; i++)
            {
                uint f = ((uint) 1 << i);
                if ((f & flags) > 0)
                {
                    rv.Add(members[i]);
                }
            }
            return rv.ToArray();
        }
    }
}
