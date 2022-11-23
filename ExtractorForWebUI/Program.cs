using System;
namespace ExtractorForWebUI;

internal class Program
{
    static void Main(string[] args)
    {
        var myApp = new MyApp()
        {
            args = args,
        };
        myApp.Run();
    }
}