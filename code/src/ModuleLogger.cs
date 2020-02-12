using System.IO;

namespace Modbed
{
    internal static class ModuleLogger
    {
        static private StreamWriter _streamWriter;
        static ModuleLogger()
        {
            _streamWriter = new StreamWriter("logs/EnhancedBattleTest.txt");
        }
        public static StreamWriter Writer
        {
            get { return _streamWriter; }
        }
        public static void Log(string format, params object[] args)
        {
            Writer.WriteLine(format, args);
            Writer.Flush();
        } 
    }
}