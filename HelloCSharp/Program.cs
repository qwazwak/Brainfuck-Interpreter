using QTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NDesk.Options;

namespace BrainFuckInterpreter
{
    class Program
    {
        static int verbosity;
        static String sourceFilePath;
        static int bufferSize = 64;
        static int tape_length = 5000;


        static char asciiSymbol(byte val)
        {
            if (val < 32) return '.';  // Non-printable ASCII
            if (val < 127) return (char)val;   // Normal ASCII
                                               // Workaround the hole in Latin-1 code page
            if (val == 127) return '.';
            if (val < 0x90) return "€.‚ƒ„…†‡ˆ‰Š‹Œ.Ž."[val & 0xF];
            if (val < 0xA0) return ".‘’“”•–—˜™š›œ.žŸ"[val & 0xF];
            if (val == 0xAD) return '.';   // Soft hyphen: this symbol is zero-width even in monospace fonts
            return (char)val;   // Normal Latin-1
        }

        /*
         * >	increment the data pointer (to point to the next cell to the right).
         * <	decrement the data pointer (to point to the next cell to the left).
         * +	increment (increase by one) the byte at the data pointer.
         * -	decrement (decrease by one) the byte at the data pointer.
         * .	output the byte at the data pointer.
         * ,	accept one byte of input, storing its value in the byte at the data pointer.
         * [	if the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.
         * ]	if the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.
        */
        enum cmds
        {
            ptr_inc,
            ptr_dec,
            dat_inc,
            dat_dec,
            output,
            input,
            left,
            right
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: greet [OPTIONS]+ message");
            Console.WriteLine("Greet a list of individuals with an optional message.");
            Console.WriteLine("If no message is specified, a generic greeting is used.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Debug(string format, params object[] args)
        {
            if (verbosity > 0)
            {
                Console.Write("# ");
                Console.WriteLine(format, args);
            }
        }
        static public void Main(String[] args)
        {
            bool show_help = false;
            List<string> names = new List<string>();
            int repeat = 1;
            bool args_failure = false;

            var p = new OptionSet() {
                { "s|source=", "the path of the source file to run.",
                  v => sourceFilePath = v.Trim() },
                { "v", "increase debug message verbosity",
                  v => { if (v != null) {++verbosity; } } },
                { "b|buffer=", "Set reading buffer size in bytes",
                  v => {  if (Int32.TryParse(v.Trim(), out int j)) { bufferSize = j; } else {args_failure = true; Console.WriteLine($"Buffer of \"{v.Trim()}\" could not be parsed"); }; }
                },
                { "h|help",  "show this message and exit",
                  v => show_help = v != null },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("greet: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `greet --help' for more information.");
                return;
            }
            if (show_help || args_failure)
            {
                ShowHelp(p);
                return;
            }


            /*
            if (extra.Count <= 0)
            {
                bool havePath = false;
                string input = "";
                Console.Write("No arguments were entered\nEnter the path to the BrainFuck source file: ");
                while (!havePath)
                {
                    if (Console.KeyAvailable)
                    {
                         ConsoleKeyInfo key = Console.ReadKey(true);
                         switch (key.Key)
                         {
                             case ConsoleKey.F1:
                                 Console.WriteLine("You pressed F1!");
                                 break;
                             default:
                                 break;
                         }
            input = Console.ReadLine();
                        if (QTool.checkPath(input))
                        {
                            havePath = true;
                        }
                        else
                        {
                            Console.WriteLine($"\"{input}\" is invalid, try another path");
                            Console.Beep();
                        }

                    }
                }
           
        }*/

            /* my old crusty code, saved for documentation
            System.IO.FileStream theSourceFile = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
            byte[] buffer;
            theSourceFile.Read(buffer,0, bufferSize);
            foreach (string name in names)
            {
                for (int i = 0; i < repeat; ++i)
                    Console.WriteLine(message, name);
            }*/
            byte[] fileBuffer = null;
            try
            {
                //open the filestream and have it only exist for this section of code
                using (FileStream file_link = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                {

                    // Read the source file into a byte array.
                    //The .Length is the number of bytes in the stream. Im not sure if this is length of file or just length of the stream's buffer
                    fileBuffer = new byte[file_link.Length];
                    //oh
                    int numBytesToRead = (int)file_link.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = file_link.Read(fileBuffer, numBytesRead, numBytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    numBytesToRead = fileBuffer.Length;
                }
            }
            catch (FileNotFoundException ioEx)
            {
                Console.WriteLine(ioEx.Message);
                return;
            }


            char[] chars = System.Text.Encoding.UTF8.GetChars(fileBuffer);

           



        //Console.WriteLine(chars);
        int cmd_index = 0;
            int[] tape = new int[tape_length];
            for (int i = 0; i < tape_length; i++)
            {
                tape[i] = 0;
            }
            Array.Clear(tape, 0, tape.Length);
            int mem_index = 0;
            char currChar;
            while (true)
            {
                if(cmd_index >= chars.Length - 1)
                {
                    break;
                }
                currChar = chars[cmd_index];
               // Console.WriteLine($"char: {currChar}");

                /*
                 * >	increment the data pointer (to point to the next cell to the right).
                 * <	decrement the data pointer (to point to the next cell to the left).
                 * +	increment (increase by one) the byte at the data pointer.
                 * -	decrement (decrease by one) the byte at the data pointer.
                 * .	output the byte at the data pointer.
                 * ,	accept one byte of input, storing its value in the byte at the data pointer.
                 * [	if the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.
                 * ]	if the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.
                */
                switch (currChar)
                {
                    case '>':
                        Debug("inc_ptr");
                        mem_index++;
                        cmd_index += 1;
                        break;
                    case '<':
                        mem_index--;
                        cmd_index += 1;
                        break;
                    case '+':
                        tape[mem_index] = ((tape[mem_index]) + 1);
                        cmd_index += 1;
                        break;
                    case '-':
                        tape[mem_index] = ((tape[mem_index]) - 1);
                        cmd_index += 1;
                        break;
                    case '.':

                        Console.Write(asciiSymbol((byte)(tape[mem_index])));
                        //   Console.Write($"{(char)(tape[mem_index] + 96)}\n");
                        cmd_index += 1;
                        break;
                    case ',':
                        tape[mem_index] = (int)Console.ReadKey().KeyChar;
                        cmd_index += 1;
                        Console.ReadLine();
                        break;
                    case '[':
                        if (tape[mem_index] == 0)
                        {
                            char current;
                            bool run = true;
                            int bracketCounter = 0;
                            while (run)
                            {
                                cmd_index += 1;
                                current = chars[cmd_index];
                                switch (current)
                                {
                                    case '[':
                                        bracketCounter += 1;
                                        continue;
                                    case ']':
                                        if (bracketCounter <= 0)
                                        {
                                            cmd_index += 1;
                                            run = false;
                                        }
                                        else                                         bracketCounter -= 1;
                                        continue;
                                    default:
                                        continue;
                                }
                            }
                        }
                        else
                        {
                            cmd_index += 1;
                        }
                        break;
                    case ']':
                        // ]	if the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching[command.
                        //Console.WriteLine($"Gonn a get index {mem_index}");
                        if (tape[mem_index] != 0)
                        {
                            char current;
                            bool run = true;
                            int bracketCounter = 0;
                            while (run)
                            {
                                cmd_index -= 1;
                                current = chars[cmd_index];
                                switch (current)
                                {
                                    case '[':
                                        if (bracketCounter <= 0)
                                        {
                                            cmd_index -= 1;
                                            run = false;
                                        }
                                        else bracketCounter -= 1;
                                        continue;
                                    case ']':
                                        bracketCounter += 1;
                                        continue;
                                    default:
                                        continue;
                                }
                            }
                        }
                        else
                        {
                            cmd_index += 1;
                        }
                        break;
                    default:
                        cmd_index += 1;
                        break;
                }
            }



            Console.Write('\n');
        }
    }
}