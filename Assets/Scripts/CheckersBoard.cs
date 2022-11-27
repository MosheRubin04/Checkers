using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckersBoard : MonoBehaviour
{

    public static CheckersBoard Instance { set; get; }
    public Piece[,] pieces = new Piece[8, 8];


    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;


    [SerializeField] private bool isWhiteTurn;
    [SerializeField] private bool isWhite;
    [SerializeField] private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    [SerializeField] private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);
    [SerializeField] private float rayLength = 25.0f;

    private Piece selectedPiece;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private bool hasKilled;
    private Vector2 mouseOver;
    private List<Piece> forcedPieces;

    private Client client;

    private void Start()
    {
        Instance = this;
        client = FindObjectOfType<Client>();
        isWhite = client.isHost;
        isWhiteTurn = true;

        forcedPieces = new List<Piece>();
        GenerateBoard();
    }

    private void Update()
    {
        UpdateMouseOver();

        if ((isWhite) ? isWhiteTurn : !isWhiteTurn)
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;


            if (selectedPiece != null)
                UpdatePiecePosition(selectedPiece);

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Click");
                SelectPiece(x, y);
            }

            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Release");
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
            }
        }
    }

    private List<Piece> ScanForPossibleMove(Piece p, int x, int y)
    {
        Debug.Log("ScanForPossibleMove called with piece ", p);
        if (p.IsForceToMove(pieces, x, y))
            forcedPieces.Add(pieces[x, y]);
        Debug.Log("forcedPieces: " + forcedPieces);
        return forcedPieces;
    }
    private List<Piece> ScanForPossibleMove()
    {
        forcedPieces = new List<Piece>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece currentPiece = pieces[i, j];
                if (currentPiece != null && currentPiece.isWhite == isWhiteTurn)
                    if (currentPiece.IsForceToMove(pieces, i, j))
                        forcedPieces.Add(pieces[i, j]);
            }
        }
        Debug.Log("forcedPieces: " + forcedPieces);
        return forcedPieces;
    }
    public void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = ScanForPossibleMove();
        //? Multiplayer Support
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        Debug.Log("TryMove from start: " + startDrag);
        Debug.Log("to end: " + endDrag);

        //? check if we are out of bound
        if (IsOutOfBound(x2, y2))
        {
            if (selectedPiece != null)
                MovePiece(selectedPiece, x1, y1);

            ResetSelectedPiece();
            Debug.Log("Out of Bound - return");
            return;
        }
        Debug.Log("Inside Bound ");

        //? check if selectedPiece is valid
        if (selectedPiece != null)
        {
            Debug.Log("Found selected piece ", selectedPiece);
            //? didn't move
            if (endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                ResetSelectedPiece();
                Debug.Log("Didn't move - return");
                return;
            }

            //? Check if valid move
            if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                Debug.Log("Move is valid");

                //? did we kill any pieces
                //? if this is a jump
                if (Mathf.Abs(x2 - x1) == 2)
                {
                    Debug.Log("Move is Jump");
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p.gameObject == null)
                    {
                        Debug.Log("Didn't Destroy p ", p.gameObject);
                    }
                    else
                    {
                        //? Remove piece
                        Debug.Log("Jumped over a piece, Destroy ", p.gameObject);
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        Destroy(p.gameObject);
                        hasKilled = true;
                    }
                }


                //? were we supposed to kill anything?
                if (forcedPieces.Count != 0 && !hasKilled)
                {
                    MovePiece(selectedPiece, x1, y1);
                    ResetSelectedPiece();
                    return;
                }
                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);
                EndTurn();
            }
            else
            {
                MovePiece(selectedPiece, x1, y1);
                ResetSelectedPiece();
            }
        }

    }
    private void EndTurn()
    {
        Debug.Log("End Turn");
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;
        //? Promotion
        if (selectedPiece != null)
        {
            if (selectedPiece.isWhite && !selectedPiece.isQueen && y == 7)
            { //? white piece at the end of the board
                selectedPiece.isQueen = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            else if (!selectedPiece.isWhite && !selectedPiece.isQueen && y == 0)
            { //? black piece at the end of the board
                selectedPiece.isQueen = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
        }

        string moveMassage = "C_MOV|";
        moveMassage += startDrag.x.ToString() + "|";
        moveMassage += startDrag.y.ToString() + "|";
        moveMassage += endDrag.x.ToString() + "|";
        moveMassage += endDrag.y.ToString();

        client.Send(moveMassage);


        if (ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasKilled)
            return;

        hasKilled = false;
        ResetSelectedPiece();

        isWhiteTurn = !isWhiteTurn;
        // isWhite = !isWhite; //? Single Player
        CheckVictoryCondition();
    }
    private void CheckVictoryCondition()
    {
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasBlack = false;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }

        if (!hasWhite)
            Victory(false);
        if (!hasBlack)
            Victory(true);


    }

    private void Victory(bool isWhite)
    {
        Debug.Log(isWhite ? "White has Won" : "Black Has Won");
    }
    private void ResetSelectedPiece()
    {
        selectedPiece = null;
        startDrag = Vector2.zero;
    }
    private void GenerateBoard()
    {
        //? Generate white team
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                this.GeneratePiece((oddRow) ? x : x + 1, y, whitePiecePrefab, true, x);
            }
        }

        //? Generate black team
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                this.GeneratePiece((oddRow) ? x : x + 1, y, blackPiecePrefab, false, x);
            }
        }

    }
    private void GeneratePiece(int x, int y, GameObject prefab, bool isWhite, int id)
    {
        GameObject go = Instantiate(prefab) as GameObject;
        go.name = (isWhite ? "WhitePiece " : "Black Piece") + id;
        go.transform.SetParent(transform);
        Piece pieceScript = go.GetComponent<Piece>();
        pieceScript.isWhite = isWhite;
        pieces[x, y] = pieceScript;
        MovePiece(pieceScript, x, y);
    }
    private void MovePiece(Piece p, int x, int y)
    {
        Debug.Log("MovePiece to:  " + x + ',' + y);

        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }
    private void UpdateMouseOver()
    {
        if (!Camera.main)
        {
            Debug.Log("Main camera missing");
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, rayLength, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;

        }

    }
    private void UpdatePiecePosition(Piece p)
    {
        if (!Camera.main)
        {
            Debug.Log("Main camera missing");
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, rayLength, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }

    }
    private void SelectPiece(int x, int y)
    {

        Piece p = pieces[x, y];
        if (p != null && p.isWhite == isWhite)
        {
            if (forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = mouseOver;
            }
            else
            {
                //? Look for the piece under forced piece list
                if (forcedPieces.Find(fp => fp == p) == null)
                    return;

                selectedPiece = p;
                startDrag = mouseOver;
            }
        }
        // else
        // {
        //     selectedPiece = null;
        // }
        Debug.Log(selectedPiece ? "Piece selected: " + selectedPiece.name : "No piece selected");
    }
    private bool IsOutOfBound(int x, int y)
    {
        //? Out of bounds
        if (x < 0 || x >= 8 || y < 0 || y >= 8) return true;
        return false;

    }

}
