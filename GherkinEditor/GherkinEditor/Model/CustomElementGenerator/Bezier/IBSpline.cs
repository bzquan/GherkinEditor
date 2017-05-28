using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Model
{
    public interface IBSpline
    {
        GPoint GetPoint(double u);
        double? Curvature(double u);
    }
}
