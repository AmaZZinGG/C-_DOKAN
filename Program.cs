using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hello_world;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            DokanNet.Dokan.Mount(new Hello_World(), "M:\\");
        }
    }
}
