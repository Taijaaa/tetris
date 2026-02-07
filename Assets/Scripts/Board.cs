using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public Tilemap tilemap;
    public Piece prefabPiece;

    public TetronimoData[] tetronimos;

    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector2Int startPosition = new Vector2Int(-1, 8);

    public float dropInterval = 0.5f;

    float dropTime = 0.0f;

    // this the sequence of how the tets should fall
    Tetronimo[] Sequence = { Tetronimo.U, Tetronimo.L, Tetronimo.L, Tetronimo.T, Tetronimo.O, Tetronimo.I, Tetronimo.U };
    int currentPiece = 0;

    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();
    Piece activePiece;

    // tiles that were already set 
    private Dictionary<Vector3Int, TileBase> initialTiles = new Dictionary<Vector3Int, TileBase>();
    private BoundsInt initialBounds;

    int left { get { return -boardSize.x / 2; } }
    int right { get { return boardSize.x / 2; } }
    int bottom { get { return -boardSize.y / 2; } }
    int top { get { return boardSize.y / 2; } }

    private void Awake()
    {
        CacheInitialTiles();
    }

    private void Update()
    {
        if (tetrisManager.gameOver) return;

        dropTime += Time.deltaTime;

        if (dropTime >= dropInterval)
        {
            dropTime = 0.0f;

            Clear(activePiece);
            bool moveResult = activePiece.Move(Vector2Int.down);
            Set(activePiece);

            if (!moveResult)
            {
                activePiece.freeze = true;
                CheckBoard();
                SpawnPiece();
            }
        }
    }
    // this save my preset tiles so they always come back
    private void CacheInitialTiles()
    {
        initialTiles.Clear();
        initialBounds = tilemap.cellBounds;

        foreach (var pos in initialBounds.allPositionsWithin)
        {
            TileBase t = tilemap.GetTile(pos);
            if (t != null)
                initialTiles[pos] = t;
        }
    }
    // this brings them back
    private void RestoreInitialTiles()
    {
        tilemap.ClearAllTiles();

        foreach (var kvp in initialTiles)
            tilemap.SetTile(kvp.Key, kvp.Value);
    }

    public void SpawnPiece()
    {
        if (currentPiece >= Sequence.Length)
            currentPiece = 0;

        activePiece = Instantiate(prefabPiece);
        activePiece.Initialize(this, Sequence[currentPiece]);

        currentPiece++;

        CheckEndGame();
        Set(activePiece);
    }

    void CheckEndGame()
    {
        if (!IsPositionValid(activePiece, activePiece.position))
        {
            tetrisManager.SetGameOver(true);
        }
    }

    // this restart 
    public void UpdateGameOver()
    {
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    void ResetBoard()
    {
        
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        activePiece = null;

        
        RestoreInitialTiles();

        pieces.Clear();

       
        currentPiece = 0;

        SpawnPiece();
    }

    void SetTile(Vector3Int cellPosition, Piece piece)
    {
        if (piece == null)
        {
            tilemap.SetTile(cellPosition, null);
            pieces.Remove(cellPosition);
        }
        else
        {
            tilemap.SetTile(cellPosition, piece.data.tile);
            pieces[cellPosition] = piece;
        }
    }

    public void Set(Piece piece)
    {
        if (piece == null) return;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, piece);
        }
    }

    public void Clear(Piece piece)
    {
        if (piece == null) return;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }

    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top)
            {
                return false;
            }

            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            if (!tilemap.HasTile(cellPosition))
            {
                return false;
            }
        }
        return true;
    }

    void DestroyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y, 0);

            if (pieces.ContainsKey(cellPosition))
            {
                Piece piece = pieces[cellPosition];
                piece.ReduceActiveCount();
                SetTile(cellPosition, null);
            }
            else
            {
                tilemap.SetTile(cellPosition, null);
            }
        }
    }

    void ShiftsRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                if (pieces.ContainsKey(cellPosition))
                {
                    Piece currentPieceRef = pieces[cellPosition];

                    SetTile(cellPosition, null);

                    cellPosition.y -= 1;
                    SetTile(cellPosition, currentPieceRef);
                }
                else
                {
                    TileBase currentTile = tilemap.GetTile(cellPosition);
                    tilemap.SetTile(cellPosition, null);
                    cellPosition.y -= 1;
                    tilemap.SetTile(cellPosition, currentTile);
                }
            }
        }
    }

    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();

        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        int rowsShifted = 0;
        foreach (int y in destroyedLines)
        {
            ShiftsRowsDown(y - rowsShifted);
            rowsShifted++;
        }

        int scoreToAdd = tetrisManager.CalculateScore(destroyedLines.Count);
        tetrisManager.ChangeScore(scoreToAdd);
    }
}
