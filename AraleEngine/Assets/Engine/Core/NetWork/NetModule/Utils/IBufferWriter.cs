using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Arale.Engine{
	
    interface IBufferWriter{
        void writeUInt8(List<byte> dst, byte val);
        void writeInt8(List<byte> dst, int val);
        void writeUInt16(List<byte> dst, ushort val);
        void writeInt16(List<byte> dst, short val);
        void writeUInt32(List<byte> dst, uint val);
        void writeInt32(List<byte> dst, int val);
		void writeInt64 (List<byte> dst, long val);
        void writeUInt16(byte[] dst, int startIndex, ushort val);
    }

    class BigEndianBufferWriter : IBufferWriter{
		
        public void writeUInt16(byte[] dst, int startIndex, ushort val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            if( startIndex < 0 || startIndex > dst.Length - 1)
				throw new ArgumentException ("pos: " + "Position was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");
			if (startIndex + 2 > dst.Length)
				throw new ArgumentException ("Destination array is not long"
					+ " enough to copy all the items in the collection."
					+ " Check array index and length.");
            dst[startIndex] = (byte)(val >> 8);
            dst[startIndex + 1] = (byte)val;
        }

        public void writeUInt8(List<byte> dst, byte val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            dst.Add(val);
        }

        public void writeInt8(List<byte> dst, int val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            dst.Add((byte)val);
        }

        public void writeUInt16(List<byte> dst, ushort val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            for(int i=1; i>=0; --i)
                dst.Add((byte)(val>>(8*i)));
        }

        public void writeInt16(List<byte> dst, short val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            for(int i=1; i>=0; --i)
                dst.Add((byte)(val>>(8*i)));
        }

        public void writeUInt32(List<byte> dst, uint val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            for(int i=3; i>=0; --i)
                dst.Add((byte)(val>>(8*i)));
        }

        public void writeInt32(List<byte> dst, int val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            for(int i=3; i>=0; --i)
                dst.Add((byte)(val>>(8*i)));
        }

		public void writeInt64(List<byte> dst, long val){
			if (dst == null)
				throw new ArgumentNullException("byteArray");
			for(int i=7;i>=0;--i)
				dst.Add((byte)(val>>(8*i)));
		}
    }

    class LittleEndianBufferWriter : IBufferWriter{
		
        public void writeUInt16(byte[] dst, int startIndex, ushort val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            if( startIndex < 0 || startIndex > dst.Length - 1)
				throw new ArgumentException ("pos: " + "Position was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");
			if (startIndex + 2 > dst.Length)
				throw new ArgumentException ("Destination array is not long"
					+ " enough to copy all the items in the collection."
					+ " Check array index and length.");
            dst[startIndex] = (byte)val;
            dst[startIndex + 1] = (byte)(val >> 8);
        }

        public void writeUInt8(List<byte> dst, byte val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            dst.Add(val);
        }

        public void writeInt8(List<byte> dst, int val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            dst.Add((byte)val);
        }

        public void writeUInt16(List<byte> dst, ushort val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            for(int i=0; i<2; ++i)
                dst.Add((byte)(val>>(8*i)));
        }

        public void writeInt16(List<byte> dst, short val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            for(int i=0; i<2; ++i)
                dst.Add((byte)(val>>(8*i)));
        }

        public void writeUInt32(List<byte> dst, uint val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            for(int i=0; i<4; ++i)
                dst.Add((byte)(val>>(8*i)));
        }

        public void writeInt32(List<byte> dst, int val){
            if (dst == null)
                throw new ArgumentNullException("byteArray");
            for(int i=0; i<4; ++i)
                dst.Add((byte)(val>>(8*i)));
        }

		public void writeInt64(List<byte> dst, long val){
			if (dst == null)
				throw new ArgumentNullException("byteArray");
			for(int i=0;i<8;++i)
				dst.Add((byte)(val>>(8*i)));
		}
    }
}
