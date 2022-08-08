using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public int borderSize;

    public GameObject tilePrefab;
    public GameObject[] gamePiecePrefabs;

    public float swapTime = 0.3f;

    // Static: does not change throughout the game.
    Tile[,] m_allTiles;
    // Dynamic: updates as pieces move
    GamePiece[,] m_allGamePieces;

    Tile m_clickedTile;
    Tile m_targetTile;

    // Start is called before the first frame update
    void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        SetupTiles();
        SetupCamera();
        FillBoard();
        // HighlightMatches();
    }

    void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                tile.name = "Tile (" + i + "," + j + ")";

                m_allTiles[i, j] = tile.GetComponent<Tile>();
                m_allTiles[i, j].Init(i, j, this);

                // So that if we move the board the tiles will move with it? And to keep the hierarchy tidy
                tile.transform.parent = transform;
            }
        }
    }

    void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)((width - 1) / 2f), (float)((height - 1) / 2f), -10f);

        float ortho_horizontal = ((float)width / 2 + (float)borderSize) / Camera.main.aspect;
        float ortho_vertical = ((float)height / 2) + (float)borderSize;

        Camera.main.orthographicSize = ortho_horizontal > ortho_vertical ? ortho_horizontal : ortho_vertical;

    }

    GameObject GetRandomGamePiece()
    {
        int randomIndex = Random.Range(0, gamePiecePrefabs.Length);
        if (gamePiecePrefabs[randomIndex] == null)
        {
            Debug.LogWarning("Oops, you haven't allocated a valid gamepiece prefab to " + randomIndex);
        }
        return gamePiecePrefabs[randomIndex];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid game piece!");
        }
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;

        if (IsWithinBounds(x, y))
        {
            m_allGamePieces[x, y] = gamePiece;
        }

        gamePiece.SetCoord(x, y);
    }

    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    void FillBoard()
    {
        int maxIterations = 100;
        int iterations = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GamePiece piece = FillRandomAt(i, j);
                iterations = 0;

                while (HasMatchOnFill(i, j))
                {
                    ClearPieceAt(i, j);
                    piece = FillRandomAt(i, j);

                    iterations++;
                    if (iterations > maxIterations)
                    {
                        Debug.Log("Couldn't place game piece without creating a match...");
                        break;
                    }
                }

            }
        }
    }

    GamePiece FillRandomAt(int x, int y)
    {
        GameObject randomPiece = Instantiate(GetRandomGamePiece(), new Vector3(x, y, 0), Quaternion.identity) as GameObject;

        if (randomPiece)
        {
            randomPiece.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), x, y);
            randomPiece.transform.parent = transform;
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }


    public void ClickTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
            // Debug.Log("Clicked tile " + tile);
        }
    }

    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(m_clickedTile, tile))
        {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }

        m_clickedTile = null;
        m_targetTile = null;
    }

    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));

    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

        if (clickedPiece != null && targetPiece != null)
        {
            clickedPiece.Move(targetPiece.xIndex, targetPiece.yIndex, swapTime);
            targetPiece.Move(clickedPiece.xIndex, clickedPiece.yIndex, swapTime);

            yield return new WaitForSeconds(swapTime);

            List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedPiece.xIndex, clickedPiece.yIndex);
            List<GamePiece> targetPieceMatches = FindMatchesAt(targetPiece.xIndex, targetPiece.yIndex);

            ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());

            // ClearPieceAt(clickedPieceMatches);
            // ClearPieceAt(targetPieceMatches);

            // CollapseColumn(clickedPieceMatches);
            // CollapseColumn(targetPieceMatches);
            //HighlightMatchesAtPosition(clickedTile.xIndex, clickedTile.yIndex);
            //HighlightMatchesAtPosition(targetTile.xIndex, targetTile.yIndex);
        }

    }

    bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDir, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        if (!IsWithinBounds(startX, startY))
        {
            return null;
        }

        GamePiece startPiece = m_allGamePieces[startX, startY];

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }

        int maxSearch = width > height ? width : height;

        int nextX;
        int nextY;

        for (int i = 1; i < maxSearch - 1; i++)
        {
            nextX = startX + (int)(searchDir.x * i);
            nextY = startY + (int)(searchDir.y * i);

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = m_allGamePieces[nextX, nextY];

            if (nextPiece != null && nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }
        }

        if (matches.Count >= minLength)
        {
            return matches;
        }
        return null;
    }

    bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        Vector2 downDir = new Vector2(0, -1);
        List<GamePiece> downwardMatches = FindMatches(x, y, downDir, minLength);
        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        Vector2 leftDir = new Vector2(-1, 0);
        List<GamePiece> leftMatches = FindMatches(x, y, leftDir, minLength);
        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        return (downwardMatches.Count > 0 || leftMatches.Count > 0);
    }


    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        Vector2 upDir = new Vector2(0, 1);
        List<GamePiece> upwardMatches = FindMatches(startX, startY, upDir, 2);
        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        Vector2 downDir = new Vector2(0, -1);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, downDir, 2);
        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return combinedMatches.Count >= minLength ? combinedMatches : null;

    }

    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        Vector2 leftDir = new Vector2(-1, 0);
        List<GamePiece> leftMatches = FindMatches(startX, startY, leftDir, 2);
        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        Vector2 rightDir = new Vector2(1, 0);
        List<GamePiece> rightMatches = FindMatches(startX, startY, rightDir, 2);
        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        var combinedMatches = leftMatches.Union(rightMatches).ToList();

        return combinedMatches.Count >= minLength ? combinedMatches : null;

    }

    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAtPosition(i, j);
            }
        }
    }

    void HighlightMatchesAtPosition(int x, int y)
    {
        HighlightTileOff(x, y);

        List<GamePiece> combinedMatches = FindMatchesAt(x, y);

        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece match in combinedMatches)
            {
                HighlightTileOn(match.xIndex, match.yIndex, match.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                HighlightTileOn(gamePiece.xIndex, gamePiece.yIndex, gamePiece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightTileOn(int x, int y, Color col)
    {
        SpriteRenderer sr = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        sr.color = col;
    }

    void HighlightTileOff(int x, int y)
    {
        SpriteRenderer sr = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
    }

    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);
        if (vertMatches == null)
        {
            vertMatches = new List<GamePiece>();
        }

        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        return combinedMatches;
    }

    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece gamePiece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(gamePiece.xIndex, gamePiece.yIndex, minLength)).ToList();
        }
        return matches;
    }

    void ClearPieceAt(int x, int y)
    {
        GamePiece gamePiece = m_allGamePieces[x, y];
        if (gamePiece != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(gamePiece.gameObject);
        }
        HighlightTileOff(x, y);
    }

    void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece gp in gamePieces)
        {
            if (gp != null)
            {
                ClearPieceAt(gp.xIndex, gp.yIndex);
            }
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] != null)
                    {
                        m_allGamePieces[column, j].Move(column, i, collapseTime);
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);

                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }

                        m_allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);
        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }
        return movingPieces;

    }

    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (!columns.Contains(gamePiece.xIndex))
            {
                columns.Add(gamePiece.xIndex);
            }
        }
        return columns;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndCollapseRoutine(gamePieces));
        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        HighlightPieces(gamePieces);

        yield return new WaitForSeconds(0.25f);

        bool isFinished = false;
        while (!isFinished)
        {
            ClearPieceAt(gamePieces);

            yield return new WaitForSeconds(0.25f);
            movingPieces = CollapseColumn(gamePieces);
            yield return new WaitForSeconds(0.25f);

            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }
}
