using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine;


public struct PointInfo {
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public string Label { get; }
    public string FullLabel { get; }
    public WallInfo WallInfo { get; }
    public GameObject GridPoint { get; }

    public static readonly PointInfo Empty = new PointInfo(null, null, "<?>", "<?>");

    public PointInfo(GameObject gridPoint, WallInfo wallInfo, string label, string fullLabel)
    {
        X = 0.0f;
        Y = 0.0f;
        Z = 0.0f;
        Label = label;
        FullLabel = fullLabel;
        WallInfo = wallInfo;
        GridPoint = gridPoint;

        if (gridPoint != null)
        {
            string name = gridPoint.name;
            int startIndex = name.IndexOf('(') + 1;
            int endIndex = name.IndexOf(')') - 1;
            int length = endIndex - startIndex + 1;

            char[] coordAxis = { name[5], name[6] };
            string[] coordValuesStr = name.Substring(startIndex, length).Split(',');
            float[] coordValues = { float.Parse(coordValuesStr[0], CultureInfo.InvariantCulture), float.Parse(coordValuesStr[1], CultureInfo.InvariantCulture) };

            switch (coordAxis[0])
            {
                case 'X': X = coordValues[0];
                    break;
                case 'Y': Y = coordValues[0];
                    break;
                case 'Z': Z = coordValues[0];
                    break;
                default:
                    break;
            }

            switch (coordAxis[1])
            {
                case 'X': X = coordValues[1];
                    break;
                case 'Y': Y = coordValues[1];
                    break;
                case 'Z': Z = coordValues[1];
                    break;
                default:
                    break;
            }
        }
    }

    public override string ToString() => $"{FullLabel} (X={X}, Y={Y}, Z={Z})";
}

// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = 
public class PointPlacer : MonoBehaviour {
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - STYLES
	private const float _CURSOR_SIZE = 0.05f;
    private Color _CURSOR_COLOR = new Color(1, 1, 1, 0.3f);
    private Color _CURSOR_COLOR_FOCUSED = new Color(1, 0, 0, 1f);
    private const float POINT_SIZE = 0.025f;
    private Color POINT_COLOR = Color.black;
    private const float LABEL_SIZE_PLACED = 0.04f;
    private const float LABEL_SIZE_PICKED = 0.06f;
    private const float LABEL_OFFSET_FROM_POINT = 0.03f;
    private Color LABEL_COLOR_POINT_PLACED = Color.white;
    private Color LABEL_COLOR_POINT_PICKED = Color.red;
    private Color LABEL_COLOR_EDGE_POINT_PICKED = Color.green;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - CURSOR
	private GameObject _cursor;
    private Renderer _cursorRenderer;
    private GameObject _cursorLabelObj;
    private Label _cursorLabel;
    private RaycastHit _cursorHit;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ADD/REMOVE POINT
    private char[] _labelsColl = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private int _labelsIdx = 0;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ADD/REMOVE EDGE
    private List<Label> _labelsList = new List<Label>();
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ADD/REMOVE EDGE
    private WallController _wc;
    private MeshBuilder _mc;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - CONTEXT
    public enum Context {
        Idle,
        AddPoint,
        RemovePoint
    }
    private Context _ctx = Context.Idle;
    public Context Ctx {
        get { return _ctx; } 
        set { 
            switch (_ctx)
            {
                case Context.RemovePoint:
                    _DisableLabelPicker();
                    break;

                default:
                    break;
            }

            _ctx = value;

            switch (_ctx)
            {
                case Context.RemovePoint:
                    if (_IsGridPointWithLabels()) {
                        _EnableLabelPicker();
                    } else {
                        _ctx = Context.Idle;
                    }
                    break;

                default:
                    break;
            }
        } 
    }
    
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - START()
    void Start()
    {
        GameObject wallsObject = GameObject.Find("Walls");
        _wc = wallsObject.GetComponent<WallController>();
        _mc = (MeshBuilder)FindObjectOfType(typeof(MeshBuilder));
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - PRIVATE()
    /// <summary>
    /// Metoda określa na której ścianie znajduje się punkt trafiony Raycastem
    /// </summary>
    /// <param name="wallNormal">Wektor normalny collidera punktu trafionego Raycastem</param>
    /// <param name="pointPosition">Współrzędne trafionego punktu</param>
    /// <returns>Obiekt opisujący ścianę, na której najprawdopodobniej znajduje się trafiony punkt</returns>
    private WallInfo _EstimateWall(Vector3 wallNormal, Vector3 pointPosition)
    {
        List<WallInfo> walls = _wc.GetWalls();
        WallInfo closestWall = null;
        float distanceToClosestWall = Mathf.Infinity;
        
        foreach (var wall in walls)
        {
            float distance = Vector3.Distance(wall.gameObject.transform.position, pointPosition);

            if (distance < distanceToClosestWall && wall.GetNormal() == wallNormal)
            {
                closestWall = wall;
                distanceToClosestWall = distance;
                //Debug.Log($"num: {closestWall.number}  Nwall: {wall.GetNormal()}  Nhit: {wallNormal} - {distanceToClosestWall}");
            }
        }

        return closestWall;
    }

    private void _LocateLabels(GameObject pointClicked, WallInfo wall)
    {
        Vector3 wallNormal = wall.GetNormal();
        int labelsNum = pointClicked.transform.childCount;
        float theta = 2.0f * Mathf.PI / (float)labelsNum;

        for (int i = 0; i < labelsNum; i++)
        {
            float alpha_i = (float)i * theta;
            Vector3 offset = Vector3.zero;

            if ((int)Mathf.Abs(wallNormal.x) == 1) {
                offset.y = 0.01f + LABEL_OFFSET_FROM_POINT * Mathf.Cos(alpha_i);
                offset.z = 0.02f + LABEL_OFFSET_FROM_POINT * Mathf.Sin(alpha_i);
            }
            else if((int)Mathf.Abs(wallNormal.y) == 1) {
                offset.x = 0.02f + LABEL_OFFSET_FROM_POINT * Mathf.Cos(alpha_i);
                offset.z = 0.02f + LABEL_OFFSET_FROM_POINT * Mathf.Sin(alpha_i);
            }
            else if((int)Mathf.Abs(wallNormal.z) == 1) {
                offset.x = 0.02f + LABEL_OFFSET_FROM_POINT * Mathf.Cos(alpha_i);
                offset.y = 0.01f + LABEL_OFFSET_FROM_POINT * Mathf.Sin(alpha_i);
            }

            pointClicked
                .transform
                .GetChild(i)
                .Find("Label")
                .GetComponent<Label>()
                .SetOffset(offset);
        }
    }

    private bool _IsGridPoint()
    {
        return (_cursorHit.collider?.tag == "GridPoint") ? true : false ;
    }

    private bool _IsGridPointWithLabels()
    {
        return (_IsGridPoint() && _cursorHit.collider?.gameObject?.transform.childCount > 0) ? true : false ;
    }

    public void _EnableLabelPicker()
    {
        GameObject pointClicked = _cursorHit.collider.gameObject;
        int labelsNum = pointClicked.transform.childCount;
        int indexOfLabelToSetAsPickedEdgePoint = 0;
        List<Label> labels = new List<Label>();

        for (int i = 0; i < labelsNum; i++)
        {
            labels.Add(
                pointClicked
                    .transform
                    .GetChild(i)
                    .Find("Label")
                    .GetComponent<Label>()
            );                
        }

        if (labels.Count > 0)
        {
            labels[0].SetColor(LABEL_COLOR_EDGE_POINT_PICKED);
        }

        _labelsList = labels;
    }

    public void _DisableLabelPicker()
    {
        _labelsList.ForEach(label => label.SetColor(LABEL_COLOR_POINT_PLACED));
        _labelsList.Clear();
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - PUBLIC()
	public void CreateCursor() 
    {
		_cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        // Tworzymy nowy materiał
        Material transparentMaterial = new Material(Shader.Find("Standard"));

        // Ustawiamy kolor i przezroczystość materiału
        transparentMaterial.color = _CURSOR_COLOR;

        // Włączamy renderowanie przezroczystości
        transparentMaterial.SetFloat("_Mode", 3); // Ustawienie trybu renderowania na przeźroczystość
        transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMaterial.SetInt("_ZWrite", 0);
        transparentMaterial.DisableKeyword("_ALPHATEST_ON");
        transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
        transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        transparentMaterial.renderQueue = 3000;

        // Przypisujemy materiał do sfery
        _cursorRenderer.material = transparentMaterial;
        _cursor.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        // Ustawiamy rozmiar
        _cursor.transform.localScale = new Vector3(_CURSOR_SIZE, _CURSOR_SIZE, _CURSOR_SIZE);

        // Dodajemy obiekt etykiety
        _cursorLabelObj = new GameObject("_CursorLabel");
        _cursorLabelObj.transform.SetParent(_cursor.transform);
        _cursorLabelObj.transform.position = _cursor.transform.position + new Vector3(0, 0.1f, 0);

        // Dodajemy etykietę
        _cursorLabel = _cursorLabelObj.AddComponent<Label>();
        _cursorLabel.SetLabel($"{_labelsColl[_labelsIdx]}", LABEL_SIZE_PICKED, LABEL_COLOR_POINT_PICKED);		
    }
	
	public void MoveCursor(RaycastHit hit)
    {
		if (hit.collider == null) {
            return;
        }

        _cursor.transform.position = hit.point;
        _cursorHit = hit;

        switch (_ctx)
        {
            case Context.AddPoint:
                _cursorRenderer.material.color = (hit.collider.tag == "GridPoint") ? _CURSOR_COLOR_FOCUSED : _CURSOR_COLOR;
                _cursorLabel.SetEnable((hit.collider.tag == "GridPoint") ? true : false);
                break;

            default:
                _cursorRenderer.material.color = _CURSOR_COLOR;
                _cursorLabel.SetEnable(false);
                break;
        }
	}

    public void NextLabel()
    {
        switch (_ctx)
        {
            case Context.AddPoint:
                _labelsIdx = (_labelsIdx+1) % _labelsColl.Length;
                _cursorLabel.SetLabel($"{_labelsColl[_labelsIdx]}");
                break;

            case Context.RemovePoint:
                int currIdx = _labelsList.FindIndex(label => label.GetColor().Equals(LABEL_COLOR_EDGE_POINT_PICKED));
                int nextIdx = (currIdx+1) % _labelsList.Count;
                _labelsList[currIdx].SetColor(LABEL_COLOR_POINT_PLACED);
                _labelsList[nextIdx].SetColor(LABEL_COLOR_EDGE_POINT_PICKED);
                break;

            default:
                break;
        }
    }

    public void PreviousLabel()
    {
        switch (_ctx)
        {
            case Context.AddPoint:
                _labelsIdx = ((_labelsIdx-1) < 0) ? (_labelsColl.Length-1) : (_labelsIdx-1);
                _cursorLabel.SetLabel($"{_labelsColl[_labelsIdx]}");
                break;

            case Context.RemovePoint:
                int currIdx = _labelsList.FindIndex(label => label.GetColor().Equals(LABEL_COLOR_EDGE_POINT_PICKED));
                int nextIdx = ((currIdx-1) < 0) ? (_labelsList.Count-1) : (currIdx-1);
                _labelsList[currIdx].SetColor(LABEL_COLOR_POINT_PLACED);
                _labelsList[nextIdx].SetColor(LABEL_COLOR_EDGE_POINT_PICKED);
                break;

            default:
                break;
        }
    }

    public PointInfo AddPoint()
    {
        switch (_ctx)
        {
            case Context.AddPoint:
                if (!_IsGridPoint()) {
                    return PointInfo.Empty;
                }
                
                GameObject pointClicked = _cursorHit.collider.gameObject;
                if (pointClicked == null) {
                    return PointInfo.Empty;
                }
                
                WallInfo wall = _EstimateWall(_cursorHit.normal, pointClicked.transform.position);
                if (wall == null) {
                    return PointInfo.Empty;
                }

                string labelText = $"{_labelsColl[_labelsIdx]}";
                int index = _wc.GetWallIndex(wall);
                string fullLabelText = $"{labelText + new string('\'', index)}";

                if(_mc.CheckIfAlreadyExist(wall, labelText)) {
                    Debug.LogError($"Rzut {labelText} juz jest na tej scianie");
                    return PointInfo.Empty;
                }

                GameObject labelObj = new GameObject(labelText);
                labelObj.transform.parent = pointClicked.transform;

                Point point = labelObj.AddComponent<Point>();
                point.SetCoordinates(pointClicked.transform.position);
                point.SetStyle(POINT_COLOR, POINT_SIZE);
                point.SetEnable(true);
                point.SetLabel(fullLabelText, LABEL_SIZE_PLACED, LABEL_COLOR_POINT_PLACED);
                
                ///linia rzutująca
                LineSegment lineseg = labelObj.AddComponent<LineSegment>();
                lineseg.SetStyle(Color.blue, 0.002f);

                _mc.AddPointProjection(wall, labelText, labelObj);

                _LocateLabels(pointClicked, wall);

                return new PointInfo(pointClicked, wall, labelText, fullLabelText);
                
            default:
                return PointInfo.Empty;
        }
    }

    public PointInfo RemovePoint()
    {
        switch (_ctx)
        {
            case Context.RemovePoint:
                if (!_IsGridPoint()) {
                    return PointInfo.Empty;
                }

                GameObject pointClicked = _cursorHit.collider.gameObject;
                if (pointClicked == null) {
                    return PointInfo.Empty;
                }

                WallInfo wall = _EstimateWall(_cursorHit.normal, pointClicked.transform.position);
                if (wall == null) {
                    return PointInfo.Empty;
                }
                
                Label pickedLabel = _labelsList.Find(label => label.GetColor().Equals(LABEL_COLOR_EDGE_POINT_PICKED));
                if (pickedLabel == default(Label)) {
                    return PointInfo.Empty;
                }

                string fullLabelText = pickedLabel.GetText();
                string labelText = fullLabelText.Trim('\'');

                Transform labelObjTrans = pointClicked.transform.Find(labelText);             
                if (labelObjTrans == null) {
                    return PointInfo.Empty;
                }

                _mc.RemovePointProjection(wall, labelText);

                GameObject labelObj = labelObjTrans.transform.gameObject;
                Destroy(labelObj);

                _LocateLabels(pointClicked, wall);

                return new PointInfo(pointClicked, wall, labelText, fullLabelText);

            default:
                return PointInfo.Empty;
        }
    }

    public void RemovePoint(PointInfo pi)
    {
        if (pi.GridPoint == null || pi.WallInfo == null) {
            return;
        }

        Transform labelObjTrans = pi.GridPoint.transform.Find(pi.Label);  
        if (labelObjTrans == null) {
            Debug.LogError($"Wezel nie ma takiego dziecka jak {pi.Label}");
            return;
        }

        _mc.RemovePointProjection(pi.WallInfo, pi.Label);

        GameObject labelObj = labelObjTrans.transform.gameObject;
        Destroy(labelObj);

        _LocateLabels(pi.GridPoint, pi.WallInfo);
    }
}
