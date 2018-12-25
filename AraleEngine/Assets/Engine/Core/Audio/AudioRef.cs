using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

public class AudioRef : MonoBehaviour
{
    ResLoad mResLoad;
    public ResLoad resLoad{set{mResLoad=value;}}
	void OnDestroy()
	{
        if (mResLoad != null)mResLoad.release();
	}
}

}
