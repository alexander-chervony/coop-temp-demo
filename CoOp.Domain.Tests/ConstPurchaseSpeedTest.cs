/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using NUnit.Framework;

namespace CoOp.Domain.Tests
{
    public class ConstPurchaseSpeedTest : CoOpTestsBase
    {
        [Test]
        public void Test11()
        {
            // given speed of new members involvement is constant
            // when inflation is constant
            // then speed of purchase should be constant and == speed of new members involvement
        }
        
        [Test]
        public void Test12()
        {
            // given speed of new members involvement is constant
            // when inflation varies significantly
            // then speed of purchase should be constant and == speed of new members involvement
        }
        
        [Test]
        public void Test21()
        {
            // given speed of new members involvement varies significantly
            // when inflation is constant
            // then speed of purchase varies in the same pace as speed of new members involvement and == speed of new members involvement
        }
        
        [Test]
        public void Test22()
        {
            // given speed of new members involvement varies significantly
            // when inflation varies significantly
            // then speed of purchase varies in the same pace as speed of new members involvement and == speed of new members involvement
        }
    }
}