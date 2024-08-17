using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine;

// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = 
public class PointPlacer : MonoBehaviour {
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - STYLES
/*	private const float _CURSOR_SIZE = 0.05f;
    private Color _CURSOR_COLOR = new Color(1, 1, 1, 0.3f);
    private Color _CURSOR_COLOR_FOCUSED = new Color(1, 0, 0, 1f);
    private const float POINT_SIZE = 0.025f;
    private Color POINT_COLOR = Color.black;
    private const float LABEL_SIZE_PLACED = 0.04f;
    private const float LABEL_SIZE_PICKED = 0.06f;
    private const float LABEL_OFFSET_FROM_POINT = 0.03f;
    private Color LABEL_COLOR_PLACED = Color.white;
    private Color LABEL_COLOR_CHOSEN = Color.red;
    private Color LABEL_COLOR_PICKED_FOCUSED = Color.green;
    private Color LABEL_COLOR_PICKED_UNFOCUSED = new Color(0, 0.8f, 0);*/
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - CURSOR
	private GameObject _cursor;
    private Renderer _cursorRenderer;
    private GameObject _cursorLabelObj;
    private Label _cursorLabel;
    private RaycastHit _cursorHit;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ADD POINT
    private char[] _addPoint_Labels = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private int _addPoint_LabelIdx = 0;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - REMOVE POINT
    private List<Label> _removePoint_Labels = new List<Label>();
    private GameObject _removePoint_CurrentlyFocusedGridPoint;
    private WallInfo _removePoint_Wall;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ADD EDGE
    private Dictionary<GameObject, List<Label>> _addEdge_Labels = new Dictionary<GameObject, List<Label>>();
    private GameObject _addEdge_CurrentlyFocusedGridPoint;
    private GameObject _AddEdge_CurrentlyFocusedGridPoint {
        get { return _addEdge_CurrentlyFocusedGridPoint; }
        set {
            if (_addEdge_CurrentlyFocusedGridPoint != null && _addEdge_Labels.ContainsKey(_addEdge_CurrentlyFocusedGridPoint)) {
                Label unfocusedLabel = _FindPickedLabel(_addEdge_Labels[_addEdge_CurrentlyFocusedGridPoint]);
                unfocusedLabel.SetColor(ReconstructionInfo.LABEL_COLOR_PICKED_UNFOCUSED);
            }
            if (value != null && _addEdge_Labels.ContainsKey(value)) {
                Label focusedLabel = _FindPickedLabel(_addEdge_Labels[value]);
                focusedLabel.SetColor(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED);
            }

            _addEdge_CurrentlyFocusedGridPoint = value;
        }
    }
    private WallInfo _addEdge_Wall;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - UTILS
    private WallController _wc;
    private MeshBuilder _mc;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - CONTEXT
    public enum Context {
        Idle,
        AddPoint,
        RemovePoint,
        AddEdge
    }
    private Context _ctx = Context.Idle;
    public Context Ctx {
        get { return _ctx; } 
        set { 
            switch (_ctx)
            {
                case Context.RemovePoint:
                    if (_ctx != value) {
                        _DisableLabelPicker(_removePoint_Labels);
                        _removePoint_CurrentlyFocusedGridPoint = null;
                        _removePoint_Wall = null;
                    }
                    break;

                case Context.AddEdge:
                    if (_ctx != value) {
                        foreach (var labels in _addEdge_Labels.Values) { _DisableLabelPicker(labels); }
                        _addEdge_Labels.Clear();
                        _AddEdge_CurrentlyFocusedGridPoint = null;
                        _addEdge_Wall = null;
                    }
                    break;

                default:
                    break;
            }
            Debug.Log($"(state) {_ctx} ==> {value}");
            _ctx = value;
        } 
    }
    
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - UNITY()
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
                offset.y = 0.01f + ReconstructionInfo.LABEL_OFFSET_FROM_POINT * Mathf.Cos(alpha_i);
                offset.z = 0.02f + ReconstructionInfo.LABEL_OFFSET_FROM_POINT * Mathf.Sin(alpha_i);
            }
            else if((int)Mathf.Abs(wallNormal.y) == 1) {
                offset.x = 0.02f + ReconstructionInfo.LABEL_OFFSET_FROM_POINT * Mathf.Cos(alpha_i);
                offset.z = 0.02f + ReconstructionInfo.LABEL_OFFSET_FROM_POINT * Mathf.Sin(alpha_i);
            }
            else if((int)Mathf.Abs(wallNormal.z) == 1) {
                offset.x = 0.02f + ReconstructionInfo.LABEL_OFFSET_FROM_POINT * Mathf.Cos(alpha_i);
                offset.y = 0.01f + ReconstructionInfo.LABEL_OFFSET_FROM_POINT * Mathf.Sin(alpha_i);
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

    private void _EnableLabelPicker(GameObject clickedPoint, List<Label> labels)
    {
        _DisableLabelPicker(labels);

        int labelsNum = clickedPoint.transform.childCount;

        for (int i = 0; i < labelsNum; i++)
        {
            labels.Add(
                clickedPoint
                    .transform
                    .GetChild(i)
                    .Find("Label")
                    .GetComponent<Label>()
            );                
        }
        
        labels[0].SetColor(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED);
    }

    private void _DisableLabelPicker(List<Label> labels)
    {
        labels.ForEach(label => label.SetColor(ReconstructionInfo.LABEL_COLOR_PLACED));
        labels.Clear();
    }

    private PointINFO _AddPoint()
    {
        GameObject pointClicked = _cursorHit.collider.gameObject;
        if (pointClicked == null) {
            return PointINFO.Empty;
        }
        
        WallInfo wall = _EstimateWall(_cursorHit.normal, pointClicked.transform.position);
        if (wall == null) {
            return PointINFO.Empty;
        }

        string labelText = $"{_addPoint_Labels[_addPoint_LabelIdx]}";
        int index = _wc.GetWallIndex(wall);
        string fullLabelText = $"{labelText + new string('\'', index)}";

        if(_mc.CheckIfAlreadyExist(wall, labelText)) {
            Debug.LogError($"Rzut {labelText} juz jest na tej scianie");
            return PointINFO.Empty;
        }

        GameObject labelObj = new GameObject(labelText);
        labelObj.transform.parent = pointClicked.transform;

        Point point = labelObj.AddComponent<Point>();
        point.SetCoordinates(pointClicked.transform.position);
        point.SetStyle(ReconstructionInfo.POINT_COLOR, ReconstructionInfo.POINT_SIZE);
        point.SetEnable(true);
        point.SetLabel(fullLabelText, ReconstructionInfo.LABEL_SIZE_PLACED, ReconstructionInfo.LABEL_COLOR_PLACED);
        
        ///linia rzutująca
        LineSegment lineseg = labelObj.AddComponent<LineSegment>();
        lineseg.SetStyle(ReconstructionInfo.PROJECTION_LINE_COLOR, ReconstructionInfo.PROJECTION_LINE_WIDTH);

        _mc.AddPointProjection(wall, labelText, labelObj);

        _LocateLabels(pointClicked, wall);

        return new PointINFO(pointClicked, wall, labelText, fullLabelText);
    }

    private Label _FindPickedLabel(List<Label> labels)
    {
        return labels.Find(label => {
            Color color = label.GetColor();
            return (color.Equals(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED) || color.Equals(ReconstructionInfo.LABEL_COLOR_PICKED_UNFOCUSED));
        });
    }

    private PointINFO _RemovePoint()
    {
        Label pickedLabel = _FindPickedLabel(_removePoint_Labels);

        string fullLabelText = pickedLabel.GetText();
        string labelText = fullLabelText.Trim('\'');

        _mc.RemovePointProjection(_removePoint_Wall, labelText);

        Destroy(_removePoint_CurrentlyFocusedGridPoint.transform.Find(labelText).gameObject);

        _LocateLabels(_removePoint_CurrentlyFocusedGridPoint, _removePoint_Wall);

        return new PointINFO(_removePoint_CurrentlyFocusedGridPoint, _removePoint_Wall, labelText, fullLabelText);
    }

    private EdgeINFO _AddEdge()
    {
        GameObject[] clickedPoints = _addEdge_Labels.Keys.ToArray();

        Label pickedLabel_1 = _FindPickedLabel(_addEdge_Labels[clickedPoints[0]]);
        Label pickedLabel_2 = _FindPickedLabel(_addEdge_Labels[clickedPoints[1]]);

        string fullLabelText_1 = pickedLabel_1.GetText();
        string fullLabelText_2 = pickedLabel_2.GetText();

        string labelText_1 = fullLabelText_1.Trim('\'');
        string labelText_2 = fullLabelText_2.Trim('\'');

        var point_1 = new PointINFO(clickedPoints[0], _addEdge_Wall, labelText_1, fullLabelText_1);
        var point_2 = new PointINFO(clickedPoints[1], _addEdge_Wall, labelText_2, fullLabelText_2);

        GameObject edgeObj = new GameObject($"{fullLabelText_1}-{fullLabelText_2}");
        LineSegment edge = edgeObj.AddComponent<LineSegment>();
        edge.SetStyle(ReconstructionInfo.EDGE_COLOR, ReconstructionInfo.EDGE_LINE_WIDTH);
        edge.SetCoordinates(
            point_1.GridPoint.transform.position,
            point_2.GridPoint.transform.position
        );

        _mc.AddEdgeProjection(labelText_1, labelText_2);
        return new EdgeINFO(edgeObj, edge, point_1, point_2);
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - PUBLIC()
	public void CreateCursor() 
    {
		_cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        // Tworzymy nowy materiał
        Material transparentMaterial = new Material(Shader.Find("Standard"));

        // Ustawiamy kolor i przezroczystość materiału
        transparentMaterial.color = ReconstructionInfo._CURSOR_COLOR;

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
        _cursor.transform.localScale = new Vector3(ReconstructionInfo._CURSOR_SIZE, ReconstructionInfo._CURSOR_SIZE, ReconstructionInfo._CURSOR_SIZE);

        // Dodajemy obiekt etykiety
        _cursorLabelObj = new GameObject("_CursorLabel");
        _cursorLabelObj.transform.SetParent(_cursor.transform);
        _cursorLabelObj.transform.position = _cursor.transform.position + new Vector3(0, 0.1f, 0);

        // Dodajemy etykietę
        _cursorLabel = _cursorLabelObj.AddComponent<Label>();
        _cursorLabel.SetLabel($"{_addPoint_Labels[_addPoint_LabelIdx]}", ReconstructionInfo.LABEL_SIZE_PICKED, ReconstructionInfo.LABEL_COLOR_CHOSEN);		
    }
	
	public void MoveCursor(RaycastHit hit)
    {
		if (hit.collider == null) {
            return;
        }

        _cursor.transform.position = hit.point;
        _cursorHit = hit;

        switch (Ctx)
        {
            case Context.AddPoint:
                _cursorRenderer.material.color = (hit.collider.tag == "GridPoint") ? ReconstructionInfo._CURSOR_COLOR_FOCUSED : ReconstructionInfo._CURSOR_COLOR;
                _cursorLabel.SetEnable((hit.collider.tag == "GridPoint") ? true : false);
                break;

            default:
                _cursorRenderer.material.color = ReconstructionInfo._CURSOR_COLOR;
                _cursorLabel.SetEnable(false);
                break;
        }
	}

    public void NextLabel()
    {
        switch (Ctx)
        {
            case Context.AddPoint:
            {
                _addPoint_LabelIdx = (_addPoint_LabelIdx+1) % _addPoint_Labels.Length;
                _cursorLabel.SetLabel($"{_addPoint_Labels[_addPoint_LabelIdx]}");
            }
                break;

            case Context.RemovePoint:
            {
                int currIdx = _removePoint_Labels.FindIndex(label => label.GetColor().Equals(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED));
                int nextIdx = (currIdx+1) % _removePoint_Labels.Count;
                _removePoint_Labels[currIdx].SetColor(ReconstructionInfo.LABEL_COLOR_PLACED);
                _removePoint_Labels[nextIdx].SetColor(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED);
            }
                break;

            case Context.AddEdge:
            {
                var labels = _addEdge_Labels[_AddEdge_CurrentlyFocusedGridPoint];
                int currIdx = labels.FindIndex(label => label.GetColor().Equals(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED));
                int nextIdx = (currIdx+1) % labels.Count;
                labels[currIdx].SetColor(ReconstructionInfo.LABEL_COLOR_PLACED);
                labels[nextIdx].SetColor(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED);
            }
                break;

            default:
                break;
        }
    }

    public void PreviousLabel()
    {
        switch (Ctx)
        {
            case Context.AddPoint:
            {
                _addPoint_LabelIdx = ((_addPoint_LabelIdx-1) < 0) ? (_addPoint_Labels.Length-1) : (_addPoint_LabelIdx-1);
                _cursorLabel.SetLabel($"{_addPoint_Labels[_addPoint_LabelIdx]}");   
            }
                break;

            case Context.RemovePoint:
            {
                int currIdx = _removePoint_Labels.FindIndex(label => label.GetColor().Equals(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED));
                int nextIdx = ((currIdx-1) < 0) ? (_removePoint_Labels.Count-1) : (currIdx-1);
                _removePoint_Labels[currIdx].SetColor(ReconstructionInfo.LABEL_COLOR_PLACED);
                _removePoint_Labels[nextIdx].SetColor(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED);
            }
                break;
            
            case Context.AddEdge:
            {
                var labels = _addEdge_Labels[_AddEdge_CurrentlyFocusedGridPoint];
                int currIdx = labels.FindIndex(label => label.GetColor().Equals(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED));
                int nextIdx =  ((currIdx-1) < 0) ? (labels.Count-1) : (currIdx-1);
                labels[currIdx].SetColor(ReconstructionInfo.LABEL_COLOR_PLACED);
                labels[nextIdx].SetColor(ReconstructionInfo.LABEL_COLOR_PICKED_FOCUSED);
            }
                break;

            default:
                break;
        }
    }

    public void RemovePoint(PointINFO pi)
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


    public PointINFO HandleAddingPoint()
    {
        // jeśli kliknięto pierwszy raz...
        if (Ctx != Context.AddPoint)
        {
            // ...włącz tryb dodawania punktu...
            Ctx = Context.AddPoint;
        }
        // jeśli kliknięto drugi raz...
        else
        {
            // ...i kliknięto na punkt siatki...
            if (_IsGridPoint())
            {
                // ...stwórz punkt i zakończ
                PointINFO pointINFO = _AddPoint();
                Ctx = Context.Idle;
                return pointINFO;
            }
            // ...i kliknięto gdzieś indziej...
            else
            {
                // ...zakończ
                Ctx = Context.Idle;
            }
        }
        
        return PointINFO.Empty;
    }

    public PointINFO HandleRemovingPoint()
    {
        Ctx = Context.RemovePoint;

        // jeśli kliknięto na punkt siatki z etykietami...
        if (_IsGridPointWithLabels())
        {
            GameObject clickedPoint = _cursorHit.collider.gameObject;
            // ...i jest już sfocusowany...
            if (clickedPoint.Equals(_removePoint_CurrentlyFocusedGridPoint))
            {
                // ...usuń punkt i zakończ
                PointINFO pointINFO = _RemovePoint();
                Ctx = Context.Idle;
                return pointINFO;
            }
            // ...i nie jest sfocusowany...
            else
            {
                // ...określ jego ścianę, dodaj mu wybieranie etykiety i ustaw mu focus
                _removePoint_Wall = _EstimateWall(_cursorHit.normal, clickedPoint.transform.position);
                _EnableLabelPicker(clickedPoint, _removePoint_Labels);
                _removePoint_CurrentlyFocusedGridPoint = clickedPoint;
            }     
        }
        // jeśli kliknięto gdzieś indziej...
        else
        {
            // ...zakończ
            Ctx = Context.Idle;
        }

        return PointINFO.Empty;
    }

    public EdgeINFO HandleAddingEdge()
    {
        Ctx = Context.AddEdge;

        // jeśli kliknięto na punkt siatki z etykietami...
        if (_IsGridPointWithLabels())
        {
            GameObject clickedPoint = _cursorHit.collider.gameObject;    
            // ...i jest on już dodany...
            if (_addEdge_Labels.ContainsKey(clickedPoint))
            {    
                // ...i jest sfocusowany...
                if (clickedPoint.Equals(_AddEdge_CurrentlyFocusedGridPoint))
                {
                    // ...to zapomnij wybraną etykietę, usuń mu focus i usuń go
                    _AddEdge_CurrentlyFocusedGridPoint = null;
                    _DisableLabelPicker(_addEdge_Labels[clickedPoint]);
                    _addEdge_Labels.Remove(clickedPoint);
                    // ... jeśli obecnych punktów jest 0...
                    if (_addEdge_Labels.Count == 0)
                    {
                        // ...zakończ
                        Ctx = Context.Idle;
                    }
                }
                // ...i nie jest sfocusowany...
                else
                {
                    // ...to ustaw mu focus
                    _AddEdge_CurrentlyFocusedGridPoint = clickedPoint;
                }
            }
            // ...i nie jest jeszcze dodany...
            else
            {
                // ...określ jego ścianę...
                WallInfo clickedPointWall = _EstimateWall(_cursorHit.normal, clickedPoint.transform.position);
                // ...i obecnych punktów jest 0 lub (1 i leży na tej samej ścianie)...
                if (_addEdge_Labels.Count == 0 || (_addEdge_Labels.Count == 1 && _addEdge_Wall?.number == clickedPointWall.number))
                {
                    // ...dodaj go, dodaj mu wybieranie etykiety i ustaw mu focus
                    _addEdge_Labels.Add(clickedPoint, new List<Label>());
                    _addEdge_Wall = clickedPointWall;
                    _EnableLabelPicker(clickedPoint, _addEdge_Labels[clickedPoint]);
                    _AddEdge_CurrentlyFocusedGridPoint = clickedPoint;
                }
                // ...i obecnych punktów jest 2...
                else if (_addEdge_Labels.Count == 2)
                {
                    // ...stwórz krawędź i zakończ
                    EdgeINFO edgeINFO = _AddEdge();
                    Ctx = Context.Idle;
                    return edgeINFO;
                }
            }
        // jeśli kliknięto gdzieś indziej...
        }
        else
        {
            // ...i obecnych punktów jest 2...
            if (_addEdge_Labels.Count == 2)
            {
                // ...stwórz krawędź i zakończ
                EdgeINFO edgeINFO = _AddEdge();
                Ctx = Context.Idle;
                return edgeINFO;
            }
            // ...i obecnych punktów jest 0 lub 1
            else
            {
                // ...zakończ
                Ctx = Context.Idle;
            }
        }

        return EdgeINFO.Empty;
    }
}
