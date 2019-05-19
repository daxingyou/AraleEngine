/*DllExport.xml Format
<?xml version="1.0" encoding="utf-8"?>
<Root>
    <Module>
        <Export path="E:/AraleEngine.dll"></Export>
        <RefFile path="D:/Program Files/Unity 5.3.5/Editor/Data/Managed/UnityEngine.dll" />
        <SourceFile path="Scripts/CoreScripts/DevelopTools/Editor/GitTools.cs" />
        <Define>DEBUG;RELEASE</Define>
    </Module>
</Root>
*/
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

public class DllExport
{
    static string cscPath = "C:/Windows/Microsoft.NET/Framework/v4.0.30319/csc.exe";
    static string workPath;
    static string unityPath;
    static Thread exportThread;
    public class ExportItem
    {
        //导出类型
        public string moduleType = "library";
        //输出目录
        public string outPathName = "";
        //引用文件列表
        public List<string> refFiles = new List<string>();
        //导出文件列表
        public List<string> sourceFiles = new List<string>();
        //宏定义列表
        public List<string> defines = new List<string>();

        public string toCMD()
        {
            string stype= "/target:" + moduleType;
            string sout = "/out:" + outPathName;
            string sdef = toList("/define:",defines);
            string sref = toList("/r:",refFiles);
            //string sour = toList("",sourceFiles);
			string sour = "/recurse:Engine\\Core\\Log\\*.cs";
			string doc = "/doc:builddoc.xml";
            string s = stype + " " + sdef + " " + sref + " " + sout + " " + sour + " " + doc;
            UnityEngine.Debug.LogError(s);
            return s;
        }

        string toList(string head, List<string> strs)
        {
            string str = "";
            foreach (string s in strs)
            {
                str += " " + head + s;
            }
            return str;
        }
    }

    [MenuItem("开发工具/Dll/开始生成",false,0)]
    public static void start()
    {
        workPath = Application.dataPath+"/";
        exportThread = new Thread(exportFunc);
        exportThread.Start();
    }

    [MenuItem("开发工具/Dll/停止生成",false,1)]
    public static void stop()
    {
        if (exportThread != null)
        {
            exportThread.Abort();
        }
    }

    static void exportFunc()
    {
        export();
        exportThread = null;
    }



    static bool export()
    {
        Process p = new Process();
        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.WorkingDirectory = workPath;
		UnityEngine.Debug.LogError ("workpath="+workPath);
        System.Console.InputEncoding = System.Text.Encoding.UTF8;
        p.Start();

        StreamWriter sw = p.StandardInput;
        sw.WriteLine("begin");
        //导出运行时dll
        List<ExportItem> ls = parseConfigFile();
        for(int i=0;i<ls.Count;++i)
        {
            sw.WriteLine(string.Format("{0} /warn:1 {1}", cscPath, ls[i].toCMD()));
        }
        sw.Close();

        string log = p.StandardOutput.ReadToEnd();
        if (log.Contains("error"))
        {
            UnityEngine.Debug.LogError(log);
            return false;
        }
        else
        {
            UnityEngine.Debug.Log("export success");
            return true;
        }
    }

    static List<ExportItem> parseConfigFile()
    {
        List<ExportItem> ls = new List<ExportItem>();
        string configXml = workPath + "/DllExport.xml";
        if (!File.Exists(configXml))
        {
            UnityEngine.Debug.LogError("找不到DllExport.xml配置文件");
            return  ls;
        }

        XmlDocument x = new XmlDocument();
        x.Load(configXml);
        XmlNodeList md =  x.SelectNodes("Root/Module");
        for (int i = 0; i < md.Count; ++i)
        {
            ExportItem ei = new ExportItem();
            XmlNode n = md[i];
            XmlNode m = n.SelectSingleNode("./Export");
            ei.outPathName = m.Attributes["path"].Value;

            XmlNodeList ms = null;
            ms = n.SelectNodes("./RefFile");
            for (int j = 0; j < ms.Count; ++j)
            {
                ei.refFiles.Add("\""+ms[j].Attributes["path"].Value+"\"");
            }

            ms = n.SelectNodes("./SourceFile");
            for (int j = 0; j < ms.Count; ++j)
            {
                List<string> files = getSourceFile(ms[j]);
                ei.sourceFiles.AddRange(files);
            }

            m = n.SelectSingleNode("./Define");
            //ei.defines = m.InnerText;

            ls.Add(ei);
        }
        return ls;
    }

    static List<string> getSourceFile(XmlNode n)
    {
        List<string> ls = new List<string>();
        string path = n.Attributes["path"].Value;
        if (File.Exists(workPath + path))
        {
            ls.Add(workPath+path);
        }
        else if (Directory.Exists(workPath + path))
        {
            string includeExt = "";
            if (null!=n.Attributes["include"])
            {
                includeExt = n.Attributes["include"].Value;
            }
            string excludeExt = "";
            if (null!=n.Attributes["exclude"])
            {
                excludeExt = n.Attributes["exclude"].Value;
            }
            getSourceFile(ls, new DirectoryInfo(workPath + path), includeExt, excludeExt);
        }
        return ls;
    }

    static void getSourceFile(List<string> ls, DirectoryInfo dir, string include, string exclude)
    {
        FileInfo[] fis = dir.GetFiles();
        foreach (FileInfo fi in fis)
        {
            string ext = fi.Extension;
            if ((!string.IsNullOrEmpty(exclude))&&exclude.Contains(ext))
            {
                continue;
            }
            if ((!string.IsNullOrEmpty(include)) && (!include.Contains(ext)))
            {
                continue;
            }
            ls.Add(fi.FullName);
        }
        DirectoryInfo[] dis = dir.GetDirectories();
        foreach (DirectoryInfo di in dis)
        {
            string dirName = ":" + di.Name;
            if ((!string.IsNullOrEmpty(exclude))&&exclude.Contains(dirName))
            {
                continue;
            }
            getSourceFile(ls, di, include, exclude);
        }
    }
}
