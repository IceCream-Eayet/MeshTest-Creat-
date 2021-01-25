using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * mesh依托于MeshFilter和MeshRenderer，
 * mesh的属性：
 * vertices：网格的顶点数组；
 * Normals：网格的法线数组，与光照方向有关；
 * tangents：网格的切线数组；
 * uv：网格的基础纹理坐标；
 * triangles：包含所有三角形顶点索引的数组，索引是顺序是三角形顶点的排列顺序；
 * uv2：网格的第二个纹理坐标；
 * tangents：切线空间的切线四维坐标，第四位标识OpenGL和directX的区别
 */
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateMesh_2 : MonoBehaviour
{
    private Mesh mesh;

    public Vector4 targent;

    void Start()
    {
        mesh = new Mesh
        {
            name = "Mesh01"
        };
        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;                                 //赋值mesh的引用，更改会发生变化
        mesh.vertices = GetVertexs();
        mesh.triangles = GetTriangles();
        mesh.uv = GetUVs();
        mesh.normals = GetNormals();
    }

    void Update()
    {
        mesh.tangents = GetTargents();
    }
    private Vector4[] GetTargents()
    {
        return new Vector4[]
        {
            targent,
            targent,
            targent,
            targent
        };
    }
    /// <summary>
    /// 获取每个顶点的坐标
    /// </summary>
    private Vector3[] GetVertexs()
    {
        return new Vector3[]
        {
             new Vector3(0,0,0),
             new Vector3(1,0,0),
             new Vector3(1,1,0),
             new Vector3(0,1,0)
        };
    }
    /// <summary>
    /// 获取材质与顶点对应的UV坐标
    /// </summary>
    private Vector2[] GetUVs()
    {
        return new Vector2[]
        {
            new Vector2(1,0),
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1)
        };
    }
    /// <summary>
    /// 设定受光照影响的的方向，面片受光照影响的程度与数组中的方向上的面一致（法向量数组）
    /// </summary>
    private Vector3[] GetNormals()
    {
        return new Vector3[]
        {
            Vector3.right,
            Vector3.right,
            Vector3.right,
            Vector3.right
        };
    }

    /// <summary>
    /// 获取顶点的排序序列（顺时针排序的三角形在正面显示，逆时针排序的三角形在反面显示）
    /// </summary>
    private int[] GetTriangles()
    {
        return new int[]
        {
            0,1,2,
            0,2,3
        };
    }
}
