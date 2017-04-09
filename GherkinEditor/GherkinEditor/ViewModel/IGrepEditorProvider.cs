using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.View;

namespace Gherkin.ViewModel
{
    public interface IGrepEditorProvider
    {
        EditorTabContentViewModel NewGrepEditor();
        EditorTabContentViewModel OpenEditor(string filePath);
    }
}
