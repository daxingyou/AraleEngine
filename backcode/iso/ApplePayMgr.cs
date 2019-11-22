using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LitJson;
using LingQu.Util;

public class ApplePayMgr : MonoBehaviour
{
    public List<string> productInfo = new List<string>();
#if APAY
    [DllImport("__Internal")]
	private static extern void TestMsg();//测试信息发送

	[DllImport("__Internal")]
	private static extern void TestSendString(string s);//测试发送字符串

	[DllImport("__Internal")]
	private static extern void TestGetString();//测试接收字符串

	[DllImport("__Internal")]
	private static extern void InitIAPManager();//初始化

	[DllImport("__Internal")]
	private static extern bool IsProductAvailable();//判断是否可以购买

	[DllImport("__Internal")]
	private static extern void RequstProductInfo(string s);//获取商品信息

	[DllImport("__Internal")]
	private static extern void BuyProduct(string s);//购买商品
#endif

    //测试从xcode接收到的字符串
    private void IOSToU(string s)
    {
        Debug.Log("[MsgFrom ios]" + s);
    }

    //获取product列表
    private void ShowProductList(string s)
    {
        productInfo.Add(s);
    }

    private bool back = false;

    //获取商品回执
    private void ProvideContent(string s)
    {
        Debug.Log("[MsgFrom ios]proivideContent : " + s);
        JsonData json = new JsonData();
        byte[] b = System.Text.Encoding.UTF8.GetBytes(s);
        s = System.Convert.ToBase64String(b);
        json["Receipt"] = s;
        json["TransactionId"] = appPayId;
        json["ChargeID"] = chargeId;
        json["RMB"] = rmb;
		//发送参数到游戏服务器,游戏服务器拿到Receipt后到苹果服务器验证是否发货
        back = true;
        LockScreen(false);
    }

    //交易ID
    public string appPayId;

    //chargeid
    public int chargeId;

    //RMB
    public float rmb;

    //IOS productid

    public void SetCharge(int Chargeid)
    {
        chargeId = Chargeid;
        rmb = TypeChargeMgr.It.GetItem(Chargeid).renminbi;
        string productid = TypeChargeMgr.It.GetItem(Chargeid).Purchase_id;
        RequestApplePay(productid);
    }

    private void SetAppPayId(string s)
    {
        appPayId = s;
    }

    private void PayFail(string fails)
    {
        Debug.Log("pay fail fails =" + fails);
        ScriptState.Instance.GetLuaFunction("ShowMessagetil").call(StrUtil.GetStr("AppPayFail"));
        LockScreen(false);
    }

    // Use this for initialization
    private void Start()
    {
        _instance = this;
#if APAY
#if !UNITY_EDITOR

        InitIAPManager();
#endif
#endif
    }

    private static ApplePayMgr _instance;

    public static ApplePayMgr It
    {
        get
        {
            return _instance;
        }
    }

    public void LockScreen(bool locked)
    {
        ScriptState.Instance.GetLuaFunction("SetLoadStatus").call(locked); ;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            //ScriptState.Instance.GetLuaFunction("ErrorRetCodeNotify").call(3);
        }
    }

    private void OnGUI()
    {
        return;

        if (Btn("GetProducts"))
        {
#if APAY
			if(!IsProductAvailable())
				throw new System.Exception("IAP not enabled");
#endif
            productInfo = new List<string>();
            //			RequstProductInfo("com.dollar.one\tcom.dollar.two");
            //			RequstProductInfo("com.aladdin.fishpocker1");
#if APAY
			RequstProductInfo("com.lvc.yzmxd");
#endif
        }

        GUILayout.Space(40);

        if (back)
            GUI.Label(new Rect(10, 150, 100, 100), "Message back");

        for (int i = 0; i < productInfo.Count; i++)
        {
            if (GUILayout.Button(productInfo[i], GUILayout.Height(100), GUILayout.MinWidth(200)))
            {
                string[] cell = productInfo[i].Split('\t');
                Debug.Log("[Buy]" + cell[cell.Length - 1]);
#if APAY
				BuyProduct(cell[cell.Length-1]);
#endif
                GUI.Label(new Rect(10, 10, 100, 200), string.Format("[Buy]{0}", cell[cell.Length - 1]));
            }
        }
    }

    private bool Btn(string msg)
    {
        GUILayout.Space(100);
        return GUILayout.Button(msg, GUILayout.Width(200), GUILayout.Height(100));
    }

    public void RequestApplePay(string produtcId)
    {
#if APAY
        LockScreen(true);
        if (!IsProductAvailable())
        {
          throw new System.Exception("IAP not enabled");
            LockScreen(false);
        }
#endif
        productInfo = new List<string>();
        Debug.Log("buy ... " + produtcId);
#if APAY
        BuyProduct(produtcId);
#endif
    }
}