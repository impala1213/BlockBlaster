using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ShapeMaker))]
public class GUI_ShapeMaker : Editor
{
    private ShapeMaker shapeM;
    [SerializeField] private SerializedProperty _data;

    [SerializeField] private SerializedProperty PD_arr;

    [SerializeField] private SerializedProperty _pieceId;
    [SerializeField] private SerializedProperty _blocks;
    [SerializeField] private SerializedProperty _origin;
    [SerializeField] private SerializedProperty _sprite;
    [SerializeField] private SerializedProperty _color;
    [SerializeField] private SerializedProperty _mat;

    [SerializeField] private SerializedProperty _name;
    [SerializeField] private SerializedProperty _isOverwrite, _isOverlap;

    PieceDefinition _selectData;

    float box_size = 20f;
    SerializedProperty cells;

    private int select_num = -1;

    bool b;
    private SerializedProperty w_, h_ ;
    private SerializedProperty s_x_, s_y_;
    bool isDrag, isTrue;

    string prev_name, prev_id;



    private void OnEnable()
    {
        shapeM = (ShapeMaker)target;
        _name = serializedObject.FindProperty("_name");
        _data = serializedObject.FindProperty("_data");
        PD_arr = serializedObject.FindProperty("pd_arr");

        _pieceId = serializedObject.FindProperty("_pieceId");
        _blocks = serializedObject.FindProperty("_blocks");
        _origin = serializedObject.FindProperty("_origin");
        _sprite = serializedObject.FindProperty("_sprite");
        _color = serializedObject.FindProperty("_color");
        _mat = serializedObject.FindProperty("_mat");


        cells = serializedObject.FindProperty("slot_cell");
        w_ = serializedObject.FindProperty("w");
        h_ = serializedObject.FindProperty("h");
        s_x_ = serializedObject.FindProperty("s_x");
        s_y_ = serializedObject.FindProperty("s_y");
        _isOverwrite = serializedObject.FindProperty("isOverwrite");
        _isOverlap = serializedObject.FindProperty("isOverlap");
        shapeM.Img_Enable(false);
        shapeM.Get_Files();
    }

    private void OnDisable()
    {
        shapeM.Img_Enable(false);
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        serializedObject.Update();

        List<string> d_arr = shapeM.FileNames;
        d_arr.Add("새로 생성.");


        EditorGUILayout.Space(20);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;

        GUILayout.BeginHorizontal();
        GUILayout.Label("데이터 선택", titleStyle);
        int idx = EditorGUILayout.Popup(select_num, d_arr.ToArray());
        GUILayout.EndHorizontal();
        if (idx != select_num)
        {
            shapeM.Select_Data(idx);
            select_num = idx;
            shapeM.Setting_Data();
            shapeM.Img_Enable(true);
        }

        if (idx >= 0)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DisplayField();

            GUILayout.Space(15);

            // 버튼 초기화.
            if(prev_name != _name.stringValue)
            {
                _isOverwrite.boolValue = false;
                prev_name = _name.stringValue;
            }
            if(prev_id != _pieceId.stringValue)
            {
                _isOverlap.boolValue = false;
                prev_id = _pieceId.stringValue;
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.richText = true;
            // 버튼
            if (!_isOverlap.boolValue && !_isOverwrite.boolValue)
            {
                if (GUILayout.Button("Save Shape", buttonStyle))
                {
                    shapeM.Save_Data();
                }
            }
            else if (_isOverlap.boolValue)
            {
                if (GUILayout.Button("<color=red>ID가 중복되었습니다.</color>", buttonStyle))
                { }
            }
            else if (_isOverwrite.boolValue)
            {
                if (GUILayout.Button("<color=yellow>중복된 이름이 존재합니다. 덮어씌우겠습니까?</color>", buttonStyle))
                { shapeM.Save_Data(); }
            }

                GUILayout.EndVertical();
        }
        EditorGUILayout.Space(20);
        serializedObject.ApplyModifiedProperties();

    }


    private void DisplayField()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_data);
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(5);
        GUILayout.Label("저장할 파일 이름.", titleStyle);
        EditorGUILayout.PropertyField(_name);

        GUILayout.Space(13);
        //GUILayout.Label("세부 사항.");
        //GUILayout.Space(5);
        GUILayout.Label("브릭 아이디. (조각 식별용 문자.)", titleStyle);
        EditorGUILayout.PropertyField(_pieceId);

        GUILayout.Space(5);
        GUILayout.Label("모양 만들기.", titleStyle);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_blocks);
        EditorGUI.EndDisabledGroup();

        // 클릭으로 만드는 상자 제작.
        Point_MouseClick();
        // 회전기능.
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        if (GUILayout.Button("반시계 방향"))
        { shapeM.Rotate_Block(false); }

        // 반전기능.
        GUILayout.BeginVertical();
        if (GUILayout.Button("상하 반전"))
        { shapeM.Reversal_Block(true); }
        if (GUILayout.Button("좌우 반전"))
        { shapeM.Reversal_Block(false); }
        GUILayout.EndVertical();

        //회전기능
        if (GUILayout.Button("시계 방향"))
        { shapeM.Rotate_Block(true); }
        GUILayout.EndHorizontal();
        

        GUILayout.Space(5);
        GUILayout.Label("원점. 블럭의 중심점.", titleStyle);
        EditorGUILayout.PropertyField(_origin);
        int d_w = Mathf.FloorToInt(w_.intValue / 2);
        int d_h = Mathf.FloorToInt(h_.intValue / 2);
        _origin.vector2IntValue = new Vector2Int( Mathf.Clamp(_origin.vector2IntValue.x,-d_w,d_w), 
            Mathf.Clamp(_origin.vector2IntValue.y,-d_h, d_h)  );

        GUILayout.Space(15);
        GUILayout.Label("브릭의 세부 사항. 스프라이트, 색깔, 재질.", titleStyle);
        EditorGUILayout.PropertyField(_sprite);
        EditorGUILayout.PropertyField(_color);
        EditorGUILayout.PropertyField(_mat);

        shapeM.CheckSprite(false);
    }

    private void Point_MouseClick()
    {


        EditorGUILayout.Space(5f);

        int w = w_.intValue, h = h_.intValue;
        int s_x = s_x_.intValue, s_y = s_y_.intValue;
        float space = 1f;

        Vector2Int[] body_pointlist = shapeM.Data.blocks;

        Rect picker_rect = GUILayoutUtility.GetRect(box_size, box_size, box_size, box_size,
            GUIStyle.none, GUILayout.ExpandWidth(false));

        if (cells.arraySize != w * h)
        { cells.arraySize = w * h; }



        // 마우스 클릭 관련 이벤트를 받아옴.
        Event e = Event.current;

        for(int i = h-1; i >=0; i--)
        {
            EditorGUILayout.BeginHorizontal();
            for(int j = 0; j<w; j++)
            {
                int idx = i * w + j;

                SerializedProperty d_cell = cells.GetArrayElementAtIndex(idx);

                bool isOrigin = _origin.vector2IntValue.x + s_x == j && _origin.vector2IntValue.y+s_y == i;

                // 체크할 박스 그리기
                Rect rect = GUILayoutUtility.GetRect(
                    box_size, box_size,
                    GUILayout.Width(box_size),
                    GUILayout.Height(box_size));

                Rect drawRect = new Rect(
                    rect.x + space * 0.5f,
                    rect.y + space * 0.5f,
                    rect.width - space,
                    rect.height - space
                    );
                EditorGUI.DrawRect(drawRect, d_cell.boolValue ? (isOrigin?Color.yellow: Color.green ): Color.gray);

                // 마우스 위치가 해당 상자 안에 있다면
                if (rect.Contains(e.mousePosition))
                {
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        isDrag = true;
                        isTrue = !d_cell.boolValue;
                        d_cell.boolValue = isTrue;
                        e.Use();
                    }
                    else if (e.type == EventType.MouseDrag && isDrag)
                    {
                        d_cell.boolValue = isTrue;
                        e.Use();
                    }
                }

                if (isOrigin)
                { d_cell.boolValue = true; }

            }

            EditorGUILayout.EndHorizontal();
        }


        if (e.type == EventType.MouseUp)
        { isDrag = false; }
    }

}
