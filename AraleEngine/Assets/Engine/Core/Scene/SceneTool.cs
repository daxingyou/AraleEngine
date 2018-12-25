using UnityEngine;
using System.Collections;
using System.Text;
using System;
using System.Xml;
using System.IO;

public class SceneTool : MonoBehaviour
{
	public void ExportXml()
	{
		string path = Application.dataPath+"/scene.xml";
		XmlDocument xml = new XmlDocument();
		string xmlStr = string.Format ("<Scene version='{0}'></Scene>", 0);
		xml.LoadXml(xmlStr);
		XmlElement root = xml.DocumentElement; 
		XmlDeclaration c = xml.CreateXmlDeclaration ("1.0", "utf-8", null);
		xml.InsertBefore (c, root);

		ActorCreator[] acs = GetComponentsInChildren<ActorCreator> ();
		for (int i = 0; i < acs.Length; ++i)
		{
			XmlNode n = xml.CreateElement("BornPoint");
			root.AppendChild(n);
			acs [i].Serialize (xml,n);
		}

		//save version.xml
		byte[] utf8 = Encoding.GetEncoding("UTF-8").GetBytes(FormatXml(xml));
		FileStream fs = new FileStream(path, FileMode.Create);
		fs.Write(utf8, 0, utf8.Length);
		fs.Close();
		Debug.Log ("save ok path="+path);
	}

	string FormatXml(XmlDocument xml)
	{
		StringBuilder sb = new StringBuilder ();
		StringWriter sw = new StringWriter (sb);
		XmlTextWriter xtw = new XmlTextWriter (sw);
		xtw.Formatting = Formatting.Indented;
		xtw.Indentation = 1;
		xtw.IndentChar = '\t';
		xml.WriteTo (xtw);
		xtw.Close ();
		return sb.ToString ();
	}
}
