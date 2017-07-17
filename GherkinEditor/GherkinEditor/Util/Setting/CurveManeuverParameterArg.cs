using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class CurveManeuverParameterArg : EventArgs
    {
        public CurveManeuverParameterArg(CurveManeuverParameter arg)
        {
            CurveManeuverParameter = arg;
        }

        public CurveManeuverParameter CurveManeuverParameter { get; private set; }

    }
}
