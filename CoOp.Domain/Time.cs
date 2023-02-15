/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;

namespace CoOp.Domain
{
    public static class Time
    {
        public const int DaysInMonth = 30;
        public const int DaysInYear = 365;

        public static int PeriodInMonth(DateTime start, DateTime end) =>
            12 * (end.Year - start.Year)
            + end.Month - start.Month;
    }
}