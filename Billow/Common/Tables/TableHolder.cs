using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Common.Tables
{
    [ProtoContract]
    public class TableHolder<T>
    {
        [ProtoMember(1)]
        public List<T> Data { get; set; }
    }
}