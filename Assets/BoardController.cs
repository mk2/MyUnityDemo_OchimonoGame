using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{

    public GameObject BlockCherryPrefab;

    public GameObject BlockStrawberryPrefab;

    public GameObject BlockGrapePrefab;

    public GameObject BlockOrangePrefab;

    public GameObject BlockPaprikaPrefab;

    public GameObject BlockPlaceholerPrefab;

    float screenWidth;

    float screenHeight;

    float originX;

    float originY;

    float blockSize;

    const int BoardColumns = 6;

    const int BoardRows = 12;

    bool dragStart = false;

    BlockType[,] board;

    GameObject[,] boardObjects;

    const int MaxDragCount = 4;

    int dragCount = -1;

    int[,] mouseDownPosArray = new int[MaxDragCount, 2];

    // Use this for initialization
    void Start()
    {
        var bottomLeft = Camera.main.ViewportToWorldPoint(Vector3.zero);
        var topRight = Camera.main.ViewportToWorldPoint(Vector3.one);
        originX = bottomLeft.x;
        originY = bottomLeft.y;
        screenWidth = topRight.x - originX;
        screenHeight = topRight.y - originY;
        blockSize = screenWidth / BoardColumns;

        Debug.Log("originX=" + originX);
        Debug.Log("originY=" + originY);

        ResetBoard();

        for (int row = 0; row < BoardRows; row++)
        {
            for (int column = 0; column < BoardColumns; column++)
            {
                if (row < 7)
                {
                    var placeholder = Instantiate(BlockPlaceholerPrefab);
                    LocateBlock(placeholder, row, column);
                }

                var block = NewBlock(board[row, column]);
                LocateBlock(block, row, column);
            }
        }
    }

    void LocateBlock(GameObject go, int row, int col)
    {
        float x = col * blockSize + originX + blockSize / 2;
        float y = row * blockSize + originY + blockSize / 2;
        go.transform.position = new Vector3(x, y, 0f);
        var goWidth = go.GetComponent<SpriteRenderer>().bounds.size.x;
        var goHeight = go.GetComponent<SpriteRenderer>().bounds.size.y;
        float scaleX = blockSize / goWidth - 0.02f;
        float scaleY = blockSize / goHeight - 0.02f;
        go.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        boardObjects[row, col] = go;
    }

    void ResetBoard()
    {
        board = new BlockType[BoardRows, BoardColumns];
        boardObjects = new GameObject[BoardRows, BoardColumns];
        for (int row = 0; row < BoardRows; row++)
        {
            for (int col = 0; col < BoardColumns; col++)
            {
                board[row, col] = (BlockType) Mathf.RoundToInt(Random.Range(1, 5));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStart = true;
            dragCount = -1;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragStart = false;
            dragCount = -1;
        }

        if (Input.GetMouseButton(0))
        {
            var sx = Input.mousePosition.x;
            var sy = Input.mousePosition.y;
            var pos = Camera.main.ScreenToWorldPoint(new Vector3(sx, sy, 0f));
            var px = pos.x;
            var py = pos.y;
            var col = Mathf.RoundToInt((px - originX - blockSize / 2f) / blockSize);
            var row = Mathf.RoundToInt((py - originY - blockSize / 2f) / blockSize);

            if (dragCount < MaxDragCount && 
                (dragCount < 0 ||
                !(mouseDownPosArray[dragCount, 0] == col && mouseDownPosArray[dragCount, 1] == row)))
            {
                dragCount++;
                mouseDownPosArray[dragCount, 0] = col;
                mouseDownPosArray[dragCount, 1] = row;
                Debug.Log(mouseDownPosArray[dragCount, 0] + "," + mouseDownPosArray[dragCount, 1]);
            }
        }

        if (dragCount > 0)
        {
            for (int i = 0; i < dragCount; i++)
            {
                var px = mouseDownPosArray[i, 0];
                var py = mouseDownPosArray[i, 1];
                var go = boardObjects[py, px];
                var color = go.GetComponent<SpriteRenderer>().color;
                color.a = 0.5f;
                go.GetComponent<SpriteRenderer>().color = color;
            }
        }
    }

    GameObject NewBlock(BlockType blockType)
    {
        switch(blockType)
        {
            case BlockType.Cherry:
                return Instantiate(BlockCherryPrefab);
            case BlockType.Grape:
                return Instantiate(BlockGrapePrefab);
            case BlockType.Orange:
                return Instantiate(BlockOrangePrefab);
            case BlockType.Paprika:
                return Instantiate(BlockPaprikaPrefab);
            case BlockType.Strawberry:
                return Instantiate(BlockStrawberryPrefab);
            default:
                return null;
        }
    }
}

enum BlockType
{
    Empty,
    Cherry,
    Grape,
    Orange,
    Strawberry,
    Paprika
}