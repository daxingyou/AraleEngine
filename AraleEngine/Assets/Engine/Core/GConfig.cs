using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
    
    public class GConfig : MonoBehaviour
    {
    	public string mGameServer="127.0.0.1:80";
    	public string mResServer="http://127.0.0.1:8080/update/";
        public void Start()
        {
            Application.targetFrameRate = 60;
        }
    }

}
