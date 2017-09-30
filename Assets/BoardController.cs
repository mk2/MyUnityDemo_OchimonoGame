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

    const int BoardRows = 6;

    const int BoardMaxRows = 12;

    bool dragStart = false;

    BlockType[,] board;

    GameObject[,] boardObjects;

    const int MaxDraggedBlockCount = 4;

    List<Block> draggedBlocks = new List<Block>();

    bool isProgress = false;

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
                var placeholder = ScaleBlock(Instantiate(BlockPlaceholerPrefab));
                placeholder.GetComponent<SpriteRenderer>().sortingOrder = -1;
                LocateBlock(placeholder, row, column);

                var block = ScaleBlock(NewBlock(board[row, column]));
                block.GetComponent<SpriteRenderer>().sortingOrder = 1;
                LocateBlock(block, row, column);
            }
        }
    }

    void LocateBlock(GameObject go, int row, int col)
    {
        float x = col * blockSize + originX + blockSize / 2;
        float y = row * blockSize + originY + blockSize / 2;
        go.transform.position = new Vector3(x, y, 0f);

        boardObjects[row, col] = go;
    }

    void ResetBoard()
    {
        board = new BlockType[BoardMaxRows, BoardColumns];
        boardObjects = new GameObject[BoardMaxRows, BoardColumns];
        for (int row = 0; row < BoardRows; row++)
        {
            for (int col = 0; col < BoardColumns; col++)
            {
                board[row, col] = (BlockType)Random.Range(1, 5);
                //board[row, col] = BlockType.Cherry;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isProgress && Input.GetMouseButtonDown(0))
        {
            draggedBlocks.Clear();
            dragStart = true;
        }

        if (!isProgress && Input.GetMouseButtonUp(0))
        {
            StartCoroutine(ProcessBoard());
            dragStart = false;
        }

        if (!isProgress && Input.GetMouseButton(0))
        {
            var sx = Input.mousePosition.x;
            var sy = Input.mousePosition.y;
            var pos = Camera.main.ScreenToWorldPoint(new Vector3(sx, sy, 0f));
            var px = pos.x;
            var py = pos.y;
            var col = Mathf.FloorToInt((px - originX) / blockSize);
            var row = Mathf.FloorToInt((py - originY) / blockSize);

            if (0 <= row && row < BoardRows && 0 <= col && col < BoardColumns)
            {
                var blockType = board[row, col];
                var draggedBlock = new Block { BlockType = blockType, Row = row, Col = col };

                if (draggedBlocks.Count < 1)
                {
                    draggedBlocks.Add(draggedBlock);
                    boardObjects[row, col].GetComponent<Animator>().SetTrigger("Blink");
                }
                else
                {
                    var lastDraggedBlock = draggedBlocks[draggedBlocks.Count - 1];
                    if (lastDraggedBlock.IsClosed(draggedBlock))
                    {
                        draggedBlocks.Add(draggedBlock);
                        boardObjects[row, col].GetComponent<Animator>().SetTrigger("Blink");
                    }
                }
            }
        }
    }

    void NewRandomBlock(out BlockType blockType, out GameObject block)
    {
        blockType = (BlockType)Random.Range(1, 5);
        block = NewBlock(blockType);
    }

    GameObject NewBlock(BlockType blockType)
    {
        GameObject go = null;
        switch (blockType)
        {
            case BlockType.Cherry:
                go = Instantiate(BlockCherryPrefab);
                break;
            case BlockType.Grape:
                go = Instantiate(BlockGrapePrefab);
                break;
            case BlockType.Orange:
                go = Instantiate(BlockOrangePrefab);
                break;
            case BlockType.Paprika:
                go = Instantiate(BlockPaprikaPrefab);
                break;
            case BlockType.Strawberry:
                go = Instantiate(BlockStrawberryPrefab);
                break;
        }

        return go;
    }

    GameObject ScaleBlock(GameObject go)
    {
        var goWidth = go.GetComponent<SpriteRenderer>().bounds.size.x;
        var goHeight = go.GetComponent<SpriteRenderer>().bounds.size.y;
        float scaleX = blockSize / goWidth - 0.02f;
        float scaleY = blockSize / goHeight - 0.02f;
        go.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        return go;
    }

    IEnumerator ProcessBoard()
    {
        if (isProgress)
        {
            yield break;
        }
        isProgress = true;

        yield return new WaitForSeconds(1);
        DeleteDraggedBlocks();
        do
        {
            yield return new WaitForSeconds(1);
            FallBlocks(BoardRows);
            yield return new WaitForSeconds(1);
        } while (DeleteChainBlocks());
        yield return new WaitForSeconds(1);
        FillBoard();
        yield return new WaitForSeconds(1);
        FallBlocks(BoardMaxRows);
        yield return new WaitForSeconds(1);
        while (DeleteChainBlocks())
        {
            yield return new WaitForSeconds(1);
            FillBoard();
            yield return new WaitForSeconds(1);
            FallBlocks(BoardMaxRows);
            yield return new WaitForSeconds(1);
        }

        isProgress = false;
    }

    void DeleteDraggedBlocks()
    {
        foreach (var block in draggedBlocks)
        {
            Debug.Log(block);
        }
        StartCoroutine(DestroyBlocks(draggedBlocks));
    }

    /// <summary>
    /// 消えた盤面を埋める
    /// </summary>
    void FillBoard()
    {
        // 列ごとにEmptyの数を調べ、上に並べる
        for (int col = 0; col < BoardColumns; col++)
        {
            int emptyCount = 0;
            for (int row = 0; row < BoardRows; row++)
            {
                if (board[row, col] == BlockType.Empty)
                {
                    // emptyCount分の新しいブロックを、BoardRowsより上に積み重ねる
                    BlockType blockType;
                    GameObject block;
                    NewRandomBlock(out blockType, out block);
                    block = ScaleBlock(block);
                    block.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    int newBlockRow = BoardRows + emptyCount;
                    board[newBlockRow, col] = blockType;
                    LocateBlock(block, newBlockRow, col);
                    emptyCount++;
                }
            }
        }
    }

    /// <summary>
    /// 盤面内の空中にいるブロックたちを落下させる
    /// </summary>
    void FallBlocks(int topRow)
    {
        for (int col = 0; col < BoardColumns; col++)
        {
            for (int row = 0; row < topRow; row++)
            {
                var blockType = board[row, col];
                if (blockType == BlockType.Empty)
                {
                    for (int row2 = row; row2 < topRow; row2++)
                    {
                        Debug.Log("row2=" + row2);
                        var blockType2 = board[row2, col];
                        if (blockType2 != BlockType.Empty)
                        {
                            var block = new Block { BlockType = blockType2, Row = row2, Col = col };
                            MoveBlock(block, row, col);
                            break;
                        }
                    }
                }
            }
        }
    }

    void MoveBlock(Block srcBlock, int tgtRow, int tgtCol)
    {
        board[tgtRow, tgtCol] = srcBlock.BlockType;
        board[srcBlock.Row, srcBlock.Col] = BlockType.Empty;
        var go = boardObjects[srcBlock.Row, srcBlock.Col];
        if (go != null)
        {
            Debug.Log("row=" + tgtRow + " col=" + tgtCol);
            LocateBlock(go, tgtRow, tgtCol);
        }
    }

    /// <summary>
    /// 盤面で消せそうなものを消す
    /// </summary>
    bool DeleteChainBlocks()
    {
        bool isUpdated = false;

        var chainBlockCandidateDict = new Dictionary<int, Block>();

        // 右下から1行ずつ連鎖チェックしていく(連鎖ブロック候補はマーキングすること)
        for (int row = 0; row < BoardRows; row++)
        {
            for (int col = 0; col < BoardColumns; col++)
            {
                int idx = row * BoardColumns + col;
                // 既に連鎖ブロック候補としてマーキングされていたらスキップする
                if (chainBlockCandidateDict.ContainsKey(idx))
                {
                    continue;
                }

                // 上下左右をチェックし、同タイプのブロックがあったら連鎖候補としてマーキング
                var checkList = new int[,]
                {
                    { 0, 1 }, // 上
                    { 1, 0 }, // 右
                    { 0, -1 },// 下
                    { -1, 0 } // 左
                };
                for (int i = 0; i < 4; i++)
                {
                    BlockType myBlockType = board[row, col];
                    if (myBlockType == BlockType.Empty)
                    {
                        continue;
                    }

                    int upX = col + checkList[i, 0];
                    int upY = row + checkList[i, 1];
                    if (0 <= upX && upX < BoardColumns && 0 <= upY && upY < BoardColumns)
                    {
                        BlockType yourBlockType = board[upY, upX];
                        if (myBlockType == yourBlockType)
                        {
                            chainBlockCandidateDict.Add(idx, new Block { BlockType = myBlockType, Row = row, Col = col });
                            break;
                        }
                    }
                }
            }
        }

        // 連鎖候補ブロックから、連鎖を抽出する
        // とりあえずBlockTypeごとに検出する
        var chainGroup = new ChainGroup();
        foreach (var entry in chainBlockCandidateDict)
        {
            var block = entry.Value;
            chainGroup.AddToChain(block);
            Debug.Log("candidate=" + block);
        }

        // 抽出した連鎖を消す
        foreach (var chain in chainGroup.UnionChains())
        {
            Debug.Log("NEW CHAIN: " + chain.Count);
            if (chain.Count < 4)
            {
                continue;
            }

            foreach (var block in chain.Blocks)
            {
                var px = block.Col;
                var py = block.Row;
                boardObjects[py, px].GetComponent<Animator>().SetTrigger("Rainbow");
            }
            StartCoroutine(DestroyBlocks(chain.Blocks));

            isUpdated = true;
        }

        return isUpdated;
    }

    IEnumerator DestroyBlocks(List<Block> blocks)
    {
        yield return new WaitForSeconds(1);
        foreach (var block in blocks)
        {
            var row = block.Row;
            var col = block.Col;
            board[row, col] = BlockType.Empty;
            Destroy(boardObjects[row, col]);
        }
    }
}

class ChainGroup
{
    public List<Chain> chainList = new List<Chain>();

    public List<Chain> Chains
    {
        get { return chainList; }
    }

    public void AddToChain(Block block)
    {
        // 既存の連鎖に追加できないかチェックし、追加できなければ新しく連鎖を作る
        bool isAdded = false;
        foreach (var chain in chainList)
        {
            if (chain.BlockType != block.BlockType)
            {
                continue;
            }

            isAdded |= chain.AddBlockIfChained(block);
        }

        if (!isAdded)
        {
            var chain = new Chain
            {
                BlockType = block.BlockType
            };
            chain.AddBlock(block);
            chainList.Add(chain);
        }
    }

    public List<Chain> UnionChains()
    {
        var unionChains = new List<Chain>();
        foreach (var myChain in chainList)
        {
            foreach (var otherChain in chainList)
            {
                if (myChain.IsOverlapped(otherChain))
                {
                    var chain = myChain.Union(otherChain);
                    unionChains.Add(chain);
                }
            }
        }
        return unionChains;
    }

    public List<Chain> ValidChains()
    {
        var validChains = new List<Chain>();
        foreach (var chain in chainList)
        {
            if (chain.Count > 3)
            {
                validChains.Add(chain);
            }
        }
        return validChains;
    }
}

class Chain
{
    public BlockType BlockType { get; set; }

    List<Block> blocks = new List<Block>();

    public List<Block> Blocks
    {
        get { return blocks; }
    }

    public int Count
    {
        get { return blocks.Count; }
    }

    public void Clear()
    {
        blocks.Clear();
    }

    public void AddBlock(Block block)
    {
        blocks.Add(block);
    }

    public bool AddBlockIfChained(Block block)
    {
        bool isChained = IsChained(block);
        if (isChained)
        {
            blocks.Add(block);
        }
        return isChained;
    }

    public Chain Union(Chain otherChain)
    {
        var chain = new Chain();
        chain.blocks.AddRange(blocks);
        foreach (var otherBlock in otherChain.blocks)
        {
            bool isContained = false;
            foreach (var myBlock in chain.blocks)
            {
                if (myBlock.Equals(otherBlock))
                {
                    isContained = true;
                    break;
                }
            }
            if (!isContained)
            {
                chain.blocks.Add(otherBlock);
            }
        }
        return chain;
    }

    public bool IsOverlapped(Chain other)
    {
        bool isOverlapped = false;
        foreach (var myBlock in blocks)
        {
            foreach (var otherBlock in other.blocks)
            {
                isOverlapped = myBlock.Equals(otherBlock);
                if (isOverlapped)
                {
                    goto FinishOverlapCheck;
                }
            }
        }
        FinishOverlapCheck:
        return isOverlapped;
    }

    public bool IsChained(Block other)
    {
        bool isChained = false;

        foreach (var block in blocks)
        {
            isChained = block.IsClosed(other);
            if (isChained)
            {
                break;
            }
        }

        return isChained;
    }

}

struct Block
{
    public BlockType BlockType { get; set; }

    public int Row { get; set; }

    public int Col { get; set; }

    public bool IsClosed(Block other)
    {
        int distance = Mathf.Abs(this.Row - other.Row) + Mathf.Abs(this.Col - other.Col);
        return distance == 1;
    }

    public override string ToString()
    {
        return "type=" + BlockType + " Row=" + Row + " Col=" + Col;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Block))
        {
            return false;
        }
        Block other = (Block)obj;
        return Row == other.Row && Col == other.Col;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
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