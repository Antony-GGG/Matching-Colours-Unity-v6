using System.Collections;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] APIManager _APIManager;

    public BottleController FirstBottle;
    public BottleController SecondBottle;
    public BottleController[] bottles;

    private bool allFull = false; // all bottles are full

    public int levelToUnlock;
    int numberOfUnlockedLevel;

    [Range(0, 10)] int completedLevel;

    public GameObject LevelCompletedCanvas;
    public GameObject GameOverCanvas;

    private float bottleUp = 0.3f; // select bottle
    private float bottleDown = -0.3f; // deselect bottle

    bool timerIsRunning;
    bool gameOver;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float timeElapsed;
    float[] timeThresholds = { 50f, 100f };

    [SerializeField] TextMeshProUGUI scoreText;
    int playerScore;
    int[] score = { 100, 200, 300 };

    void Start()
    {

        if (!PlayerPrefs.HasKey("PlayerScore"))
        {
            PlayerPrefs.SetInt("PlayerScore", 00);
        }

        if (!PlayerPrefs.HasKey("CompletedLevels"))
        {
            PlayerPrefs.SetInt("CompletedLevels", 0);
        }

        playerScore = PlayerPrefs.GetInt("PlayerScore");

        timerIsRunning = true;

        scoreText.text = "Score : " + PlayerPrefs.GetInt("PlayerScore").ToString();

    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeElapsed > 1f)
            {
                // Reduce the timer
                timeElapsed -= Time.deltaTime;
                DisplayTime(timeElapsed);
            }
            else
            {
                GameOver();
            }
        }

        if (Input.GetMouseButtonDown(0) && !LevelCompletedCanvas.activeInHierarchy && !GameOverCanvas.activeInHierarchy)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.GetComponent<BottleController>() != null)
                {
                    if (FirstBottle == null)
                    {
                        FirstBottle = hit.collider.GetComponent<BottleController>();

                        if (FirstBottle.numberOfColorsInBottle != 0)
                        {
                            FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x,
                                                                         FirstBottle.transform.position.y + bottleUp,
                                                                         FirstBottle.transform.position.z);
                        }
                    }
                    else
                    {
                        if (FirstBottle == hit.collider.GetComponent<BottleController>())
                        {
                            if (FirstBottle.numberOfColorsInBottle != 0)
                            {
                                FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x,
                                                 FirstBottle.transform.position.y + bottleDown,
                                                 FirstBottle.transform.position.z);
                            }
                            FirstBottle = null;
                        }
                        else
                        {
                            SecondBottle = hit.collider.GetComponent<BottleController>();
                            FirstBottle.bottleControllerRef = SecondBottle;

                            FirstBottle.UpdateTopColorValue();
                            SecondBottle.UpdateTopColorValue();

                            if (SecondBottle.FillBottleCheck(FirstBottle.topColor) == true)
                            {
                                FirstBottle.startColorTransfer();
                                FirstBottle = null;
                                SecondBottle = null;
                            }
                            else
                            {
                                if (FirstBottle.numberOfColorsInBottle != 0)
                                {
                                    FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x,
                                                                                 FirstBottle.transform.position.y + bottleDown,
                                                                                 FirstBottle.transform.position.z);
                                }
                                FirstBottle = null;
                                SecondBottle = null;
                            }
                        }
                    }
                }
            }
            /*else // tab anywhere on the screen to deselect bottles
            {
                if (FirstBottle != null)
                {
                    if (FirstBottle.numberOfColorsInBottle != 0)
                    {
                        FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x,
                                                                     FirstBottle.transform.position.y + bottleDown,
                                                                     FirstBottle.transform.position.z);
                        FirstBottle = null;
                    }
                    if (SecondBottle != null)
                    {
                        FirstBottle = null;
                        SecondBottle = null;
                    }
                }
            }*/
        }

        if (allFull == false) // keep checking on bottles
        {
            StartCoroutine(AllBottlesAreFull());
        }
    }

    public void DisplayTime(float timeToDisplay)
    {
        int timeToDisp = Mathf.FloorToInt(timeToDisplay);
        timerText.text = timeToDisp.ToString() + "s";
    }

    IEnumerator AllBottlesAreFull() // check to completing the level
    {
        if (bottles.All(y => y.numberOfColorsInBottle == 0 || y.numberOfTopColorLayer == 4))
        {
            allFull = true;

            timerIsRunning = false;

            yield return new WaitForSeconds(2f);

            Win();
        }
    }

    private void GameOver()
    {
        gameOver = true;

        if (gameOver)
        {
            timerIsRunning = false;

            if (!GameOverCanvas.activeInHierarchy)
            {
                GameOverCanvas.SetActive(true);

                _APIManager.UpdateGameScore(0, "loss", levelToUnlock);

            }
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(levelToUnlock + 1);
    }

    private void Win()
    {
        if (allFull == true)
        {
            float timeUsed = 500f - timeElapsed;
            timerIsRunning = false;

            numberOfUnlockedLevel = PlayerPrefs.GetInt("LevelIsUnlocked");
            completedLevel = PlayerPrefs.GetInt("CompletedLevels");

            if (numberOfUnlockedLevel <= levelToUnlock)
            {
                if ((numberOfUnlockedLevel + 1) <= 10)
                {
                    PlayerPrefs.SetInt("LevelIsUnlocked", numberOfUnlockedLevel + 1);
                }
            }

            FindFirstObjectByType<AudioManager>().Play("WinSound");

            if (levelToUnlock > completedLevel)
            {
                PlayerPrefs.SetInt("CompletedLevels", completedLevel + 1);

                if (levelToUnlock % GrandAdManager.instance.adsAfter == 0)
                {
                    Debug.Log(levelToUnlock);
                    GrandAdManager.instance.ShowAd("startAd");
                }

                if (timeUsed <= timeThresholds[0])
                {
                    playerScore += score[2]; // Highest score for quickest completion

                    PlayerPrefs.SetInt("PlayerScore", playerScore);

                    _APIManager.coinsEarningLevelBased(levelToUnlock);

                    _APIManager.UpdateGameScore(score[2], "win", levelToUnlock);
                }
                else if (timeUsed <= timeThresholds[1])
                {
                    playerScore += score[1]; // Mid score for medium speed completion

                    PlayerPrefs.SetInt("PlayerScore", playerScore);

                    _APIManager.coinsEarningLevelBased(levelToUnlock);

                    _APIManager.UpdateGameScore(score[1], "win", levelToUnlock);
                }
                else
                {
                    playerScore += score[0]; // Lowest score for slow completion

                    PlayerPrefs.SetInt("PlayerScore", playerScore);

                    _APIManager.coinsEarningLevelBased(levelToUnlock);

                    _APIManager.UpdateGameScore(score[0], "win", levelToUnlock);
                }
            }

            if(levelToUnlock == 10)
            {
                SceneManager.LoadScene(12); //game completed screen
            }
            else if (!LevelCompletedCanvas.activeInHierarchy)
            {
                LevelCompletedCanvas.SetActive(true);
            }
        }
    }
}