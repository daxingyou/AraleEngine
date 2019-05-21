--AutoGen Code,Don't Modify

if not TestProto then
TestProto =
{
	a;
	b;
	c;
	d={};

	Serializer = function(self,dst)
		ProtoWriter.WriteFieldHeader(1,WireType.String,dst);
		self.WriteString(self.a, dst);
		ProtoWriter.WriteFieldHeader(2,WireType.Variant,dst);
		self.WriteInt32(self.b, dst);
		ProtoWriter.WriteFieldHeader(3,WireType.Variant,dst);
		self.WriteInt32(self.c, dst);
			self.WriterTypeList(self.d,src,4,self.WriteInt32)
	end;

	Deserialize = function(self,src)
		local tag;
		tag = src:ReadFieldHeader();
		if tag==1 then self.a = self.ReadString(self.a, src) end
		tag = src:ReadFieldHeader();
		if tag==2 then self.b = self.ReadInt32(self.b, src) end
		tag = src:ReadFieldHeader();
		if tag==3 then self.c = self.ReadInt32(self.c, src) end
		tag = src:ReadFieldHeader();
		if tag==4 then
			self.ReadTypeList(self.d,src,WireType.Variant,self.ReadInt32)
		end
	end;

}

createClass("TestProto",TestProto,LProtoWrite)
end

