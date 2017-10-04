using System;
using System.Collections.Generic;
using Arke.DSL.Step;

namespace Arke.SipEngine.Device
{
    public class Dsl : Dictionary<int, DslStep>, IComparable
    {
        public int CompareTo(object obj)
        {
            var step = obj as Dsl;
            if (step == null)
                return 0;

            return step.Keys == Keys ? 1 : 0;
        }
    }
}