using UnityEngine;
using System.Collections;
using Arale.Engine;

public class IndicatorMesh : MonoBehaviour
{
    private Mesh m_mesh;
    Material  circleMat;
    Material  rectMat;
    RectMesh  rangMesh;
    RectMesh  rectMesh;
    FanMesh   fanMesh;
    Transform sticker;
    Matrix4x4 mLocalMX = Matrix4x4.identity;
    Vector3 dir = Vector3.zero;
    [ExecuteInEditMode]
    private void Awake()
    {
        sticker = new GameObject("indicator").transform;
        sticker.SetParent(transform, false);
        rectMesh = new RectMesh();
        rangMesh = new RectMesh();
        fanMesh =  new FanMesh();
    }

    void Start()
    {
        circleMat= ResLoad.get("Mat/Indicator1", ResideType.InScene).asset<Material>();
        rectMat = ResLoad.get("Mat/Indicator2", ResideType.InScene).asset<Material>();
    }

    void Update()
    {
        if (!sticker.gameObject.activeSelf)return;
        sticker.forward = dir;
        //rangMesh.draw(sticker.localToWorldMatrix, circleMat);
        //rectMesh.draw(sticker.localToWorldMatrix*rectMesh.pivotMX, rectMat);
        fanMesh.draw(sticker.localToWorldMatrix, circleMat);
    }

    void OnDestroy()
    {
        GameObject.Destroy(sticker.gameObject);
    }

    public void Show(Vector2 dir, float dis, float disPercent, bool show)
    {
        sticker.gameObject.SetActive(show);
        if (!show)return;
        this.dir.x = dir.x;
        this.dir.z = dir.y;
        rangMesh.get(dis*2, dis*2);
        fanMesh.get(dis, 60);
        rectMesh.get(1, dis, new Vector2(0f, -0.5f));
        mLocalMX.SetTRS(Vector3.forward*(dis*disPercent), Quaternion.identity, Vector3.one);
    }

    class StickerMesh
    {
        protected Mesh mMesh;
        public void draw(Matrix4x4 mx, Material mat)
        {
            if(mMesh!=null)Graphics.DrawMesh(mMesh, mx, mat, 0);
        }
    }

    class FanMesh : StickerMesh
    {
        float r;
        float ang;
        public Mesh get(float r, float ang)
        {
            if (this.r == r && this.ang == ang)return mMesh;
            mMesh = new Mesh();
            int segs = 32;//偶数,保证第一个点在0度
            Vector3[] vertices = new Vector3[3 + segs - 1];
            vertices[0] = new Vector3(0, 0, 0);
            float angle = Mathf.Deg2Rad * ang;
            float beginAngle = (-angle+Mathf.PI)/2;//起始度数为90度
            //生成顶点数据
            for (int i = 1; i < segs; i++)
            {
                float curAng = beginAngle + angle * i / segs;
                vertices[i] = new Vector3(Mathf.Cos(curAng) * r, 0.1f, Mathf.Sin(curAng) * r);
            }
            mMesh.vertices = vertices;
            //生成三角形数据
            int[] triangles = new int[segs * 3];//有segments个三角形，每3个数据构成一个三角形
            for (int i = 0, vi = 1; i < triangles.Length; i += 3, vi++)
            {
                triangles[i] = 0;
                triangles[i + 1] = vi;
                triangles[i + 2] = vi + 1;
            }
            mMesh.triangles = triangles;
            //纹理坐标
            Vector2[] uvs = new Vector2[vertices.Length];
            float len = 2 * r;
            for (int i = 0; i < uvs.Length; i++)
            {//归一化+坐标系反转与uv坐标系一致
                uvs[i] = new Vector2(vertices[i].x/len+0.5f, -vertices[i].z/len+0.5f);
            }
            mMesh.uv = uvs;
            return mMesh;
        }
    }

    class RectMesh : StickerMesh
    {
        float w;
        float h;
        public Matrix4x4 pivotMX = Matrix4x4.identity;
        public Mesh get(float w,float h,Vector2 pivot=default(Vector2))
        {
            pivotMX.SetTRS(new Vector3(pivot.x * w,0.1f,-pivot.y * h), Quaternion.identity, Vector3.one);
            if (this.w == w && this.h == h)return mMesh;
            mMesh = new Mesh();
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(-w/2, 0, h/2);
            vertices[1] = new Vector3(w/2, 0, h/2);
            vertices[2] = new Vector3(w/2, 0, -h/2);
            vertices[3] = new Vector3(-w/2, 0, -h/2);
            mMesh.vertices = vertices;
            Vector2[] uvs = new Vector2[vertices.Length];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);
            mMesh.uv = uvs;
            mMesh.triangles = new int[]{2,3,0,2,0,1};
            this.w = w;
            this.h = h;
            return mMesh;
        }
    }
}