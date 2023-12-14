using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharaSelectScript : MonoBehaviour
{
    public int index;//選択されたキャラのインデックス

    public Button angryButton;
    public Button disgustButton;
    public Button fearButton;
    public Button sadButton;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;

        // 各ボタンにOnClickイベントを設定
        angryButton.onClick.AddListener(AngryStory);
        disgustButton.onClick.AddListener(DisgustStory);
        fearButton.onClick.AddListener(FearStory);
        sadButton.onClick.AddListener(SadStory);
    }

    // 怒りのストーリーを選択した場合の処理
    public void AngryStory()
    {
        if (gameManager != null)
        {
            //タグ管理しているパズルゲームの目標をikariに書き換える
            GameManager.targetTag = "angry";
            SetTargetImage(0);
            //怒りのストーリーに進む
            SceneManager.LoadScene(GameManager.scene[0, 0]);
        }
    }

    // 嫌悪のストーリーを選択した場合の処理
    public void DisgustStory()
    {
        if (gameManager != null)
        {
            //タグ管理しているパズルゲームの目標をdisgustに書き換える
            GameManager.targetTag = "disgust";
            SetTargetImage(1);
            //嫌悪のストーリーに進む
            SceneManager.LoadScene(GameManager.scene[1, 0]);
        }
    }
    // 恐怖のストーリーを選択した場合の処理
    public void FearStory()
    {
        if (gameManager != null)
        {
            //タグ管理しているパズルゲームの目標をfearに書き換える
            GameManager.targetTag = "fear";
            SetTargetImage(2);
            //恐怖のストーリーに進む
            SceneManager.LoadScene(GameManager.scene[2, 0]);
        }
    }
    // 悲しみのストーリーを選択した場合の処理
    public void SadStory()
    {
        if (gameManager != null)
        {
            //タグ管理しているパズルゲームの目標をsadに書き換える
            GameManager.targetTag = "sad";
            SetTargetImage(3);
            //悲しみのストーリーに進む
            SceneManager.LoadScene(GameManager.scene[3, 0]);
        }
    }

    void SetTargetImage(int index)
    {
        //パズル画面に表示されるキャラクターと消す目標ターゲットの画像を
        ////選択された画像に変更
        gameManager.targetChara = gameManager.sourceImages[index];
        gameManager.targetEmo = gameManager.emotionImages[index];
        this.index = index;//index保持
    }

}
