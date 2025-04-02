using System.Collections;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using GameAnalyticsSDK;

public class GameController : MonoBehaviour
{
    public BottleController FirstBottle;
    public BottleController SecondBottle;
    public BottleController[] bottles;

    private bool allFull = false; // all bottles are full

    public int currentLevel;
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

    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] GameObject _GGTextObject;
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

        scoreText.text = "Score : " + PlayerPrefs.GetInt("PlayerScore").ToString();
        levelText.text = "Level " + currentLevel.ToString();

        playerScore = PlayerPrefs.GetInt("PlayerScore");

        timerIsRunning = true;

        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "Level_" + currentLevel.ToString());
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
                            FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x, FirstBottle.transform.position.y + bottleUp, FirstBottle.transform.position.z);
                        }
                    }
                    else
                    {
                        if (FirstBottle == hit.collider.GetComponent<BottleController>())
                        {
                            if (FirstBottle.numberOfColorsInBottle != 0)
                            {
                                FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x, FirstBottle.transform.position.y + bottleDown, FirstBottle.transform.position.z);
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
                                    FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x, FirstBottle.transform.position.y + bottleDown, FirstBottle.transform.position.z);
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
                        FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x, FirstBottle.transform.position.y + bottleDown, FirstBottle.transform.position.z);
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

                APIManager.Instance.UpdateGameScore(APIManager.Instance.ggScore, APIManager.Instance.ggCoins, "loss", currentLevel);

                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Level_" + currentLevel.ToString(), "Score_", 0);
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Level_" + currentLevel.ToString(), "Time_", (int)timeElapsed);
            }
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(currentLevel + 1);
    }

    private void Win()
    {
        if (allFull == true)
        {
            float timeUsed = 500f - timeElapsed;
            timerIsRunning = false;

            numberOfUnlockedLevel = PlayerPrefs.GetInt("LevelIsUnlocked");
            completedLevel = PlayerPrefs.GetInt("CompletedLevels");

            if ((numberOfUnlockedLevel + 1) <= 10)
            {
                PlayerPrefs.SetInt("LevelIsUnlocked", numberOfUnlockedLevel + 1);
            }

            if (currentLevel >= completedLevel + 1)
            {
                PlayerPrefs.SetInt("CompletedLevels", completedLevel + 1);
            }

            if (currentLevel == 10)
            {
                SceneManager.LoadScene(12); //game completed screen
            }
            else if (!LevelCompletedCanvas.activeInHierarchy)
            {
                LevelCompletedCanvas.SetActive(true);
                if (_GGTextObject.activeSelf)
                {
                    _GGTextObject.SetActive(false);
                }
            }

            FindFirstObjectByType<AudioManager>().Play("WinSound");

            if (currentLevel % GrandAdManager.instance.adsAfter == 0)
            {
                GrandAdManager.instance.ShowAd("startAd");
            }


            if (completedLevel < 10)
            {
                APIManager.Instance.coinsEarningLevelBased(currentLevel, _GGTextObject);
                
                if (timeUsed <= timeThresholds[0])
                {
                    playerScore += score[2]; // Highest score for quickest completion
                    APIManager.Instance.ggScore += score[2];

                    PlayerPrefs.SetInt("PlayerScore", playerScore);

                    scoreText.text = "Score : " + PlayerPrefs.GetInt("PlayerScore").ToString();

                    APIManager.Instance.UpdateGameScore(APIManager.Instance.ggScore, APIManager.Instance.ggCoins, "win", currentLevel);

                    GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Score", score[2]);
                    GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Time", (int)timeElapsed);
                }
                else if (timeUsed <= timeThresholds[1])
                {
                    playerScore += score[1]; // Mid score for medium speed completion
                    APIManager.Instance.ggScore += score[1];

                    PlayerPrefs.SetInt("PlayerScore", playerScore);

                    scoreText.text = "Score : " + PlayerPrefs.GetInt("PlayerScore").ToString();

                    APIManager.Instance.UpdateGameScore(APIManager.Instance.ggScore, APIManager.Instance.ggCoins, "win", currentLevel);

                    GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Score", score[1]);
                    GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Time", (int)timeElapsed);
                }
                else
                {
                    playerScore += score[0]; // Lowest score for slow completion
                    APIManager.Instance.ggScore += score[0];

                    PlayerPrefs.SetInt("PlayerScore", playerScore);

                    scoreText.text = "Score : " + PlayerPrefs.GetInt("PlayerScore").ToString();

                    APIManager.Instance.UpdateGameScore(APIManager.Instance.ggScore, APIManager.Instance.ggCoins, "win", currentLevel);

                    GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Score", score[0]);
                    GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Time", (int)timeElapsed);
                }
            }
        }
    }
}