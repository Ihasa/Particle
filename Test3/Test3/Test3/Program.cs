using System;

namespace Test3
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string fileName = args.Length != 0 ? args[0] : "";
            using (Game1 game = new Game1(fileName))
            {
                game.Run();
            }
        }
    }
#endif
}

