using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class AllyMovement : MonoBehaviour
{
    public Transform player;
    public List<GameObject> allies;
    public List<GameObject> enemies;

    public List<GameObject> ally1Movement;
    public List<GameObject> ally2Movement;
    public List<GameObject> ally3Movement;
    
    public float speed;
    public float targetDistance;
    public float allowedDistance;
    public RaycastHit hit;

    public TextMeshProUGUI commandCountText;
    public TextMeshProUGUI commandListText;
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI roomsLeftToClear;

    private Dictionary<string, Action> keywordActions = new Dictionary<string, Action>();
    private KeywordRecognizer keywordRecognizer;

    // To display
    private List<string> commandList; // stores all the words spoken
    private int commandCount = 0;
    public string grade = "F"; // default fail

    private string ally = "";
    private static GameObject currAlly = null;

    //private int layerMask = 1 << 9;

    public int roomsLeft;
    public static int _roomsLeft;

    private Vector3 playerPosition;
    private int numOfAllies = 3;

    private void Awake()
    {
        _roomsLeft = roomsLeft;
    }

    private void Start()
    {
        // check which ally called
        keywordActions.Add("first", AllyChange);
        keywordActions.Add("second", AllyChange);
        keywordActions.Add("third", AllyChange);

        // actions to take
        keywordActions.Add("cover", Cover);
        keywordActions.Add("scan", Scan);
        keywordActions.Add("clear room", ClearRoom);

        // end scene
        keywordActions.Add("room clear", RoomClear);
        keywordActions.Add("apartment clear", ApartmentClear);

        keywordRecognizer = new KeywordRecognizer(keywordActions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnKeywordsRecognized;
        keywordRecognizer.Start();
        Debug.Log(keywordRecognizer.IsRunning);

        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }
    }

    private void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        ally = args.text;
        //commandList.Add(args.text);
        commandCount++;

        Debug.Log("Keyword: " + args.text);
        keywordActions[args.text].Invoke();
    }

    private void AllyChange()
    {
        if (ally == "first")
            currAlly = allies[0];
        else if (ally == "second")
            currAlly = allies[1];
        else if (ally == "third")
            currAlly = allies[2];
    }

    private void ClearRoom()
    {
        // When this command is activated, the operation starts.
        // Move the players into the room
        // 1st ally move forward with the door position
        // 2nd ally move sideways
        // 3rd ally moves forward as well
        // Commander is supposed to go in as well.

    }

    private void RoomClear()
    {
        // When this command is activated, the room is cleared.
        // Mark the room as completed
        roomsLeft--;
    }

    private void ApartmentClear()
    {
        // When this command is activated, the apartment is cleared.
        // Mark the room as completed.
        roomsLeft--;
    }

    private void Cover()
    {
        Debug.Log("cover");

        currAlly.GetComponent<Animator>().Play("Firing Rifle");
        currAlly = null;
    }

    private void Scan()
    {
        Debug.Log("scan");

        currAlly.GetComponent<Animator>().Play("Scan");
        currAlly = null;

        // Play audio clip here
        // TODO: Record: No IED, no booby trap, door swing inwards.
    }

    // Grades are assigned based on completeness of commands
    void AssignGrades()
    {
        gradeText.text = String.Format("{0}", grade);
    }

    void DisplayText(int count, List<String> text)
    {
        commandCountText.text = string.Format("{0}", count);
        commandListText.text = "";
    }

    bool updated = false;

    void Update()
    {
        playerPosition = player.transform.position;

        // Respawn allies behind user when the first reticle is stepped on
        if (TimerTrigger.triggered && !updated)
        {
            moveToStartingPositions();
            updated = true;
        }

        //foreach (GameObject ally in allies)
        //{
        //    // Ally look away from player
        //    // ally.transform.LookAt(player);

        //    // Limit the distance between player and allies at all times
        //    float dist = Vector3.Distance(player.position, ally.transform.position);

        //    // Follow player
        //    if (Physics.Raycast(ally.transform.position, ally.transform.TransformDirection(Vector3.forward), out hit, layerMask))
        //    {
        //        Vector3 direction = playerPosition - ally.transform.position;
        //        direction.y = 0;

        //        Debug.DrawRay(ally.transform.position, ally.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
        //        targetDistance = hit.distance;

        //        if (targetDistance >= allowedDistance && dist > 1)
        //        {
        //            speed = 0.015f;
        //            Debug.Log(ally + " run! " + dist);
        //            ally.GetComponent<Animator>().Play("Rifle Walk");
        //            ally.transform.position = Vector3.MoveTowards(ally.transform.position, player.transform.position, speed);
        //        }
        //        else
        //        {
        //            Debug.Log(ally + " stop! " + dist);
        //            speed = 0;
        //            ally.GetComponent<Animator>().Play("Idle");
        //        }

        //    }
        //}

        // Display text
        AssignGrades();
        DisplayText(commandCount, commandList);
        roomsLeftToClear.text = String.Format("{0}", _roomsLeft);

        //foreach (string str in commandList)
        //{
        //    commandListText.text = string.Format("%s\n", str);
        //}
    }

    // Respawn allies behind user
    public void moveToStartingPositions()
    {
        // Allies positions are preassigned based on the lists created above:
        // ally1Movement, ally2Movement, ally3Movement

        // When player first steps onto the reticle, move the allies to their starting positions
        allies[0].transform.position = ally1Movement[0].transform.position;
        allies[1].transform.position = ally2Movement[0].transform.position;
        allies[2].transform.position = ally3Movement[0].transform.position;

        allies[2].transform.rotation = Quaternion.Euler(0, 130, 0);
    }

    private void OnApplicationQuit()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.OnPhraseRecognized -= OnKeywordsRecognized;
            keywordRecognizer.Stop();
        }
    }
}
