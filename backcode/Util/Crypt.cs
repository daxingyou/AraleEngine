using UnityEngine;
using System.Collections;
using System.IO;

namespace Core.Utils
{
	public class Crypt
	{
		public static void DoCrypt(byte[] buf, string password)
		{
			int pid = 0;
			for (int i=0; i<buf.Length; i++,pid++)
			{
				if (pid >= password.Length)
				{
					pid = 0;
				}
				byte t = (byte)(buf[i] ^ (byte)password[pid]);
				int x = (int)password[pid] % 8;
				byte t1 = (byte)(t >> x);
				byte t2 = (byte)(t << (8 - x));
				buf[i] = (byte)(t1 | t2);
			}
		}
		
		public static void UnCrypt(byte[] buf, string password)
		{
			int pid = 0;
			for (int i = 0; i < buf.Length; i++, pid++)
			{
				if (pid >= password.Length)
				{
					pid = 0;
				}
				int x = (int)password[pid] % 8;
				byte t1 = (byte)(buf[i] << x);
				byte t2 = (byte)(buf[i] >> (8 - x));
				byte t = (byte)(t1 | t2);
				buf[i] = (byte)(t ^ (byte)password[pid]);
			}
		}
	
		public static void DoFileCrypt(string inpath, string outpath, string password)
		{
			FileStream InputFile = new FileStream (inpath, FileMode.Open);
			FileStream OutputFile = new FileStream (outpath, FileMode.Create);
			int count = 0;
			byte[] buf = new byte[1024];
			do {
				count = InputFile.Read (buf, 0, 1024);
				DoCrypt (buf, password);
				OutputFile.Write (buf, 0, count);
			} while (count > 0);
			InputFile.Close ();
			OutputFile.Close ();
		}

		public static void DoFileCrypt(string inpath, string password)
		{
			byte[] buf = File.ReadAllBytes (inpath);
			DoCrypt (buf, password);
			File.WriteAllBytes (inpath, buf);
		}
		
		public static void UnFileCrypt(string inpath, string outpath, string password)
		{
			FileStream InputFile = new FileStream(inpath, FileMode.Open);
			FileStream OutputFile = new FileStream(outpath, FileMode.Create);
			int count = 0;
			byte[] buf = new byte[1024];
			do
			{
				count = InputFile.Read(buf, 0, 1024);
				UnCrypt(buf, password);
				OutputFile.Write(buf, 0, count);
			} while (count > 0);
			InputFile.Close();
			OutputFile.Close();
		}
		
		public static void DoDirectoryCrypt(DirectoryInfo indir, DirectoryInfo outdir, string password, string filter=null)
		{
			foreach (FileInfo file in indir.GetFiles())
			{
				if(filter!=null)
				{
					string ext = file.Extension;
					if (string.IsNullOrEmpty(ext) || !filter.Contains (file.Extension))continue;
				}
				DoFileCrypt(file.FullName, outdir.FullName + "/" + file.Name, password);
			}
			
			foreach (DirectoryInfo dinfo in indir.GetDirectories())
			{
				DirectoryInfo newOutDir = outdir.CreateSubdirectory(dinfo.Name);
				DoDirectoryCrypt(dinfo, newOutDir, password, filter);
			}
		}

		public static void DoDirectoryCrypt(DirectoryInfo indir, string password, string filter=null)
		{
			foreach (FileInfo file in indir.GetFiles())
			{
				if(filter!=null)
				{
					string ext = file.Extension;
					if (string.IsNullOrEmpty(ext) || !filter.Contains (file.Extension))continue;
				}
				DoFileCrypt(file.FullName, password);
			}

			foreach (DirectoryInfo dinfo in indir.GetDirectories())
			{
				DoDirectoryCrypt(dinfo, password, filter);
			}
		}
	
		public static void UnDirectoryCrypt(DirectoryInfo indir, DirectoryInfo outdir, string password)
		{
			foreach (FileInfo file in indir.GetFiles())
			{
				UnFileCrypt(file.FullName, outdir.FullName + "/" + file.Name, password);
			}
			
			foreach (DirectoryInfo dinfo in indir.GetDirectories())
			{
				DirectoryInfo newOutDir = outdir.CreateSubdirectory(dinfo.Name);
				UnDirectoryCrypt(dinfo, newOutDir, password);
			}
		}
	}
}