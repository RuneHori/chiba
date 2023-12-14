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
    //����
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI nameText;
    private const char SEPARATE_MAIN_START = '�u';
    private const char SEPARATE_MAIN_END = '�v';
    private const char SEPARATE_PAGE = '&';//���C���e�L�X�g���̕����̃y�[�W����؂邽�߂̕���
    private Queue<char> charQueue;//���������ԂɊi�[���邽�߂̃L���[
    private Queue<string> pageQueue;//�y�[�W�����ԂɊi�[���邽�߂̃L���[

    [SerializeField] private float captionSpeed = 0.5f;//�e�L�X�g�̕\�����x�𐧌䂷�邽�߂̕ϐ�
    [SerializeField] private GameObject nextPageIcon;//���̃y�[�W�ɐi�ނ��Ƃ��������߂̃A�C�R��



    //�w�i
    [SerializeField] private Image backgroundImage;
    [SerializeField] private string spritesDirectory = "Sprites/";
    private const char SEPARATE_COMMAND = '!';
    private const char COMMAND_SEPARATE_PARAM = '=';
    private const string COMMAND_BACKGROUND = "background";
    private const string COMMAND_SPRITE = "_sprite";
    private const string COMMAND_COLOR = "_color";

    //�L���� 
    [SerializeField] private GameObject charaImages;
    [SerializeField] private string prefabsDirectory = "Prefabs/";
    private const string COMMAND_CHARACTER_IMAGE = "charaimg";
    private const string COMMAND_SIZE = "_size";
    private const string COMMAND_POSITION = "_pos";
    private const string COMMAND_ROTATION = "rotate";
    private const string CHARACTER_IMAGE_PREFAB = "CharaImage";

    private List<Image> charaImageList = new List<Image>();//�L�����N�^�[�̃C���[�W���Ǘ����邽�߂̃��X�g

    //�O���t�@�C��
    [SerializeField] private string textFile = "Texts/";
    private string text = "";

    //���O�E�B���h�E�E�����G��\���^�폜
    private const string COMMAND_ACTIVE = "_active";
    private const string COMMAND_DELETE = "_delete";

    //���x���W�����v
    private const char SEPARATE_SUBSCENE = '#';
    private const string COMMAND_JUMP = "jump_to";
    private Dictionary<string, Queue<string>> _subScenes =
        new Dictionary<string, Queue<string>>();

    //�I����
    private const string COMMAND_SELECT = "select";
    private const string COMMAND_TEXT = "_text";
    private const string SELECT_BUTTON_PREFAB = "SelectButton";
    [SerializeField] private GameObject selectButtons;
    private List<Button> _selectButtonList = new List<Button>();// �I�����{�^�����Ǘ����邽�߂̃��X�g


    //�A�j���[�V����
    private const char COMMAND_SEPARATE_ANIM = '%';
    private const string COMMAND_ANIM = "_anim";
    [SerializeField] private string animationsDirectory = "Animations/";
    [SerializeField] private string overrideAnimationClipName = "Clip";

    //WAIT
    private const string COMMAND_WAIT_TIME = "wait";
    private float waitTime = 0.2f;//�ҋ@���Ԃ�ێ����邽�߂̕ϐ�

    //BGM
    private const string COMMAND_BGM = "bgm";
    private const string COMMAND_SE = "se";
    private const string COMMAND_PLAY = "_play";
    private const string COMMAND_MUTE = "_mute";
    private const string COMMAND_SOUND = "_sound";
    private const string COMMAND_VOLUME = "_volume";
    private const string COMMAND_PRIORITY = "_priority"; //�����̗D��x��ݒ肷��R�}���h
    private const string COMMAND_LOOP = "_loop";
    private const string COMMAND_FADE = "_fade";//�t�F�[�h�C��/�t�F�[�h�A�E�g�𐧌䂷��R�}���h
    private const string SE_AUDIOSOURCE_PREFAB = "SEAudioSource";
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private GameObject seAudioSources;
    [SerializeField] private string audioClipsDirectory = "AudioClips/";
    private List<AudioSource> seList = new List<AudioSource>(); //�Đ�����SE�̃I�[�f�B�I�\�[�X���Ǘ����邽�߂̃��X�g

    //��ʃt�F�[�h
    private const string COMMAND_FOREGROUND = "foreground";
    [SerializeField] private Image foregroundImage;

    //�V�[��
    private const string COMMAND_CHANGE_SCENE = "scene";


    //BackLog
    private const string COMMAND_BACK_LOG = "backlog";
    [SerializeField] private ScrollRect backLog;

    //Game�p�����[�^�Ȃ�
    private const string COMMAND_PARAM = "param";
    private const string COMMAND_WRITE = "_write";
    private const char OUTPUT_PARAM = '"';
    private Dictionary<string, object> _params = new Dictionary<string, object>(); //�p�����[�^

    void Start()
    {
        Init();//����������
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) OnClick();
        if (Input.GetMouseButtonDown(1)) OnClickRight();
        MouseScroll();
    }

    private void Init()//����������
    {
        text = LoadTextFile(textFile);//�e�L�X�g�t�@�C���ǂݍ��݁Btext�Ɋi�[
        //���@�����ŕ������T�u�V�[�����Ƃ̏����i�[�����L���[���쐬
        Queue<string> subScenes = SeparateString(text, SEPARATE_SUBSCENE);
        //�e�T�u�V�[��������
        foreach (string subScene in subScenes)
        {
            //��̃T�u�V�[���͖���
            if (subScene.Equals("")) continue;
            //&�@�����Ńy�[�W���Ƃɕ��������̌��ʂ��i�[�����L���[���쐬
            Queue<string> pages = SeparateString(subScene, SEPARATE_PAGE);
            //�y�[�W�L���[�̐擪�̗v�f�����o�����T�u�V�[�����Ƃ���i�Ⴆ��NEXT�P�j
            _subScenes[pages.Dequeue()] = pages;
        }
        //�ŏ��̃T�u�V�[���̃y�[�W�L���[��pageQueue�ɐݒ肷��
        pageQueue = _subScenes.First().Value;
        //���̃y�[�W��\������
        ShowNextPage();
    }

    private void OnClick()//���N���b�N
    {
        if (isStop) { return; }// �Q�[������~���̏ꍇ�����𒆒f
        // �����L���[�ɂ܂��������c���Ă���ꍇ�S�Ă̕�����\��
        if (charQueue.Count > 0) { OutputAllChar(); }
        else
        {
            // �I�����{�^�����\������Ă���ꍇ�͏������f
            if (_selectButtonList.Count > 0) { return; }

            // ���̃y�[�W���\������Ȃ������ꍇ�A�v���P�[�V�������I��
#if UNITY_EDITOR
            if (!ShowNextPage()) { EditorApplication.isPlaying = false; }
#else
            if (!ShowNextPage()) { Application.Quit(); }
#endif
        }
    }

    private void OnClickRight() //�E�N���b�N
    {
        //���C���e�L�X�g�E�B���h�E�̐e�I�u�W�F�N�g���擾
        GameObject mainWindow = mainText.transform.parent.gameObject;
        //���O�E�B���h�E�̐e�I�u�W�F�N�g���擾
        GameObject nameWindow = nameText.transform.parent.gameObject;

        //���C���e�L�X�g�E�B���h�E�̕\����Ԃ�؂�ւ�
        //���݂̕\����Ԃ̔��΂̏�Ԃɐݒ�
        mainWindow.SetActive(!mainWindow.activeSelf);

        //�������O�E�B���h�E���e�L�X�g�������Ă���ꍇ
        //���C���e�L�X�g�E�B���h�E�Ɠ����\����Ԃɐݒ�i��\�����������\���ɂ���j
        if (nameText.text.Length > 0) { nameWindow.SetActive(mainWindow.activeSelf); }

        // �����L���[�ɕ\�����ׂ��������Ȃ��ꍇ�A���C���e�L�X�g�E�B���h�E�̕\����Ԃɉ�����
        // ���̃y�[�W�A�C�R����\���E��\����
        if (charQueue.Count <= 0) { nextPageIcon.SetActive(mainWindow.activeSelf); }
    }

    private void MouseScroll()//�X�N���[�������ۂ̏���
    {
        //�o�b�N���O�E�B���h�E���\������Ă��āA�X�N���[���_�E�����s���
        //���o�b�N���O�̃X�N���[���ʒu���ŏ㕔�̏ꍇ�A�o�b�N���O�E�B���h�E���\����
        if (backLog.gameObject.activeSelf && Input.mouseScrollDelta.y < 0 && backLog.verticalNormalizedPosition <= 0)
        { backLog.gameObject.SetActive(false); }

        //�o�b�N���O�E�B���h�E����\���ł���A�X�N���[���A�b�v���s��ꂽ�ꍇ
        //�o�b�N���O�̃X�N���[���ʒu���ŏ㕔�ɐݒ�
        if (!backLog.gameObject.activeSelf && Input.mouseScrollDelta.y > 0)
        {
            //�o�b�N���O�̐����X�N���[���ʒu���ŏ㕔�ɐݒ�
            backLog.verticalNormalizedPosition = 0;
            //�o�b�N���O�E�B���h�E��\��
            backLog.gameObject.SetActive(true);

            //�o�b�N���O�E�B���h�E���A�N�e�B�u�ɂȂ������A�����X�N���[���o�[�Ƀt�H�[�J�X���ڂ�
            EventSystem.current.SetSelectedGameObject(backLog.verticalScrollbar.gameObject);
        }
    }

    //��������R���[�`��
    private IEnumerator ShowChars(float wait)
    {
        while (true)//�������[�v
        {
            if (!isStop)//�Q�[������~���Ă��Ȃ��ꍇ�Ɉȉ��̏��������s
            {
                //1�������������\�����S�Ă̕������\�����ꂽ�ꍇ�Ƀ��[�v���I��
                if (!OutputChar()) { break; }
            }
            //�w�肳�ꂽ�ҋ@���ԁiwait�j�����������ꎞ��~
            yield return new WaitForSeconds(wait);
        }
        yield break;
    }

    private IEnumerator WaitForCommand()//���̓ǂݍ��݂�ҋ@����R���[�`��
    {
        float time = 0;//�o�ߎ��ԏ�����
        //�o�ߎ��Ԃ��w�肳�ꂽ�ҋ@���ԁiwaitTime�j�����̊ԁA�ȉ��̏������J��Ԃ�
        while (time < waitTime)
        {
            //�Q�[������~���Ă��Ȃ��ꍇ�o�ߎ��Ԃ����Z
            if (!isStop) { time += Time.deltaTime; }
            yield return null;//1�t���[���ҋ@
        }
        waitTime = 0;//�ҋ@���ԃ��Z�b�g
        ShowNextPage();//���̃y�[�W��\��
        yield break;//�R���[�`�����I��
    }

    //���̃t�F�[�h�Ǘ��R���[�`��
    private IEnumerator FadeSound(AudioSource audio, float time, float volume)
    {
        // �t�F�[�h�ɂ����鎞�ԁitime�j�ƖڕW�̉��ʁivolume�j����A�t���[�����Ƃ̉��ʂ̕ω��ʂ��v�Z
        float vo = (volume - audio.volume) / (time / Time.deltaTime);
        //���݂̉��ʂ��ڕW�̉��ʂ��傫���ꍇ�A���ʂ�������t�F�[�h �������s�����߂�isOut�Ƃ����u�[���ϐ����`
        bool isOut = audio.volume > volume;

        //���݂̉��ʂ��ڕW�̉��ʂɒB����܂ŁA�ȉ��̏������J��Ԃ�
        while ((!isOut && audio.volume < volume) || (isOut && audio.volume > volume))
        {
            audio.volume += vo;//���ʂ�ω��ʁivo�j�����X�V
            yield return null;
        }
        audio.volume = volume;//���ʂ�ڕW�̉��ʁivolume�j�ɐݒ�
    }


    private bool OutputChar()//�P�������o��
    {
        //�L���[����̏ꍇnextPageIcon ��\��
        if (charQueue.Count <= 0)
        {
            nextPageIcon.SetActive(true);
            return false;
        }
        else
        {
            mainText.text += charQueue.Dequeue(); //�L���[����l�����o���ăL���[������폜
            return true;
        }

    }

    private void OutputAllChar()//�S���o��
    {
        StopCoroutine(ShowChars(captionSpeed));//�R���[�`��STOP
        while (OutputChar()) ;//�L���[����ɂȂ�܂ŕ\��
        waitTime = 0;
        nextPageIcon.SetActive(true);
    }

    private bool ShowNextPage()//���y�[�W�\��
    {

        if (pageQueue.Count <= 0) { return false; }   //���̃y�[�W�����݂��Ȃ�
        else
        {
            nextPageIcon.SetActive(false);
            ReadLine(pageQueue.Dequeue());
            return true;
        }
    }

    //��������w�肵���������Ƃɋ�؂�Queue�Ɋi�[�������̂�Ԃ�
    private Queue<string> SeparateString(string str, char sep)
    {
        string[] strs = str.Split(sep);
        Queue<string> queue = new Queue<string>();
        foreach (string l in strs) queue.Enqueue(l);
        return queue;
    }

    //�������P�������Ƃɋ�؂�Queue�Ɋi�[�������̂�Ԃ�
    private Queue<char> SepareteString(string str)
    {
        char[] chars = str.ToCharArray();
        Queue<char> charQueue = new Queue<char>();
        //foreach���Ŕz��char�Ɋi�[���ꂽ���������ׂĎ��o����Queue�ɉ�����
        foreach (char c in chars) charQueue.Enqueue(c);
        return charQueue;
    }

    private void ReadLine(string text)//1�s��ǂݏo��
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
        //'�u'�̈ʒu�ŕ�����𕪂���
        string[] ts = text.Split(SEPARATE_MAIN_START);
        string name = ts[0];//���O���
                            //�Ō�̕����������폜���đ���i��ex.HelloWorld!)
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
        StartCoroutine(ShowChars(captionSpeed)); //�R���[�`���Ăяo��

        backLog.content.GetComponentInChildren<TextMeshProUGUI>().text +=
       Environment.NewLine + Environment.NewLine;
    }

    private void ReadCommand(string cmdLine)//�R�}���h�ǂݏo��
    {
        //�ŏ��́I�폜
        cmdLine = cmdLine.Remove(0, 1);
        Queue<string> cmdQueue = SeparateString(cmdLine, SEPARATE_COMMAND);
        foreach (string cmd in cmdQueue)
        {
            //�u���v�ŕ�����
            string[] cmds = cmd.Split(COMMAND_SEPARATE_PARAM);
            //�����w�i�R�}���h�̕����񂪊܂܂�Ă�����
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

    private void ChangeNextScene(string parameter)//�Ή��V�[���ɐ؂�ւ���
    {
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        SceneManager.LoadSceneAsync(parameter);
    }

    private void JumpTo(string parameter)//�Ή����郉�x���܂ŃW�����v
    {
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        pageQueue = _subScenes[parameter];
    }

    private void SetWaitTime(string parameter) //�ҋ@���Ԑݒ�
    {
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        waitTime = float.Parse(parameter);
    }

    private void WriteBackLog(string parameter)//�o�b�N���O���L�q
    {
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        backLog.content.GetComponentInChildren<TextMeshProUGUI>().text += parameter + Environment.NewLine + Environment.NewLine;
    }



    private void SetForegroundImage(string cmd, string parameter)  //�O�i�ݒ�
    {
        cmd = cmd.Replace(COMMAND_FOREGROUND, "");
        SetImage(cmd, parameter, foregroundImage);
    }


    //�����G�ݒ�
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

    //�I�����̐ݒ�
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

    private void SelectButtonOnClick(string label)//�I�������N���b�N���ꂽ
    {
        foreach (Button button in _selectButtonList) Destroy(button.gameObject);
        _selectButtonList.Clear();
        JumpTo('"' + label + '"');
        ShowNextPage();
    }

    //�摜�̐ݒ�
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

    //Sprite���t�@�C������ǂݏo���ăC���X�^���X������
    private Sprite LoadSprite(string name)
    {
        return Instantiate(Resources.Load<Sprite>(spritesDirectory + name));
    }

    //Parameter����x�N�g�����擾
    private Vector3 ParameterToVector3(string parameter)
    {
        string[] ps = parameter.Replace(" ", "").Split(',');
        return new Vector3(float.Parse(ps[0]), float.Parse(ps[1]), float.Parse(ps[2]));
    }



    //�A�j���[�V�������摜�ɐݒ肷��
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

    //�p�����[�^����A�j���[�V�����N���b�v�𐶐�����
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

    //���ׂẴA�j���[�V�������ꎞ��~�E�ĊJ����
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

    //�A�j���[�V�������ꎞ��~�E�ĊJ����
    private void StopAnimation(bool isStop, Image image)
    {
        Animator animator = image.GetComponent<Animator>();
        if (isStop) animator.speed = 0;
        else animator.speed = 1;
    }

    //BGM�ݒ�
    private void SetBackgroundMusic(string cmd, string parameter)
    {
        cmd = cmd.Replace(COMMAND_BGM, "");
        SetAudioSource(cmd, parameter, bgmAudioSource);
    }

    //���ʉ��ݒ�
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

    //�����̐ݒ�
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

    //�����t�@�C����ǂݏo���C���X�^���X��
    private AudioClip LoadAudioClip(string name)
    {
        return Instantiate(Resources.Load<AudioClip>(audioClipsDirectory + name));
    }

    //�����Ƀt�F�[�h��������
    private void FadeSound(string parameter, AudioSource audio)
    {
        string[] ps = parameter.Replace(" ", "").Split(',');
        StartCoroutine(FadeSound(audio, int.Parse(ps[0]), int.Parse(ps[1])));
    }

    //�Q�[�����̃p�����[�^�[�ύX
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

    //�Q�[�����p�����[�^�[�̒u������
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

    //Text�t�@�C����ǂݍ���
    private string LoadTextFile(string fname)
    {
        TextAsset textasset = Resources.Load<TextAsset>(fname);
        return textasset.text.Replace("\n", "").Replace("\r", "");
    }


    //�p�����[�^����F�ǉ�
    private Color ParameterToColor(string parameter)
    {
        //�󔒍폜�A�J���}�ŕ����𕪂���
        string[] ps = parameter.Replace(" ", "").Split(',');
        //������������i�������j���S�ȏ゠��ꍇ
        if (ps.Length > 3)
        {
            //�����x�ݒ�@�������byte�^�ɒ����F�쐬
            return new Color32(byte.Parse(ps[0]), byte.Parse(ps[1]), byte.Parse(ps[2]), byte.Parse(ps[3]));
        }
        else
        {
            return new Color32(byte.Parse(ps[0]), byte.Parse(ps[1]), byte.Parse(ps[2]), 255);
        }
    }

    private void SetBackgroundImage(string cmd, string parameter)//�w�i�ݒ�
    {
        //�󔒍폜�A�w�i�R�}���h�̕�������폜
        cmd = cmd.Replace(COMMAND_BACKGROUND, "");

        SetImage(cmd, parameter, backgroundImage);


    }

}
