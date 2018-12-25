if not LProtoWrite then

LProtoWrite =
{
	Serializer = function(dst)
		--base.Serializer();
		--Member.Serializer();
	end;

	Deserialize = function(src)
	end;

	--==================
	WriteByte = function(val,dst)
		ProtoWriter.WriteByte(val, dst);
	end;
	ReadByte  = function(val,src)
		return src:ReadByte();
	end;
	WriteSByte = function(val,dst)
		ProtoWriter.WriteSByte(val, dst);
	end;
	ReadSByte = function(val,src)
		return src:ReadSByte();
	end;
	--
	WriteInt16 = function(val,dst)
		ProtoWriter.WriteInt16(val, dst);
	end;
	ReadInt16 = function(val,src)
		return src:ReadInt16();
	end;
	WriteUInt16 = function(val,dst)
		ProtoWriter.WriteUInt16(val, dst);
	end;
	ReadUInt16 = function(val,src)
		return src:ReadUInt16();
	end;
	--
	WriteInt32 = function(val,dst)
		ProtoWriter.WriteInt32(val, dst);
	end;
	ReadInt32 = function(val,src)
		return src:ReadInt32();
	end;
	WriteUInt32 = function(val,dst)
		ProtoWriter.WriteUInt32(val, dst);
	end;
	ReadUInt32 = function(val,src)
		return src:ReadUInt32();
	end;
	--
	WriteInt64 = function(val,dst)
		ProtoWriter.WriteInt64(val, dst);
	end;
	ReadInt64 = function(val,src)
		return src:ReadInt64();
	end;
	WriteUInt64 = function(val,dst)
		ProtoWriter.WriteUInt64(val, dst);
	end;
	ReadUInt64 = function(val,src)
		return src:ReadUInt64();
	end;
	--
	WriteBoolean = function(val,dst)
		ProtoWriter.WriteBoolean(val, dst);
	end;
	ReadBoolean = function(val,src)
		return src:ReadBoolean();
	end;
	--
	WriteSingle = function(val,dst)
		ProtoWriter.WriteSingle(val, dst);
	end;
	ReadSingle = function(val,src)
		return src:ReadSingle();
	end;
	--
	WriteDouble = function(val,dst)
		ProtoWriter.WriteDouble(val, dst);
	end;
	ReadDouble = function(val,src)
		return src:ReadDouble();
	end;
	--
	WriteString = function(val,dst)
		ProtoWriter.WriteString(val, dst);
	end;
	ReadString = function(val,src)
		return src:ReadString();
	end;
	--
	WriteBytes = function(val,dst)
		ProtoWriter.WriteBytes(val, dst);
	end;
	ReadBytes = function(val,src)
		return ProtoReader.AppendBytes(val, src);
	end;
	--扩展类型
	WriteDecimal = function(val,dst)
		BclHelpers.WriteDecimal(val, dst);
	end;
	ReadDecimal = function(val,src)
		return BclHelpers.ReadDecimal(src);
	end;
	--
	WriteGuid = function(val,dst)
		BclHelpers.WriteGuid(val, dst);
	end;
	ReadGuid = function(val,src)
		return BclHelpers.ReadGuid(src);
	end;
	--
	WriteDateTime = function(val,dst)
		BclHelpers.WriteDateTime(val, dst);
	end;
	ReadDateTime = function(val,src)
		return BclHelpers.ReadDateTime(src);
	end;
	--
	WriteTimeSpan = function(val,dst)
		BclHelpers.WriteTimeSpan(val, dst);
	end;
	ReadTimeSpan = function(val,src)
		return BclHelpers.ReadTimeSpan(src);
	end;
	--嵌套类型
	WriteObject = function(val,dst)
		local token = ProtoWriter.StartSubItem(val,dst);
		val:Serializer(dst)
		ProtoWriter.EndSubItem(token, dst);
	end;
	ReadObject = function(val,src)
		local token = ProtoReader.StartSubItem(src);
		local ret = newLuaObject(val);
		ret:Deserialize(src);
		ProtoReader.EndSubItem(token, src);
		return ret;
	end;
}

--must--
createClass("LProtoWrite",LProtoWrite)
--======
end
