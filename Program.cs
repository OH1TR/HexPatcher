using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexPatcher
{
    class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       Process(o);
                   });

        }

        /// <summary>
        /// Process input file and patch it
        /// </summary>
        static void Process(Options o)
        {
            try
            {
                // Address of first byte is this file. User may change this with SB:
                long baseAddress = 0;

                // Input file. We modify this string
                var inputFile = MakeHex(File.ReadAllBytes(o.Input));

                // Patch command file
                var patchFile = File.ReadAllLines(o.Patch);

                // Key to find
                byte[] findBytes = null;

                for (int lineNum = 0; lineNum < patchFile.Length; lineNum++)
                {
                    string line = patchFile[lineNum];
                    //Find
                    if (line.StartsWith("F:"))
                    {
                        findBytes = FlexibleParseHex(line.Substring(2));
                    }
                    //Replace
                    else if (line.StartsWith("R:"))
                    {
                        var replaceBytes = FlexibleParseHex(line.Substring(2));

                        if (findBytes == null)
                            throw new Exception("F: not defined!");

                        inputFile = Replace(inputFile, MakeHex(findBytes), MakeHex(replaceBytes), (i) => Console.WriteLine("(" + (lineNum + 1) + ")Replacement at " + (baseAddress + i).ToString("X8")));
                    }
                    //Set base address
                    else if (line.StartsWith("SB:"))
                    {
                        string[] tok = line.Substring(3).Split('=');
                        var key = MakeHex(FlexibleParseHex(tok[1]));
                        long posAddr = Convert.ToInt64(tok[0], 16);

                        if (inputFile.IndexOf(key) != inputFile.LastIndexOf(key))
                            throw new Exception("Multiple base address candidates");

                        baseAddress = posAddr - (inputFile.IndexOf(key) / 3);
                    }
                }
                File.WriteAllBytes(o.Output, StringToByteArrayFastest(inputFile));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// Replace substrings insize str by finding key and replacing it with replacement. Call callback each time.
        /// </summary>
        static string Replace(string str, string key, string replacement, Action<int> callback)
        {
            int pos = 0;
            do
            {
                pos = str.IndexOf(key, pos);
                if (pos != -1)
                {
                    str = str.Substring(0, pos) + replacement + str.Substring(pos + key.Length, str.Length - pos - key.Length);
                    callback(pos / 3);
                    pos++;
                }
            }
            while (pos > 0);

            return (str);
        }

        /// <summary>
        /// Parse input string to bytes. This is slower but handles different separators etc.
        /// </summary>
        static byte[] FlexibleParseHex(string hex)
        {
            int i = 0;
            List<byte> result = new List<byte>();

            do
            {
                if (i + 1 >= hex.Length)
                {
                    if (IsHexChar(hex[i]))
                        throw new Exception("Unbalanced hex code");
                    break;
                }

                if (!IsHexChar(hex[i]))
                {
                    i++;
                    continue;
                }

                result.Add(Convert.ToByte(hex.Substring(i, 2), 16));
                i += 2;
            }
            while (i < hex.Length);

            return result.ToArray();
        }

        /// <summary>
        /// Check if char is hex char.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        static bool IsHexChar(Char c)
        {
            return ("0123456789abcdefABCDEF".Contains(c));
        }

        /// <summary>
        /// Convert bytes to hex string.
        /// </summary>
        static string MakeHex(byte[] input)
        {
            return (BitConverter.ToString(input));
        }

        /// <summary>
        /// Convert hex string to bytes fast.
        /// </summary>
        public static byte[] StringToByteArrayFastest(string hex)
        {
            // 01-02-03

            if (hex.Length % 3 != 2)
                throw new Exception("Internal error");

            byte[] arr = new byte[(hex.Length / 3) + 1];

            for (int i = 0; i < hex.Length / 3; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i * 3]) << 4) + (GetHexVal(hex[(i * 3) + 1])));
            }

            return arr;
        }

        /// <summary>
        /// Convert char to int. (hex parsing)
        /// </summary>
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;            
            return val - (val < 58 ? 48 : 55);
        }
    }
}
