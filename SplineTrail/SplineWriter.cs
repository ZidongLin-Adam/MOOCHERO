using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
//文件输入输出命名空间
using System.IO;
using System.Text;

public class SplineWriter : MonoBehaviour 
{
    public GameObject trailEditorCanvas;        //宝物车轨迹绘制提示画布
    public Text segmentText;

	public SplineTrailRenderer trailReference;
	public string groundLayerName = "Ground";
	public string playerLayerName = "Default";
    public Vector3 cameraPosition;

    private Vector3 trailOffset = new Vector3(0, 0.025f, 0);
    private bool playerSelected = false;

    //样条曲线分段控制
    private int currentKnotsNumber;
    private Stack<int> splineSegments = new Stack<int>();

    private string trailPath;
    private Transform mainCamreaTransform;
    public float cameraMoveSpeed = 20.0f;

    //摄像机控制
    void CameraControl()
    {
        if (Input.GetKey(KeyCode.W))
            mainCamreaTransform.Translate(0.0f, cameraMoveSpeed * Time.deltaTime, 0.0f);
        if (Input.GetKey(KeyCode.S))
            mainCamreaTransform.Translate(0.0f, -cameraMoveSpeed * Time.deltaTime, 0.0f);
        if (Input.GetKey(KeyCode.A))
            mainCamreaTransform.Translate(-cameraMoveSpeed * Time.deltaTime, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.D))
            mainCamreaTransform.Translate(cameraMoveSpeed * Time.deltaTime, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.Z))
            mainCamreaTransform.Translate(0.0f, 0.0f, cameraMoveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.X))
            mainCamreaTransform.Translate(0.0f, 0.0f, -cameraMoveSpeed * Time.deltaTime);
    }
    void Start()
    {
        mainCamreaTransform = Camera.main.transform;
        mainCamreaTransform.position = cameraPosition;
        mainCamreaTransform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
        trailEditorCanvas.SetActive(true);
        trailPath = Application.dataPath + "/Resources/Trail.txt";
    }

	void Update () 
	{
		if(Input.GetMouseButtonDown(0))
		{
            //样条曲线分段控制
            currentKnotsNumber = trailReference.spline.knots.Count;

            RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

			if(Physics.Raycast(ray, out hit, float.MaxValue, LayerNameToIntMask(playerLayerName)))
			{
				playerSelected = true;
				MoveOnFloor();
			}
		}
		else if(Input.GetMouseButtonUp(0))
		{
			playerSelected = false;
            if (splineSegments.Count == 0)
                splineSegments.Push(currentKnotsNumber);
            else
            {
                if (currentKnotsNumber != trailReference.spline.knots.Count)
                    splineSegments.Push(currentKnotsNumber);
            }
        }

		if(Input.GetMouseButton(0) && playerSelected)
		{
			MoveOnFloor();
        }

        //删除上一段样条曲线
        if (Input.GetKeyDown(KeyCode.Q) && splineSegments.Count!=0)
        {
            int p = splineSegments.Pop();
            trailReference.RemoveKnots(p,splineSegments.Count);
        }

        //摄像机操控
        CameraControl();

        //预设材质更换
        if (Input.GetKeyDown(KeyCode.E))
        {
            trailReference.ChangeMaterial();
        }

        //当前控制线段个数显示
        segmentText.text = "当前控制线段个数：" + splineSegments.Count;

        //预设材质更换
        if (Input.GetKeyDown(KeyCode.C))
        {
            WriteKnotsData(trailPath);
        }
    }
    

	void MoveOnFloor()
	{
		RaycastHit hit;
		if(Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, 
			Input.mousePosition.y, 0)), out hit, float.MaxValue, LayerNameToIntMask(groundLayerName)))
		{
			trailReference.transform.position = hit.point + trailOffset;
		}
	}

	static int LayerNameToIntMask(string layerName)
	{
		int layer = LayerMask.NameToLayer(layerName);

		if(layer == 0)
			return int.MaxValue; 

		return 1 << layer;
	}
    
    void WriteKnotsData(string path)
    {
        Debug.Log("Knots saved");
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        List<Knot> knots = trailReference.spline.knots;
        int l = knots.Count;
        sw.WriteLine(l);
        for(int i = 0; i < l; i++)
        {
            sw.WriteLine(knots[i].position);
        }
        sw.Flush();
        sw.Close();
    }
}
