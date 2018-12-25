using UnityEngine;
using System.Collections;
using System;

namespace Arale.Engine{
	
    interface IBufferReader{
		bool readBool(byte[] src, int startIndex);
        byte readUInt8(byte[] src, int startIndex);
        int readInt8(byte[] src, int startIndex);
        ushort readUInt16(byte[] src, int startIndex);
        short readInt16(byte[] src, int startIndex);
        uint readUInt32(byte[] src, int startIndex);
        int readInt32(byte[] src, int startIndex);
		ulong readUInt64(byte[] src, int startIndex);
		long readInt64(byte[] src, int startIndex);
    }

    class LittleEndianBufferReader : IBufferReader{
		public bool readBool(byte[] src, int startIndex){
			if ( src == null)
				throw new ArgumentNullException("byteArray");

			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");
			return src[startIndex] != 0;
		}

        public byte readUInt8(byte[] src, int startIndex){
            if ( src == null)
                throw new ArgumentNullException("byteArray");
            
			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");
            return (byte)src[startIndex];
        }

        public int readInt8(byte[] src, int startIndex){
            if ( src == null)
                throw new ArgumentNullException("byteArray");
            
			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");
            return (int)src[startIndex];
        }

        public ushort readUInt16(byte[] src, int startIndex){
            if ( src == null)
                throw new ArgumentNullException("byteArray");
            
			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");

			if (src.Length - 2 < startIndex )
				throw new ArgumentException ("Destination array is not long"
					+ " enough to copy all the items in the collection."
					+ " Check array index and length.");
            ushort ret = 0;
            ushort tmp = (ushort)src[startIndex];
            ret |= tmp;
            tmp = (ushort)src[startIndex + 1];
            ret |= (ushort)(tmp << 8);
            return ret;
        }

        public short readInt16(byte[] src, int startIndex){
            return (short)readUInt16(src, startIndex);
        }

        public uint readUInt32(byte[] src, int startIndex){
            if ( src == null)
                throw new ArgumentNullException("byteArray");
            
			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");

			if (src.Length - 4 < startIndex )
				throw new ArgumentException ("Destination array is not long"
					+ " enough to copy all the items in the collection."
					+ " Check array index and length.");
            uint ret = 0;
            uint tmp = (uint)src[startIndex];
            ret |= tmp;
            tmp = (uint)src[startIndex + 1];
            ret |= (uint)(tmp << 8);
            tmp = (uint)src[startIndex + 2];
            ret |= (uint)(tmp << 16);
            tmp = (uint)src[startIndex + 3];
            ret |= (uint)(tmp << 24);
            return ret;
        }

        public int readInt32(byte[] src, int startIndex){
            return (int)readUInt32(src, startIndex);
        }

		public ulong readUInt64(byte[] src, int startIndex){
			if ( src == null)
				throw new ArgumentNullException("byteArray");

			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");

			if (src.Length - 8 < startIndex )
				throw new ArgumentException ("Destination array is not long"
					+ " enough to copy all the items in the collection."
					+ " Check array index and length.");

			return ((ulong)src[startIndex])
				| (((ulong)src[startIndex+1]) << 8)
				| (((ulong)src[startIndex+2]) << 16)
				| (((ulong)src[startIndex+3]) << 24)
				| (((ulong)src[startIndex+4]) << 32)
				| (((ulong)src[startIndex+5]) << 40)
				| (((ulong)src[startIndex+6]) << 48)
				| (((ulong)src[startIndex+7]) << 56);
		}

		public long readInt64(byte[] src, int startIndex){
			return (long)readUInt64(src, startIndex);
		}

    }

    class BigEndianBufferReader : IBufferReader{
		public bool readBool(byte[] src, int startIndex){
			if ( src == null)
				throw new ArgumentNullException("byteArray");

			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");
			return src[startIndex] != 0;
		}

        public byte readUInt8(byte[] src, int startIndex){
            if ( src == null)
                throw new ArgumentNullException("byteArray");
            
			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was" + " out of range. Must be non-negative and less than the" + " size of the collection.");
            return (byte)src[startIndex];
        }

        public int readInt8(byte[] src, int startIndex){
            if ( src == null)
                throw new ArgumentNullException("byteArray");
            
			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was" + " out of range. Must be non-negative and less than the" + " size of the collection.");
            return (int)src[startIndex];
        }

        public ushort readUInt16(byte[] src, int startIndex){
            if ( src == null)
                throw new ArgumentNullException("byteArray");
            
			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new Exception("startIndex: " + "Index was" + " out of range. Must be non-negative and less than the" + " size of the collection.");

			if (src.Length - 2 < startIndex )
				throw new ArgumentException ("Destination array is not long" + " enough to copy all the items in the collection." + " Check array index and length.");
            ushort ret = 0;
            ushort tmp = (ushort)src[startIndex];
            ret |= (ushort)(tmp<<8);
            tmp = (ushort)src[startIndex + 1];
            ret |= tmp;
            return ret;
        }

        public short readInt16(byte[] src, int startIndex){
            return (short)readUInt16(src, startIndex);
        }

        public uint readUInt32(byte[] src, int startIndex){
            if ( src == null)
                throw new ArgumentNullException("byteArray");
            
			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");

			if (src.Length - 4 < startIndex )
				throw new ArgumentException ("Destination array is not long"
					+ " enough to copy all the items in the collection."
					+ " Check array index and length.");
            uint ret = 0;
            uint tmp = (uint)src[startIndex];
            ret |= (uint)(tmp<<24);
            tmp = (uint)src[startIndex + 1];
            ret |= (uint)(tmp << 16);
            tmp = (uint)src[startIndex + 2];
            ret |= (uint)(tmp << 8);
            tmp = (uint)src[startIndex + 3];
            ret |= tmp;
            return ret;
        }

        public int readInt32(byte[] src, int startIndex){
            return (int)readUInt32(src, startIndex);
        }

		public ulong readUInt64(byte[] src, int startIndex){
			if ( src == null)
				throw new ArgumentNullException("byteArray");

			if ( startIndex< 0 || (startIndex> src.Length - 1))
				throw new ArgumentException("startIndex: " + "Index was"
					+ " out of range. Must be non-negative and less than the"
					+ " size of the collection.");

			if (src.Length - 8 < startIndex )
				throw new ArgumentException ("Destination array is not long"
					+ " enough to copy all the items in the collection."
					+ " Check array index and length.");

			return ((ulong)src[startIndex+7])
				| (((ulong)src[startIndex+6]) << 8)
				| (((ulong)src[startIndex+5]) << 16)
				| (((ulong)src[startIndex+4]) << 24)
				| (((ulong)src[startIndex+3]) << 32)
				| (((ulong)src[startIndex+2]) << 40)
				| (((ulong)src[startIndex+1]) << 48)
				| (((ulong)src[startIndex+0]) << 56);
		}

		public long readInt64(byte[] src, int startIndex){
			return (long)readUInt64(src, startIndex);
		}
    }
}