using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public int borderSize;

    public GameObject tilePrefab;
    public GameObject[] gamePiecePrefabs;

    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    // Start is called before the first frame update
    void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        SetupTiles();
        SetupCamera();
        FillRandom();
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

    void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid game piece!");
        }
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        gamePiece.SetCoord(x, y);
    }

    void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject gamePieceObject = Instantiate(GetRandomGamePiece(), new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                m_allGamePieces[i, j] = gamePieceObject.GetComponent<GamePiece>();
                PlaceGamePiece(m_allGamePieces[i, j], i, j);
            }
        }
    }



}
