
using System;
using System.Collections.Generic;
using System.Linq;


namespace Server
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var serverEngineConsole = new ServerEngineConsole();
            serverEngineConsole.Run();
        }
    }
}
