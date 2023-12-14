using UnityEngine;

public class PuzzleMoveScript : MonoBehaviour
{
    private PazzleScript puzzleScript;

    //���g�̓����Ă���z��̍��W
    public int myColumn;//��
    public int myRow;//�s

    //�X���C�v�����Ƃ��̍��W���m�F
    private Vector2 mouseDown;
    private Vector2 mouseUp;
    private Vector2 distance;

    //�אڂ���p�Y���̎Q�Ƃ�ێ�
    private GameObject neighborPuzzle;
    //�p�Y����3�̃}�b�`���O�O���[�v�ɏ������Ă��邩�ǂ����������t���O
    public bool is3Matching;

    //�ړ��O�̍��W�ێ�
    public Vector2 myPreviousPos;

    //�p�Y�����ړ������ǂ����������t���O
    //�ړ�����Puzzle�𑀍�ł��Ȃ��悤��
    public bool isDontMoving;

    void Start()
    {
        //PazzleScript�̃C���X�^���X��������
        puzzleScript = FindObjectOfType<PazzleScript>();

        //�p�Y���̈ʒu�����W�z��̔ԍ��i�C���f�b�N�X�j�ɐݒ�
        myColumn = (int)transform.position.x;
        myRow = (int)transform.position.y;

        //myPreviousPos�Ɍ��݂̈ʒu(�X�^�[�g�ʒu)���L�^
        myPreviousPos = new Vector2(myColumn, myRow);
    }

    public void OnMouseDown()
    {    //�}�E�X�̍��{�^���� �����ꂽ�Ƃ�
        if (Input.GetMouseButtonDown(0))
        {
            //�}�E�X�̌��݂̍��W��Camera.main.ScreenToWorldPoint(Input.mousePosition)
            //���g�p���ă��[���h���W�ɕϊ����AmouseDown�Ɋi�[
            mouseDown = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public void OnMouseUp()
    {
        //isDontMoving��true�ł���΁A�֐����I��
        if (isDontMoving) { return; }

        //�}�E�X�͂Ȃ����Ƃ�
        if (Input.GetMouseButtonUp(0))
        {
            //�}�E�X�̃{�^���������ꂽ���̍��W���擾�@mouseUp�Ɋi�[
            mouseUp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //mouseDown mouseUp�̃x�N�g���̍����v�Z
            distance = mouseUp - mouseDown;
            movePuzzle();//�p�Y���̈ړ����s��
             if (isDontMoving) { return; }   //isDontMoving��true�ł���΁A�֐����I��
        }
    }

    // Update is called once per frame
    void Update()
    {
        //���݂̍��W�ƁAcolumn�Arow�̒l���قȂ�Ƃ�
        if (transform.position.x != myColumn || transform.position.y != myRow)
        {
            //column,row�̈ʒu�ɏ��X�Ɉړ�����
            transform.position = Vector2.Lerp(transform.position, new Vector2(myColumn, myRow), 0.3f);
            //���݂̈ʒu�ƁA�ړI�n(column,row)�Ƃ̋����𑪂�
            Vector2 dif = (Vector2)transform.position - new Vector2(myColumn, myRow);
            //�ړI�n�Ƃ̋�����0.1f��菬�����Ȃ�����
            if (Mathf.Abs(dif.magnitude) < 0.1f)
            {
                transform.position = new Vector2(myColumn, myRow);
                //���g��PuzzleArray�z��Ɋi�[����
                SetPuzzleToArray();
            }
        }
        //�������O�s�ځi��ԉ��j�ł͂Ȃ��A���A����Puzzle���Ȃ��ꍇ�A����������
        else if (myRow > 0 && puzzleScript.puzzleArray[myColumn, myRow - 1] == null)
        {
            FallPuzzle();
        }
    }


    void movePuzzle()
    {
        //���ׂĂ�Puzzle�𑀍�ł��Ȃ��悤�ɂ���
        puzzleScript.DontMovePuzzle();

        //�E�ɃX���C�v���Ă����Ȃ�iMathf.Abs�Ƃ͐�Βl�������j
        if (distance.x >= 0 && Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
        {
            //���g����ԉE�ɂ��Ȃ��ꍇ�A�ƂȂ��Puzzle�ƈʒu����������
            if (myColumn < 5)
            {
                //�E�ׂ��vPuzzle����neighborPuzzle�ɑ��
                neighborPuzzle = puzzleScript.puzzleArray[myColumn + 1, myRow];
                //�ׂ�Puzzle���P�񍶂�
                neighborPuzzle.GetComponent<PuzzleMoveScript>().myColumn -= 1;
                myColumn += 1; //���g�͂P��E��
            }
        }
        //���ɃX���C�v���Ă����Ȃ�
        if (distance.x < 0 && Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
        {
            //���g����ԍ��ɂ��Ȃ��ꍇ�A�ƂȂ��Puzzle�ƈʒu����������
            if (myColumn > 0)
            {
                //���ׂ��vPuzzle�����擾
                neighborPuzzle = puzzleScript.puzzleArray[myColumn - 1, myRow];
                //�ׂ�Puzzle���P��E��
                neighborPuzzle.GetComponent<PuzzleMoveScript>().myColumn += 1;
                myColumn -= 1;//���g�͂P�񍶂�
            }
        }

        //��ɃX���C�v���Ă����Ȃ�
        if (distance.y >= 0 && Mathf.Abs(distance.x) < Mathf.Abs(distance.y))
        {
            //���g����ԏ�ɂ��Ȃ��ꍇ�A�ƂȂ��Puzzle�ƈʒu����������
            if (myRow < 5)
            {
                //���Puzzle�����擾
                neighborPuzzle = puzzleScript.puzzleArray[myColumn, myRow + 1];
                //�ׂ�Puzzle���P�s����
                neighborPuzzle.GetComponent<PuzzleMoveScript>().myRow -= 1;
                myRow += 1; //���g�͂P�s���
            }
        }

        //���ɃX���C�v���Ă����Ȃ�
        if (distance.y < 0 && Mathf.Abs(distance.x) < Mathf.Abs(distance.y))
        {
            //���g����ԉ��ɂ��Ȃ��ꍇ�A�ƂȂ��Puzzle�ƈʒu����������
            if (myRow > 0)
            {
                //����Puzzle�����擾
                neighborPuzzle = puzzleScript.puzzleArray[myColumn, myRow - 1];
                //�ׂ�Puzzle���P�s���
                neighborPuzzle.GetComponent<PuzzleMoveScript>().myRow += 1;
                //���g�͂P �s����
                myRow -= 1;
            }
        }
        Invoke("DoCheckMatching", 0.5f);
    }

    void DoCheckMatching()
    {
        puzzleScript.CheckMatching();
    }

    //PuzzleArray�z��ɁA���g���i�[����
    public void SetPuzzleToArray()
    {
        puzzleScript.puzzleArray[myColumn, myRow] = gameObject;
    }

    void FallPuzzle()
    {
        //�����̂����z�����ɂ���
        puzzleScript.puzzleArray[myColumn, myRow] = null;
        //���������Ɉړ�������
        myRow -= 1;
    }

    //���̈ʒu�ɖ߂�
    public void BackToPreviousPos()
    {
        myColumn = (int)myPreviousPos.x;
        myRow = (int)myPreviousPos.y;
    }
}
