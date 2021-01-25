using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * 1，mesh赋值：对一个已经存在的对象频繁修改mesh，需要先将执行mesh.clear(),否则会报错
 * 疑似在修改mesh的时候，并不会修改原模型mesh的数组大小
 * */
public class GenerateMesh_4 : MonoBehaviour
{
    /// <summary>
    /// 鼠标滑动的起始点和终点
    /// </summary>
    private Vector3 _startPos, _endPos;
    /// <summary>
    /// 射线碰撞到物体的撞击点
    /// </summary>
    private Vector3 _point;
    /// <summary>
    /// 形成切面的向量与它的法线向量
    /// </summary>
    private Vector3 _dir, _upDir, _planeNormal;
    /// <summary>
    /// 射线碰撞对象的Transform
    /// </summary>
    private Transform _hitTrans;
    /// <summary>
    /// 射线碰撞对象的mesh
    /// </summary>
    private Mesh _mesh;
    /// <summary>
    /// 射线碰撞对象的MeshRenderer
    /// </summary>
    private MeshRenderer _renderer;
    /// <summary>
    /// 生成的新的左侧对象mesh的Vectices的缓存
    /// </summary>
    private List<Vector3> _leftVectices = new List<Vector3>();
    /// <summary>
    /// 生成的新的左侧对象mesh的Trangles的缓存
    /// </summary>
    private List<int> _leftTrangles = new List<int>();
    /// <summary>
    /// 生成的新的左侧对象mesh的Normals的缓存
    /// </summary>
    private List<Vector3> _leftNormals = new List<Vector3>();
    /// <summary>
    /// 生成的新的右侧对象mesh的Vectices的缓存
    /// </summary>
    private List<Vector3> _rightVectices = new List<Vector3>();
    /// <summary>
    /// 生成的新的右侧对象mesh的Trangles的缓存
    /// </summary>
    private List<int> _rightTrangles = new List<int>();
    /// <summary>
    /// 生成的新的右侧对象mesh的Normals的缓存
    /// </summary>
    private List<Vector3> _rightNormals = new List<Vector3>();
    /// <summary>
    /// 切割产生的新的顶点坐标的缓存
    /// </summary>
    private List<Vector3> _newVectices = new List<Vector3>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _startPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _endPos = Input.mousePosition;
            RayAndCut();
        }
    }
    /// <summary>
    /// 使用射线计算形成划线切面的向量
    /// </summary>
    private void RayAndCut()
    {
        var center = (_endPos + _startPos) * 0.5f;
        var ray = Camera.main.ScreenPointToRay(center);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f)) 
        {
            _point = hit.point;
            _hitTrans = hit.transform;
            _mesh = hit.transform.GetComponent<MeshFilter>().mesh;
            _renderer = hit.transform.GetComponent<MeshRenderer>();

            Vector3 startPos = Camera.main.ScreenToWorldPoint(new Vector3(_startPos.x, _startPos.y, 10));
            Vector3 endPos = Camera.main.ScreenToWorldPoint(new Vector3(_endPos.x, _endPos.y, 10));

            _dir = (hit.point - Camera.main.transform.position).normalized;
            _upDir = (endPos - startPos).normalized;
            _upDir = (-_dir * Vector3.Dot(_upDir, _dir) + _upDir).normalized;
            _planeNormal = Vector3.Cross(_dir, _upDir);

            ClearData();
            GeneratCutedObject();
        }
    }
    /// <summary>
    /// 生成切割之后的object
    /// </summary>
    private void GeneratCutedObject()
    {
        Vector3[] vetices = _mesh.vertices;
        int[] trangles = _mesh.triangles;

        //1，计算出被切割的切面两边的顶点信息
        Vector3[] vetexTemp = new Vector3[3];
        float[] result = new float[3];
        for (int i = 0; i < trangles.Length; i += 3)
        {
            for (int j = 0; j < result.Length; j++)
            {
                vetexTemp[j] = _hitTrans.TransformPoint(vetices[trangles[i + j]]);
                result[j] = Vector3.Dot(_planeNormal, _point - vetexTemp[j]);
            }

            if (result[0] <= 0 && result[1] <= 0 && result[2] <= 0)
            {
                //全部在左侧
                SaveVetexInfo(_leftVectices, _leftTrangles, _leftNormals, i, trangles);
            }
            else if (result[0] >= 0 && result[1] >= 0 && result[2] >= 0)
            {
                //全部在右侧
                SaveVetexInfo(_rightVectices, _rightTrangles, _rightNormals, i, trangles);
            }
            else
            {
                //与切面相交的三角形顶点
                int index = DeffrentSectionPoint(result);
                if (result[index] <= 0)
                {
                    int p0_Index = index + i;
                    int p1_Index = (p0_Index + 1) % 3 + i;
                    int p2_Index = (p0_Index + 2) % 3 + i;

                    SaveSectionPoint(true, p0_Index, p1_Index, p2_Index, trangles);
                }
                else
                {
                    int p0_Index = index + i;
                    int p1_Index = (p0_Index + 1) % 3 + i;
                    int p2_Index = (p0_Index + 2) % 3 + i;

                    SaveSectionPoint(false, p0_Index, p1_Index, p2_Index, trangles);
                }
            }
        }

        //2,根据顶点生成切割后生成的顶点信息...
        if (_newVectices.Count > 0)
        {
            Vector3 center = (_newVectices[0] + _newVectices[_newVectices.Count / 2]) * 0.5f;
            Vector3 normalNew = Vector3.Cross(_newVectices[0] - _newVectices[_newVectices.Count / 2], _newVectices[_newVectices.Count - 1] - _newVectices[_newVectices.Count / 2]);

            for (int i = 0; i < _newVectices.Count; i += 2)
            {
                _leftVectices.Add(center);
                _leftTrangles.Add(_leftVectices.Count - 1);
                _leftVectices.Add(_newVectices[i]);
                _leftTrangles.Add(_leftVectices.Count - 1);
                _leftVectices.Add(_newVectices[(i + 1) % _newVectices.Count]);
                _leftTrangles.Add(_leftVectices.Count - 1);

                for (int j = 0; j < 3; j++)
                {
                    _leftNormals.Add(normalNew);
                }
            }

            for (int i = 0; i < _newVectices.Count; i += 2)
            {
                _rightVectices.Add(center);
                _rightTrangles.Add(_rightVectices.Count - 1);

                _rightVectices.Add(_newVectices[(i + 1) % _newVectices.Count]);
                _rightTrangles.Add(_rightVectices.Count - 1);

                _rightVectices.Add(_newVectices[i]);
                _rightTrangles.Add(_rightVectices.Count - 1);

                for (int j = 0; j < 3; j++)
                {
                    _rightNormals.Add(-normalNew);
                }
            }
        }

        //3，把其中一边的mesh赋值给原物体
        _mesh.Clear();
        _mesh.vertices = _leftVectices.ToArray();
        _mesh.triangles = _leftTrangles.ToArray();
        _mesh.normals = _leftNormals.ToArray();

        //4，生成一个新的物体，赋值另一边的mesh值
        GameObject rightObject = new GameObject();
        rightObject.transform.position = _hitTrans.position;
        rightObject.AddComponent<MeshFilter>();
        rightObject.AddComponent<MeshRenderer>();
        rightObject.AddComponent<Rigidbody>();
        rightObject.AddComponent<MeshCollider>();

        Mesh rightMesh = rightObject.GetComponent<MeshFilter>().mesh;
        rightMesh.vertices = _rightVectices.ToArray();
        rightMesh.triangles = _rightTrangles.ToArray();
        rightMesh.normals = _rightNormals.ToArray();

        rightObject.GetComponent<MeshRenderer>().material = _renderer.material;
    }

    /// <summary>
    /// 清除缓存数据
    /// </summary>
    private void ClearData()
    {
        _leftVectices.Clear();
        _leftTrangles.Clear();
        _leftNormals.Clear();
        _rightVectices.Clear();
        _rightTrangles.Clear();
        _rightNormals.Clear();
        _newVectices.Clear();
    }

    /// <summary>
    /// 在被切割的三角形上计算出不同与另外两个的点
    /// </summary>
    private int DeffrentSectionPoint(float[] result)
    {
        List<int> temp1 = new List<int>(2);
        List<int> temp2 = new List<int>(2);
        for (int i = 0; i < result.Length; i++)
        {
            if (result[i] > 0)
            {
                temp1.Add(i);
            }
            else
            {
                temp2.Add(i);
            }
        }
        if (temp1.Count >= 2)
        {
            return temp2[0];
        }
        else
        {
            return temp1[0];
        }
    }
    /// <summary>
    /// 存储无切割的顶点坐标信息到指定的数组
    /// </summary>
    private void SaveVetexInfo(List<Vector3> curVectices, List<int> curTrangles, List<Vector3> curNormals, int index, int[] trangles)
    {
        for (int i = 0; i < 3; i++)
        {
            curVectices.Add(_mesh.vertices[trangles[index + i]]);
            curNormals.Add(_mesh.normals[trangles[index + i]]);
            curTrangles.Add(curVectices.Count - 1);
        }
    }

    /// <summary>
    /// 存储被切割的三角形的旧顶点和新顶点信息
    /// </summary>
    private void SaveSectionPoint(bool isLeft, int p0_Index, int p1_Index, int p2_Index, int[] trangles)
    {
        bool isLeftTemp = isLeft ? true : false;
        Vector3 c1 = CalculateSectionPoint(p0_Index, p1_Index, trangles);
        Vector3 c2 = CalculateSectionPoint(p0_Index, p2_Index, trangles);

        if (!isLeft)
        {
            _newVectices.Add(c1);
            _newVectices.Add(c2);
        }
        else
        {
            _newVectices.Add(c2);
            _newVectices.Add(c1);
        }

        SaveOldVetexInfo(isLeftTemp, p0_Index, trangles);
        SaveNewVetexInfo(isLeftTemp, c1, p0_Index, trangles);
        SaveNewVetexInfo(isLeftTemp, c2, p0_Index, trangles);

        SaveNewVetexInfo(!isLeftTemp, c1, p0_Index, trangles);
        SaveOldVetexInfo(!isLeftTemp, p1_Index, trangles);
        SaveOldVetexInfo(!isLeftTemp, p2_Index, trangles);

        SaveNewVetexInfo(!isLeftTemp, c2, p0_Index, trangles);
        SaveNewVetexInfo(!isLeftTemp, c1, p0_Index, trangles);
        SaveOldVetexInfo(!isLeftTemp, p2_Index, trangles);
    }
    /// <summary>
    /// 计算与切面相交的需要生成的新的顶点
    /// </summary>
    private Vector3 CalculateSectionPoint(int index1, int index2, int[] trangles)
    {
        Vector3 p0 = _hitTrans.TransformPoint(_mesh.vertices[trangles[index1]]);
        Vector3 p1 = _hitTrans.TransformPoint(_mesh.vertices[trangles[index2]]);
        Vector3 dirP01 = (p1 - p0).normalized;

        float pointLent = Vector3.Dot(_point, _planeNormal);
        float p0Lent = Vector3.Dot(p0, _planeNormal);
        float lenght = (pointLent - p0Lent) / Vector3.Dot(dirP01, _planeNormal);

        Vector3 cals = p0 + lenght * dirP01;

        return _hitTrans.InverseTransformPoint(cals);
    }
    /// <summary>
    /// 存储被切割的判断为左侧Or右侧的旧顶点坐标信息
    /// </summary>
    private void SaveOldVetexInfo(bool isLeft, int index, int[] trangles)
    {
        if (isLeft)
        {
            _leftVectices.Add(_mesh.vertices[trangles[index]]);
            _leftNormals.Add(_mesh.normals[trangles[index]]);
            _leftTrangles.Add(_leftVectices.Count - 1);
        }
        else
        {
            _rightVectices.Add(_mesh.vertices[trangles[index]]);
            _rightNormals.Add(_mesh.normals[trangles[index]]);
            _rightTrangles.Add(_rightVectices.Count - 1);
        }
    }
    /// <summary>
    /// 存储被切割的判断为左侧Or右侧的新顶点坐标信息
    /// </summary>
    private void SaveNewVetexInfo(bool isLeft, Vector3 cals, int p0_Index, int[] trangles)
    {
        if (isLeft)
        {
            _leftVectices.Add(cals);
            _leftNormals.Add(_mesh.normals[trangles[p0_Index]]);
            _leftTrangles.Add(_leftVectices.Count - 1);
        }
        else
        {
            _rightVectices.Add(cals);
            _rightNormals.Add(_mesh.normals[trangles[p0_Index]]);
            _rightTrangles.Add(_rightVectices.Count - 1);
        }
    }
}
