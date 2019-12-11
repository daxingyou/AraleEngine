using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshEffect : MonoBehaviour
{
    static MeshEffect mThis;
    List<Vector3> vs = new List<Vector3>();
    List<int> tris = new List<int>(); 
    List<Vector2> uvs = new List<Vector2>();
    MeshFilter mf;
	// Use this for initialization
	void Start () {
        mThis = this;
        mf = gameObject.GetComponent<MeshFilter>();
	}

    void OnDestroy()
    {
        mThis = null;
    }
	
	// Update is called once per frame
	void Update () {
        if (vs.Count < 4)return;
        Mesh mesh = new Mesh();
        mesh.SetVertices(vs);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mf.mesh = mesh;

	}
            
    int innerAddQuad(Vector3 pos, float rotation, Vector2 size, int spriteIdx, bool skewed=false)
    {
        int i = vs.Count;
        vs.Add(pos+Quaternion.Euler(0,rotation,0)*new Vector3(-size.x,0.05f,size.y));
        vs.Add(pos+Quaternion.Euler(0,rotation,0)*new Vector3(size.x,0.05f,size.y));
        vs.Add(pos+Quaternion.Euler(0,rotation,0)*new Vector3(size.x,0.05f,-size.y));
        vs.Add(pos+Quaternion.Euler(0,rotation,0)*new Vector3(-size.x,0.05f,-size.y));
        tris.Add(i);
        tris.Add(i + 1);
        tris.Add(i + 2);
        tris.Add(i);
        tris.Add(i+2);
        tris.Add(i+3);
        uvs.Add(new Vector2(0f,1f));
        uvs.Add(new Vector2(1f,1f));
        uvs.Add(new Vector2(1f,0f));
        uvs.Add(new Vector2(0f,0f));
        return i;
    }

    public static int addQuad(Vector3 pos, float rotation, Vector2 size, int spriteIdx, bool skewed=false)
    {
        if (mThis == null)return -1;
        return mThis.innerAddQuad(pos, rotation, size, spriteIdx, skewed);
    }
}
