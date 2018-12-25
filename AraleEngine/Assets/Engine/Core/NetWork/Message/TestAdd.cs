using System;

using System.IO;
using System.Text;
using ProtoBuf;

namespace Proto.Message{
	
	[ProtoContract]
	public class TestAdd{
		[ProtoMember(1)]
		public int n1 { get; set; }
		[ProtoMember(2)]
		public int n2 { get; set; }
	}
}

