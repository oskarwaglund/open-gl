using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TriSpin
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Win win = new Win())
            {
                win.Run(60);
            }
        }
    }
}
