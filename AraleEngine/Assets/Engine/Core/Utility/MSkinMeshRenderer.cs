using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
    
    public class MSkinMeshRenderer : MonoBehaviour
    {
    	Transform mTrans;
    	Transform[] mBones;//顶点绑定的骨骼集合，不是所有骨骼
    	BoneWeight[] mWeights;//网格每个顶点受骨骼影响的权重
    	Matrix4x4[] mBindPos;//固定不变的，从根骨骼到当前骨骼的变换矩阵
    	MeshFilter mMeshFilter;
    	MeshRenderer mMeshRender;
    	Mesh mMesh;
    	Vector3[] mOriginV;
    	Vector3[] mOriginN;
    	Vector3[] mAnimV;
    	Vector3[] mAnimN;
    	// Use this for initialization
    	void Start () {
    		mTrans = transform;
    		SkinnedMeshRenderer skin = GetComponent<SkinnedMeshRenderer> ();
    		mMeshFilter = gameObject.AddComponent<MeshFilter> ();
    		mMesh = mMeshFilter.mesh = GameObject.Instantiate(skin.sharedMesh);
    		Material mat = skin.material;
    		mBones = skin.bones;
    		DestroyImmediate (skin);
    		mMeshRender = gameObject.AddComponent<MeshRenderer> ();
    		mMeshRender.sharedMaterial = mat;
    		mBindPos = mMesh.bindposes;
    		mWeights = mMesh.boneWeights;
    		mOriginV = mMesh.vertices;
    		mOriginN = mMesh.normals;
    		mAnimV = new Vector3[mOriginV.Length];
    		mAnimN = new Vector3[mOriginN.Length];
    	}

    	// Update is called once per frame
    	void LateUpdate () {
    		for (int i = 0, max = mAnimV.Length; i < max; ++i) 
    		{
    			BoneWeight bw = mWeights [i];
    			//顶点最多受4个顶点影响(因GPU优化渲染限制)少于4个时剩余的权值是0，其他权值和始终为1
    			//1.先将模型网格顶点通过mBindPos矩阵变换到对应的骨骼空间
    			//2.然后通过localToWorldMatrix变回世界坐标
    			//3.因网格Render不在世界坐标系，所以还要变到render所在节点的本地坐标系
    			Matrix4x4 m0 = mTrans.worldToLocalMatrix * mBones[bw.boneIndex0].localToWorldMatrix * mBindPos[bw.boneIndex0];
    			Matrix4x4 m1 = mTrans.worldToLocalMatrix * mBones[bw.boneIndex1].localToWorldMatrix * mBindPos[bw.boneIndex1];
    			Matrix4x4 m2 = mTrans.worldToLocalMatrix * mBones[bw.boneIndex2].localToWorldMatrix * mBindPos[bw.boneIndex2];
    			Matrix4x4 m3 = mTrans.worldToLocalMatrix * mBones[bw.boneIndex3].localToWorldMatrix * mBindPos[bw.boneIndex3];

    			mAnimV[i] = m0.MultiplyPoint(mOriginV[i]) * bw.weight0;
    			mAnimV[i]+= m1.MultiplyPoint(mOriginV[i]) * bw.weight1;
    			mAnimV[i]+= m2.MultiplyPoint(mOriginV[i]) * bw.weight2;
    			mAnimV[i]+= m3.MultiplyPoint(mOriginV[i]) * bw.weight3;

    			mAnimN[i] = m0.MultiplyPoint(mOriginN[i]) * bw.weight0;
    			mAnimN[i]+= m1.MultiplyPoint(mOriginN[i]) * bw.weight1;
    			mAnimN[i]+= m2.MultiplyPoint(mOriginN[i]) * bw.weight2;
    			mAnimN[i]+= m3.MultiplyPoint(mOriginN[i]) * bw.weight3;
    		}
    		mMesh.vertices = mAnimV;
    		mMesh.RecalculateNormals ();
    	}
    }

}
