using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBS
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.CreateLog("*****************本次同步已开始*****************");
            new ProcessB().SyncGroups();

            Logger.CreateLog("*****************本次同步已完成*****************");
            Console.WriteLine("Process End");
            //Console.ReadLine();
            //new GroupProcess().Do();
        }
    }
}
