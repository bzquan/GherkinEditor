using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util.Geometric;

namespace Gherkin.Util.Bezier
{
    public interface IBSpline
    {
        GPoint GetPoint(double u);
        double? Curvature(double u);
    }
}
