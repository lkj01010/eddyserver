using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

interface IPostprocessor
{
    void OnReimported(string path);
    void OnDeleted(string path);
    void OnMoved(string from, string to);
}
