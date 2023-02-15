/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;

namespace CoOp.Domain
{
    // note: other id props must be added here depending on DocumentType
    public interface IMemberUIdData
    {
        string DocumentType { get; }
        string PassportPn { get; }
    }

    public static class MemberUIdExtensions
    {
        /// <summary>
        /// Gets unique identifier for external system (1C in particular)
        /// </summary>
        public static string GetUId(this IMemberUIdData data)
        {
            switch (data.DocumentType)
            {
                case  "passport_by":
                    return data.PassportPn;
                default:
                    throw new NotImplementedException("doc type to field mapping must be defined!");
            }
        }
        
        public static MemberId GetMemberId(this IMemberUIdData data)
        {
            return MemberId.NewDeterministic(data.GetUId());
        }
    }
}