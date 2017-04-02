using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.ViewModel
{
    public interface ICanCloseAllDocumentsChecker
    {
        bool CanCloseAllDocuments();
        bool CanCloseAllButThis();
    }
}
