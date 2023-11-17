using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illumine.LPR
{
    class SpaceService
    {
        static string filePath => Path.Combine(Environment.CurrentDirectory, "space");
        public static int GetSpaceCount()
        {
            int count = 0;
            if (!File.Exists(filePath))
                return count;
            using (StreamReader sr = new StreamReader(filePath, Container.Get<Encoding>()))
            {
                int.TryParse(sr.ReadLine(), out count);
            }
            return count;
        }
        public static void SetSpaceCount(int count)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, Container.Get<Encoding>()))
            {
                sw.WriteLine(count.ToString());
            }
        }
        public static int AddOneSpace()
        {
            int count = GetSpaceCount();
            SetSpaceCount(count + 1);
            return count + 1;
        }
        public static int MinusOneSpace()
        {
            int count = GetSpaceCount();
            SetSpaceCount(count - 1);
            return count - 1;
        }
    }
}
