using System;
using System.Collections.Generic;
using Client;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Input;

namespace PsmClient
{
	public class AppMain
	{
	    internal static ClientEngine PsmClientEngine;

            public static void Main(string[] args)
            {
                PsmClientEngine = new ClientEngine();
                while (true)
                {
                    PsmClientEngine.Run();
                }
            }
	}
}
