using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharaSelectScript : MonoBehaviour
{
    public int index;//�I�����ꂽ�L�����̃C���f�b�N�X

    public Button angryButton;
    public Button disgustButton;
    public Button fearButton;
    public Button sadButton;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;

        // �e�{�^����OnClick�C�x���g��ݒ�
        angryButton.onClick.AddListener(AngryStory);
        disgustButton.onClick.AddListener(DisgustStory);
        fearButton.onClick.AddListener(FearStory);
        sadButton.onClick.AddListener(SadStory);
    }

    // �{��̃X�g�[���[��I�������ꍇ�̏���
    public void AngryStory()
    {
        if (gameManager != null)
        {
            //�^�O�Ǘ����Ă���p�Y���Q�[���̖ڕW��ikari�ɏ���������
            GameManager.targetTag = "angry";
            SetTargetImage(0);
            //�{��̃X�g�[���[�ɐi��
            SceneManager.LoadScene(GameManager.scene[0, 0]);
        }
    }

    // �����̃X�g�[���[��I�������ꍇ�̏���
    public void DisgustStory()
    {
        if (gameManager != null)
        {
            //�^�O�Ǘ����Ă���p�Y���Q�[���̖ڕW��disgust�ɏ���������
            GameManager.targetTag = "disgust";
            SetTargetImage(1);
            //�����̃X�g�[���[�ɐi��
            SceneManager.LoadScene(GameManager.scene[1, 0]);
        }
    }
    // ���|�̃X�g�[���[��I�������ꍇ�̏���
    public void FearStory()
    {
        if (gameManager != null)
        {
            //�^�O�Ǘ����Ă���p�Y���Q�[���̖ڕW��fear�ɏ���������
            GameManager.targetTag = "fear";
            SetTargetImage(2);
            //���|�̃X�g�[���[�ɐi��
            SceneManager.LoadScene(GameManager.scene[2, 0]);
        }
    }
    // �߂��݂̃X�g�[���[��I�������ꍇ�̏���
    public void SadStory()
    {
        if (gameManager != null)
        {
            //�^�O�Ǘ����Ă���p�Y���Q�[���̖ڕW��sad�ɏ���������
            GameManager.targetTag = "sad";
            SetTargetImage(3);
            //�߂��݂̃X�g�[���[�ɐi��
            SceneManager.LoadScene(GameManager.scene[3, 0]);
        }
    }

    void SetTargetImage(int index)
    {
        //�p�Y����ʂɕ\�������L�����N�^�[�Ə����ڕW�^�[�Q�b�g�̉摜��
        ////�I�����ꂽ�摜�ɕύX
        gameManager.targetChara = gameManager.sourceImages[index];
        gameManager.targetEmo = gameManager.emotionImages[index];
        this.index = index;//index�ێ�
    }

}
