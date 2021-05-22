using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTools
{
    public static class QTool
    {
        public static bool checkPath(String path)
        {
            bool isValid = false;
            isValid = System.IO.File.Exists(path);
            return isValid;
        }
    }
}
