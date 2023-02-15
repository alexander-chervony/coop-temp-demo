/*******************************************************
 * Copyright (C) 2018-2019 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

namespace AwsActiveX
{
    public class ReceiveResponse
    {
        public int HttpStatusCode { get; set; }
        public ReceivedMessage[] ReceivedMessages { get; set; }
    }
}