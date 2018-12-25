using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Text;

namespace Arale.Engine{
	
    public class BufferWriter{
        private static IBufferWriter bw = createInterface(true);

        private static IBufferWriter createInterface(bool bIsLittleEndian){
            if( bIsLittleEndian )
                return new LittleEndianBufferWriter();
            return new BigEndianBufferWriter();
        }

        private List<byte> _dat = null;
        public BufferWriter(){
            _dat = new List<byte>(); 
        }

        public byte[] toBuffer(){
            byte[] buf = new byte[_dat.Count + 2];
			bw.writeUInt16(buf, 0, (ushort)(_dat.Count+2));
            _dat.CopyTo(buf, 2);
			buf [3] |= (byte)0x80;//protocol buffer
            return buf;
        }

        public void writeUInt8(byte v){
            bw.writeUInt8(_dat, v);
        }

        public void writeInt8(int v){
            bw.writeInt8(_dat, v);
        }

        public void writeUInt16(ushort v){
            bw.writeUInt16(_dat, v);
        }

        public void writeInt16(short v){
            bw.writeInt16(_dat, v);
        }

        public void writeUInt32(uint v){
            bw.writeUInt32(_dat, v);
        }

        public void writeInt32(int v){
            bw.writeInt32(_dat, v);
        }

		public void writeInt64(long v){
			bw.writeInt64(_dat, v);
		}

        public void writeString(string s){
//            byte[] bytes = Encoding.UTF8.GetBytes(s);
			byte[] bytes = Convert.FromBase64String(s);
			_dat.Add((byte)bytes.Length);
			_dat.Add((byte)(bytes.Length >> 8));
            for (int i = 0; i < bytes.Length; ++i)
                _dat.Add(bytes[i]);
            //m_dat.Add((byte)0);
        }

		/*
        public void writeGUID(GUID v)
        {
        	bw.writeUInt32(_dat, v.Guid);
        }
        */
    }
}