using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Sprites;

/*
 * mesh依托于MeshFilter和MeshRenderer，
 * mesh的属性：
 * vertices：网格的顶点数组；
 * Normals：网格的法线数组，与光照方向有关；
 * tangents：网格的切线数组,最常用于凹凸贴图着色器中；
 * uv：网格的基础纹理坐标；
 * triangles：包含所有三角形顶点索引的数组，索引是顺序是三角形顶点的排列顺序；
 * uv2：网格的第二个纹理坐标；
 * tangents：切线空间的切线四维坐标，第四位标识OpenGL和directX的区别
 */
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateMesh_3 : MonoBehaviour
{
    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector3[] _upVertices, _downVertices, _forwardVertices, _backVertices, _leftVertices, _rightVertices;
    private Vector4[] tangents;

    public int xGirount, yGirount, zGirount;
    public float lineLenght;

    void Start()
    {
        _mesh = new Mesh { name = "Mesh03" };
        MeshFilter filter = GetComponent<MeshFilter>();
        _vertices = new Vector3[(xGirount + 1) * (yGirount + 1) * 2 + (xGirount + 1) * (zGirount + 1) * 2 + (yGirount + 1) * (zGirount + 1) * 2];
        _upVertices = new Vector3[(xGirount + 1) * (zGirount + 1)];
        _downVertices = new Vector3[(xGirount + 1) * (zGirount + 1)];
        _forwardVertices = new Vector3[(yGirount + 1) * (zGirount + 1)];
        _backVertices = new Vector3[(yGirount + 1) * (zGirount + 1)];
        _leftVertices = new Vector3[(xGirount + 1) * (yGirount + 1)];
        _rightVertices = new Vector3[(xGirount + 1) * (yGirount + 1)];

        //赋值mesh的引用，更改会发生变化
        filter.mesh = _mesh;
        _mesh.vertices = GetVertex();
        _mesh.triangles = GetTriangles();
        _mesh.uv = SetUVs();
        _mesh.normals = SetNormals();
        _mesh.tangents = tangents;
    }
    /// <summary>
    /// 获取物体的全部顶点本地坐标
    /// </summary>
    private Vector3[] GetVertex()
    {
        int i = 0;
        _upVertices = GetUpDwnVertex(ref i,  true);
        _leftVertices = GetLeftRightVertex(ref i, true);
        _rightVertices = GetLeftRightVertex(ref i, false);
        _forwardVertices = GetForwardBackVertex(ref i, true);
        _backVertices = GetForwardBackVertex(ref i, false);
        _downVertices = GetUpDwnVertex(ref i, false);

        return _vertices;
    }
    /// <summary>
    /// 获取三角形顶点索引顺序的数组
    /// </summary>
    private int[] GetTriangles()
    {
        int tranglesIndex = 0;
        int verticesIndex = 0;
        int[] trangles = new int[_vertices.Length * 6];

        //上面
        GetForwardDirectionTrangles(ref tranglesIndex, ref trangles, ref verticesIndex, xGirount, zGirount);

        //左面
        GetForwardDirectionTrangles(ref tranglesIndex, ref trangles, ref verticesIndex, xGirount, yGirount);

        //右面
        GetBackDirectionTrangles(ref tranglesIndex, ref trangles, ref verticesIndex, xGirount, yGirount);

        //前面
        GetForwardDirectionTrangles(ref tranglesIndex, ref trangles, ref verticesIndex, zGirount, yGirount);

        //后面
        GetBackDirectionTrangles(ref tranglesIndex, ref trangles, ref verticesIndex, zGirount, yGirount);

        //底面
        GetBackDirectionTrangles(ref tranglesIndex, ref trangles, ref verticesIndex, xGirount, zGirount);

        return trangles;
    }
    /// <summary>
    /// 正向方向的顶点索引顺序
    /// </summary>
    private void GetForwardDirectionTrangles(ref int index, ref int[] trangles, ref int verticesIndex, int row, int column)
    {
        for (int z = 0; z < column; z++)
        {
            for (int i = 0; i < row; verticesIndex++, i++)
            {
                trangles[index] = verticesIndex;
                trangles[index + 1] = verticesIndex + row + 1;
                trangles[index + 2] = verticesIndex + 1;

                trangles[index + 3] = verticesIndex + 1;
                trangles[index + 4] = verticesIndex + row + 1;
                trangles[index + 5] = verticesIndex + row + 2;

                index += 6;
            }
            verticesIndex += 1;
        }

        verticesIndex += row + 1;
    }
    /// <summary>
    /// 倒向方向的顶点索引顺序
    /// </summary>
    private void GetBackDirectionTrangles(ref int index, ref int[] trangles, ref int verticesIndex, int row, int column)
    {
        for (int z = 0; z < column; z++)
        {
            for (int i = 0; i < row; verticesIndex++, i++)
            {
                trangles[index] = verticesIndex;
                trangles[index + 1] = verticesIndex + 1;
                trangles[index + 2] = verticesIndex + row + 1;

                trangles[index + 3] = verticesIndex + 1;
                trangles[index + 4] = verticesIndex + row + 2;
                trangles[index + 5] = verticesIndex + row + 1;

                index += 6;
            }
            verticesIndex += 1;
        }

        verticesIndex += row + 1;
    }
    /// <summary>
    /// 上下两个面的顶点
    /// </summary>
    private Vector3[] GetUpDwnVertex(ref int i, bool isUp)
    {
        Vector3[] vectors = new Vector3[(xGirount + 1) * (zGirount + 1)];
        float y = isUp ? yGirount/(lineLenght * 2) : _rightVertices[_rightVertices.Length - 1].y;

        for (int z = 0, index = 0; z < zGirount + 1; z++)
        {
            for (int x = 0; x < xGirount + 1; x++, i++, index++)
            { 
                Vector3 temp = new Vector3(-xGirount / (lineLenght * 2) + (1 / lineLenght) * (x % (xGirount + 1)), y, -zGirount / (lineLenght * 2) + (1 / lineLenght) * (z % (zGirount + 1)));
                _vertices[i] = GetIsContainsVertex(temp, vectors, i, index);
            }
        }
        return vectors;
    }
    /// <summary>
    /// 前后两个面的顶点
    /// </summary>
    private Vector3[] GetForwardBackVertex(ref int i, bool isForward)
    {
        Vector3[] vectors = new Vector3[(yGirount + 1) * (zGirount + 1)];
        float x = isForward ? _rightVertices[_rightVertices.Length - 1].x : _upVertices[_upVertices.Length - 1].x;

        for (int y = 0, index = 0; y < yGirount + 1; y++)
        {
            for (int z = 0; z < zGirount + 1; z++, i++, index++)
            {
                Vector3 temp = new Vector3(x, _rightVertices[_rightVertices.Length - 1].y + (1 / lineLenght) * (y % (yGirount + 1)), _rightVertices[_rightVertices.Length - 1].z - (1 / lineLenght) * (z % (zGirount + 1)));
                _vertices[i] = GetIsContainsVertex(temp, vectors, i, index);
            }
        }
        return vectors;
    }
    /// <summary>
    /// 左右两个面的顶点
    /// </summary>
    private Vector3[] GetLeftRightVertex(ref int i, bool isLeft)
    {
        Vector3[] vectors = new Vector3[(xGirount + 1) * (yGirount + 1)];
        float z = isLeft ? -zGirount / (lineLenght * 2) : _upVertices[_upVertices.Length - 1].z;

        for(int y = 0, index = 0; y < yGirount + 1; y++)
        {
            for(int x = 0; x < xGirount + 1; x++, index++, i++)
            {
                Vector3 temp = new Vector3(_upVertices[_upVertices.Length - 1].x - (1 / lineLenght) * (x % (xGirount + 1)), _upVertices[_upVertices.Length - 1].y - (1 / lineLenght) * (y % (yGirount + 1)), z);
                _vertices[i] = GetIsContainsVertex(temp, vectors, i, index);
            }
        }

        return vectors;
    }

    /// <summary>
    /// 判断数组是否已包含元素，包含则返回已存在的，不包含返回新值
    /// </summary>
    private Vector3 GetIsContainsVertex(Vector3 temp, Vector3[] vectors, int i, int index)
    {
        if (!_vertices.Contains(temp))
        {
            vectors[index] = temp;
            _vertices[i] = temp;
        }
        else
        {
            ArrayList array = new ArrayList(_vertices);
            int arrayIndex = array.IndexOf(temp);

            vectors[index] = _vertices[arrayIndex];
        }
        return vectors[index];
    }
    /// <summary>
    /// 设置UV坐标，返回UV数组
    /// </summary>
    private Vector2[] SetUVs()
    {
        int i = 0;
        Vector2[] uvs = new Vector2[_vertices.Length];

        Debug.Log(_vertices.Length);

        for(int upz = 0; upz <= zGirount; upz++)
        {
            for (int upx = 0; upx <= xGirount; upx++, i++) 
            {
                uvs[i] = new Vector2(upx / (float)xGirount, upz / (float)zGirount);
            }
        }

        for (int leftx = 0; leftx <= xGirount; leftx++)
        {
            for (int lefty = 0; lefty <= yGirount; lefty++, i++)
            {
                uvs[i] = new Vector2(leftx / (float)xGirount, lefty / (float)yGirount);
            }
        }

        for (int leftx = 0; leftx <= xGirount; leftx++)
        {
            for (int lefty = 0; lefty <= yGirount; lefty++, i++)
            {
                uvs[i] = new Vector2( lefty / (float)yGirount, leftx / (float)xGirount);
            }
        }

        for (int forwardz = 0; forwardz <= zGirount; forwardz++)
        {
            for (int forwardy = 0; forwardy <= yGirount; forwardy++, i++)
            {
                uvs[i] = new Vector2(forwardy / (float)yGirount, forwardz / (float)zGirount);
            }
        }

        for (int backz = 0; backz <= zGirount; backz++)
        {
            for (int backy = 0; backy <= yGirount; backy++, i++)
            {
                uvs[i] = new Vector2( backz / (float)zGirount, backy / (float)yGirount);
            }
        }

        for (int downz = 0; downz <= zGirount; downz++)
        {
            for (int downx = 0; downx <= xGirount; downx++, i++)
            {
                uvs[i] = new Vector2(downz / (float)zGirount, downx / (float)xGirount);
            }
        }

        return uvs;
    }
    /// <summary>
    /// 设置法线向量，返回Normals数组
    /// </summary>
    private Vector3[] SetNormals()
    {
        Vector3[] normals = new Vector3[_vertices.Length];
        tangents = new Vector4[_vertices.Length];

        for(int i = 0; i < _upVertices.Length; i++)
        {
            normals[i] = Vector3.up;
            tangents[i] = new Vector4(1, 0, 0, -1);
        }

        for (int i = 0; i < _leftVertices.Length; i++)
        {
            normals[i] = Vector3.left;
            tangents[i] = new Vector4(0, 1, 0, -1);
        }

        for (int i = 0; i < _rightVertices.Length; i++)
        {
            normals[i] = Vector3.right;
            tangents[i] = new Vector4(0, -1, 0, -1);
        }

        for (int i = 0; i < _forwardVertices.Length; i++)
        {
            normals[i] = Vector3.forward;
            tangents[i] = new Vector4(0, 1, 0, -1);
        }

        for (int i = 0; i < _backVertices.Length; i++)
        {
            normals[i] = Vector3.back;
            tangents[i] = new Vector4(0, 1, 0, -1);
        }

        for (int i = 0; i < _downVertices.Length; i++)
        {
            normals[i] = Vector3.down;
            tangents[i] = new Vector4(-1, 0, 0, -1);
        }

        return normals;
    }

    /// <summary>
    /// 编辑器视图下渲染出辅助图示
    /// </summary>
    private void OnDrawGizmos()
    {
        if (_vertices != null)
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                Gizmos.DrawSphere(transform.TransformPoint(_vertices[i]), 0.02f);
            }
        }
    }
}
