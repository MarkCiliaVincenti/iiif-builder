﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Wellcome.Dds.Common
{
    public static class AccessCondition
    {
        public const string Open = "Open";
        public const string RequiresRegistration = "Requires registration";
        public const string ClinicalImages = "Clinical images";
        public const string RestrictedFiles = "Restricted files";
        public const string Closed = "Closed";

        public const string ClosedSectionError = "I will not serve a b number that has a closed section";

        // Added for IIIF experiment
        public const string Degraded = "Degraded";

        private static readonly List<ComparableAccessCondition> SecurityOrder
            = new List<ComparableAccessCondition>
                  {
                      new ComparableAccessCondition(Open, 0),
                      new ComparableAccessCondition(Degraded, 1),
                      new ComparableAccessCondition(RequiresRegistration, 2),
                      new ComparableAccessCondition(ClinicalImages, 3),
                      new ComparableAccessCondition(RestrictedFiles, 4),
                      new ComparableAccessCondition(Closed, 5)
                  };

        public static bool IsValid(string s)
        {
            return (s == Open || s == Degraded || s == RequiresRegistration || s == ClinicalImages || s == RestrictedFiles || s == Closed);
        }


        public static string GetMostSecureAccessCondition(IEnumerable<string> accessConditions)
        {
            return accessConditions.Select(s => SecurityOrder.Single(ac => ac.ToString() == s)).Max().ToString();
        }


        private class ComparableAccessCondition : IComparable<ComparableAccessCondition>
        {
            private readonly string ac;
            private readonly int order;

            public ComparableAccessCondition(string ac, int order)
            {
                this.ac = ac;
                this.order = order;
            }

            public override string ToString()
            {
                return ac;
            }

            public int CompareTo(ComparableAccessCondition other)
            {
                return order.CompareTo(other.order);
            }
        }
    }
}
