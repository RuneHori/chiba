using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class TextScript : MonoBehaviour
{
    //文章
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI nameText;
    private const char SEPARATE_MAIN_START = '「';
    private const char SEPARATE_MAIN_END = '」';
    private const char SEPARATE_PAGE = '&';//メインテキスト内の複数のページを区切るための文字
    private Queue<char> charQueue;//文字を順番に格納するためのキュー
    private Queue<string> pageQueue;//ページを順番に格納するためのキュー

    [SerializeField] private float captionSpeed = 0.5f;//テキストの表示速度を制御するための変数
    [SerializeField] private GameObject nextPageIcon;//次のページに進むことを示すためのアイコン



    //背景
    [SerializeField] private Image backgroundImage;
    [SerializeField] private string spritesDirectory = "Sprites/";
    private const char SEPARATE_COMMAND = '!';
    private const char COMMAND_SEPARATE_PARAM = '=';
    private const string COMMAND_BACKGROUND = "background";
    private const string COMMAND_SPRITE = "_sprite";
    private const string COMMAND_COLOR = "_color";

    //キャラ 
    [SerializeField] private GameObject charaImages;
    [SerializeField] private string prefabsDirectory = "Prefabs/";
    private const string COMMAND_CHARACTER_IMAGE = "charaimg";
    private const string COMMAND_SIZE = "_size";
    private const string COMMAND_POSITION = "_pos";
    private const string COMMAND_ROTATION = "rotate";
    private const string CHARACTER_IMAGE_PREFAB = "CharaImage";

    private List<Image> charaImageList = new List<Image>();//キャラクターのイメージを管理するためのリスト

    //外部ファイル
    [SerializeField] private string textFile = "Texts/";
    private string text = "";

    //名前ウィンドウ・立ち絵非表示／削除
    private const string COMMAND_ACTIVE = "_active";
    private const string COMMAND_DELETE = "_delete";

    //ラベルジャンプ
    private const char SEPARATE_SUBSCENE = '#';
    private const string COMMAND_JUMP = "jump_to";
    private Dictionary<string, Queue<string>> _subScenes =
        new Dictionary<string, Queue<string>>();

    //選択肢
    private const string COMMAND_SELECT = "select";
    private const string COMMAND_TEXT = "_text";
    private const string SELECT_BUTTON_PREFAB = "SelectButton";
    [SerializeField] private GameObject selectButtons;
    private List<Button> _selectButtonList = new List<Button>();// 選択肢ボタンを管理するためのリスト


    //アニメーション
    private const char COMMAND_SEPARATE_ANIM = '%';
    private const string COMMAND_ANIM = "_anim";
    [SerializeField] private string animationsDirectory = "Animations/";
    [SerializeField] private string overrideAnimationClipName = "Clip";

    //WAIT
    private const string COMMAND_WAIT_TIME = "wait";
    private float waitTime = 0.2f;//待機時間を保持するための変数

    //BGM
    private const string COMMAND_BGM = "bgm";
    private const string COMMAND_SE = "se";
    private const string COMMAND_PLAY = "_play";
    private const string COMMAND_MUTE = "_mute";
    private const string COMMAND_SOUND = "_sound";
    private const string COMMAND_VOLUME = "_volume";
    private const string COMMAND_PRIORITY = "_priority"; //音声の優先度を設定するコマンド
    private const string COMMAND_LOOP = "_loop";
    private const string COMMAND_FADE = "_fade";//フェードイン/フェードアウトを制御するコマンド
    private const string SE_AUDIOSOURCE_PREFAB = "SEAudioSource";
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private GameObject seAudioSources;
    [SerializeField] private string audioClipsDirectory = "AudioClips/";
    private List<AudioSource> seList = new List<AudioSource>(); //再生するSEのオーディオソースを管理するためのリスト

    //画面フェード
    private const string COMMAND_FOREGROUND = "foreground";
    [SerializeField] private Image foregroundImage;

    //シーン
    private const string COMMAND_CHANGE_SCENE = "scene";


    //BackLog
    private const string COMMAND_BACK_LOG = "backlog";
    [SerializeField] private ScrollRect backLog;

    //Gameパラメータなど
    private const string COMMAND_PARAM = "param";
    private const string COMMAND_WRITE = "_write";
    private const char OUTPUT_PARAM = '"';
    private Dictionary<string, object> _params = new Dictionary<string, object>(); //パラメータ

    void Start()
    {
        Init();//初期化処理
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) OnClick();
        if (Input.GetMouseButtonDown(1)) OnClickRight();
        MouseScroll();
    }

    private void Init()//初期化処理
    {
        text = LoadTextFile(textFile);//テキストファイル読み込み。textに格納
        //＃　文字で分割しサブシーンごとの情報を格納したキューを作成
        Queue<string> subScenes = SeparateString(text, SEPARATE_SUBSCENE);
        //各サブシーンを処理
        foreach (string subScene in subScenes)
        {
            //空のサブシーンは無視
            if (subScene.Equals("")) continue;
            //&　文字でページごとに分割しその結果を格納したキューを作成
            Queue<string> pages = SeparateString(subScene, SEPARATE_PAGE);
            //ページキューの先頭の要素を取り出す→サブシーン名とする（例えばNEXT１）
            _subScenes[pages.Dequeue()] = pages;
        }
        //最初のサブシーンのページキューをpageQueueに設定する
        pageQueue = _subScenes.First().Value;
        //次のページを表示する
        ShowNextPage();
    }

    private void OnClick()//左クリック
    {
        if (isStop) { return; }// ゲームが停止中の場合処理を中断
        // 文字キューにまだ文字が残っている場合全ての文字を表示
        if (charQueue.Count > 0) { OutputAllChar(); }
        else
        {
            // 選択肢ボタンが表示されている場合は処理中断
            if (_selectButtonList.Count > 0) { return; }

            // 次のページが表示されなかった場合アプリケーションを終了
#if UNITY_EDITOR
            if (!ShowNextPage()) { EditorApplication.isPlaying = false; }
#else
            if (!ShowNextPage()) { Application.Quit(); }
#endif
        }
    }

    private void OnClickRight() //右クリック
    {
        //メインテキストウィンドウの親オブジェクトを取得
        GameObject mainWindow = mainText.transform.parent.gameObject;
        //名前ウィンドウの親オブジェクトを取得
        GameObject nameWindow = nameText.transform.parent.gameObject;

        //メインテキストウィンドウの表示状態を切り替え
        //現在の表示状態の反対の状態に設定
        mainWindow.SetActive(!mainWindow.activeSelf);

        //もし名前ウィンドウがテキストを持っている場合
        //メインテキストウィンドウと同じ表示状態に設定（非表示だったら非表示にする）
        if (nameText.text.Length > 0) { nameWindow.SetActive(mainWindow.activeSelf); }

        // 文字キューに表示すべき文字がない場合、メインテキストウィンドウの表示状態に応じて
        // 次のページアイコンを表示・非表示に
        if (charQueue.Count <= 0) { nextPageIcon.SetActive(mainWindow.activeSelf); }
    }

    private void MouseScroll()//スクロールした際の処理
    {
        //バックログウィンドウが表示されていて、スクロールダウンが行われ
        //かつバックログのスクロール位置が最上部の場合、バックログウィンドウを非表示に
        if (backLog.gameObject.activeSelf && Input.mouseScrollDelta.y < 0 && backLog.verticalNormalizedPosition <= 0)
        { backLog.gameObject.SetActive(false); }

        //バックログウィンドウが非表示であり、スクロールアップが行われた場合
        //バックログのスクロール位置を最上部に設定
        if (!backLog.gameObject.activeSelf && Input.mouseScrollDelta.y > 0)
        {
            //バックログの垂直スクロール位置を最上部に設定
            backLog.verticalNormalizedPosition = 0;
            //バックログウィンドウを表示
            backLog.gameObject.SetActive(true);

            //バックログウィンドウがアクティブになった時、垂直スクロールバーにフォーカスを移す
            EventSystem.current.SetSelectedGameObject(backLog.verticalScrollbar.gameObject);
        }
    }

    //文字送りコルーチン
    private IEnumerator ShowChars(float wait)
    {
        while (true)//無限ループ
        {
            if (!isStop)//ゲームが停止していない場合に以下の処理を実行
            {
                //1文字ずつ文字列を表示し全ての文字が表示された場合にループを終了
                if (!OutputChar()) { break; }
            }
            //指定された待機時間（wait）だけ処理を一時停止
            yield return new WaitForSeconds(wait);
        }
        yield break;
    }

    private IEnumerator WaitForCommand()//次の読み込みを待機するコルーチン
    {
        float time = 0;//経過時間初期化
        //経過時間が指定された待機時間（waitTime）未満の間、以下の処理を繰り返す
        while (time < waitTime)
        {
            //ゲームが停止していない場合経過時間を加算
            if (!isStop) { time += Time.deltaTime; }
            yield return null;//1フレーム待機
        }
        waitTime = 0;//待機時間リセット
        ShowNextPage();//次のページを表示
        yield break;//コルーチンを終了
    }

    //音のフェード管理コルーチン
    private IEnumerator FadeSound(AudioSource audio, float time, float volume)
    {
        // フェードにかける時間（time）と目標の音量（volume）から、フレームごとの音量の変化量を計算
        float vo = (volume - audio.volume) / (time / Time.deltaTime);
        //現在の音量が目標の音量より大きい場合、音量を下げるフェード 処理を行うためにisOutというブール変数を定義
        bool isOut = audio.volume > volume;

        //現在の音量が目標の音量に達するまで、以下の処理を繰り返す
        while ((!isOut && audio.volume < volume) || (isOut && audio.volume > volume))
        {
            audio.volume += vo;//音量を変化量（vo）だけ更新
            yield return null;
        }
        audio.volume = volume;//音量を目標の音量（volume）に設定
    }


    private bool OutputChar()//１文字を出力
    {
        //キューが空の場合nextPageIcon を表示
        if (charQueue.Count <= 0)
        {
            nextPageIcon.SetActive(true);
            return false;
        }
        else
        {
            mainText.text += charQueue.Dequeue(); //キューから値を取り出してキュー内から削除
            return true;
        }

    }

    private void OutputAllChar()//全文出力
    {
        StopCoroutine(ShowChars(captionSpeed));//コルーチンSTOP
        while (OutputChar()) ;//キューが空になるまで表示
        waitTime = 0;
        nextPageIcon.SetActive(true);
    }

    private bool ShowNextPage()//次ページ表示
    {

        if (pageQueue.Count <= 0) { return false; }   //次のページが存在しない
        else
        {
            nextPageIcon.SetActive(false);
            ReadLine(pageQueue.Dequeue());
            return true;
        }
    }

    //文字列を指定した文字ごとに区切りQueueに格納したものを返す
    private Queue<string> SeparateString(string str, char sep)
    {
        string[] strs = str.Split(sep);
        Queue<string> queue = new Queue<string>();
        foreach (string l in strs) queue.Enqueue(l);
        return queue;
    }

    //文字を１文字ごとに区切りQueueに格納したものを返す
    private Queue<char> SepareteString(string str)
    {
        char[] chars = str.ToCharArray();
        Queue<char> charQueue = new Queue<char>();
        //foreach文で配列charに格納された文字をすべて取り出してQueueに加える
        foreach (char c in chars) charQueue.Enqueue(c);
        return charQueue;
    }

    private void ReadLine(string text)//1行を読み出す
    {
        if (text[0].Equals(SEPARATE_COMMAND))
        {
            ReadCommand(text);
            if (_selectButtonList.Count > 0) { return; }
            if (waitTime > 0)
            {
                StartCoroutine(WaitForCommand());
                return;
            }
            ShowNextPage();
            return;
        }
        text = ReplaceParameterForGame(text);
        //'「'の位置で文字列を分ける
        string[] ts = text.Split(SEPARATE_MAIN_START);
        string name = ts[0];//名前代入
                            //最後の閉じかっこを削除して代入（＝ex.HelloWorld!)
        string main = ts[1].Remove(ts[1].LastIndexOf(SEPARATE_MAIN_END));
        nameText.text = name;
        if (name.Equals(""))
        {
            nameText.transform.parent.gameObject.SetActive(false);
            backLog.content.GetComponentInChildren<TextMeshProUGUI>().text += main;
        }
        else
        {
            nameText.transform.parent.gameObject.SetActive(true);
            backLog.content.GetComponentInChildren<TextMeshProUGUI>().text += text;
        }

        mainText.text = "";
        charQueue = SepareteString(main);
        StartCoroutine(ShowChars(captionSpeed)); //コルーチン呼び出し

        backLog.content.GetComponentInChildren<TextMeshProUGUI>().text +=
       Environment.NewLine + Environment.NewLine;
    }

    private void ReadCommand(string cmdLine)//コマンド読み出し
    {
        //最初の！削除
        cmdLine = cmdLine.Remove(0, 1);
        Queue<string> cmdQueue = SeparateString(cmdLine, SEPARATE_COMMAND);
        foreach (string cmd in cmdQueue)
        {
            //「＝」で分ける
            string[] cmds = cmd.Split(COMMAND_SEPARATE_PARAM);
            //もし背景コマンドの文字列が含まれていたら
            if (cmds[0].Contains(COMMAND_BACKGROUND))
            {
                SetBackgroundImage(cmds[0], cmds[1]);
            }
            if (cmds[0].Contains(COMMAND_FOREGROUND))
            {
                SetForegroundImage(cmds[0], cmds[1]);
            }
            if (cmds[0].Contains(COMMAND_CHARACTER_IMAGE))
            {
                SetCharaImage(cmds[1], cmds[0], cmds[2]);
            }

            if (cmds[0].Contains(COMMAND_JUMP))
            {
                JumpTo(cmds[1]);
            }
            if (cmds[0].Contains(COMMAND_SELECT))
            {
                SetSelectButton(cmds[1], cmds[0], cmds[2]);
            }
            if (cmds[0].Contains(COMMAND_WAIT_TIME))
            {
                SetWaitTime(cmds[1]);
            }
            if (cmds[0].Contains(COMMAND_BGM))
            {
                SetBackgroundMusic(cmds[0], cmds[1]);
            }
            if (cmds[0].Contains(COMMAND_SE))
            {
                SetSoundEffect(cmds[1], cmds[0], cmds[2]);
            }
            if (cmds[0].Contains(COMMAND_CHANGE_SCENE))
            {
                ChangeNextScene(cmds[1]);
            }
            if (cmds[0].Contains(COMMAND_BACK_LOG))
            {
                WriteBackLog(cmds[1]);
            }
            if (cmds[0].Contains(COMMAND_PARAM))
            {
                SetParameterForGame(cmds[1], cmds[0], cmds[2]);
            }


        }
    }

    private void ChangeNextScene(string parameter)//対応シーンに切り替える
    {
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        SceneManager.LoadSceneAsync(parameter);
    }

    private void JumpTo(string parameter)//対応するラベルまでジャンプ
    {
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        pageQueue = _subScenes[parameter];
    }

    private void SetWaitTime(string parameter) //待機時間設定
    {
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        waitTime = float.Parse(parameter);
    }

    private void WriteBackLog(string parameter)//バックログを記述
    {
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        backLog.content.GetComponentInChildren<TextMeshProUGUI>().text += parameter + Environment.NewLine + Environment.NewLine;
    }



    private void SetForegroundImage(string cmd, string parameter)  //前景設定
    {
        cmd = cmd.Replace(COMMAND_FOREGROUND, "");
        SetImage(cmd, parameter, foregroundImage);
    }


    //立ち絵設定
    private void SetCharaImage(string name, string cmd, string parameter)
    {
        cmd = cmd.Replace(COMMAND_CHARACTER_IMAGE, "");
        name = name.Substring(name.IndexOf('"') + 1, name.LastIndexOf('"') - name.IndexOf('"') - 1);
        Image image = charaImageList.Find(n => n.name == name);
        if (image == null)
        {
            image = Instantiate(Resources.Load<Image>(prefabsDirectory + CHARACTER_IMAGE_PREFAB), charaImages.transform);
            image.name = name;
            charaImageList.Add(image);
        }
        SetImage(cmd, parameter, image);
    }

    //選択肢の設定
    private void SetSelectButton(string name, string cmd, string parameter)
    {
        cmd = cmd.Replace(COMMAND_SELECT, "");
        name = name.Substring(name.IndexOf('"') + 1, name.LastIndexOf('"') - name.IndexOf('"') - 1);
        Button button = _selectButtonList.Find(n => n.name == name);
        if (button == null)
        {
            button = Instantiate(Resources.Load<Button>(prefabsDirectory + SELECT_BUTTON_PREFAB),
                selectButtons.transform);
            button.name = name;
            button.onClick.AddListener(() => SelectButtonOnClick(name));
            _selectButtonList.Add(button);
        }
        SetImage(cmd, parameter, button.image);
    }

    private void SelectButtonOnClick(string label)//選択肢がクリックされた
    {
        foreach (Button button in _selectButtonList) Destroy(button.gameObject);
        _selectButtonList.Clear();
        JumpTo('"' + label + '"');
        ShowNextPage();
    }

    //画像の設定
    private void SetImage(string cmd, string parameter, Image image)
    {
        cmd = cmd.Replace(" ", "");
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        switch (cmd)
        {
            case COMMAND_TEXT:
                image.GetComponentInChildren<TextMeshProUGUI>().text = parameter;
                break;
            case COMMAND_SPRITE:
                image.sprite = LoadSprite(parameter);
                break;
            case COMMAND_COLOR:
                image.color = ParameterToColor(parameter);
                break;
            case COMMAND_SIZE:
                image.GetComponent<RectTransform>().sizeDelta = ParameterToVector3(parameter);
                break;
            case COMMAND_POSITION:
                image.GetComponent<RectTransform>().anchoredPosition = ParameterToVector3(parameter);
                break;
            case COMMAND_ROTATION:
                image.GetComponent<RectTransform>().eulerAngles = ParameterToVector3(parameter);
                break;
            case COMMAND_ACTIVE:
                image.gameObject.SetActive(ParameterToBool(parameter));
                break;
            case COMMAND_DELETE:
                charaImageList.Remove(image);
                Destroy(image.gameObject);
                break;
            case COMMAND_ANIM:
                ImageSetAnimation(image, parameter);
                break;
           
        }
    }

    //Spriteをファイルから読み出してインスタンス化する
    private Sprite LoadSprite(string name)
    {
        return Instantiate(Resources.Load<Sprite>(spritesDirectory + name));
    }

    //Parameterからベクトルを取得
    private Vector3 ParameterToVector3(string parameter)
    {
        string[] ps = parameter.Replace(" ", "").Split(',');
        return new Vector3(float.Parse(ps[0]), float.Parse(ps[1]), float.Parse(ps[2]));
    }



    //アニメーションを画像に設定する
    private void ImageSetAnimation(Image image, string parameter)
    {
        Animator animator = image.GetComponent<Animator>();
        AnimationClip clip = ParameterToAnimationClip(image, parameter.Split(COMMAND_SEPARATE_ANIM));
        AnimatorOverrideController overrideController;

        if (animator.runtimeAnimatorController is AnimatorOverrideController)
        {
            overrideController = (AnimatorOverrideController)animator.runtimeAnimatorController;
        }
        else
        {
            overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = animator.runtimeAnimatorController;
            animator.runtimeAnimatorController = overrideController;
        }
        overrideController[overrideAnimationClipName] = clip;
        animator.Update(0.0f);
        animator.Play(overrideAnimationClipName, 0);
    }

    //パラメータからアニメーションクリップを生成する
    private AnimationClip ParameterToAnimationClip(Image image, string[] parameters)
    {
        string[] ps = parameters[0].Replace(" ", "").Split(',');
        string path = animationsDirectory + SceneManager.GetActiveScene().name + "/" + image.name;
        AnimationClip prevAnimation = Resources.Load<AnimationClip>(path + "/" + ps[0]);
        AnimationClip animation;
#if UNITY_EDITOR
        if (ps[3].Equals("Replay") && prevAnimation != null)
        {
            return Instantiate(prevAnimation);
        }

        animation = new AnimationClip();
        Color startcolor = image.color;
        Vector3[] start = new Vector3[3];

        start[0] = image.GetComponent<RectTransform>().sizeDelta;
        start[1] = image.GetComponent<RectTransform>().anchoredPosition;
        Color endcolor = startcolor;
        if (parameters[1] != "")
        {
            endcolor = ParameterToColor(parameters[1]);
        }
        Vector3[] end = new Vector3[3];

        for (int i = 0; i < 2; i++)
        {
            if (parameters[i + 2] != "")
            {
                end[i] = ParameterToVector3(parameters[i + 2]);
            }
            else
            {
                end[i] = start[i];
            }
        }
        AnimationCurve[,] curves = new AnimationCurve[4, 4];
        if (ps[3].Equals("EaseInOut"))
        {
            curves[0, 0] = AnimationCurve.EaseInOut(float.Parse(ps[1]), startcolor.r, float.Parse(ps[2]), endcolor.r);
            curves[0, 1] = AnimationCurve.EaseInOut(float.Parse(ps[1]), startcolor.g, float.Parse(ps[2]), endcolor.g);
            curves[0, 2] = AnimationCurve.EaseInOut(float.Parse(ps[1]), startcolor.b, float.Parse(ps[2]), endcolor.b);
            curves[0, 3] = AnimationCurve.EaseInOut(float.Parse(ps[1]), startcolor.a, float.Parse(ps[2]), endcolor.a);
            for (int i = 0; i < 2; i++)
            {
                curves[i + 1, 0] = AnimationCurve.EaseInOut(float.Parse(ps[1]), start[i].x, float.Parse(ps[2]), end[i].x);
                curves[i + 1, 1] = AnimationCurve.EaseInOut(float.Parse(ps[1]), start[i].y, float.Parse(ps[2]), end[i].y);
                curves[i + 1, 2] = AnimationCurve.EaseInOut(float.Parse(ps[1]), start[i].z, float.Parse(ps[2]), end[i].z);
            }
        }
        else
        {
            curves[0, 0] = AnimationCurve.Linear(float.Parse(ps[1]), startcolor.r, float.Parse(ps[2]), endcolor.r);
            curves[0, 1] = AnimationCurve.Linear(float.Parse(ps[1]), startcolor.g, float.Parse(ps[2]), endcolor.g);
            curves[0, 2] = AnimationCurve.Linear(float.Parse(ps[1]), startcolor.b, float.Parse(ps[2]), endcolor.b);
            curves[0, 3] = AnimationCurve.Linear(float.Parse(ps[1]), startcolor.a, float.Parse(ps[2]), endcolor.a);
            for (int i = 0; i < 2; i++)
            {
                curves[i + 1, 0] = AnimationCurve.Linear(float.Parse(ps[1]), start[i].x, float.Parse(ps[2]), end[i].x);
                curves[i + 1, 1] = AnimationCurve.Linear(float.Parse(ps[1]), start[i].y, float.Parse(ps[2]), end[i].y);
                curves[i + 1, 2] = AnimationCurve.Linear(float.Parse(ps[1]), start[i].z, float.Parse(ps[2]), end[i].z);
            }
        }
        string[] b1 = { "r", "g", "b", "a" };
        for (int i = 0; i < 4; i++)
        {
            AnimationUtility.SetEditorCurve(
                animation,
                EditorCurveBinding.FloatCurve("", typeof(Image), "m_Color." + b1[i]),
                curves[0, i]
            );
        }
        string[] a = { "m_SizeDelta", "m_AnchoredPosition" };
        string[] b2 = { "x", "y", "z" };
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                AnimationUtility.SetEditorCurve(
                    animation,
                    EditorCurveBinding.FloatCurve("", typeof(RectTransform), a[i] + "." + b2[j]),
                    curves[i + 1, j]
                );
            }
        }
        if (!Directory.Exists("Assets/Resources/" + path))
        {
            Directory.CreateDirectory("Assets/Resources/" + path);
        }
        AssetDatabase.CreateAsset(animation, "Assets/Resources/" + path + "/" + ps[0] + ".anim");
        AssetDatabase.ImportAsset("Assets/Resources/" + path + "/" + ps[0] + ".anim");
#elif UNITY_STANDALONE_WIN
           animation = prevAnimation;
#endif
        return Instantiate(animation);
    }

    //すべてのアニメーションを一時停止・再開する
    private void StopAllAnimation(bool isStop)
    {
        StopAnimation(isStop, foregroundImage);
        foreach (Image image in charaImageList)
        {
            StopAnimation(isStop, image);
        }
        foreach (Button button in _selectButtonList)
        {
            StopAnimation(isStop, button.image);
        }
    }

    //アニメーションを一時停止・再開する
    private void StopAnimation(bool isStop, Image image)
    {
        Animator animator = image.GetComponent<Animator>();
        if (isStop) animator.speed = 0;
        else animator.speed = 1;
    }

    //BGM設定
    private void SetBackgroundMusic(string cmd, string parameter)
    {
        cmd = cmd.Replace(COMMAND_BGM, "");
        SetAudioSource(cmd, parameter, bgmAudioSource);
    }

    //効果音設定
    private void SetSoundEffect(string name, string cmd, string parameter)
    {
        cmd = cmd.Replace(COMMAND_SE, "");
        name = name.Substring(name.IndexOf('"') + 1, name.LastIndexOf('"') - name.IndexOf('"') - 1);
        AudioSource audio = seList.Find(n => n.name == name);
        if (audio == null)
        {
            audio = Instantiate(Resources.Load<AudioSource>(prefabsDirectory + SE_AUDIOSOURCE_PREFAB),
                seAudioSources.transform);
            audio.name = name;
            seList.Add(audio);
        }
        SetAudioSource(cmd, parameter, audio);
    }

    //音声の設定
    private void SetAudioSource(string cmd, string parameter, AudioSource audio)
    {
        cmd = cmd.Replace(" ", "");
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        switch (cmd)
        {
            case COMMAND_PLAY:
                audio.Play();
                break;
            case COMMAND_MUTE:
                audio.mute = ParameterToBool(parameter);
                break;
            case COMMAND_SOUND:
                audio.clip = LoadAudioClip(parameter);
                break;
            case COMMAND_VOLUME:
                audio.volume = float.Parse(parameter);
                break;
            case COMMAND_PRIORITY:
                audio.priority = int.Parse(parameter);
                break;
            case COMMAND_LOOP:
                audio.loop = ParameterToBool(parameter);
                break;
            case COMMAND_FADE:
                FadeSound(parameter, audio);
                break;
            case COMMAND_ACTIVE:
                audio.gameObject.SetActive(ParameterToBool(parameter));
                break;
            case COMMAND_DELETE:
                seList.Remove(audio);
                Destroy(audio.gameObject);
                break;
        }
    }

    //音声ファイルを読み出しインスタンス化
    private AudioClip LoadAudioClip(string name)
    {
        return Instantiate(Resources.Load<AudioClip>(audioClipsDirectory + name));
    }

    //音声にフェードをかける
    private void FadeSound(string parameter, AudioSource audio)
    {
        string[] ps = parameter.Replace(" ", "").Split(',');
        StartCoroutine(FadeSound(audio, int.Parse(ps[0]), int.Parse(ps[1])));
    }

    //ゲーム中のパラメーター変更
    private void SetParameterForGame(string name, string cmd, string parameter)
    {
        cmd = cmd.Replace(COMMAND_PARAM, "").Replace(" ", "");
        name = name.Substring(name.IndexOf('"') + 1, name.LastIndexOf('"') - name.IndexOf('"') - 1);
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        switch (cmd)
        {
            case COMMAND_WRITE:
                _params[name] = parameter;
                break;
        }
    }

    private bool isStop
    {
        get
        { return !mainText.transform.parent.gameObject.activeSelf || backLog.gameObject.activeSelf; }
    }

    //ゲーム中パラメーターの置き換え
    private string ReplaceParameterForGame(string line)
    {
        string[] lines = line.Split(OUTPUT_PARAM);
        for (int i = 1; i < lines.Length; i += 2)
            lines[i] = _params[lines[i]].ToString();
        return String.Join("", lines);
    }


    private bool ParameterToBool(string parameter)
    {
        string p = parameter.Replace(" ", "");
        return p.Equals("true") || p.Equals("TRUE");
    }

    //Textファイルを読み込む
    private string LoadTextFile(string fname)
    {
        TextAsset textasset = Resources.Load<TextAsset>(fname);
        return textasset.text.Replace("\n", "").Replace("\r", "");
    }


    //パラメータから色追加
    private Color ParameterToColor(string parameter)
    {
        //空白削除、カンマで文字を分ける
        string[] ps = parameter.Replace(" ", "").Split(',');
        //分けた文字列（＝引数）が４つ以上ある場合
        if (ps.Length > 3)
        {
            //透明度設定　文字列をbyte型に直し色作成
            return new Color32(byte.Parse(ps[0]), byte.Parse(ps[1]), byte.Parse(ps[2]), byte.Parse(ps[3]));
        }
        else
        {
            return new Color32(byte.Parse(ps[0]), byte.Parse(ps[1]), byte.Parse(ps[2]), 255);
        }
    }

    private void SetBackgroundImage(string cmd, string parameter)//背景設定
    {
        //空白削除、背景コマンドの文字列も削除
        cmd = cmd.Replace(COMMAND_BACKGROUND, "");

        SetImage(cmd, parameter, backgroundImage);


    }

}
