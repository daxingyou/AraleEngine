using UnityEngine;
using System.Collections;
using Arale.Engine;

public class NetMgr : MgrBase<NetMgr>
{
    public static LanClient client;
    public static LanHost   server;

    public void createLanClient()
    {
        GameObject go = new GameObject("LanClient");
        client = go.AddComponent<LanClient>();
        Log.i("createLanClient", Log.Tag.Net);
    }

    public void destroyLanClient()
    {
        if (client != null)
        {
            GameObject.Destroy(client.gameObject);
            client = null;
        }
        Log.i("destroyLanClient", Log.Tag.Net);
    }

    public void createLanHost(string hostName="")
    {
        GameObject go = new GameObject("LanHost");
        server = go.AddComponent<LanHost>();
        server.gameName = hostName;
        server.startHost();
        Log.i("createLanHost", Log.Tag.Net);
    }

    public void destroyLanHost()
    {
        if (server != null)
        {
            GameObject.Destroy(server.gameObject);
            server = null;
        }
        Log.i("destroyLanHost", Log.Tag.Net);
    }

    public override void Deinit()
    {
        destroyLanClient();
        destroyLanHost();
    }
}
