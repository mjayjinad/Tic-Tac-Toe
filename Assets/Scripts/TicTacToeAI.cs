
/// <summary>
/// <name>Abdulmaliq Jinad</name>
/// <date>{03-06-2025}</date>
/// For my AI, I am going with a typical human approach where you try to select the center or corner 
/// cells first as they give the most opportunities to form a winning line
/// </summary>
/// 

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState 
{ 
    none, 
    cross, 
    circle 
}

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{
    private int _aiLevel;
    TicTacToeState[,] boardState;

    [SerializeField]
    private bool _isPlayerTurn;

    [SerializeField]
    private int _gridSize = 3;

    [SerializeField]
    private TicTacToeState playerState = TicTacToeState.cross;
    private TicTacToeState aiState = TicTacToeState.circle;

    [SerializeField]
    private GameObject _xPrefab;

    [SerializeField]
    private GameObject _oPrefab;

    private ClickTrigger[,] _triggers;

    public UnityEvent onGameStarted;
    public WinnerEvent onPlayerWin;


    private static readonly (int, int)[] _cornerMoves =
    {
        (0, 0), (0, 2), (2, 0), (2, 2)
    };


    private void Awake()
    {
        if (onPlayerWin == null)
        {
            onPlayerWin = new WinnerEvent();
        }
    }

    /// <summary>
    /// Initializes the game and stors the gamemode selected by the user
    /// </summary>
    /// <param name="AILevel"></param>
    public void StartAI(int AILevel)
    {
        _aiLevel = AILevel;
        StartGame();
    }

    /// <summary>
    /// Initializes the game logic
    /// </summary>
    private void StartGame()
    {
        _triggers = new ClickTrigger[3, 3];
        boardState = new TicTacToeState[3, 3];
        onGameStarted.Invoke();
        _isPlayerTurn = true;
    }

    /// <summary>
    /// Stores a reference to where each cell is located on the board
    /// </summary>
    /// <param name="myCoordX"></param>
    /// <param name="myCoordY"></param>
    /// <param name="clickTrigger"></param>
    public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
    {
        _triggers[myCoordX, myCoordY] = clickTrigger;
    }

    /// <summary>
    /// Called when player makes a move on the board
    /// </summary>
    /// <param name="coordX"></param>
    /// <param name="coordY"></param>
    public void PlayerSelects(int coordX, int coordY)
    {
        if (!_isPlayerTurn || boardState[coordX, coordY] != TicTacToeState.none)
            return;

        SetVisual(coordX, coordY, playerState);
        boardState[coordX, coordY] = playerState;
        _isPlayerTurn = false;

        if (CheckForWin(playerState))
        {
            onPlayerWin.Invoke(0);
            return;
        }
        if (IsBoardFull())
        {
            onPlayerWin.Invoke(-1);
            return;
        }

        StartCoroutine(AiMove());
    }

    /// <summary>
    /// Wait time to simulate the AI thinking time
    /// </summary>
    /// <returns></returns>
    private IEnumerator AiMove()
    {
        yield return new WaitForSeconds(1);

        if(_aiLevel == 0)
            AiSelects(GetBestMoveForEasyMode());
        else if (_aiLevel == 1)
            AiSelects(GetBestMoveForHardMode());
    }

    /// <summary>
    /// Makes a move for the AI
    /// </summary>
    /// <param name="move"></param>
    public void AiSelects((int, int) move)
    {
        SetVisual(move.Item1, move.Item2, aiState);
        boardState[move.Item1, move.Item2] = aiState;
        _isPlayerTurn = true;

        if (CheckForWin(aiState))
        {
            onPlayerWin.Invoke(1);
            return;
        }
        if (IsBoardFull())
        {
            onPlayerWin.Invoke(-1);
            return;
        }
    }

    /// <summary>
    /// Places the right prefab for visual representation when a move is made by player or AI
    /// </summary>
    /// <param name="coordX"></param>
    /// <param name="coordY"></param>
    /// <param name="targetState"></param>
    private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
    {
        Instantiate(
            targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
            _triggers[coordX, coordY].transform.position,
            Quaternion.identity
        );
    }

    /// <summary>
    /// Checks through a list of possible win conditions for both Player and AI
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private bool CheckForWin(TicTacToeState state)
    {
        //Check if win condition is true for horizontal or vertical cells
        for (int i = 0; i < _gridSize; i++)
        {
            if (boardState[i, 0] == state && boardState[i, 1] == state && boardState[i, 2] == state)
                return true;
            if (boardState[0, i] == state && boardState[1, i] == state && boardState[2, i] == state)
                return true;
        }

        //Check if win condition is tru for diagonal cells
        if (boardState[0, 0] == state && boardState[1, 1] == state && boardState[2, 2] == state)
            return true;
        if (boardState[0, 2] == state && boardState[1, 1] == state && boardState[2, 0] == state)
            return true;

        return false;
    }

    /// <summary>
    /// Check if there are no empty slots on the board for a move
    /// </summary>
    /// <returns></returns>
    private bool IsBoardFull()
    {
        foreach (var cell in boardState)
        {
            if (cell == TicTacToeState.none)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns the best possible move for the AI in easy gamemode.
    /// </summary>
    /// <returns></returns>
    private (int, int) GetBestMoveForEasyMode()
    {
        //Check if AI can win
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (boardState[i, j] == TicTacToeState.none)
                {
                    boardState[i, j] = aiState;
                    if (CheckForWin(aiState))
                    {
                        boardState[i, j] = TicTacToeState.none;
                        return (i, j);
                    }
                    boardState[i, j] = TicTacToeState.none;
                }
            }
        }

        //Randomly decide whether to prioritize corners or center first to avoid prediction
        int randomValue = Random.Range(0, 2);
        bool prioritizeCorners = randomValue > 0.5f ? true : false;

        // Shuffle the corner moves to avoid prediction
        List<(int, int)> shuffledCorners = _cornerMoves.OrderBy(x => Random.value).ToList();

        if (prioritizeCorners)
        {
            // Try picking a corner first
            foreach (var move in shuffledCorners)
            {
                if (boardState[move.Item1, move.Item2] == TicTacToeState.none)
                    return move;
            }

            // If no corners are available, try picking the center if available
            if (boardState[1, 1] == TicTacToeState.none)
                return (1, 1);
        }
        else
        {
            // Try picking the center first
            if (boardState[1, 1] == TicTacToeState.none)
                return (1, 1);

            // If center is not available, try picking the corners
            foreach (var move in shuffledCorners)
            {
                if (boardState[move.Item1, move.Item2] == TicTacToeState.none)
                    return move;
            }
        }

        // Collect all available cells in boardstate
        List<(int, int)> availableMoves = new List<(int, int)>();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (boardState[i, j] == TicTacToeState.none)
                {
                    availableMoves.Add((i, j));
                }
            }
        }

        // Shuffle the available cells
        availableMoves = availableMoves.OrderBy(_ => Random.value).ToList();

        // Return a move from the shuffled list
        return availableMoves[0];
    }

    /// <summary>
    /// Returns the best possible move for the AI in hard gamemode.
    /// </summary>
    /// <returns></returns>
    private (int, int) GetBestMoveForHardMode()
    {
        //Check if AI can win
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (boardState[i, j] == TicTacToeState.none)
                {
                    boardState[i, j] = aiState;
                    if (CheckForWin(aiState))
                    {
                        boardState[i, j] = TicTacToeState.none;
                        return (i, j);
                    }
                    boardState[i, j] = TicTacToeState.none;
                }
            }
        }

        //Check if AI needs to block the player
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (boardState[i, j] == TicTacToeState.none)
                {
                    boardState[i, j] = playerState;
                    if (CheckForWin(playerState))
                    {
                        boardState[i, j] = TicTacToeState.none;
                        return (i, j);
                    }
                    boardState[i, j] = TicTacToeState.none;
                }
            }
        }

        //Randomly decide whether to prioritize corners or center first to avoid prediction
        int randomValue = Random.Range(0, 2);
        bool prioritizeCorners = randomValue > 0.5f? true : false;
        Debug.Log("randome value: " + randomValue);

        // Shuffle the corner moves to avoid prediction
        List<(int, int)> shuffledCorners = _cornerMoves.OrderBy(x => Random.value).ToList();

        if (prioritizeCorners)
        {
            // Try picking a corner first
            foreach (var move in shuffledCorners)
            {
                if (boardState[move.Item1, move.Item2] == TicTacToeState.none)
                    return move;
            }

            // If no corners are available, try picking the center if available
            if (boardState[1, 1] == TicTacToeState.none) 
                return (1, 1);
        }
        else
        {
            // Try picking the center first
            if (boardState[1, 1] == TicTacToeState.none) 
                return (1, 1);

            // If center is not available, try picking the corners
            foreach (var move in shuffledCorners)
            {
                if (boardState[move.Item1, move.Item2] == TicTacToeState.none)
                    return move;
            }
        }

        // Collect all available cells in boardstate
        List<(int, int)> availableMoves = new List<(int, int)>();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (boardState[i, j] == TicTacToeState.none)
                {
                    availableMoves.Add((i, j));
                }
            }
        }

        // Shuffle the available cells
        availableMoves = availableMoves.OrderBy(_ => Random.value).ToList();

        // Return a move from the shuffled list
        return availableMoves[0];
    }
}
