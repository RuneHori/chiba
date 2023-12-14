using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    //PuzzleSceneを管理する際に使用する（難易度）
    public static int cnt = 0;

    //PuzzleSceneの目標設定数
    public static int target = 10;

    //シーン
    public static string[,] scene = {
        { "Angry00Scene", "Angry01Scene", "Angry02Scene", "Angry03Scene" } ,
          { "Disgust00Scene","Disgust01Scene" ,"Disgust02Scene" ,"Disgust03Scene"  },
        { "Fear00Scene","Fear01Scene" ,"Fear02Scene" ,"Fear03Scene"  },
         { "Sad00Scene","Sad01Scene" ,"Sad02Scene" ,"Sad03Scene"  }
    };

    public enum EmotionType
    {
        Angry = 0,
        Disgust,
        Fear,
        Sad,
        End
    }
    //シーンが今どこまで進んでいるか管理する変数
    public static EmotionType sceneCnt = EmotionType.Angry;

    //ターゲットタグ
    public static string targetScene;
    public static string targetTag;

    public Sprite[] sourceImages;
    public Sprite[] emotionImages;
    public Sprite targetChara;
    public Sprite targetEmo;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //初期化
        targetTag = "angry";
        targetScene = "Angry00Scene";
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    // ターゲット数を設定する
    static public void SetTarget()
    {
        switch (cnt)
        {
            case 0:
                target = 10;
                break;
            case 1:
                target = 20;
                break;
            case 2:
                target = 30;
                break;
            case 3:
                target = 40;
                break;

        }
    }


    // シーンを設定する
    static public void SetScene()
    {
        switch (sceneCnt)
        {
            case EmotionType.Angry:
                if (targetTag == "angry")
                {
                    targetScene = scene[0, 0];
                }
                else if (targetTag == "disgust")
                {
                    targetScene = scene[1, 0];
                }
                else if (targetTag == "fear")
                {
                    targetScene = scene[2, 0];
                }
                else
                {
                    targetScene = scene[3, 0];
                }
                break;
            case EmotionType.Disgust:
                if (targetTag == "angry")
                {
                    targetScene = scene[0, 1];
                }
                else if (targetTag == "disgust")
                {
                    targetScene = scene[1, 1];
                }
                else if (targetTag == "fear")
                {
                    targetScene = scene[2, 1];
                }
                else
                {
                    targetScene = scene[3, 1];
                }
                break;
            case EmotionType.Fear:
                if (targetTag == "angry")
                {
                    targetScene = scene[0, 2];
                }
                else if (targetTag == "disgust")
                {
                    targetScene = scene[1, 2];
                }
                else if (targetTag == "fear")
                {
                    targetScene = scene[2, 2];
                }
                else
                {
                    targetScene = scene[3, 2];
                }
                break;
            case EmotionType.Sad:
                if (targetTag == "angry")
                {
                    targetScene = scene[0, 3];
                }
                else if (targetTag == "disgust")
                {
                    targetScene = scene[1, 3];
                }
                else if (targetTag == "fear")
                {
                    targetScene = scene[2, 3];
                }
                else
                {
                    targetScene = scene[3, 3];
                }
                break;
            case EmotionType.End:
                targetScene = "EndScene";
                break;
        }
    }

}

