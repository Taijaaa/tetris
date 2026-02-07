using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TetronimoData data;
    public Board board;
    public Vector2Int[] cells;

 

    public Vector2Int position;

    public bool freeze = false;

    int activeCellCount = -1;

    public void Initialize(Board board, Tetronimo tetronimo)
    {
        
        this.board = board;

        
        for (int i = 0; i < board.tetronimos.Length; i++)
        {
            if (board.tetronimos[i].tetronimo == tetronimo)
            {
                this.data = board.tetronimos[i];
                break;
            }
        }

        
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0; i < data.cells.Length; i++)
        {
            cells[i] = data.cells[i];
        }

       
        position = board.startPosition;

        activeCellCount = cells.Length;
    }

    private void Update()
    {
        if (board.tetrisManager.gameOver) return;

        if (freeze) return;

        board.Clear(this);

        //this movement

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }
          

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Rotate(1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Rotate(-1);
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                board.CheckBoard();
            }
        }


        board.Set(this);

        
        if (freeze)
        {
            board.CheckBoard();
            board.SpawnPiece();
        }
    }

    void Rotate(int direction)
    {
        Vector2Int[] originalCells = new Vector2Int[cells.Length];
        for (int i = 0; i < cells.Length; i++) originalCells[i] = cells[i];

        ApplyRotation(direction);

        if (!board.IsPositionValid(this, position))
        {
            if (!TryWallKicks()) RevertRotation(originalCells);
        }
    }

    void RevertRotation(Vector2Int[] originalCells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = originalCells[i];
        }
    }
    // do a flip
    bool TryWallKicks()
    {
        List<Vector2Int> wallKickOffsets = new List<Vector2Int>
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            new Vector2Int(-1, -1), 
            new Vector2Int(1, -1) 
        };

       
        if (data.tetronimo == Tetronimo.I)
        {
            wallKickOffsets.Add(new Vector2Int(-2, 0)); 
            wallKickOffsets.Add(new Vector2Int(2, 0)); 
        }

        foreach (Vector2Int offset in wallKickOffsets)
        {
            if (Move(offset)) return true;
        }

        return false;
    }

    // do a spin
    void ApplyRotation(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, direction * 90);

        for (int i = 0; i < cells.Length; i++)
        {
           
            Vector3 cellPosition = new Vector3(cells[i].x, cells[i].y);

            
            bool isSpecial = data.tetronimo == Tetronimo.O || data.tetronimo == Tetronimo.I;
            if (isSpecial)
            {
                cellPosition -= new Vector3(0.5f, 0.5f);
            }

           
            Vector3 result = rotation * cellPosition;

            if (isSpecial)
            {
                cells[i] = new Vector2Int(Mathf.CeilToInt(result.x), Mathf.CeilToInt(result.y));
            }
            else
            {
                cells[i] = new Vector2Int(Mathf.RoundToInt(result.x), Mathf.RoundToInt(result.y));
            }
        }
    }

    void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            
        }

        freeze = true;
    }

    public bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position + translation;

        bool positionIsValid = board.IsPositionValid(this, newPosition);

        if (positionIsValid)
        {
            position += translation;
        }

        return positionIsValid;
    }

    public void ReduceActiveCount()
    {
        activeCellCount--;
        if (activeCellCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}