
https://ru.stackoverflow.com/questions/16968/activex-%D0%B8-c
/*******************************************************
 * Copyright (C) 2018-2019 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

to register:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe AwsActiveX.dll /tlb /codebase


to check in console app on windows:

using System;

namespace ConsoleApplication2
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            dynamic utilites = Activator.CreateInstance(Type.GetTypeFromProgID("AwsActiveX.Sqs"));
            var test = utilites.Receive("test-string-queue.fifo", 2);
            Console.WriteLine(test);
            Console.ReadKey();
        }
    }
}