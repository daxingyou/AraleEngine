using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


namespace Arale.Engine{
	
    public class BufferReader{
        private static IBufferReader br = createInterface( true );

        private static IBufferReader createInterface( bool isLittleEndian ){
        	if( isLittleEndian )
                return new LittleEndianBufferReader();
            return new BigEndianBufferReader();
        }

        private int _offset;
        private byte[] _buffer;
		public int offset{get{return _offset;}}

        public BufferReader(byte[] buffer, int offset){
            _buffer = buffer;
            _offset = offset;
        }

        public byte readUInt8(){
            return br.readUInt8(_buffer, _offset++);
        }

        public int readInt8(){
            return br.readInt8(_buffer, _offset++);
        }

        public bool readBool(){
            return br.readBool(_buffer, _offset++);
        }

        public ushort readUInt16(){
            ushort ret = br.readUInt16(_buffer, _offset);
            _offset += 2;
            return ret;
        }

        public short readInt16(){
            short ret = br.readInt16(_buffer, _offset);
            _offset += 2;
            return ret;
        }

        public uint readUInt32(){
            uint ret = br.readUInt32(_buffer, _offset);
            _offset += 4;
            return ret;
        }

        public int readInt32(){
            int ret = br.readInt32(_buffer, _offset);
            _offset += 4;
            return ret;
        }

		public ulong readUInt64(){
			ulong ret = br.readUInt64(_buffer, _offset);
			_offset += 8;
			return ret;
		}

		public long readInt64(){
			long ret = br.readInt64(_buffer, _offset);
			_offset += 8;
			return ret;
		}
        
        public string readString(){
			int low = (int )_buffer [_offset++];
			int high = (int )_buffer [_offset++];
			int strsize = (high << 8) + low;
            int start = _offset;
			return Convert.ToBase64String(_buffer, start, strsize);
        }
        
        public static ushort readUInt16(byte[] buffer, int startIndex){
            return br.readUInt16(buffer, startIndex);
        }

        public static uint readUInt32(byte[] buffer, int startIndex){
			return br.readUInt32(buffer, startIndex);
		}
    }
}
