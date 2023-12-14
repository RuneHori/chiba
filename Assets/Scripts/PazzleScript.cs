using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PazzleScript : MonoBehaviour
{
    GameManager gameManager = GameManager.instance;

    //�p�Y���̎�ނ��i�[���邽�߂̔z��ϐ�
    public GameObject[] puzzles;
    //�z��̑傫����`
    private int width = 6;
    private int height = 6;

    //�p�Y���̔z�u���Ǘ����邽�߂�2�����z��
    public GameObject[,] puzzleArray = new GameObject[6, 6];
    //�폜����p�Y�����i�[���邽�߂̃��X�g
    private List<GameObject> deleteList = new List<GameObject>();

    //�Q�[�����X�^�[�g���邩�ǂ���
    private bool isGameStart;

    //�X�R�A��\��
    public TMP_Text scoreText;
    //�L�����Z���N�g��ʂőI�����ꂽ�L�����Ɗ���iTarget�j�̉摜��\�����邽�߂̕ϐ�
    public Image charaImg;
    public Image emoImg;

    void Start()
    {
        //�L�����I����ʂőI�΂ꂽ�摜�����蓖�Ă�
        charaImg.sprite = gameManager.targetChara;
        emoImg.sprite = gameManager.targetEmo;

        scoreText.text = GameManager.target.ToString();
        CreatePuzzle();
    }

    //�p�Y���𐶐�
    void CreatePuzzle()
    {
        //2�d���[�v�̎w�肳�ꂽ�͈͓��Ńp�Y���̈ʒu����
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //�O����R�܂ł̃����_���Ȑ��l���C���f�b�N�X�Ƃ��Ďg�p���A�p�Y���𐶐�
                int random = UnityEngine.Random.Range(0, 4);
                if (puzzles[random] != null)
                {
                    var puzzle = Instantiate(puzzles[random]);
                    //�������ꂽ�p�Y����(i,j)�̍��\���ݒ�
                    puzzle.transform.position = new Vector2(i, j);
                    //�p�Y���̈ʒu����puzzleArray�z��̑Ή�����v�f�Ɋi�[
                    puzzleArray[i, j] = puzzle;
                }
            }
        }
        CheckStartset();
    }

    //�Q�[���J�n���Ƀp�Y���̏����z�u��3�ȏ�̃p�Y����
    //�������܂��͏c�����ɘA�����Ă��邩�ǂ������m�F���}�b�`���O�����p�Y������������
    void CheckStartset()
    {
        //�������Ɍ������ă��R�̂Ȃ�����m�F
        for (int i = 0; i < height; i++)
        {
            for (int x = 0; x < width - 2; x++) //�E����Q�ڈȍ~�͊m�F�s�v(width-2)
            {
                if ((puzzleArray[x, i].tag == puzzleArray[x + 1, i].tag) && (puzzleArray[x, i].tag == puzzleArray[x + 2, i].tag))
                {
                    puzzleArray[x, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    puzzleArray[x + 1, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    puzzleArray[x + 2, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                }
            }
        }

        //������E�Ɍ������ă^�e�̂Ȃ�����m�F
        for (int i = 0; i < width; i++)
        {
            for (int y = 0; y < height - 2; y++) //�ォ��Q�ڈȍ~�͊m�F�s�v(height-2)
            {
                if ((puzzleArray[i, y].tag == puzzleArray[i, y + 1].tag) && (puzzleArray[i, y].tag == puzzleArray[i, y + 2].tag))
                {
                    puzzleArray[i, y].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    puzzleArray[i, y + 1].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    puzzleArray[i, y + 2].GetComponent<PuzzleMoveScript>().is3Matching = true;
                }
            }
        }

        //�}�b�`���O�����p�Y���̏���
        foreach (var item in puzzleArray)
        {
            if (item != null && item.GetComponent<PuzzleMoveScript>().is3Matching)
            {
                //is3Matching�t���O��true�ƂȂ��Ă���p�Y����deleteList�ɒǉ�
                deleteList.Add(item);
            }
        }

        //deleteList���Ƀp�Y�������݂���ꍇ
        if (deleteList.Count > 0)
        {
            //�Y������z��(puzzleArray)��null�ɂ���(�����Ǘ�)
            //�p�Y���̃Q�[���I�u�W�F�N�g��j���B��ʏォ��p�Y��������
            foreach (var item in deleteList)
            {
                puzzleArray[(int)item.transform.position.x, (int)item.transform.position.y] = null;
                Destroy(item);
            }

            //�N���A�ɂ���
            deleteList.Clear();
            //�V�����p�Y���𐶐��B�󂢂��ӏ��ɔz�u
            SpawnNewPuzzle();
        }
        else�@//����deleteList����ł���ꍇ�AisGameStart�t���O��true�ɐݒ�
        {
            isGameStart = true;
        }

    }

    //�V�����p�Y���𐶐��B�󂢂��ӏ��ɔz�u
    void SpawnNewPuzzle()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //����puzzleArray[i, j]�̈ʒu����inull�j�ł���ꍇ�V�����p�Y���𐶐�
                if (puzzleArray[i, j] == null)
                {
                    int random = UnityEngine.Random.Range(0, 4);
                    if (puzzles[random] != null)
                    {
                        var puzzle = Instantiate(puzzles[random]);
                        //�����ځi�\���ʒu�j�̏���
                        puzzle.transform.position = new Vector2(i, j + 0.3f);
                        //�����Ǘ��̏���
                        puzzleArray[i, j] = puzzle;
                    }
                }
            }
        }

        //�p�Y���̏����z�u�ł̃}�b�`���O���m�F���Ă��邩�ǂ���
        //���Ă��Ȃ��ꍇ�ifalse�jCheckStartset�֐��Ăяo��
        if (isGameStart == false)
        {
            CheckStartset();
        }
        else
        {
            //�e�p�Y���̈ʒu����myPreviousPos�Ƃ��Đݒ�
            //myPreviousPos�̓p�Y�����ړ�����ۂɑO�̈ʒu����ێ����邽�߂Ɏg�p�����ϐ�
            foreach (var item in puzzleArray)
            {
                int column = (int)item.transform.position.x;
                int row = (int)item.transform.position.y;
                item.GetComponent<PuzzleMoveScript>().myPreviousPos = new Vector2(column, row);
            }
            //�������܂ɂR�ȏセ����Ă��邩�ǂ�������
            Invoke("CheckMatching", 0.2f);
        }
    }

    public void CheckMatching()
    {
        //���̍s���烈�R�̂Ȃ�����m�F
        for (int i = 0; i < height; i++)
        {
            //�E����Q�ڈȍ~�͊m�F�s�v
            for (int x = 0; x < width - 2; x++)
            {
                //�^�O����v���Ă��邩�ǂ����m�F
                if (puzzleArray[x, i] != null && puzzleArray[x, i].tag == puzzleArray[x + 1, i].tag &&
                    puzzleArray[x, i].tag == puzzleArray[x + 2, i].tag)
                {
                    if (puzzleArray[x, i] != null)
                    {
                        //PuzzleMoveScript��isMatching��true��
                        puzzleArray[x, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                        puzzleArray[x + 1, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                        puzzleArray[x + 2, i].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    }
                }
            }
        }

        //���̗񂩂�^�e�̂Ȃ�����m�F
        for (int i = 0; i < width; i++)
        {
            //�ォ��Q�ڈȍ~�͊m�F�s�v
            for (int y = 0; y < height - 2; y++)
            {
                if ((puzzleArray[i, y].tag == puzzleArray[i, y + 1].tag) &&
                    (puzzleArray[i, y].tag == puzzleArray[i, y + 2].tag))
                {
                    //�^�O����v���Ă��邩�ǂ����m�F
                    if (puzzleArray[y, i] != null)
                    {
                        //PuzzleMoveScript��isMatching��true��
                        puzzleArray[i, y].GetComponent<PuzzleMoveScript>().is3Matching = true;
                        puzzleArray[i, y + 1].GetComponent<PuzzleMoveScript>().is3Matching = true;
                        puzzleArray[i, y + 2].GetComponent<PuzzleMoveScript>().is3Matching = true;
                    }
                }
            }
        }

        //isMatching=true�̂��̂��p�Y���̕\���𔼓����ɐݒ�
        foreach (var item in puzzleArray)
        {
            if (item.GetComponent<PuzzleMoveScript>().is3Matching)
            {
                item.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);//������
                deleteList.Add(item);//�p�Y����deleteList�ɒǉ�
            }
        }

        //List����Puzzle������ꍇ
        if (deleteList.Count > 0)
        {
            //0.2�b���DeletePuzzle���\�b�h���ďo
            //���̃��\�b�h�́AdeleteList���̃p�Y�������ۂɍ폜
            Invoke("DeletePuzzle", 0.2f);
        }
        else
        {
            //�p�Y���̈ʒu�����̈ʒu�ɖ߂����߂�BackToPreviousPos���\�b�h�ďo
            //���O�̈ʒu�Ɉړ������p�Y�������̈ʒu�ɖ߂�
            foreach (var item in puzzleArray)
            {
                item.GetComponent<PuzzleMoveScript>().BackToPreviousPos();
            }
        }
        Invoke("CanMovePuzzle", 0.4f);//�p�Y���̈ړ����ĊJ
    }

    //�}�b�`���O�����p�Y���̍폜�⓾�_�̍X�V�B�V�����p�Y���̐���
    void DeletePuzzle()
    {
        //deleteList����Puzzle���폜���ApuzzleArray��null��
        foreach (var item in deleteList)
        {
            Destroy(item);//�폜
            puzzleArray[(int)item.transform.position.x, (int)item.transform.position.y] = null;//NULL��

           //�����p�Y����GameManager.targetTag�Ƃ����^�O�����ꍇ�ȉ��̏������s��
            if (item.CompareTag(GameManager.targetTag))
            {
                GameManager.target--;//�ڕW�������̐���1���炷�i�R�ō폜�Ȃ̂ōŒ�R����j
                if (GameManager.target <= 0)//�����ڕW���������O�ȉ��ɂȂ����ꍇ
                {
                    GameManager.target = 0;//�ڕW�������ɂO����
                    GameManager.cnt++;//�p�Y���̃v���C�����J�E���g���Ă���ϐ��Ɂ{�P�ǉ�
                    GameManager.sceneCnt++;//�ǂ��̃V�[���ɑJ�ڂ��邩���Ǘ����Ă���ϐ��Ɂ{�P�ǉ�
                    GameManager.SetTarget();//�ڕW�������̍Đݒ肷��֐��Ăяo��
                    GameManager.SetScene();//�ǂ̃V�[���ɑJ�ڂ��邩�m�肷��֐��ďo
                    SceneManager.LoadScene(GameManager.targetScene);//�V�[���J��
                }
            }
        }
       scoreText.text = GameManager.target.ToString();

        //List������ۂ�
        deleteList.Clear();
        //�p�Y���̐����i�V�����p�Y���̔z�u�j�������Ȃ�
        Invoke("SpawnNewPuzzle", 1.2f);
    }

    //isDontMoving�t���O��true�ɐݒ�
    //����ɂ��A�����̃p�Y���͈ړ��s��
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

    //�ړ����\�ȃp�Y���ɑ΂���isDontMoving�t���O��false�ɐݒ�
    // ����ɂ��A�p�Y���͍Ăшړ��\��
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
