using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.UI;

#region 출력용 클래스.
[Serializable]
public class C_PieceClass
{
    public PieceDefinition data;
    public C_PieceClass(PieceDefinition _data)
    {
        data = _data;
    }
}
#endregion

public class ShapeMaker : MonoBehaviour
{
    string _path = "Assets/PieceDefinition";
    string default_img_path = "Assets/Z_Other_LSM/Block_One.png";

    private Sprite default_png;
    [SerializeField] private C_PieceClass[] pd_arr;

    [SerializeField] private PieceDefinition _data;

    [SerializeField] private string _pieceId;
    [SerializeField] private Vector2Int[] _blocks;
    [SerializeField] private Vector2Int _origin;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private Color _color;
    [SerializeField] private Material _mat;

    [SerializeField]private string _name;

    private List<string> file_names;

    [SerializeField] private int w=5, h=5;
    [SerializeField] private int s_x , s_y;

    // gui 관련
    [SerializeField] private bool[] slot_cell;       // gui에서 반환할 bool배열.

    [SerializeField] private bool isOverwrite;
    [SerializeField] private bool isOverlap;

    [SerializeField] private Image _prevImage=null;

    [SerializeField] private Sprite pre_sprite;
    [SerializeField] private Color pre_color;
    [SerializeField] private Material pre_mat;

    #region Getter
    [field:SerializeField]public int SelectNum { get; set; }
    public List<string> FileNames => file_names;
    public PieceDefinition Data => _data;
    public Image PrevImage
    {
        get
        {
            if (_prevImage == null)
            {
                _prevImage = this.GetComponentInChildren<Image>();
            }
            return _prevImage;
        }
    }
    #endregion

    public void Get_Files()
    {
        var d_a = AssetDatabase.FindAssets($"t:{typeof(PieceDefinition)}", new[] {_path });

        PieceDefinition[] dummy_arr = d_a.Select(g =>
        AssetDatabase.LoadAssetAtPath<PieceDefinition>(AssetDatabase.GUIDToAssetPath(g)))
            .OrderBy(g=>g.name)
            .ToArray();

        pd_arr = dummy_arr.Select(g => new C_PieceClass(g)).ToArray();
        file_names = dummy_arr.Select(g => g.name).ToList();
    }

    public void Setting_Data()
    {
        //_data = pd_arr[_idx].data;
        w=7; h = 7;
        _pieceId = _data.pieceId;
        _blocks = _data.blocks;
        _origin = _data.dragAnchor;
        _sprite = _data.tileSprite;
        _color = _data.tileColor;
        _mat = _data.tileMaterial;
        _name = _data.name;

        slot_cell = new bool[w*h];
        s_x = Mathf.RoundToInt(w / 2);
        s_y = Mathf.RoundToInt(h / 2);

        for (int i = 0; i < _blocks.Length; i++)
        {
            int x_ = s_x + _blocks[i].x;
            int y_ = s_y + _blocks[i].y;
            slot_cell[y_ * w + x_] = true;
        }
    }

    // true => Clockwise, false => CounterClockwise
    public void Rotate_Block(bool b)
    {
        bool[] d_cell = slot_cell.ToArray();


        for (int i = 0; i < slot_cell.Length; i++) {
            int x_ = i % w;
            int y_ = Mathf.FloorToInt(i / w);

            Vector2Int rotate_d_vec = new Vector2Int(x_ - s_x, y_ - s_y);
            if (b)
            { rotate_d_vec = new Vector2Int(rotate_d_vec.y, -rotate_d_vec.x); }
            else
            { rotate_d_vec = new Vector2Int(-rotate_d_vec.y, rotate_d_vec.x); }

            rotate_d_vec += new Vector2Int(s_x, s_y);
            d_cell[(rotate_d_vec.y * w) + rotate_d_vec.x] = slot_cell[i];
        }
        slot_cell = d_cell.ToArray();
    }

    // true => TopDown, false => LeftRight
    public void Reversal_Block(bool b)
    {
        bool[] d_cell = slot_cell.ToArray();

        for (int i = 0; i < slot_cell.Length; i++)
        {
            int x_ = i % w;
            int y_ = Mathf.FloorToInt(i / w);
            Vector2Int rotate_d_vec = new Vector2Int(x_ - s_x, y_ - s_y);
            if (b)
            { rotate_d_vec = new Vector2Int(rotate_d_vec.x, -rotate_d_vec.y); }
            else
            { rotate_d_vec = new Vector2Int(-rotate_d_vec.x, rotate_d_vec.y); }

            rotate_d_vec += new Vector2Int(s_x,s_y);
            d_cell[(rotate_d_vec.y * w) + rotate_d_vec.x] = slot_cell[i];
        }
        slot_cell = d_cell.ToArray();
    }

    public void Select_Data(int _idx)
    {
        if(_idx < pd_arr.Length)
        {
            _data = pd_arr[_idx].data;
        }
        else
        {
            _data = new PieceDefinition();
            _data.blocks = new Vector2Int[0];
            _data.tileSprite = AssetDatabase.LoadAssetAtPath<Sprite>(default_img_path);
            _data.pieceId = "0";
        }
        isOverlap = false;
        isOverwrite = false;
        Setting_Data();
        CheckSprite(true);
    }



    public void Save_Data()
    {
        List<Vector2Int> d_blocks = new List<Vector2Int>();
        for (int i = 0; i < slot_cell.Length; i++)
        {
            if (slot_cell[i])
            {
                int y_ = Mathf.FloorToInt(i/w) - s_x;
                int x_ = (i % w) - s_y;
                Vector2Int d_vec = new Vector2Int(x_,y_);
                d_blocks.Add(d_vec);
            }
        }
        _blocks = d_blocks.ToArray();

        PieceDefinition newData = ScriptableObject.CreateInstance<PieceDefinition>();
        newData.pieceId = _pieceId;
        newData.blocks = _blocks;
        newData.dragAnchor = _origin;
        newData.tileSprite = _sprite;
        newData.tileColor = _color;
        newData.tileMaterial = _mat;
        
        Debug.Log("저장 중.");

        if (!isOverwrite)
        {
            if(file_names.Count(g => g.Equals(_name)) > 0)
            {
                isOverwrite = true;
                Debug.Log("<color=yellow>중복된 이름이 존재합니다. 덮어씌우겠습니까?</color>");
                return;
            }
            if(pd_arr.Count(g=>g.data.pieceId.Equals(_pieceId)) > 0)
            {
                isOverlap = true;
                Debug.Log("<color=red>중복된 아이디가 존재합니다. 저장이 불가능합니다.</color>");
                return;
            }

        }

        CreateData(newData);
    }


    public void CreateData(PieceDefinition d_data)
    {
        string d_name = d_data.pieceId;
        if (!string.IsNullOrEmpty(_name)) { d_name = _name; }

        string assetPath = $"{_path}/{d_name}.asset";
        AssetDatabase.CreateAsset(d_data,assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=green>저장 완료!</color>");
        isOverwrite = false;
        Get_Files();
        _data = d_data;
    }

    public void CheckSprite(bool reset)
    {
        if(pre_sprite != _sprite || reset)
        {
            PrevImage.sprite = _sprite;
            pre_sprite = _sprite;
        }
        if (pre_color != _color || reset)
        {
            PrevImage.color = _color;
            pre_color = _color;
        }
        if (pre_mat != _mat || reset)
        {
            PrevImage.material = _mat;
            pre_mat = _mat;
        }
    }

    public void Img_Enable(bool b)
    {
        PrevImage.gameObject.SetActive(b);
    }
}
