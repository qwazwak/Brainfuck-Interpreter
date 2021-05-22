using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainFuckInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                bool havePath = false;
                string input = "";
                int counter = 0;
                Console.Write("No arguments were entered\nEnter the path to the BrainFuck source file: ");
                while (!havePath)
                {
                    if (Console.KeyAvailable)
                    {
                        /* ConsoleKeyInfo key = Console.ReadKey(true);
                         switch (key.Key)
                         {
                             case ConsoleKey.F1:
                                 Console.WriteLine("You pressed F1!");
                                 break;
                             default:
                                 break;
                         }*/
                        input = Console.ReadLine();
                        Console.WriteLine($"{input}");
                        Console.WriteLine($"Counter: {counter}");

                    }
                    counter += 1;
                }

            }
            Console.Write("Hello\n");
            Console.Write("2nd line");
        }
    }
}
