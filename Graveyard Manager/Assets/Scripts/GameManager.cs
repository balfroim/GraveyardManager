using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public DeceasedData deceasedData;
    public Text dateText;
    public GameObject[] profiles;
    public Slider contentmentSlider;
    public Tilemap tileMap;
    [Header("GameObjects")]
    public GameObject dialogPanel;
    public GameObject graveInfo;
    public GameObject rightPanel;
    public GameObject topPanel;
    public GameObject[] tutoImages;
    public GameObject pointer;
    [Header("Music & Sound")]
    public AudioClip burySound;
    public AudioClip unburySound;
    public AudioClip gameTheme;
    public AudioClip menuTrollTheme;
    public AudioClip menuTheme;
    [Header("Tiles")]
    public TileBase emptySpot;
    public TileBase correctGrave;
    public TileBase wellMaintainGrave;
    public TileBase abandonedGrave;
    public TileBase horizontalPath;
    public TileBase horizontalPathWithVisitor;

    private int monthsPassed = 0;

    private List<Deceased> deceasedMonth;
    private Dictionary<Vector3Int, Deceased> buried;
    private bool isGameOver = false;
    public bool IsGameOver
    {
        get
        {
            return isGameOver;
        }
    }
    private bool isGameStarted = false;
    public bool IsGameStarted
    {
        get
        {
            return isGameStarted;
        }
    }

    // Use this for initialization
    void Start() {
        if (instance == null)
            instance = this;
        else
            GameObject.Destroy(this.gameObject);

        Screen.SetResolution(1024, 768, Screen.fullScreen);

        deceasedMonth = new List<Deceased>();
        WhoDieThisMonth();
        buried = new Dictionary<Vector3Int, Deceased>();
        // Update slider display
        ChangeContentment(0);

        TutorialManager();

    }

    void UpdateDate()
    {
        int currentMonth = (7 + monthsPassed) % 12 + 1;
        string monthDisplay = (currentMonth < 10) ? "0" + currentMonth.ToString() : currentMonth.ToString();
        int currentYear = 2018 + (7 + monthsPassed) / 12;

        string dateDisplay = string.Format("{0}/{1}", monthDisplay, currentYear);
        dateText.text = dateDisplay;
    }

    // Go to next month if all the dead is buried
    public void NextMonth()
    {
        if (!isGameOver && isGameStarted)
        {
            if (deceasedMonth.Count < 4)
            {
                // If there is still body in the morgue generate discontement
                foreach (Deceased corpse in deceasedMonth)
                {
                    corpse.StayInMorgue();
                    ChangeContentment(-Mathf.RoundToInt(RandomBiased(0f, 5f, corpse.VisitChance + 0.5f) * corpse.MonthsSpendMorgue));
                }
                monthsPassed += 1;
                UpdateDate();
                WhoDieThisMonth();
                MonthlyVisit();
                UpdateGraveDisplay();
                foreach (KeyValuePair<Vector3Int, Deceased> grave in buried)
                {
                    grave.Value.Age();
                }

            }
        }
    }

    public void WhoDieThisMonth()
    {
        // Number of corpses is biased to be more around the high boundary the more the time passed
        // If there is already corpses in the morgue the high boundary is obviously smaller.
        int numberOfDeath = Mathf.RoundToInt(GameManager.RandomBiased(1f, 4f - deceasedMonth.Count, 24f / (monthsPassed + 1)));
        for (int i = 0; i < numberOfDeath; i++)
        {
            Deceased newCorpse = new Deceased();
            deceasedMonth.Add(newCorpse);
        }
        DisplayDeath();
    }

    // Display the death profile on the right panel
    public void DisplayDeath()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i >= deceasedMonth.Count)
            {
                profiles[i].transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                profiles[i].transform.GetChild(0).gameObject.SetActive(true);
                profiles[i].transform.GetChild(0).GetComponentInChildren<Text>().text = deceasedMonth[i].Profile();
            }
        }
    }

    // Generate a random number between low and high with a bias
    public static float RandomBiased(float low, float high, float bias)
    {
        float r = UnityEngine.Random.Range(0f, 1f);
        r = Mathf.Pow(r, bias);
        return low + (high - low) * r;
    }

    // Use to bury a corpse
    // Return false if there is no corpse to bury
    public bool Bury(Vector3Int cellPosition)
    {
        if (deceasedMonth.Count == 0)
            return false;
        else
        {

            buried.Add(cellPosition, deceasedMonth[0]);
            deceasedMonth.RemoveAt(0);
            DisplayDeath();
            return true;
        }
    }

    public bool Unbury(Vector3Int cellPosition)
    {
        if (deceasedMonth.Count == 0)
            return false;
        else
        {
            ChangeContentment(buried[cellPosition].UnburyMalus());
            buried.Remove(cellPosition);
            buried.Add(cellPosition, deceasedMonth[0]);
            deceasedMonth.RemoveAt(0);
            DisplayDeath();
            return true;
        }
    }

    public void DisplayGraveInfo(Vector3Int cellPosition, Vector3 worldPosition)
    {
        graveInfo.GetComponent<RectTransform>().SetPositionAndRotation(worldPosition, Quaternion.identity);
        graveInfo.GetComponentInChildren<Text>().text = buried[cellPosition].GraveInfo();
        graveInfo.SetActive(true);
    }

    public void HideGraveInfo()
    {
        graveInfo.SetActive(false);
    }

    public bool IsSomeoneBuriedHere(Vector3Int cellPosition)
    {
        return buried.ContainsKey(cellPosition);
    }

    public void MonthlyVisit()
    {
        foreach (KeyValuePair<Vector3Int, Deceased> grave in buried)
        {
            // Determine if the grave will be visited this months or not
            // Chances are higher in the first 3 months
            bool isVisited = grave.Value.VisitChance <= RandomBiased(0f, 1f, (grave.Value.GraveAge + 1f) / 3f);
            grave.Value.Visit(isVisited);
            if (isVisited)
            {
                // Each visit the contentment bar refill a little.
                // Less effective as the months passed.
                ChangeContentment(Mathf.RoundToInt(RandomBiased(0f, 3f, (grave.Value.GraveAge + 1f) / 3f)));
                // Change the tile below the grave
                tileMap.SetTile(grave.Key + Vector3Int.down, horizontalPathWithVisitor);
            }
            else
            {
                // Change the tile below the grave
                tileMap.SetTile(grave.Key + Vector3Int.down, horizontalPath);
            }
        }
    }

    public void UpdateGraveDisplay()
    {
        foreach (KeyValuePair<Vector3Int, Deceased> grave in buried)
        {
            Deceased.GraveState graveState = grave.Value.GetGraveState();

            switch (graveState)
            {
                case Deceased.GraveState.Correct:
                    tileMap.SetTile(grave.Key, correctGrave);
                    break;
                case Deceased.GraveState.WellMaintain:
                    tileMap.SetTile(grave.Key, wellMaintainGrave);
                    break;
                case Deceased.GraveState.Abandoned:
                    tileMap.SetTile(grave.Key, abandonedGrave);
                    break;
            }
        }
    }

    public void ChangeContentment(int amount)
    {
        if(!isGameOver)
        {
            contentmentSlider.value += amount;
            contentmentSlider.GetComponentInChildren<Text>().text = string.Format("{0} %", contentmentSlider.value);
            if (contentmentSlider.value == 0f)
            {
                GameOver();
            }
        }  
    }

    public void GameOver()
    {
        isGameOver = true;
        Dialog("We've been reported that you make the population angry !\n<b> You're fired !</b>", "<b>Try again</b>", Restart, "<b>Quit</b>",  Quit);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        print("Quit");
        Application.Quit();
    }

    public void Dialog(string dialog, string response1, UnityAction action1, string response2, UnityAction action2)
    {
        dialogPanel.SetActive(true);
        dialogPanel.GetComponentInChildren<Text>().text = dialog;
        Button button1 = dialogPanel.transform.Find("Buttons").Find("BT1").GetComponent<Button>();
        Button button2 = dialogPanel.transform.Find("Buttons").Find("BT2").GetComponent<Button>();

        button1.gameObject.SetActive(response1 != "");
        button2.gameObject.SetActive(response2 != "");

        if (button1.gameObject.activeSelf)
        {
            button1.onClick.RemoveAllListeners();
            button1.onClick.AddListener(delegate { dialogPanel.SetActive(false); });
            button1.onClick.AddListener(action1);
            button1.GetComponentInChildren<Text>().text = response1;
        }
        
        if (button2.gameObject.activeSelf)
        {
            button2.onClick.RemoveAllListeners();
            button2.onClick.AddListener(delegate { dialogPanel.SetActive(false); });
            button2.onClick.AddListener(action2);
            button2.GetComponentInChildren<Text>().text = response2;
        }
        
    }
    
    public void TutorialManager()
    {
        // Common responses
        string responseYes = "Yes !";
        string responseSucks = "No, it sucks.";
        string responseContinue = "[CONTINUE]";
        string responsePlay = "[PLAY]";
        string noResponse = "";
        // Common actions
        UnityAction actionQuit = Quit;
        UnityAction actionPlay = delegate {
            isGameStarted = true;
            GetComponent<AudioSource>().clip = gameTheme;
            GetComponent<AudioSource>().Play();
            // RIGHT PANEL
            for (int i = 0; i < rightPanel.transform.childCount; i++)
            {
                rightPanel.transform.GetChild(i).gameObject.SetActive(true);
            }
            // TOP PANEL
            for (int i = 0; i < topPanel.transform.childCount; i++)
            {
                topPanel.transform.GetChild(i).gameObject.SetActive(true);
            }
            tileMap.gameObject.transform.parent.gameObject.SetActive(true);
            pointer.SetActive(false);
        };
        UnityAction actionNothing = delegate { };

        // Dialogues : from last to first

        // Dialogue 12
        string dialog12 = "Now it's time to work ! Make us proud !";
        UnityAction Dialog12 = delegate { Dialog(dialog12, responsePlay, actionPlay, noResponse, actionNothing); };

        // Dialogue 11
        string dialog11 = "Don't forget to bury all the corpses from the morgue before going to the next month, or it will make the people angry";
        UnityAction Dialog11 = delegate { Dialog(dialog11, responseContinue, Dialog12, responsePlay, actionPlay); };

        // Dialogue 10
        string dialog10 = "But don't worry, each time a grave is visited, the bar will refill a little";
        UnityAction action10 = delegate {
            Dialog11();
            // Display Data
            topPanel.transform.GetChild(0).gameObject.SetActive(true);
            // Display Next Month
            rightPanel.transform.GetChild(2).gameObject.SetActive(true);
            pointer.transform.SetParent(GameObject.Find("Tutorial11Pointer").transform);
            pointer.transform.localPosition = Vector3.zero;
            pointer.transform.localRotation = Quaternion.identity;
        };
        UnityAction Dialog10 = delegate { Dialog(dialog10, responseContinue, action10, responsePlay, actionPlay); };

        // Dialogue 9
        string dialog9 = "You can see the level of anger of the population on the contentment bar. If it reaches 0, you'll be fired.";
        UnityAction Dialog9 = delegate { Dialog(dialog9, responseContinue, Dialog10, responsePlay, actionPlay); };

        // Dialogue 8
        string dialog8 = "This will anger the families of the deceased, which is why you have to prioritize the graves that are less visited";
        UnityAction action8 = delegate {
            Dialog9();
            // Display Grid
            tileMap.gameObject.transform.parent.gameObject.SetActive(true);
            // Display Bar
            topPanel.transform.GetChild(1).gameObject.SetActive(true);
            // Hide images
            for (int i = 0; i < 3; i++)
            {
                tutoImages[i].SetActive(false);
            }
            pointer.transform.SetParent(GameObject.Find("Tutorial9Pointer").transform);
            pointer.transform.localPosition = Vector3.zero;
            pointer.transform.localRotation = Quaternion.identity;
        };
        UnityAction Dialog8 = delegate { Dialog(dialog8, responseContinue, action8, responsePlay, actionPlay); };

        // Dialogue 7
        string dialog7 = "Our graveyard is small.\nAt some point, you'll run out of space, and when that happens you'll have to bury the corpses in used graves.";
        UnityAction action7 = delegate {
            Dialog8();
            // Hide Grid
            tileMap.gameObject.transform.parent.gameObject.SetActive(false);
            // Display images
            for (int i = 0; i < 3; i++)
            {
                tutoImages[i].SetActive(true);
            }
            pointer.transform.SetParent(GameObject.Find("Tutorial8Pointer").transform);
            pointer.transform.localPosition = Vector3.zero;
            pointer.transform.localRotation = Quaternion.identity;

        };
        UnityAction Dialog7 = delegate { Dialog(dialog7, responseContinue, action7, responsePlay, actionPlay); };

        // Dialogue 6
        string dialog6 = "Click on a free spot to bury the corpse on top of the list.\n(It does nothing for now, it's normal. Everything is disable for the tutorial.)";
        UnityAction Dialog6 = delegate { Dialog(dialog6, responseContinue, Dialog7, responsePlay, actionPlay); };

        // Dialogue 5
        string dialog5 = "Every month, there will be between 1 and 4 corpses in the morgue for you to bury.";
        UnityAction action5 = delegate {
            Dialog6();
            // Display Grid
            tileMap.gameObject.transform.parent.gameObject.SetActive(true);
            pointer.transform.SetParent(GameObject.Find("Tutorial6Pointer").transform);
            pointer.transform.localPosition = Vector3.zero;
        };
        UnityAction Dialog5 = delegate { Dialog(dialog5, responseContinue, action5, responsePlay, actionPlay);  };

        // Dialogue 4
        string dialog4 = "Yes me too !";
        UnityAction action4 = delegate {
            Dialog5();
            // Display Morgue
            rightPanel.transform.GetChild(0).gameObject.SetActive(true);
            rightPanel.transform.GetChild(1).gameObject.SetActive(true);
            pointer.SetActive(true);
            pointer.transform.SetParent(GameObject.Find("Tutorial5Pointer").transform);
            pointer.transform.localPosition = Vector3.zero;
        };
        UnityAction Dialog4 = delegate { Dialog(dialog4, responseContinue, action4, noResponse, actionNothing); };

        // Dialogue Troll 2
        string dialogTroll2 = "Ok, as you want!";
        UnityAction DialogTroll2 = delegate { Dialog(dialogTroll2, responseContinue, action4, noResponse, actionNothing); };

        // Dialogue Troll 1
        string dialogTroll1 = "Ok maybe I will change it a little bit.\nAnd now?";
        string responseTroll1A = "I like this one";
        string responseTroll1B = "Erf, the first one was just fine.\nGet back to this one please.";
        UnityAction actionTroll1B = delegate {
            DialogTroll2();
            GetComponent<AudioSource>().clip = menuTheme;
            GetComponent<AudioSource>().Play();
        };
        UnityAction DialogTroll1 = delegate { Dialog(dialogTroll1, responseTroll1A, DialogTroll2, responseTroll1B, actionTroll1B); };

        // Dialog 3
        string dialog3 = "Before starting this tutorial, I want to thank Bastien for the theme songs.\nDo you like the theme ? ";
        UnityAction action3B = delegate {
            DialogTroll1();
            GetComponent<AudioSource>().clip = menuTrollTheme;
            GetComponent<AudioSource>().Play();
        };
        UnityAction Dialog3 = delegate { Dialog(dialog3, responseYes, Dialog4, responseSucks, action3B); };

        // Dialog 2
        string dialog2 = "Wonderful ! Do you want me to explain how to do your future job ?";
        string response2B = "No, thanks you. I already know. [PLAY]";
        UnityAction Dialog2 = delegate { Dialog(dialog2, responseYes, Dialog3, response2B, actionPlay); };

        // Dialog 1
        string dialog1 = "Welcome to StupidTown, I am the mayor ! It's a small and peaceful village but strangely, people tend to die for stupid reasons over here.\nOur graveyard is small and we need someone capable of managing it without angering the population.\nDo you want to be our new Graveyard Manager ?";
        Dialog(dialog1, responseYes, Dialog2, responseSucks, actionQuit);
    }

    public static float SCurve(float x, float beta = 1.6f)
    {
        return 1f / (1f + (Mathf.Pow(x / (1f - x), -beta)));
    }
}
