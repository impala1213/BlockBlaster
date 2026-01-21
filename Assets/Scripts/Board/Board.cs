using UnityEngine;

public sealed class Board
{
    public const int GridWidth = 8;
    public const int GridHeight = 8;

    private readonly CellState[,] cells = new CellState[GridWidth, GridHeight];

    public Board()
    {
        ClearAll();
    }

    public bool IsInBounds(Vector2Int p) // 8by8 안에 있는지 체크
    {
        return p.x >= 0 && p.x < GridWidth && p.y >= 0 && p.y < GridHeight;
    }

    public CellState GetCell(Vector2Int p) //cell 정보 가져오기
    {
        return cells[p.x, p.y];
    }

    public void SetCell(Vector2Int p, CellState state) // cell 설정
    {
        cells[p.x, p.y] = state;
    }

    public void ClearAll() //board 초기화
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                cells[x, y] = CellState.Empty;
            }
        }
    }
}
