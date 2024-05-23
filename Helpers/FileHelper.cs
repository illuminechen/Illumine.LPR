using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illumine.LPR.Helpers
{
    public class FileHelper
    {
        public static byte[] ReadFileBytes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist.");
                return null;
            }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[fs.Length];
                    int numBytesToRead = (int)fs.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = fs.Read(bytes, numBytesRead, numBytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    return bytes;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("An IO exception has been caught: " + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception has been caught: " + ex.Message);
                return null;
            }
        }
    }
}
