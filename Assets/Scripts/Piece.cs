using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{

    public bool isWhite; //white = true
    public bool isQueen;

    public bool IsForceToMove(Piece[,] board, int x, int y)
    {
        if (isWhite || isQueen)
        {
            //? Top left
            if (x >= 2 && y <= 5)
            { //? if there is a piece, and it is not the same color as ours
                Piece p = board[x - 1, y + 1];
                if (p != null && p.isWhite != isWhite)
                {//? check if its possible to land after the jump
                    if (board[x - 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
            //? Top Right
            if (x <= 5 && y <= 5)
            { //? if there is a piece, and it is not the same color as ours
                Piece p = board[x + 1, y + 1];
                if (p != null && p.isWhite != isWhite)
                {//? check if its possible to land after the jump
                    if (board[x + 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        if (!isWhite || isQueen)
        {
            //? Bot left
            if (x >= 2 && y >= 2)
            { //? if there is a piece, and it is not the same color as ours
                Piece p = board[x - 1, y - 1];
                if (p != null && p.isWhite != isWhite)
                {//? check if its possible to land after the jump
                    if (board[x - 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
            //? Bot Right
            if (x <= 2 && y >= 2)
            { //? if there is a piece, and it is not the same color as ours
                Piece p = board[x + 1, y - 1];
                if (p != null && p.isWhite != isWhite)
                {//? check if its possible to land after the jump
                    if (board[x + 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    internal bool ValidMove(Piece[,] board, int startX, int startY, int endX, int endY)
    {
        //? if moving on top of another piece
        if (board[endX, endY] != null)
            return false;

        int deltaMoveX = Mathf.Abs(startX - endX);
        int deltaMoveY = Mathf.Abs(endY - startY);
        Debug.Log("deltaMoveX: " + deltaMoveX);
        Debug.Log("deltaMoveY: " + deltaMoveY);
        Debug.Log("isWhite: " + isWhite);
        Debug.Log("isQueen: " + isQueen);


        if (isWhite || isQueen)
        {
            Debug.Log("white move");

            if (deltaMoveX == 1)
            {
                if (deltaMoveY == 1)
                {
                    return true;
                }
            }
            else if (deltaMoveX == 2)
            {
                if (deltaMoveY == 2)
                {
                    Piece p = board[(startX + endX) / 2, (startY + endY) / 2];
                    if (p != null && p.isWhite != isWhite)
                    {
                        Debug.Log("killed Piece ", p);
                        return true;
                    }
                }
            }
        }

        if (isWhite == false || isQueen)
        {
            Debug.Log("black move");

            if (deltaMoveX == 1)
            {
                if (deltaMoveY == 1)
                {
                    return true;
                }
            }
            else if (deltaMoveX == 2)
            {
                if (deltaMoveY == 2)
                {
                    Piece p = board[(startX + endX) / 2, (startY + endY) / 2];
                    if (p != null && p.isWhite != isWhite)
                    {
                        Debug.Log("killed Piece ", p);
                        return true;
                    }
                }
            }

        }

        Debug.Log("no condition met, return false");
        return false;
    }
}
