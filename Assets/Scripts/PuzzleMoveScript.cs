using UnityEngine;

public class PuzzleMoveScript : MonoBehaviour
{
    private PazzleScript puzzleScript;

    //自身の入っている配列の座標
    public int myColumn;//列
    public int myRow;//行

    //スワイプしたときの座標を確認
    private Vector2 mouseDown;
    private Vector2 mouseUp;
    private Vector2 distance;

    //隣接するパズルの参照を保持
    private GameObject neighborPuzzle;
    //パズルが3つのマッチンググループに所属しているかどうかを示すフラグ
    public bool is3Matching;

    //移動前の座標保持
    public Vector2 myPreviousPos;

    //パズルが移動中かどうかを示すフラグ
    //移動中はPuzzleを操作できないように
    public bool isDontMoving;

    void Start()
    {
        //PazzleScriptのインスタンスを見つける
        puzzleScript = FindObjectOfType<PazzleScript>();

        //パズルの位置を座標配列の番号（インデックス）に設定
        myColumn = (int)transform.position.x;
        myRow = (int)transform.position.y;

        //myPreviousPosに現在の位置(スタート位置)を記録
        myPreviousPos = new Vector2(myColumn, myRow);
    }

    public void OnMouseDown()
    {    //マウスの左ボタンが 押されたとき
        if (Input.GetMouseButtonDown(0))
        {
            //マウスの現在の座標をCamera.main.ScreenToWorldPoint(Input.mousePosition)
            //を使用してワールド座標に変換し、mouseDownに格納
            mouseDown = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public void OnMouseUp()
    {
        //isDontMovingがtrueであれば、関数を終了
        if (isDontMoving) { return; }

        //マウスはなしたとき
        if (Input.GetMouseButtonUp(0))
        {
            //マウスのボタンが離された時の座標を取得　mouseUpに格納
            mouseUp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //mouseDown mouseUpのベクトルの差を計算
            distance = mouseUp - mouseDown;
            movePuzzle();//パズルの移動を行う
             if (isDontMoving) { return; }   //isDontMovingがtrueであれば、関数を終了
        }
    }

    // Update is called once per frame
    void Update()
    {
        //現在の座標と、column、rowの値が異なるとき
        if (transform.position.x != myColumn || transform.position.y != myRow)
        {
            //column,rowの位置に徐々に移動する
            transform.position = Vector2.Lerp(transform.position, new Vector2(myColumn, myRow), 0.3f);
            //現在の位置と、目的地(column,row)との距離を測る
            Vector2 dif = (Vector2)transform.position - new Vector2(myColumn, myRow);
            //目的地との距離が0.1fより小さくなったら
            if (Mathf.Abs(dif.magnitude) < 0.1f)
            {
                transform.position = new Vector2(myColumn, myRow);
                //自身をPuzzleArray配列に格納する
                SetPuzzleToArray();
            }
        }
        //自分が０行目（一番下）ではなく、かつ、下にPuzzleがない場合、落下させる
        else if (myRow > 0 && puzzleScript.puzzleArray[myColumn, myRow - 1] == null)
        {
            FallPuzzle();
        }
    }


    void movePuzzle()
    {
        //すべてのPuzzleを操作できないようにする
        puzzleScript.DontMovePuzzle();

        //右にスワイプしていたなら（Mathf.Absとは絶対値を示す）
        if (distance.x >= 0 && Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
        {
            //自身が一番右にいない場合、となりのPuzzleと位置を交換する
            if (myColumn < 5)
            {
                //右隣りのvPuzzle情報をneighborPuzzleに代入
                neighborPuzzle = puzzleScript.puzzleArray[myColumn + 1, myRow];
                //隣のPuzzleを１列左へ
                neighborPuzzle.GetComponent<PuzzleMoveScript>().myColumn -= 1;
                myColumn += 1; //自身は１列右へ
            }
        }
        //左にスワイプしていたなら
        if (distance.x < 0 && Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
        {
            //自身が一番左にいない場合、となりのPuzzleと位置を交換する
            if (myColumn > 0)
            {
                //左隣りのvPuzzle情報を取得
                neighborPuzzle = puzzleScript.puzzleArray[myColumn - 1, myRow];
                //隣のPuzzleを１列右へ
                neighborPuzzle.GetComponent<PuzzleMoveScript>().myColumn += 1;
                myColumn -= 1;//自身は１列左へ
            }
        }

        //上にスワイプしていたなら
        if (distance.y >= 0 && Mathf.Abs(distance.x) < Mathf.Abs(distance.y))
        {
            //自身が一番上にいない場合、となりのPuzzleと位置を交換する
            if (myRow < 5)
            {
                //上のPuzzle情報を取得
                neighborPuzzle = puzzleScript.puzzleArray[myColumn, myRow + 1];
                //隣のPuzzleを１行下へ
                neighborPuzzle.GetComponent<PuzzleMoveScript>().myRow -= 1;
                myRow += 1; //自身は１行上へ
            }
        }

        //下にスワイプしていたなら
        if (distance.y < 0 && Mathf.Abs(distance.x) < Mathf.Abs(distance.y))
        {
            //自身が一番下にいない場合、となりのPuzzleと位置を交換する
            if (myRow > 0)
            {
                //下のPuzzle情報を取得
                neighborPuzzle = puzzleScript.puzzleArray[myColumn, myRow - 1];
                //隣のPuzzleを１行上へ
                neighborPuzzle.GetComponent<PuzzleMoveScript>().myRow += 1;
                //自身は１ 行下へ
                myRow -= 1;
            }
        }
        Invoke("DoCheckMatching", 0.5f);
    }

    void DoCheckMatching()
    {
        puzzleScript.CheckMatching();
    }

    //PuzzleArray配列に、自身を格納する
    public void SetPuzzleToArray()
    {
        puzzleScript.puzzleArray[myColumn, myRow] = gameObject;
    }

    void FallPuzzle()
    {
        //自分のいた配列を空にする
        puzzleScript.puzzleArray[myColumn, myRow] = null;
        //自分を下に移動させる
        myRow -= 1;
    }

    //元の位置に戻る
    public void BackToPreviousPos()
    {
        myColumn = (int)myPreviousPos.x;
        myRow = (int)myPreviousPos.y;
    }
}
