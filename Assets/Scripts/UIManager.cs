using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
#region Singleton
    public static UIManager instance;
    void Awake() {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
#endregion


    public GameObject GameOverScreen; //panel that shows up when game is over
    public TMP_Text GameOverText;     //text component that displays win or lose message
    public TMP_Text PopupText;        //text component that shows up whenever something important in game happens

    public void DisplayGameOver(bool win)
    {
        GameOverScreen.SetActive(true);
        GameOverText.text = win ? "Checkmate! You Won" : "You Lost";
    }

    public void ShowPopup(string message, float duration)
    {
        StartCoroutine(ShowPopupCoroutine(message, duration));
    }
    IEnumerator ShowPopupCoroutine(string message, float duration)
    {
        PopupText.text = message;
        PopupText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        PopupText.gameObject.SetActive(false);
    }
}
