#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml;
/*project tree
home
  |-unitproc
  |-Android
      |-sdk
      |   |-uc 渠道sdk
      |   |-xiaomi 渠道sdk
      |   |-alipay 功能sdk
      |   |-talkingdata 功能sdk
      |-icons
      |   |-1
      |   |-2
      |-app
          |-src
     
=============*/
using System.Text;


public class BatchBuild
{
	public static string unityOutPath{get{return Application.dataPath + "/../"+Application.productName;}} 
	public static string androidPath{get{return Application.dataPath + "/../../";}}
    public static string appPath { get { return androidPath + "/app/"; } }
    public static string mainPath { get { return androidPath + "/app/src/main/"; } }
    public static string manifestPath{get{return androidPath+"/app/src/main/AndroidManifest.xml";}}
	public static string valuePath{get{return androidPath+"/app/src/main/res/values";}}
	public static string resPath{get{return androidPath+"/app/src/main/res/";}}
	public static string javaPath{get{return androidPath+"/app/src/main/java/";}} 
	static BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
	class JenkinsParam
	{
		public string publish;
		public string configFile;
		public string appVer;
		public string resVer;
		public string obb;
		public string package;
	}
	
	[MenuItem("工具/打包/1.导出Android",false,0)]
	public static void buildAndroid()
	{ 
		Debug.Log ("[unity]buildAndroid");
		buildTargetGroup = BuildTargetGroup.Android;
		JenkinsParam param = initFromArgs();
		
		List<string> levels = new List<string>();
		foreach (EditorBuildSettingsScene sc in EditorBuildSettings.scenes) {
			if(!sc.enabled)continue;
			levels.Add(sc.path);
		}
		Debug.Log ("[unity]buildAndroid dst="+unityOutPath);
		BuildPipeline.BuildPlayer (levels.ToArray(), unityOutPath, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
		Debug.Log ("[unity]buildAndroid completed");
		copyUnity (param);
	}

	[MenuItem("工具/打包/1.导出IOS",false,0)]
	public static void buildIOS()
	{
		Debug.Log ("[unity]buildIOS");
        buildTargetGroup = BuildTargetGroup.iOS;
        JenkinsParam param = initFromArgs();
    }

	static void enableResource(bool enable)
	{
		string resourcePath = Application.dataPath + "/AOH/Resources";
		if(enable)
		{
			if(Directory.Exists(resourcePath))return;
			Directory.Move (resourcePath + "Hide", resourcePath);
		}
		else
		{
			if(!Directory.Exists(resourcePath))return;
			Directory.Move (resourcePath, resourcePath + "Hide");
		}
	}
	
	static void copyUnity(JenkinsParam param)
	{
		string appPath = unityOutPath + "/" + Application.productName;
		string targetPath = androidPath + "/app/src/main/assets";
		string srcPath = appPath+"/assets";
		Debug.Log ("[unity]copyAssets dst="+targetPath);
		FileUtil.ReplaceDirectory(srcPath, targetPath);
		Debug.Log ("[unity]copyAssets completed");

		targetPath = androidPath + "/app/src/main/jniLibs";
		srcPath = appPath+"/libs";
		Debug.Log ("[unity]copyLibs dst="+targetPath);
		CopyFiles (new DirectoryInfo (srcPath), new DirectoryInfo (targetPath));
		Debug.Log ("[unity]copyLibs completed");

		string obbPath = appPath + ".main.obb";
		targetPath = param.publish + "/main." + param.resVer + "." + param.package + ".obb";
		if (File.Exists (obbPath))
		{
			Debug.Log ("[unity]copyOBB dst="+targetPath);
			File.Move (obbPath, targetPath);
			Debug.Log ("[unity]copyOBB completed");
		}
	}
	static JenkinsParam initFromArgs()
	{
		string[] args = Environment.GetCommandLineArgs ();
        JenkinsParam param = null;
        if (args.Length>1)
        {
            string arg = args[args.Length - 1].Replace('\\', '/');
            Debug.Log("[unity]batchmode参数:" + arg);
            try
            {
                param = JsonUtility.FromJson<JenkinsParam>(arg);
                Debug.LogError("[unity]config file:" + param.configFile);
            }
            catch (Exception e)
            {
                Debug.LogError("[unity]json err!!!");
                return null;
            }
        }
        else
        {
            param = defaultParam();
        }
		
		PlayerSettings.Android.useAPKExpansionFiles = bool.Parse(param.obb);
		runConfig (param);
		return param;
	}

    static JenkinsParam defaultParam()
    {
        JenkinsParam param = new JenkinsParam();
        param.publish = "E:/";
        param.configFile = Application.dataPath + "/BuildConfig.xml";
        param.resVer  = "1";
        param.appVer  = "1.0";//bc 3位有错
        param.obb = "false";
        return param;
    }

    static void runConfig(JenkinsParam param)
	{
		XmlDocument config = new XmlDocument ();
		config.Load (param.configFile);

		XmlNode n = config.SelectSingleNode ("/BuildConfig/icon");
		if (n != null)configIcon(n.InnerText);

		n = config.SelectSingleNode ("/BuildConfig/appname");
		if (n != null)configName(n.InnerText);

		n = config.SelectSingleNode ("/BuildConfig/package");
		if (n != null)configPackage(n.InnerText,param);

		n = config.SelectSingleNode ("/BuildConfig/define");
		if (n != null)configDefine(n.InnerText);
		
		XmlNodeList ns = config.SelectNodes ("/BuildConfig/sdk");
		foreach (XmlNode nd in ns)
		{
			configSdk(nd.InnerText, nd.Attributes["param"].Value, param);
		}
	}

	static void configIcon(string val)
	{
		DirectoryInfo di = new DirectoryInfo (resPath);
		DirectoryInfo[] dis = di.GetDirectories ("drawable*");
		foreach (DirectoryInfo d in dis)
		{
			d.Delete(true);
		}
		CopyFiles(new DirectoryInfo(androidPath+"/icons/"+val), new DirectoryInfo(resPath));
	}

	static void configName(string val)
	{
		string path = valuePath + "/strings.xml";
		string txt = File.ReadAllText (path);
		txt = txt.Replace ("${app_name}", val);
		File.WriteAllText (path, txt);
	}

	static void configPackage(string val, JenkinsParam param)
	{
		param.package = val;
		string txt = File.ReadAllText (manifestPath);
		txt = txt.Replace ("${package}", val);
		txt = txt.Replace ("${versionName}", param.appVer);
		txt = txt.Replace ("${versionCode}", param.resVer);
		File.WriteAllText (manifestPath, txt);
	}

	static void configDefine(string val)
	{
		PlayerSettings.SetScriptingDefineSymbolsForGroup (buildTargetGroup, val);
	}

	static void configSdk(string val, string param, JenkinsParam jenkinsParam)
	{
		Debug.Log ("[unity]"+val+","+param);
        string sdkPath = androidPath + "sdk/" + val + "/";
        if (!Directory.Exists(sdkPath)) throw new Exception("sdk "+val+" not found!!!");
        CopyFiles(new DirectoryInfo(sdkPath +"assets"), new DirectoryInfo(mainPath+"assets"));
        CopyFiles(new DirectoryInfo(sdkPath+"libs"), new DirectoryInfo(appPath+"libs"));
        CopyFiles(new DirectoryInfo(sdkPath+"jniLibs"), new DirectoryInfo(mainPath+ "jniLibs"));
        CopyFiles(new DirectoryInfo(sdkPath+"java"), new DirectoryInfo(mainPath + "java"));
        CopyFiles(new DirectoryInfo(sdkPath+"res"), new DirectoryInfo(mainPath + "res"));
        File.Copy(sdkPath + "build.gradle", appPath + "build.gradle", true);
        XmlDocument xml = new XmlMerge().merge(sdkPath + "AndroidManifest.xml", mainPath + "AndroidManifest.xml");
        if(xml!=null)xml.Save(mainPath + "AndroidManifest.xml");
    }

    static bool CopyFiles(DirectoryInfo from, DirectoryInfo to,  string searchPattern="*.*")
    {
        if (!from.Exists) return false;
        FileInfo[] fis = from.GetFiles(searchPattern);
        if (!to.Exists) to.Create();
        foreach(FileInfo fi in fis)
        {
            fi.CopyTo(Path.Combine(to.FullName, fi.Name), true);
        }
        DirectoryInfo[] dis = from.GetDirectories();
        foreach (DirectoryInfo di in dis)
        {
            if (!CopyFiles(di, new DirectoryInfo(Path.Combine(to.FullName, di.Name)))) continue;
        }
        return true;
    }
}

public class XmlMerge
{
    const string RootName = "manifest";
    const string NameAttr = "android:name";
    XmlDocument from;
    XmlNamespaceManager fromXNM;
    XmlDocument to;
    XmlNamespaceManager toXNM;
    public XmlDocument merge(string fromPath, string toPath)
    {
        if (!File.Exists(fromPath) || !File.Exists(toPath)) return null;
        from = new XmlDocument();
        from.Load(fromPath);
        fromXNM = new XmlNamespaceManager(from.NameTable);
        fromXNM.AddNamespace("android", from.DocumentElement.Attributes["xmlns:android"].Value);
        to = new XmlDocument();
        to.Load(toPath);
        toXNM = new XmlNamespaceManager(to.NameTable);
        toXNM.AddNamespace("android", to.DocumentElement.Attributes["xmlns:android"].Value);
        mergeNode(from.DocumentElement);
        return to;
    }

    void mergeNode(XmlNode node)
    {
        if (node.NodeType == XmlNodeType.Comment) return;//忽略注释
        string xpath = getNodeXPath(node);
        Debug.LogError(xpath);
        XmlNode tonode = to.SelectSingleNode(xpath, toXNM);
        if (tonode != null)
        {
            if (!string.IsNullOrEmpty(node.InnerText))tonode.InnerText = node.InnerText;
            mergeAttr(node, tonode);
            foreach(XmlNode n in node.ChildNodes) mergeNode(n);
        }
        else
        {
            xpath = xpath.Substring(0,xpath.LastIndexOf('/'));
            tonode = to.SelectSingleNode(xpath);
            tonode.AppendChild(to.ImportNode(node,true));
        }
    }

    void mergeAttr(XmlNode fromNode, XmlNode toNode)
    {
        foreach (XmlNode node in fromNode.Attributes)
        {
            XmlAttribute fromAttr = node as XmlAttribute;
            XmlAttribute toAttr = toNode.Attributes.GetNamedItem(fromAttr.Name) as XmlAttribute;
            if (toAttr == null) toAttr = toNode.Attributes.Append(to.CreateAttribute(fromAttr.Name));
            toAttr.Value = fromAttr.Value;
        }
    }

    string getNodeXPath(XmlNode n)
    {
        XmlNode attr = n.Attributes[NameAttr];
        string path = attr == null ? "" : string.Format("[@{0}='{1}']", NameAttr, attr.Value);
        while (n.Name != RootName)
        {
            path = '/' + n.Name + path;
            n = n.ParentNode;
        }
        return RootName + path;
    }

    bool needMerge(XmlNode n)
    {
        if (n.Attributes[NameAttr] != null) return true;
        if (n.Name == RootName || n.Name == "application") return true;
        return false;
    }
}
#endif
