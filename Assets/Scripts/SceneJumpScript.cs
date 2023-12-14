using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneJumpScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform buttonRectTransform;
    private Vector2 originalSize;
    private Vector2 enlargedSize = new Vector2(200f, 100f);
    private void Start()
    {
        buttonRectTransform = GetComponent<RectTransform>();
        originalSize = buttonRectTransform.sizeDelta;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void OnClickStartButton()
    {
        SceneManager.LoadScene("PrologueScene");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // �}�E�X�J�[�\�����{�^����ɓ��������̏���
        buttonRectTransform.sizeDelta = enlargedSize;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // �}�E�X�J�[�\�����{�^������o�����̏���
        buttonRectTransform.sizeDelta = originalSize;
    }
}
