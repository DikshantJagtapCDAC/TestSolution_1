// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Numerics;

class Program
{
    static void Main()
    {
        HashSet<string> distinctTerms = new HashSet<string>();

        for (int a = 2; a <= 100; a++)
        {
            for (int b = 2; b <= 100; b++)
            {
                distinctTerms.Add(BigInteger.Pow(a, b).ToString());
            }
        }

        int result = distinctTerms.Count;

        Console.WriteLine(result);
    }
}
