using System;
using System.IO;

namespace Core.Utils
{
    public static class DataPaths
    {
        public static string DataDir
        {
            get
            {
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string File(string name) => Path.Combine(DataDir, name);
    }
}