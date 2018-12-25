using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{
        
    public class MeshTools{
    	//合并节点及子节点上所有mesh
    	public static void combine(Transform root)
    	{
    		MeshFilter[] mfs = root.GetComponentsInChildren<MeshFilter> ();
    		CombineInstance[] c = new CombineInstance[mfs.Length];
    		for (int i = 0; i < mfs.Length; ++i) 
    		{
    			c [i].mesh = mfs [i].sharedMesh;
    			c [i].transform = mfs [i].transform.localToWorldMatrix;
    			mfs [i].gameObject.SetActive (false);
    		}
    			
    		Mesh newMesh = new Mesh();
    		newMesh.CombineMeshes (c);
    		root.GetComponent<MeshFilter>().mesh = newMesh;
    		root.gameObject.SetActive (true);
    	}
    		
    	//最好不要将不同属性的网格合并，如带法线和不带法线的，合并后会都带法线，内存占用变大
    	public static Mesh combine(Mesh[] ms)
    	{
    		CombineInstance[] c = new CombineInstance[ms.Length];
    		for (int i = 0; i < ms.Length; ++i) 
    		{
    			c [i].mesh = ms[i];
    		}

    		Mesh newMesh = new Mesh();
    		newMesh.CombineMeshes (c,true,false);
    		return newMesh;
    	}

    	//焊接
    	//weldDistance 小于该距离的两个点将合并
    	//dstToSrcSpace 为dst所在坐标系转换到src所在坐标系的变换矩阵
    	//weldVertex   合并方式，false只将位置重叠，ture,合并为一个顶点(会有纹理坐标问题)
    	public static Mesh weld(Mesh src, Mesh dst, Matrix4x4 dstToSrcSpace, float weldDistance, bool weldVertex = false)
    	{
    		Vector3[] v1 = src.vertices;
    		Vector3[] v2 = dst.vertices;
    		if (dstToSrcSpace != null)
    		{
    			for(int i=0,max = v2.Length;i<max;++i)v2[i]=dstToSrcSpace.MultiplyPoint (v2[i]);
    		}

    		int weldCount = 0;
    		int[] mapIndx = new int[v2.Length];
    		float sqrWeldDistance = weldDistance * weldDistance;
    		for (int i = 0; i < v1.Length; ++i)
    		{
    			int j = 0;
    			float minSqrDistance = float.MaxValue;
    			int weldIndex = 0;
    			while(j<v1.Length)
    			{
    				float sqrdis = (v1 [i] - v2 [j]).sqrMagnitude;
    				if (minSqrDistance > sqrdis) 
    				{
    					weldIndex = j;
    					minSqrDistance = sqrdis;
    				}
    				++j;
    			}

    			if (minSqrDistance <= sqrWeldDistance) 
    			{
    				if (weldVertex) 
    				{
    					++weldCount;
    					//建立原顶点索引到焊接后的顶点索引的映射查询表，没映射的为0，保留的顶点
    					mapIndx[weldIndex] = i+1;
    				} 
    				else 
    				{
    					v2 [weldIndex] = v1 [i];
    				}
    			}
    		}

    		//===========
    		Vector3[] v = new Vector3[v1.Length+v2.Length-weldCount];
    		int[] tri1 = src.triangles;
    		int[] tri2 = dst.triangles;
    		int[] tri = new int[tri1.Length + tri2.Length];
    		for (int i = 0,max=v1.Length; i < max; ++i) 
    		{
    			v [i] = v1 [i];
    		}
    		for (int i = 0,j=v1.Length,max = v2.Length; i < max; ++i) 
    		{
    			if (mapIndx [i] > 0)//焊接点
    				continue;
    			v [j++] = v2 [i];
    			mapIndx [i] = j;
    		}
    		for (int i = 0,max=tri1.Length; i < max; ++i) 
    		{
    			tri [i] = tri1 [i];
    		}
    		for (int i = 0,j=tri1.Length,max=tri2.Length; i < max; ++i) 
    		{
    			tri [j++] = mapIndx[tri2[i]]-1;
    		}

    		Mesh newMesh = new Mesh();
    		newMesh.vertices = v;
    		newMesh.triangles = tri;
    		return newMesh;
    	}

    	//子节点的mesh合并焊接到父节点上
    	public static void weld(Transform root, float weldDistance, bool weldVertex = false)
    	{
    		MeshFilter[] mfs = root.GetComponentsInChildren<MeshFilter> ();
    		Mesh src = mfs [0].mesh;
    		for (int i = 1; i < mfs.Length; ++i) 
    		{
    			Mesh dst = mfs [i].mesh;
    			Matrix4x4 m = root.worldToLocalMatrix*mfs [i].transform.localToWorldMatrix;//矩阵变化为左乘
    			src = weld (src, dst, m, weldDistance, weldVertex);
    			mfs [i].gameObject.SetActive (false);
    		}
    		mfs [0].mesh = src;
    	}

    	//剔除重复顶点(优化网格)
    	public static Mesh eliminateDupVtx(Mesh mesh)
    	{
    		Vector3[] v = mesh.vertices;
    		int[]   t = mesh.triangles;
    		List<Vector3> nv = new List<Vector3> ();
    		for (int i = 0, max = t.Length; i < max; ++i) 
    		{
    			int k = nv.Count;
    			int j = 0;
    			for (;j<k;++j) 
    			{
    				if (v[t[i]].x==nv[j].x && v[t[i]].y==nv[j].y && v[t[i]].z==nv[j].z)
    					break;
    			}

    			if (j >= k) 
    			{
    				nv.Add (v[t[i]]);
    			} 
    			t [i] = j;
    		}

    		Mesh newMesh = new Mesh();
    		newMesh.vertices = nv.ToArray();
    		newMesh.triangles = t;
    		newMesh.RecalculateNormals ();
    		return newMesh;
    	}
    		
    	//右手坐标系
    	//tex为灰度图模拟高度图
    	//w mesh总宽度
    	//h mesh总高度
    	//rows y向网格数
    	//clos x向网格数
    	//factor 高度因子
    	public static Mesh createFromHMap(Texture2D tex, float w, float h, int rows, int cols, float factor)
    	{
    		Vector3[] vs = new Vector3[(rows+1)*(cols+1)];
    		for (int y = 0; y <= rows; ++y)
    		for (int x = 0; x <= cols; ++x) 
    		{
    				//Color c = tex.GetPixelBilinear (1.0f * x / rows, 1.0f * y / cols);
    				int k = y * (cols + 1) + x;
    				vs[k].x = 1.0f* x / cols * w - 0.5f*w;
    				vs[k].z = 1.0f* y / rows * h - 0.5f*h;
    				vs[k].y = 0;
    		}
    		int[] tri = new int[3 * 2 * rows * cols];
    		int vclos = cols + 1;
    		int idx = 0;
    		for (int y = 0; y < rows; ++y)
    		for (int x = 0; x < cols; ++x)
    		{
    				tri [idx++] = y * vclos + x;
    				tri [idx++] = (y+1) * vclos + x;
    				tri [idx++] = (y+1) * vclos + x + 1;
    				tri [idx++] = y * vclos + x;
    				tri [idx++] = (y+1) * vclos + x + 1;
    				tri [idx++] = y * vclos + x + 1;
    		}

    		Mesh newMesh = new Mesh();
    		newMesh.name = "noasset";
    		newMesh.vertices = vs;
    		newMesh.triangles = tri;
    		newMesh.RecalculateNormals ();
    		return newMesh;
    	}

    	//切割
    	struct SplitData
    	{
    		public int tri; //三角形索引
    		public float d1;//第一个顶点到面的距离
    		public float d2;//第二个顶点到面的距离
    		public float d3;//第三个顶点到面的距离
    	}

    	public static Mesh split(Mesh mesh, Plane splitPlane)
    	{
    		List<SplitData> ld = new List<SplitData> ();
    		Vector3[] v = mesh.vertices;
    		int[]    tri = mesh.triangles;
    		for (int i = 0, max = tri.Length; i < max; i += 3) 
    		{
    			Vector3 v1 = v [tri [i]];
    			Vector3 v2 = v [tri [i + 1]];
    			Vector3 v3 = v [tri [i + 2]];
    			float d1 = splitPlane.GetDistanceToPoint (v1);
    			float d2 = splitPlane.GetDistanceToPoint (v2);
    			float d3 = splitPlane.GetDistanceToPoint (v3);
    			if (d1 * d2 < 0 || d1 * d3 < 0 || d2 * d3 < 0) //存在新交点，该面片需要重构
    			{
    				SplitData data = new SplitData ();
    				data.tri= i ;
    				data.d1 = d1;
    				data.d2 = d2;
    				data.d3 = d3;
    				ld.Add (data);
    			}
    		}

    		//修改面拓扑结构
    		int nvStart = v.Length;
    		List<Vector3> nv = new List<Vector3> ();
    		List<int> ntri = new List<int> ();
    		for (int i = 0, max = ld.Count; i < max; ++i) 
    		{
    			//建立点的环状排列,这样后面建立三角形的顶点顺序才正确
    			int[] vidx = new int[5];
    			int k = 0;
    			int n = 0;
    			SplitData data = ld [i];
    			Vector3 v1 = v [tri [data.tri]];
    			Vector3 v2 = v [tri [data.tri + 1]];
    			Vector3 v3 = v [tri [data.tri + 2]];
    			vidx [k++] = tri [data.tri];
    			if (data.d1 * data.d2 < 0) {
    				//添加切割点时，可以使用映射表查询，排除一条边上重复生成切割点
    				nv.Add (v1 - data.d1 / (data.d1 - data.d2) * (v1 - v2));
    				vidx [n=k++] = nvStart++;
    			} 
    			vidx [k++] = tri [data.tri+1];
    			if (data.d2 * data.d3 < 0) 
    			{
    				nv.Add (v2 - data.d2 / (data.d2 - data.d3) * (v2 - v3));
    				vidx [n=k++] = nvStart++;
    			}
    			vidx [k++] = tri [data.tri+2];
    			if (data.d3 * data.d1 < 0) 
    			{
    				nv.Add (v3 - data.d3 / (data.d3 - data.d1) * (v3 - v1));
    				vidx [n=k++] = nvStart++;
    			}

    			//建立新面片
    			tri [data.tri] = vidx [n];
    			tri [data.tri+1] = vidx [(n+1) % k];
    			tri [data.tri+2] = vidx [(n+2) % k];
    			ntri.Add (vidx [n]);
    			ntri.Add (vidx [(n+2) % k]);
    			ntri.Add (vidx [(n+3) % k]);
    			if (k > 4) 
    			{
    				ntri.Add (vidx [n]);
    				ntri.Add (vidx [(n+3) % k]);
    				ntri.Add (vidx [(n+4) % k]);
    			}
    		}

    		//合并顶点和面
    		Vector3[] nnv = new Vector3[v.Length+nv.Count];
    		int[] nntri = new int[tri.Length+ntri.Count];
    		for (int i = 0,max=v.Length; i < max; ++i) 
    		{
    			nnv [i] = v [i];
    		}
    		for (int i = 0,j=v.Length,max=nv.Count; i < max; ++i) 
    		{
    			nnv [j++] = nv [i];
    		}
    		for (int i = 0,max=tri.Length; i < max; ++i) 
    		{
    			nntri [i] = tri [i];
    		}
    		for (int i = 0,j=tri.Length,max=ntri.Count; i < max; ++i) 
    		{
    			nntri [j++] = ntri[i];
    		}

    		Mesh newMesh = new Mesh();
    		newMesh.vertices = nnv;
    		newMesh.triangles = nntri;
    		newMesh.RecalculateNormals ();
    		return newMesh;
    	}

    	//细分(根号3细分算法),该算法只针对封闭的多面体有效，因为非封闭的存在边界点，边界点的特点是环绕的三角面不具有连续封闭性
    	struct VertexData
    	{
    		public Vector3 nv;
    		public List<int> lt;
    		public void addTriangle(int idx)
    		{
    			if (lt == null)lt = new List<int> ();
    			lt.Add (idx);
    		}

    		//逆时针排序周围顶点,排列环面,并计算V点位置
    		//idx是该VertexData在vd数组中的索引位置
    		public void sortTriangleAndComputeV(int idx, Vector3[] v, int[] t)
    		{
    			//vn为环绕点的和
    			int n = lt.Count;
    			Vector3 vn = Vector3.zero;
    			for (int i = 0; i < n; ++i) 
    			{
    				//保证每个三角形的第一个点都是idx顶点
    				if (t [lt [i] + 1] == idx) {
    					t [lt [i] + 1] = t[lt [i] + 2];
    					t [lt [i] + 2] = t[lt [i]];
    					t [lt [i]] = idx;
    				} else if (t [lt [i] + 2] == idx) {
    					t [lt [i] + 2] = t[lt [i] + 1];
    					t [lt [i] + 1] = t[lt [i]];
    					t [lt [i]] = idx;
    				}
    				vn+=v[t[lt[i]+1]];
    			}
    			//根据相邻顶点排序三角形,对于非封闭多面体该步逻辑是错误的，三角形面不一定是连续的
    			for (int j =0; j <n-1; ++j)
    			{
    				int lastIdx = t[lt[j]+2];
    				for (int i=j+1; i < n; ++i) 
    				{
    					if (t[lt[i]+1] != lastIdx)
    						continue;
    					int tmp = lt[i];
    					lt[i] = lt[j+1];
    					lt[j+1] = tmp;
    					break;
    				}
    			}
    			//计算V点位置
    			float a = (4-2*Mathf.Cos(Mathf.PI*2/n))/9;
    			nv = (1 - a) * v [idx] + a / n * vn;
    		}
    	}
    		
    	public static Mesh subdivision(Mesh mesh)
    	{
    		//unity的cube顶点数为24,这是为了让边缘颜色完全不同，每个顶点的法相量都是按照面来计算的即每个顶点法向量要计算3次
    		//3x8，这导致它的空间拓扑关系被破坏，我们需要做适当的相同顶点的合并处理，这样才能做后面的测试
    		mesh = eliminateDupVtx(mesh);
    		Vector3[] v = mesh.vertices;
    		int[]     t = mesh.triangles;
    		//找到每个顶点相邻的三角形,vd保存是该索引顶点周围的三角形信息
    		VertexData[] vd = new VertexData[v.Length];
    		Vector3[] nv = new Vector3[v.Length+t.Length/3];//新增加的点是F点，V点是根据原顶点计算周围点得到的新位置，数目没有增加
    		int offsetF = v.Length;//F点在nv数组中的偏移
    		for (int i = 0, max = t.Length; i < max; i+=3) 
    		{
    			//计算F点位置即Face的中心点,索引位置与面索引一致
    			nv[offsetF+i/3] = (v[t[i]]+v[t[i+1]]+v[t[i+2]])/3;
    			vd [t[i]].addTriangle (i);
    			vd [t[i+1]].addTriangle (i);
    			vd [t[i+2]].addTriangle (i);
    		}

    		//重建拓扑结构
    		int[] nt = new int[3*t.Length];//每个三角形都会分裂出三个新的
    		int nti = 0;
    		for (int i = 0, max = vd.Length; i < max; ++i) 
    		{
    			VertexData d = vd [i];
    			d.sortTriangleAndComputeV (i,v,t);
    			nv [i] = d.nv;
    			int count = d.lt.Count;
    			for(int j=0;j<count;++j)
    			{
    				nt [nti++] = i;
    				nt [nti++] = offsetF+d.lt[j%count]/3;
    				nt [nti++] = offsetF+d.lt[(j+1)%count]/3;
    			}
    		}

    		Mesh newMesh = new Mesh();
    		newMesh.vertices = nv;
    		newMesh.triangles = nt;
    		newMesh.RecalculateNormals ();
    		return newMesh;
    	}

    	//简化
    }

}