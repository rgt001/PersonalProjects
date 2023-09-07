using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.Threads
{
    public static class ParallelInvoke
    {
        public static void SafeInvoke(this Form form, Action action)
        {
            MethodInvoker method = delegate { action.Invoke(); };
            form.Invoke(method);
        }
    }
}
