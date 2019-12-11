using UnityEngine;
using System.Collections;
using Arale.Engine;

public class IndicatorMesh : MonoBehaviour
{
    private Mesh m_mesh;
    Material  rangeMat;
    Material  rectMat;
    RectMesh  rangMesh;
    RectMesh  rectMesh;
    Transform sticker;
    Matrix4x4 mLocalMX = Matrix4x4.identity;
    Vector3 dir = Vector3.zero;
    GameSkill gs;
    [ExecuteInEditMode]
    private void Awake()
    {
        sticker = new GameObject("indicator").transform;
        sticker.SetParent(transform, false);
        rectMesh = new RectMesh();
        rangMesh = new RectMesh();
    }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  
    void Start()
    {
        rangeMat = Object.Instantiate(ResLoad.get("Mat/Indicator", ResideType.InScene).asset<Material>());
        rectMat = Object.Instantiate(ResLoad.get("Mat/Indicator", ResideType.InScene).asset<Material>());
    }

    void LateUpdate()
    {
        if (!sticker.gameObject.activeSelf)return;
        sticker.forward = dir;
        switch (gs.pointType)
        {
            case Skill.PointType.Pos:
                drawRang();
                if (gs.area == null)
                {
                    rectMat.SetFloat("_HalfWidth", 1);
                    rectMat.SetFloat("_Ang", 360f);
                    rectMesh.get(2, 2,new Vector3(0,0.02f,0));
                    rectMesh.draw(sticker.localToWorldMatrix*mLocalMX, rectMat);
                }
                else
                {
                    drawArea();
                }

                break;
            case Skill.PointType.Dir:
                if (gs.area == null)
                {
                    rectMat.SetFloat("_HalfWidth", gs.distance/2);
                    rectMat.SetFloat("_HalfHeight", 0);
                    rectMat.SetFloat("_Ang", 0);
                    rectMesh.get(1, gs.distance, new Vector3(0,0.02f,gs.distance/2));
                    rectMesh.draw(sticker.localToWorldMatrix*mLocalMX, rectMat);
                }
                else
                {
                    drawArea();
                }
                break;
            case Skill.PointType.Target:
            case Skill.PointType.None:
                drawRang();
                break;
        }
    }

    void drawRang()
    {
        if (gs.distance <= 0)return;
        rangMesh.get(gs.distance*2, gs.distance*2, new Vector3(0,0.01f,0));
        rangeMat.SetFloat("_HalfWidth", gs.distance);
        rangeMat.SetFloat("_Ang", 360f);
        rangMesh.draw(sticker.localToWorldMatrix, rangeMat);
    }

    void drawArea()
    {
        IArea area = gs.area;
        switch (area.type)
        {
            case GameArea.AreaType.circle:
                CircleArea cr = area as CircleArea;
                rectMat.SetFloat("_HalfWidth", cr.R);
                rectMat.SetFloat("_Ang", 360f);
                rectMesh.get(2*area.R, 2*area.R, new Vector3(0,0.02f,0));
                rectMesh.draw(sticker.localToWorldMatrix * mLocalMX, rectMat);
                break;
            case GameArea.AreaType.Fan:
                FanArea fr = area as FanArea;
                rectMat.SetFloat("_HalfWidth", fr.R);
                rectMat.SetFloat("_Ang", fr.ang);
                rectMesh.get(2*area.R, 2*area.R, new Vector3(0,0.02f,0));
                rectMesh.draw(sticker.localToWorldMatrix * mLocalMX, rectMat);
                break;
            case GameArea.AreaType.Rectangle:
                RectangleArea rar = area as RectangleArea;
                rectMat.SetFloat("_HalfWidth", rar.w/2);
                rectMat.SetFloat("_HalfHeight", rar.R/2);
                rectMat.SetFloat("_Ang", 0);
                rectMesh.get(area.R, area.R, new Vector3(0,0.02f,rar.R/2));
                rectMesh.draw(sticker.localToWorldMatrix * mLocalMX, rectMat);
                break;
            case GameArea.AreaType.Square:
                SquareArea sr = area as SquareArea;
                rectMat.SetFloat("_HalfWidth", sr.R/2);
                rectMat.SetFloat("_HalfHeight", sr.R/2);
                rectMat.SetFloat("_Ang", 0);
                rectMesh.get(area.R, area.R, new Vector3(0,0.02f,sr.R/2));
                rectMesh.draw(sticker.localToWorldMatrix * mLocalMX, rectMat);
                break;
        }
    }

    void OnDestroy()
    {
        GameObject.Destroy(sticker.gameObject);
    }

    public void Show(GameSkill gs, Vector2 dir, float disPercent, bool show)
    {
        sticker.gameObject.SetActive(show);
        if (!show)return;
        this.gs = gs;
        this.dir.x = dir.x;
        this.dir.z = dir.y;
        if (gs.pointType == Skill.PointType.Pos)
        {
            mLocalMX.SetTRS(Vector3.forward * (gs.distance * disPercent), Quaternion.identity, Vector3.one);
        }
        else
        {
            mLocalMX = Matrix4x4.identity;
        }
    }

    class StickerMesh
    {
        protected Mesh mMesh;
        protected Matrix4x4 pivotMX = Matrix4x4.identity;
        public void draw(Matrix4x4 mx, Material mat)
        {
            if(mMesh!=null)Graphics.DrawMesh(mMesh, mx*pivotMX, mat, 0);
        }
    }

    class FanMesh : StickerMesh
    {
        float r;
        float ang;
        public Mesh get(float r, float ang, Vector3 pivot=default(Vector3))
        {
            pivotMX.SetTRS(pivot, Quaternion.identity, Vector3.one);
            if (this.r == r && this.ang == ang)return mMesh;
            int segs = 32;
            //生成顶点数据
            Vector3[] vertices = new Vector3[segs + 2+8];
            vertices[0] = new Vector3(0, 0, 0);
            float angle = Mathf.Deg2Rad * ang;
            float beginAngle = (-angle+Mathf.PI)/2;//起始度数为90度
            for (int i = 0; i < segs+1; i++)
            {
                float curAng = beginAngle + angle * i / segs;
                vertices[i+1] = new Vector3(Mathf.Cos(curAng) * r, 0f, Mathf.Sin(curAng) * r);
            }
            //生成三角形数据
            int[] triangles = new int[segs * 3+12];//有segments个三角形，每3个数据构成一个三角形
            for (int i = 0, vi = 1; i < triangles.Length; i += 3, vi++)
            {
                triangles[i] = 0;
                triangles[i + 1] = vi;
                triangles[i + 2] = vi + 1;
            }
            //纹理坐标
            Vector2[] uvs = new Vector2[vertices.Length];
            float len = 2 * r;
            for (int i = 0; i < uvs.Length; i++)
            {//归一化+坐标系反转与uv坐标系一致
                uvs[i] = new Vector2(vertices[i].x/len+0.5f, -vertices[i].z/len+0.5f);
            }
            //添加左边界
            Vector3 l0 = vertices[0];
            Vector3 l1 = vertices[vertices.Length-10];
            Vector3 l2 = vertices[vertices.Length-9];
            Vector3 nl = l0 + (l1 - l2);
            int n = vertices.Length - 8;
            vertices[n]   = l2;
            vertices[n+1] = l1;
            vertices[n+2] = nl;
            vertices[n+3] = l0;
            int tn = triangles.Length - 12;
            triangles[tn]   = n;
            triangles[tn+1] = n+1;
            triangles[tn+2] = n+2;
            triangles[tn+3] = n;
            triangles[tn+4] = n+2;
            triangles[tn+5] = n+3;
            int un = uvs.Length - 8;
            uvs[un]   = new Vector2(0.5f, 0f);
            uvs[un+1] = new Vector2(0.5f, 0f);
            uvs[un+2] = new Vector2(0.5f, 1f);
            uvs[un+3] = new Vector2(0.5f, 1f);
            //添加右边界
            Vector3 r0 = vertices[0];
            Vector3 r1 = vertices[1];
            Vector3 r2 = vertices[2];
            Vector3 nr = r0 + (r2 - r1);
            n = vertices.Length - 4;
            vertices[n]   = r2;
            vertices[n+1] = r1;
            vertices[n+2] = r0;
            vertices[n+3] = nr;
            tn = triangles.Length - 6;
            triangles[tn]   = n;
            triangles[tn+1] = n+1;
            triangles[tn+2] = n+3;
            triangles[tn+3] = n;
            triangles[tn+4] = n+2;
            triangles[tn+5] = n+3;
            un = uvs.Length - 4;
            uvs[un]   = new Vector2(0.5f, 0f);
            uvs[un+1] = new Vector2(0.5f, 0f);
            uvs[un+2] = new Vector2(0.5f, 1f);
            uvs[un+3] = new Vector2(0.5f, 1f);
            mMesh = new Mesh();
            mMesh.vertices = vertices;
            mMesh.triangles = triangles;
            mMesh.uv = uvs;
            return mMesh;
        }
    }

    class RectMesh : StickerMesh
    {
        float w;
        float h;
        public Mesh get(float w,float h, Vector3 pivot=default(Vector3))
        {
            pivotMX.SetTRS(pivot, Quaternion.identity, Vector3.one);
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