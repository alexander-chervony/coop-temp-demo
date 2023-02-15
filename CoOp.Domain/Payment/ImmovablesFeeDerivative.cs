/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

namespace CoOp.Domain
{
    public class ImmovablesFeeDerivative
    {
        private readonly bool _accumulated;

        public ImmovablesFeeDerivative(double amount, bool accumulated)
        {
            _accumulated = accumulated;
            Amount = amount;
        }
        
        public double Amount { get; }
        public string AmountFormatted => Amount.ToString("C2");

        public double Accumulated => _accumulated ? Amount : 0;
    }
}