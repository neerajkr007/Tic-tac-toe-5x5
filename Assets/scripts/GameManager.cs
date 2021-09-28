using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;


public class GameManager : MonoBehaviourPun
{
    public Sprite xSprite;
    public Sprite oSprite;
    public Image top;
    public Transform board;
    public Image bottom;
    public GameObject gameParent;
    public GameObject mainMenuParent;
    public TMPro.TMP_Text youAre;
    public GameObject winnerAlert;

    Color lightGrey = new Color();
    Color grey = new Color();
    Color win = new Color(0.6207725f, 0.8490566f, 0.7056828f, 1);
    Color lose = new Color(0.7924528f, 0.5644357f, 0.5644357f, 1);
    PhotonView pv;
    int player = 0; // 0 is x and 1 is o
    int size = 5;
    int maxContinuous = 4;
    [SerializeField]
    bool gameOver = false;
    bool gameDraw = false;
    public int[,] markedCells = new int[5 , 5];
    bool offlineMode = true;
    [SerializeField]
    bool myTurn = false;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        lightGrey = top.color;
        grey = bottom.color;
        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                markedCells[i, j] = -1;
            }
        }
    }

    public void startgame(bool value)
    {
        mainMenuParent.SetActive(false);
        gameParent.SetActive(true);
        offlineMode = value;
        if(!offlineMode)
        {
            setupForOnline();
        }
    }

    void setupForOnline()
    {
        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            player = 0;
            myTurn = true;
            youAre.text = "(you are X)";
        }
        else
        {
            player = 1;
            myTurn = false;
            youAre.text = "(you are O)";
            makeAllUnIntractable();
        }
    }

    public void turnClicked()
    {
        setSprite();
        if(!gameOver && offlineMode)
        {
            switchTurn();
            makeUnIntractable();
        }
    }

    void makeUnIntractable()
    {
        EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable = false;
    }

    void makeAllUnIntractable()
    {
        for (int k = 0; k < board.childCount; k++)
        {
            board.GetChild(k).GetComponent<Button>().interactable = false;
        }
    }

    void setSprite()
    {
        Image image = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Image>();
        int i = 0, j = 0;
        if (player == 0)
        {
            image.sprite = xSprite;
            i = Int16.Parse(EventSystem.current.currentSelectedGameObject.name[0].ToString());
            j = Int16.Parse(EventSystem.current.currentSelectedGameObject.name[1].ToString());
            markedCells[i, j] = player;
        }
        else
        {
            image.sprite = oSprite;
            i = Int16.Parse(EventSystem.current.currentSelectedGameObject.name[0].ToString());
            j = Int16.Parse(EventSystem.current.currentSelectedGameObject.name[1].ToString());
            markedCells[i, j] = player;
        }
        
        gameOver = checkWinner(i, j);
        
        if (gameOver)
        {
            makeAllUnIntractable();
            if (player == 0)
            {
                top.color = win;
                bottom.color = lose;
                winnerAlert.SetActive(true);
                winnerAlert.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "X is the winner !!!";
            }
            else
            {
                top.color = lose;
                bottom.color = win;
                winnerAlert.SetActive(true);
                winnerAlert.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "O is the winner !!!";
            }
        }
        else
        {
            gameDraw = checkDraw();
            if (gameDraw)
            {
                winnerAlert.SetActive(true);
                winnerAlert.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "Game Draw";
                if(offlineMode)
                gameOver = true;
            }
        }
        if (!offlineMode)
        {
            makeAllUnIntractable();
            pv.RPC("playedTurn", RpcTarget.Others, i, j, gameOver, gameDraw);
            if (gameDraw)
                gameOver = true;
        }
    }

    void switchTurn()
    {
        if(player == 0)
        {
            player = 1;
            top.color = grey;
            bottom.color = lightGrey;
        }
        else
        {
            player = 0;
            top.color = lightGrey;
            bottom.color = grey;
        }
    }

    bool checkWinner(int i, int j)
    {
        if(checkRow(j))
        {
            // player winner
            print("row");
            return true;
        }
        else if(checkCol(i))
        {
            // player winner
            print("col");
            return true;
        }

        // for diag
        else if(i == j)
        {
            if(checkDiag())
            {
                // player winner
                print("diag");
                return true;
            }
        }

        // for anti diag
        else if(i + j == size - 1)
        {
            if(checkAntiDiag())
            {
                // player winner
                print("anti diag");
                return true;
            }
        }

        //for side diag 1
        else if(i - j == 1 || j - i == 1)
        {
            if (checkSideDiag())
            {
                // player winner
                print("side diag");
                return true;
            }
        }
        return false;
    }

    bool checkRow(int j)
    {
        // high time complexity way to check for scaleable version of the game 

        /*for (int k = 0; k <= size - maxContinuous; k++)
        {
            for (int i = k; i < size - k; i++)
            {
                if (markedCells[i, j] != player)
                { 
                    break; 
                }
                if (i == size - 1 - k)
                {
                    //report win for player
                    return true;
                }
            }
        }*/

        for (int i = 0; i < size - 1; i++)
        {
            if (markedCells[i, j] != player)
            { 
                break; 
            }
            if (i == size - 2)
            {
                //report win for player
                return true;
            }
        }
        for(int i = 1; i < size; i++)
        {
            if (markedCells[i, j] != player)
            {
                break;
            }
            if (i == size - 1)
            {
                //report win for player
                return true;
            }
        }
        return false;
    }

    bool checkCol(int i)
    {
        for (int j = 0; j < size - 1; j++)
        {
            if (markedCells[i, j] != player)
            {
                break;
            }
            if (j == size - 2)
            {
                //report win for player
                return true;
            }
        }
        for (int j = 1; j < size; j++)
        {
            if (markedCells[i, j] != player)
            {
                break;
            }
            if (j == size - 1)
            {
                //report win for player
                return true;
            }
        }
        return false;
    }

    bool checkDiag()
    {
        for (int i = 0; i < size - 1; i++)
        {
            if (markedCells[i, i] != player)
            {
                break;
            }
            if (i == size - 2)
            {
                //report win for player
                return true;
            }
        }
        for (int i = 1; i < size; i++)
        {
            if (markedCells[i, i] != player)
            {
                break;
            }
            if (i == size - 1)
            {
                //report win for player
                return true;
            }
        }
        return false;
    }

    bool checkAntiDiag()
    {
        for (int i = 0; i < size - 1; i++)
        {
            if (markedCells[i, (size - 1) - i] != player)
            {
                break;
            }
            if (i == size - 2)
            {
                //report win for player
                return true;
            }
        }
        for (int i = 1; i < size; i++)
        {
            if (markedCells[i, (size - 1) - i] != player)
            {
                break;
            }
            if (i == size - 1)
            {
                //report win for player
                return true;
            }
        }
        return false;
    }

    bool checkSideDiag()
    {
        for(int i = 1; i < size; i++)
        {
            if (markedCells[i, i - 1] != player)
            {
                break;
            }
            if (i == size - 1)
            {
                //report win for player
                return true;
            }
        }
        for (int j = 1; j < size; j++)
        {
            if (markedCells[j - 1, j] != player)
            {
                break;
            }
            if (j == size - 1)
            {
                //report win for player
                return true;
            }
        }
        return false;
    }

    bool checkDraw()
    {
        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                if(markedCells[i, j] == -1)
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    [PunRPC]
    void playedTurn(int i, int j, bool _gameOver, bool gameDraw)
    {
        Image image = GameObject.Find(i.ToString() + j.ToString()).transform.GetChild(0).GetComponent<Image>();
        if (player == 0)
        {
            image.sprite = oSprite;
            markedCells[i, j] = 1;
        }
        else
        {
            image.sprite = xSprite;
            markedCells[i, j] = 0;
        }
        if (gameDraw)
        {
            gameOver = true;
            winnerAlert.SetActive(true);
            winnerAlert.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "Game Draw !!!";
            return;
        }
        if (_gameOver)
        {
            makeAllUnIntractable();
            
            if (player == 0)
            {
                top.color = lose;
                bottom.color = win;
                winnerAlert.SetActive(true);
                winnerAlert.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "O is the winner !!!";
            }
            else
            {
                top.color = win;
                bottom.color = lose;
                winnerAlert.SetActive(true);
                winnerAlert.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "X is the winner !!!";
            }
            return;
        }
        pv.RPC("switchTurnRPC", RpcTarget.All);
    }
    
    [PunRPC]
    void switchTurnRPC()
    {
        if(myTurn)
        {
            myTurn = false;
            makeAllUnIntractable();
            if (player == 0)
            {
                top.color = grey;
                bottom.color = lightGrey;
            }
            else
            {
                top.color = lightGrey;
                bottom.color = grey;
            }
        }
        else
        {
            myTurn = true; 
            for (int i = 0; i < board.childCount; i++)
            {
                if (board.GetChild(i).GetChild(0).GetComponent<Image>().sprite == null)
                {
                    board.GetChild(i).GetComponent<Button>().interactable = true;
                }
                else
                {
                    board.GetChild(i).GetComponent<Button>().interactable = false;
                }
            }
            if (player == 1)
            {
                top.color = grey;
                bottom.color = lightGrey;
            }
            else
            {
                top.color = lightGrey;
                bottom.color = grey;
            }
        }
    }
}
