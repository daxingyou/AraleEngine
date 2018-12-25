using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System;
using System.Text;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;

namespace Arale.Engine
{ 
    public class XmlPatch
    {
	    public const string rootName="Root";
	    public const string leafName="Data";
	    public const string xmlName ="version";
	    public const string differFolder = "_Differ";
	    public const string publishFolder = "_Publish";
	    string targetPath;
	    XmlDocument xml;
	    public XmlPatch(string path)
	    {
		    targetPath = path;
	    }

    #region 创建xml树
	    //根据目录关系生成xml树
	    public void make(int xmlVersion)
	    {
		    int id = 0;
		    if(xmlVersion<1)xmlVersion = 1;
		    xml = new XmlDocument();
		    string tm = DateTime.Now.ToString("yyyyMMdd");
		    string xmlStr = string.Format ("<{0} version='{1}' date='{2}'></{3}>",rootName,xmlVersion,tm,rootName);
		    xml.LoadXml(xmlStr);

		    XmlElement root = xml.DocumentElement; 
		    XmlDeclaration c = xml.CreateXmlDeclaration ("1.0", "utf-8", null);
		    xml.InsertBefore (c, root);

		    DirectoryInfo dir=new DirectoryInfo(targetPath);
		    int pre = targetPath.Length;
		    FileInfo[] fis = dir.GetFiles ("*.data", SearchOption.AllDirectories);
		    foreach(FileInfo fi in fis)
		    {
			    string md5 =FileUtils.GetMd5Hash(fi.FullName);
				if(string.IsNullOrEmpty(md5))return;
                string path = null;
                if (fi.DirectoryName.Length >= pre) path = fi.DirectoryName.Remove(0, pre);
			
			    XmlNode dirNode = getPathNode(path,true);
			    XmlNode fileNode = xml.CreateElement(leafName);
			    dirNode.AppendChild(fileNode);

				XmlNode attr = null;
			    attr = fileNode.Attributes.GetNamedItem("id");
			    if(null==attr)attr=fileNode.Attributes.Append(xml.CreateAttribute("id"));
			    attr.Value = (++id).ToString();
				attr = fileNode.Attributes.GetNamedItem("name");
				if(null==attr)attr=fileNode.Attributes.Append(xml.CreateAttribute("name"));
				attr.Value = fi.Name;
				attr = fileNode.Attributes.GetNamedItem("size");
				if(null==attr)attr=fileNode.Attributes.Append(xml.CreateAttribute("size"));
				attr.Value = fi.Length.ToString();
				attr = fileNode.Attributes.GetNamedItem("md5");
				if(null==attr)attr=fileNode.Attributes.Append(xml.CreateAttribute("md5"));
				attr.Value = md5;

				int part = getPartCode (string.IsNullOrEmpty(path)?fi.Name:path+"/"+fi.Name);
				if (part > 0)
				{//set part code
					attr = fileNode.Attributes.GetNamedItem ("part");
					if (null == attr)attr = fileNode.Attributes.Append (xml.CreateAttribute ("part"));
					attr.Value = part.ToString ();
				}
		    }
		    //save version.xml
			byte[] utf8 = Encoding.GetEncoding("UTF-8").GetBytes(formatXml(xml));
		    string savePath = targetPath + "/" + xmlName + ".xml";
		    FileStream fs = new FileStream(savePath, FileMode.Create);
		    fs.Write(utf8, 0, utf8.Length);
		    fs.Close();
		    Debug.Log ("make version ok");
	    }

		string formatXml(XmlDocument doc)
		{
			StringWriter sw = new StringWriter ();
			XmlTextWriter w = new XmlTextWriter (sw);
			w.Indentation = 2;
			w.Formatting = Formatting.Indented;
			doc.WriteContentTo (w);
			return sw.ToString ();
		}
    #endregion


    #region 创建差分文件
	    //创建版本跟新差分文件
	    public void differ()
	    {
		    string differPath = targetPath+differFolder+"/";
		    if(Directory.Exists(differPath))Directory.Delete(differPath,true);
		    Directory.CreateDirectory (differPath);
		    DirectoryInfo dir=new DirectoryInfo(targetPath);
		    FileInfo[] fis = dir.GetFiles ("*.xml", SearchOption.AllDirectories);
		    foreach(FileInfo fi in fis)
		    {
			    string s = fi.Name.Substring(7);
			    s = s.Remove(s.IndexOf('.'));
			    if(string.IsNullOrEmpty(s))continue;
			    differ(int.Parse(s));
		    }
	    }

	    //创建与发布版本的查分文件
	    public void differPublish()
	    {
		    string differPath = targetPath+differFolder+"/";
		    if(Directory.Exists(differPath))Directory.Delete(differPath,true);
		    Directory.CreateDirectory (differPath);

		    xml = new XmlDocument ();
		    xml.Load(targetPath+xmlName+".xml");
		    XmlDocument oldXml = new XmlDocument ();
		    //Application.streamingAssetsPath根目录下放发布版本的xml版本文件/
		    oldXml.Load (Application.streamingAssetsPath+"/"+xmlName+".xml");
		    differNode (oldXml.SelectSingleNode (rootName));
		    //删除没有含义的节点
		    removeEmptyNode(xml.SelectSingleNode(rootName));
		    //save version.xml
		    XmlNode n = xml.SelectSingleNode (rootName);
		    n.Attributes.Append(xml.CreateAttribute("diff")).Value="true";
		    byte[] utf8 = Encoding.GetEncoding("UTF-8").GetBytes(xml.InnerXml);
		    FileStream fs = new FileStream(targetPath+differFolder+"/"+xmlName+".xml", FileMode.Create);
		    fs.Write(utf8, 0, utf8.Length);
		    fs.Close();
	    }

	    //根据某个版本xml文件生成差分文件
	    void differ(int ver)
	    {
		    xml = new XmlDocument ();
		    xml.Load(targetPath+xmlName+".xml");
		    XmlDocument oldXml = new XmlDocument ();
		    oldXml.Load (targetPath + xmlName + ver + ".xml");
		    differNode (oldXml.SelectSingleNode (rootName));
		    //删除没有含义的节点
		    removeEmptyNode(xml.SelectSingleNode(rootName));
		    //save version.xml
		    XmlNode n = xml.SelectSingleNode (rootName);
		    n.Attributes.Append(xml.CreateAttribute("diff")).Value="true";
		    byte[] utf8 = Encoding.GetEncoding("UTF-8").GetBytes(xml.InnerXml);
		    FileStream fs = new FileStream(targetPath+differFolder+"/"+ver+".xml", FileMode.Create);
		    fs.Write(utf8, 0, utf8.Length);
		    fs.Close();
	    }
		
	    //根据目标xml文件节点进行差分比较
	    void differNode(XmlNode n)
	    {
		    string xpath = getNodeXPath (n);
		    XmlNode sn = getXPathNode (xpath);
		    if(n.Name==leafName)
		    {//文件
			    if(sn==null)
			    {
				    string attrName = n.Attributes["name"].Value;
				    xpath = xpath.Remove(xpath.LastIndexOf('/')+1)+leafName+"[@name='"+attrName+"']";
				    XmlNode delNode = getXPathNode(xpath,true);
				    XmlNode attr = delNode.Attributes.Append(xml.CreateAttribute("name"));
				    attr.Value = attrName;
				    attr = delNode.Attributes.Append(xml.CreateAttribute("del"));
				    attr.Value = "true";
			    }
			    else
			    {
				    //md5相同,文件有可能不同，几率很低，需要手动设置force属性强制更新
				    if(sn.Attributes["md5"].Value == n.Attributes["md5"].Value && null==sn.Attributes["forced"])
				    {
					    sn.ParentNode.RemoveChild(sn);
				    }
			    }
		    }
		    else
		    {//目录
			    if(sn==null)
			    {
				    xpath = xpath.Remove(xpath.LastIndexOf('/')+1)+n.Name;
				    XmlNode delNode = getXPathNode(xpath,true);
				    XmlNode attr = delNode.Attributes.Append(xml.CreateAttribute("del"));
				    attr.Value = "true";
				    return;
			    }
			    else
			    {
				    XmlNodeList ns = n.ChildNodes;
				    for(int i=ns.Count-1;i>=0;--i)
				    {
					    differNode(ns[i]);
				    }
			    }
		    }
	    }
	#endregion


	#region 节点映射
	    //根据文件路径查找或创建xml节点
	    XmlNode getPathNode(string path, bool create=false)
	    {
            try
            {
                if (string.IsNullOrEmpty(path)) return xml.DocumentElement;
                path =path.Replace('\\','/');
		        XmlNode parent = xml.SelectSingleNode (path);
		        if(!create||null!=parent)return parent;
		        parent = xml.DocumentElement;
		        string[] dirs = path.Split('/');
		        string xpath = "/"+rootName;
		        for(int i=0,max=dirs.Length;i<max;++i)
		        {
			        xpath+="/"+dirs[i];
			        XmlNode n = xml.SelectSingleNode(xpath);
			        if(n==null)
				        parent = parent.AppendChild(xml.CreateElement(dirs[i]));
			        else
				        parent = n;
		        }
		        return parent;
            }
            catch(Exception e)
            {
                Debug.LogError(path);
                return null;
            }
        }
	
	    //获取xml节点对应的相对路径
	    string getNodePath(XmlNode node, bool create=false)
	    {
		    XmlNode attr = node.Attributes ["name"];
		    string path = "";
		    if(attr!=null)
		    {
			    path = attr.Value;
			    node = node.ParentNode;
		    }
		    while(node.Name!=rootName)
		    {
			    path=node.Name+'/'+path;
			    node = node.ParentNode;
		    }
		    if(create)createFilePath(path);
		    return path;
	    }

	    void createFilePath(string path)
	    {
		    string dir = targetPath;
		    string[] dirs = path.Split ('/');
		    for(int i=0,max=dirs.Length-1; i<max; ++i)
		    {
			    dir += dirs[i]+'/';
			    if(!Directory.Exists(dir))Directory.CreateDirectory(dir);
		    }
	    }

		//获取离path最近的节点
		XmlNode getPathNearNode(XmlNode n, string path)
		{
			int idx = path.IndexOf('/');
			string name = idx>0?path.Substring (0, idx):path;
			XmlNodeList ns = n.ChildNodes;
			for (int i = 0; i < ns.Count; ++i)
			{
				XmlNode t = ns [i];
				if (t.Name == name || (t.Name == leafName && t.Attributes["name"].Value == name))
				{
					return idx>0?getPathNearNode (t, path.Substring (idx + 1)):t;
				}
			}
			return n;
		}

	    //根据xpath路径查找或创建xml节点
	    XmlNode getXPathNode(string xpath,bool create=false)
	    {
		    XmlNode parent = xml.SelectSingleNode(xpath);
		    if(!create||parent!=null)return parent;
		    string[] xpathnodes = xpath.Split(new string[]{"/"},StringSplitOptions.RemoveEmptyEntries);
		    parent = xml.DocumentElement;
		    string subxpath="";
		    for(int i=0,max=xpathnodes.Length;i<max;++i)
		    {
			    subxpath+="/"+xpathnodes [i];
			    XmlNode n = xml.SelectSingleNode(subxpath);
			    if(n==null)
			    {
				    int idx = xpathnodes[i].IndexOf('[');//带属性参数
				    string nodeName = idx<0?xpathnodes[i]:xpathnodes[i].Remove(idx);
				    parent = parent.AppendChild(xml.CreateElement(nodeName));
			    }
			    else 
			    {
				    parent=n;
			    }
		    }
		    return parent;
	    }

	    //获取节点的xpath
	    string getNodeXPath(XmlNode n)
	    {
		    XmlNode attr = n.Attributes ["name"];
		    string path=attr==null?"":"[@name='"+attr.Value+"']";
		    while(n.Name!=rootName)
		    {
			    path='/'+n.Name+path;
			    n = n.ParentNode;
		    }
		    return rootName+path;
	    }

	    //根据文件路径生成xpath
	    string getFileXPath(string filePath)
	    {
		    //Root/Table/Data[@name='tables.data']
		    filePath = filePath.Remove (0, targetPath.Length);
		    string name = System.IO.Path.GetFileName (filePath);
		    filePath = filePath.Substring (0, filePath.Length - name.Length);
		    return "Root/"+filePath+"Data[@name='"+name+"']";
	    }

	    //优化差分树，清除没含义的空节点
	    void removeEmptyNode(XmlNode n)
	    {
		    //如果是文件节点不处理
		    if(n.Name==leafName)return;
		    //如果是要删除的子目录，子目录节点都删除不再处理
		    XmlNodeList ns = n.ChildNodes;
		    if(null!=n.Attributes["del"])
		    {
			    for(int i=ns.Count-1;i>=0;--i)n.RemoveChild(ns[i]);
			    return;
		    }
		    //子节点删除必须倒序，否则迭代出错
		    for(int i=ns.Count-1;i>=0;--i)
		    {
			    removeEmptyNode(ns[i]);
		    }
		    if(!n.HasChildNodes && n.Name!=rootName)
		    {
			    n.ParentNode.RemoveChild(n);
		    }
	    }
    #endregion


    #region 列出下载清单
	    public class DFileInfo
	    {
		    public string id;
		    public string path;
			public string md5;
		    public bool   zip;
		    public uint   size;
		    public DFileInfo(string id, string path, string md5, bool zip, uint size)
		    {
			    this.id = id;
			    this.path = path;
				this.md5 = md5;
			    this.zip = zip;
			    this.size = size;
		    }
	    }

	    public delegate void OnProgress(float percent);
		public List<DFileInfo> listDownFiles(OnProgress onProgress, ref bool cancel, int partCode=1000)
	    {
		    xml = new XmlDocument ();
		    xml.Load(targetPath+xmlName+".xml");
		    List<DFileInfo> ls = new List<DFileInfo> ();
			XmlNodeList ns = xml.GetElementsByTagName (leafName);
		    for(int i=0,max=ns.Count;i<max;++i)
		    {
			    if(cancel)return null;
			    onProgress(1.0f*(i+1)/max);
			    XmlElement n = ns[i] as XmlElement;
				XmlNode attr = n.Attributes ["part"];
				if (attr!=null && int.Parse (attr.Value) > partCode)continue;
			    if(null != n.Attributes ["del"])continue;
			    string filePath = targetPath+getNodePath(n,true);
				string md5 = n.Attributes ["md5"].Value;
				if(File.Exists(filePath) && md5==FileUtils.GetMd5Hash(filePath) && null==n.Attributes ["forced"])continue;
			    DFileInfo fi = new DFileInfo(n.Attributes["id"].Value,filePath,md5,false,uint.Parse(n.Attributes["size"].Value));
			    ls.Add(fi);
		    }
		    return ls;
	    }

	    public bool verifyFile(string path)
	    {
		    if (xml == null) {
			    string xmlPath = targetPath + xmlName + ".xml";
			    xml = new XmlDocument ();
			    xml.Load (xmlPath);
		    }

		    XmlNode n = getXPathNode (getFileXPath(path));
			if(n==null)return false;
		    return FileUtils.GetMd5Hash (path) == n.Attributes ["md5"].Value;
	    }
    #endregion


    #region 删除无用文件
	    //根据xml删除无效的文件
	    public void removeUnusedFile()
	    {
		    XmlDocument x = new XmlDocument ();
		    x.Load(targetPath+xmlName+".xml");
		    removeUnusedFile (x);
	    }

	    public void removeUnusedFile(XmlDocument x)
	    {
		    XmlNode n = x.SelectSingleNode (rootName);
		    bool diff = (null != n.Attributes ["diff"]) ? true : false;
		    if (diff)
		    {
			    removeFileByNode(n);
		    }
		    else
		    {
			    DirectoryInfo dir = new DirectoryInfo(targetPath);
			    FileInfo[] fis = dir.GetFiles("*.data",SearchOption.AllDirectories);
			    foreach(FileInfo fi in fis)
			    {
				    if(!containFile(x, fi.FullName))fi.Delete();
			    }
		    }
	    }

	    void removeFileByNode(XmlNode n)
	    {
		    if(null != n.Attributes["del"])
		    {
			    string path = targetPath + getNodePath(n);
			    if(n.Name == leafName)
				    File.Delete(path);
			    else
				    Directory.Delete(path,true);
		    }
		    else
		    {
			    if(n.Name == leafName)return;
			    XmlNodeList ns = n.ChildNodes;
			    for(int i=0; i<ns.Count; ++i)
			    {
				    removeFileByNode(ns[i]);
			    }
		    }
	    }

	    //check file is in version.xml
	    public bool containFile(XmlDocument x, string fileName)
	    {
		    string dir = System.IO.Path.GetDirectoryName (fileName);
		    string name = System.IO.Path.GetFileName (fileName);
		    string xmlPath = "Root/"+dir.Substring(targetPath.Length).Replace("\\","/");
		    XmlNode n = x.SelectSingleNode(xmlPath);
		    if(n==null)return false;
		
		    XmlNodeList ns = n.ChildNodes;
		    foreach(XmlElement bd in ns)
		    {
			    if(name == bd.GetAttribute("name"))return true;
		    }
		    return false;
	    }

    #endregion


	#region 分包
		XmlDocument partXml;
		string      resPart;
		//根据ResPart配置文件获取分包号,ResPart文件可以在version.xml文件基础上修改
		int getPartCode(string abPath)
		{
			if(resPart==null)resPart = Application.dataPath + "/ResPart.xml";
			if (!File.Exists (resPart))return 0;
			if (partXml == null)
			{
				partXml = new XmlDocument ();
				partXml.Load (resPart);
			}

			int partCode = 0;
			abPath =abPath.Replace('\\','/');
			XmlNode n = getPathNearNode (partXml.SelectSingleNode (rootName), abPath);
			while(n != null)
			{//取最小作用域的part
				if (n.Attributes != null && n.Attributes ["part"] != null)
				{
					partCode = int.Parse (n.Attributes ["part"].Value);
					break;
				}
				n = n.ParentNode;
			}
			return partCode;
		}

		//根据version.xml生成zip
		public void makeZip2(string savePath, int partCode=1000)
		{//下面的压缩方式在某些手机上不支持
			xml = new XmlDocument();
			xml.Load(targetPath + xmlName + ".xml");

			List<string> files = new List<string> ();
			XmlNodeList ns = xml.GetElementsByTagName (leafName);
			for(int i=0,max=ns.Count;i<max;++i)
			{
				XmlElement n = ns[i] as XmlElement;
				if (n.Attributes ["part"]!=null && int.Parse (n.Attributes ["part"].Value) > partCode)continue;
				files.Add (getNodePath(n));
			}
			files.Add (xmlName + ".xml");

			FileStream fs = null;
			FileStream zf = File.Create (savePath);
			try
			{
				ZipOutputStream zip = new ZipOutputStream (zf);
				zip.UseZip64 = UseZip64.Dynamic;//不设置,android提示压缩文件头不正确
				zip.SetLevel (9);
				byte[] buffer = new byte[4096];
				foreach (string file in files)
				{
					ZipEntry entry = new ZipEntry(file);
					entry.DateTime = DateTime.Now;
					zip.PutNextEntry(entry);
					using (fs = File.OpenRead(targetPath+file))
					{
						int sourceBytes;
						do
						{
							sourceBytes = fs.Read(buffer, 0, buffer.Length);
							zip.Write(buffer, 0, sourceBytes);
						} while (sourceBytes > 0);
					}
				}
				zip.Finish();
				zip.Close();
			}
			catch(Exception e)
			{
				if (fs != null)fs.Close ();
				zf.Close();
				throw(e);
			}
		}

		public void makeZip(string savePath, int partCode=1000)
		{
			string zipTmpPath = Application.dataPath+"/../Publish/ZipTmp/";
			if (Directory.Exists (zipTmpPath))Directory.Delete (zipTmpPath, true);
			Directory.CreateDirectory (zipTmpPath);

			xml = new XmlDocument();
			xml.Load(targetPath + xmlName + ".xml");

			List<string> files = new List<string> ();
			XmlNodeList ns = xml.GetElementsByTagName (leafName);
			for(int i=0,max=ns.Count;i<max;++i)
			{
				XmlElement n = ns[i] as XmlElement;
				if (n.Attributes ["part"]!=null && int.Parse (n.Attributes ["part"].Value) > partCode)continue;
				files.Add (getNodePath(n));
			}
			files.Add (xmlName + ".xml");

			foreach (string file in files)
			{
				FileUtils.copy(targetPath+file, zipTmpPath+file);
			}

			FastZip zip = new FastZip ();
			zip.CreateZip (savePath, zipTmpPath, true, "+\\.data$;+\\.xml$");
		}
	#endregion


	#region 版本信息
        public int getVersion()
        {
            string xmlPath = targetPath + "/" + xmlName + ".xml";
            if (!File.Exists(xmlPath)) return 0;
            try
            {
                XmlDocument x = new XmlDocument();
                x.Load(xmlPath);
                XmlNode root = x.SelectSingleNode("Root") as XmlNode;
                return int.Parse(root.Attributes["version"].Value);
            }
            catch (Exception e)
            {
                Log.e(e);
                return 0;
            }
        }
	#endregion
    }

}
