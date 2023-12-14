using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PazzleScript : MonoBehaviour
{
    GameManager gameManager = GameManager.instance;

    //パズルの種類を格納するための配列変数
    public GameObject[] puzzles;
    //配列の大きさ定義
    private int width = 6;
    private int height = 6;

    //パズルの配置を管理するための2次元配列
    public GameObject[,] puzzleArray = new GameObject[6, 6];
    //削除するパズルを格納するためのリスト
    private List<GameObject> deleteList = new List<GameObject>();

    //ゲームをスタートするかどうか
    private bool isGameStart;

    //スコアを表示
    public TMP_Text scoreText;
    //キャラセレクト画面で選択されたキャラと感情（Target）の画像を表示するための変数
    public Image charaImg;
    public Image emoImg;

    void Start()
    {
        //キャラ選択画面で選ばれた画像を割り当てる
        charaImg.sprite = gameManager.targetChara;
        emoImg.sprite = gameManager.targetEmo;

        scoreText.text = GameManager.target.ToString();
        CreatePuzzle();
    }

    //パズルを生成
    void CreatePuzzle()
    {
        //2重ループの指定された範囲内でパズルの位置決定
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //０から３までのランダムな数値をインデックスとして使用し、パズルを生成
                int random = UnityEngine.Random.Range(0, 4);
                if (puzzles[random] != null)
                {
                    var puzzle = Instantiate(puzzles[random]);
                    //生成されたパズルを(i,j)の座表示設定
                    puzzle.transform.position = new Vector2(i, j);
                    //パズルの位置情報をpuzzleArray配列の対応する要素に格納
                    puzzleArray[i, j] = puzzle;
                }
            }
        }
        CheckStartset();
    }

    //ゲーム開始時にパズルの初期配置で3つ以上のパズルが
    //横方向または縦方向に連続しているかどうかを確認しマッチングしたパズルを処理する
    void CheckStartset()
    {
        //下から上に向かってヨコのつながりを確認
        for (int i = 0; i < height; i++)
        {
            for (int x = 0; x < width - 2; x++) //右から２つ目以降は確認不要(width-2)
            {
                if ((puzzleArray[x, i].tag == puzzleArray[x + 1, i].tag) && (puzzleArray[x, i].tag == puzzleArray[x + 2, i].tag))
                {
                    puzzleArray[x, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    puzzleArray[x + 1, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    puzzleArray[x + 2, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                }
            }
        }

        //左から右に向かってタテのつながりを確認
        for (int i = 0; i < width; i++)
        {
            for (int y = 0; y < height - 2; y++) //上から２つ目以降は確認不要(height-2)
            {
                if ((puzzleArray[i, y].tag == puzzleArray[i, y + 1].tag) && (puzzleArray[i, y].tag == puzzleArray[i, y + 2].tag))
                {
                    puzzleArray[i, y].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    puzzleArray[i, y + 1].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    puzzleArray[i, y + 2].GetComponent<PuzzleMoveScript>().is3Matching = true;
                }
            }
        }

        //マッチングしたパズルの処理
        foreach (var item in puzzleArray)
        {
            if (item != null && item.GetComponent<PuzzleMoveScript>().is3Matching)
            {
                //is3MatchingフラグがtrueとなっているパズルをdeleteListに追加
                deleteList.Add(item);
            }
        }

        //deleteList内にパズルが存在する場合
        if (deleteList.Count > 0)
        {
            //該当する配列(puzzleArray)をnullにする(内部管理)
            //パズルのゲームオブジェクトを破棄。画面上からパズルを消去
            foreach (var item in deleteList)
            {
                puzzleArray[(int)item.transform.position.x, (int)item.transform.position.y] = null;
                Destroy(item);
            }

            //クリアにする
            deleteList.Clear();
            //新しいパズルを生成。空いた箇所に配置
            SpawnNewPuzzle();
        }
        else　//もしdeleteListが空である場合、isGameStartフラグをtrueに設定
        {
            isGameStart = true;
        }

    }

    //新しいパズルを生成。空いた箇所に配置
    void SpawnNewPuzzle()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //もしpuzzleArray[i, j]の位置が空（null）である場合新しいパズルを生成
                if (puzzleArray[i, j] == null)
                {
                    int random = UnityEngine.Random.Range(0, 4);
                    if (puzzles[random] != null)
                    {
                        var puzzle = Instantiate(puzzles[random]);
                        //見た目（表示位置）の処理
                        puzzle.transform.position = new Vector2(i, j + 0.3f);
                        //内部管理の処理
                        puzzleArray[i, j] = puzzle;
                    }
                }
            }
        }

        //パズルの初期配置でのマッチングを確認しているかどうか
        //していない場合（false）CheckStartset関数呼び出し
        if (isGameStart == false)
        {
            CheckStartset();
        }
        else
        {
            //各パズルの位置情報をmyPreviousPosとして設定
            //myPreviousPosはパズルが移動する際に前の位置情報を保持するために使用される変数
            foreach (var item in puzzleArray)
            {
                int column = (int)item.transform.position.x;
                int row = (int)item.transform.position.y;
                item.GetComponent<PuzzleMoveScript>().myPreviousPos = new Vector2(column, row);
            }
            //続けざまに３つ以上そろっているかどうか判定
            Invoke("CheckMatching", 0.2f);
        }
    }

    public void CheckMatching()
    {
        //下の行からヨコのつながりを確認
        for (int i = 0; i < height; i++)
        {
            //右から２つ目以降は確認不要
            for (int x = 0; x < width - 2; x++)
            {
                //タグが一致しているかどうか確認
                if (puzzleArray[x, i] != null && puzzleArray[x, i].tag == puzzleArray[x + 1, i].tag &&
                    puzzleArray[x, i].tag == puzzleArray[x + 2, i].tag)
                {
                    if (puzzleArray[x, i] != null)
                    {
                        //PuzzleMoveScriptのisMatchingをtrueに
                        puzzleArray[x, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                        puzzleArray[x + 1, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                        puzzleArray[x + 2, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    }
                }
            }
        }

        //左の列からタテのつながりを確認
        for (int i = 0; i < width; i++)
        {
            //上から２つ目以降は確認不要
            for (int y = 0; y < height - 2; y++)
            {
                if ((puzzleArray[i, y].tag == puzzleArray[i, y + 1].tag) &&
                    (puzzleArray[i, y].tag == puzzleArray[i, y + 2].tag))
                {
                    //タグが一致しているかどうか確認
                    if (puzzleArray[y, i] != null)
                    {
                        //PuzzleMoveScriptのisMatchingをtrueに
                        puzzleArray[i, y].GetComponent<PuzzleMoveScript>().is3Matching = true;
                        puzzleArray[i, y + 1].GetComponent<PuzzleMoveScript>().is3Matching = true;
                        puzzleArray[i, y + 2].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    }
                }
            }
        }

        //isMatching=trueのものをパズルの表示を半透明に設定
        foreach (var item in puzzleArray)
        {
            if (item.GetComponent<PuzzleMoveScript>().is3Matching)
            {
                item.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);//半透明
                deleteList.Add(item);//パズルをdeleteListに追加
            }
        }

        //List内にPuzzleがある場合
        if (deleteList.Count > 0)
        {
            //0.2秒後にDeletePuzzleメソッドを呼出
            //このメソッドは、deleteList内のパズルを実際に削除
            Invoke("DeletePuzzle", 0.2f);
        }
        else
        {
            //パズルの位置を元の位置に戻すためにBackToPreviousPosメソッド呼出
            //直前の位置に移動したパズルが元の位置に戻る
            foreach (var item in puzzleArray)
            {
                item.GetComponent<PuzzleMoveScript>().BackToPreviousPos();
            }
        }
        Invoke("CanMovePuzzle", 0.4f);//パズルの移動を再開
    }

    //マッチングしたパズルの削除や得点の更新。新しいパズルの生成
    void DeletePuzzle()
    {
        //deleteList内のPuzzleを削除かつ、puzzleArrayをnullに
        foreach (var item in deleteList)
        {
            Destroy(item);//削除
            puzzleArray[(int)item.transform.position.x, (int)item.transform.position.y] = null;//NULL化

           //もしパズルがGameManager.targetTagというタグを持つ場合以下の処理を行う
            if (item.CompareTag(GameManager.targetTag))
            {
                GameManager.target--;//目標消去数の数を1減らす（３つで削除なので最低３減る）
                if (GameManager.target <= 0)//もし目標消去数が０以下になった場合
                {
                    GameManager.target = 0;//目標消去数に０を代入
                    GameManager.cnt++;//パズルのプレイ数をカウントしている変数に＋１追加
                    GameManager.sceneCnt++;//どこのシーンに遷移するかを管理している変数に＋１追加
                    GameManager.SetTarget();//目標消去数の再設定する関数呼び出し
                    GameManager.SetScene();//どのシーンに遷移するか確定する関数呼出
                    SceneManager.LoadScene(GameManager.targetScene);//シーン遷移
                }
            }
        }
       scoreText.text = GameManager.target.ToString();

        //Listを空っぽに
        deleteList.Clear();
        //パズルの生成（新しいパズルの配置）をおこなう
        Invoke("SpawnNewPuzzle", 1.2f);
    }

    //isDontMovingフラグをtrueに設定
    //これにより、それらのパズルは移動不可に
    public void DontMovePuzzle()
    {
        foreach (var item in puzzleArray)
        {
            if (item != null && item.GetComponent<PuzzleMoveScript>().is3Matching)
            {
                item.GetComponent<PuzzleMoveScript>().isDontMoving = true;
            }
        }
    }

    //移動が可能なパズルに対してisDontMovingフラグをfalseに設定
    // これにより、パズルは再び移動可能に
    void CanMovePuzzle()
    {
        foreach (var item in puzzleArray)
        {
            if (item != null && item.GetComponent<PuzzleMoveScript>().is3Matching)
            {
                item.GetComponent<PuzzleMoveScript>().isDontMoving = false;
            }
        }
    }
}
