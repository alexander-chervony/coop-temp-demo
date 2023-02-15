/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

namespace CoOp.Domain
{
    public static class MembershipFeeRateCalc
    {
        public static double CalculateFee(int accumulationTimeInMonth, int termInYears)
        {
            // discount = 0%
            if (accumulationTimeInMonth < 12)
                return CoOpAR.MembershipFeeRate;
            
            // discount = 100%
            var percentageAccumulationOverTime = (double)accumulationTimeInMonth / (termInYears * 12);

            if (percentageAccumulationOverTime >= 0.90)
                return CoOpAR.MembershipFeeRate / 2;
            
            // calculated discount
            var discount = (CoOpAR.MembershipFeeRate / 2) * percentageAccumulationOverTime;
            return CoOpAR.MembershipFeeRate - discount;
        }
    }
}