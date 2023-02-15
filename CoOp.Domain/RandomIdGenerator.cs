/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Text;

namespace CoOp.Domain
{
    public static class RandomIdGenerator 
    {
        private static readonly char[] _base62chars = 
            "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
                .ToCharArray();

        private static readonly Random _random = new Random();

        public static string GetBase62(int length) 
        {
            var sb = new StringBuilder(length);

            for (int i=0; i<length; i++) 
                sb.Append(_base62chars[_random.Next(62)]);

            return sb.ToString();
        }       

        public static string GetBase36(int length) 
        {
            var sb = new StringBuilder(length);

            for (int i=0; i<length; i++) 
                sb.Append(_base62chars[_random.Next(36)]);

            return sb.ToString();
        }
    }
}