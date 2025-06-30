using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public const int minPadNumber = 3;
    public const int minHandNumber = 2;

    public int padNumber = minPadNumber;
    public int handNumber = minHandNumber;

    public Color unpressedColour;
    public Color pressedColour;
    private Image[] padColours;
    private Button[] padButtons; // Assuming you have buttons for each pad

    public int[] pads;
    private bool isPlayerTurn1;
    private bool isPlayerTurn2;

    public GameObject endGameMessage;
    public TextMeshProUGUI endGameHeader;
    public TextMeshProUGUI endGameBody;

    public int[] padPresses; // Counter for the number of pad presses
    public int distinctPadPresses;

    public struct gameState
    {
        public int[] pads; // Array to hold the state of the pads
        public int[] padsPresses; // Array to hold the current player pad presses
    }

    public GameObject[] games = new GameObject[3]; // Array to hold different game objects

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPlayerTurn1 = true; // Set the player's turn to true initially
        isPlayerTurn2 = false;

        padPresses = new int[padNumber];
        distinctPadPresses = 0;

        // Hide the win message initially
        endGameMessage.SetActive(false);

        // Initialize the pads array with the specified number of pads
        padColours = new Image[padNumber];
        padButtons = new Button[padNumber];

        for (int i = 0; i < 3; i++)
        {
            if (i == padNumber - 3)
            {
                games[i].SetActive(true);
            }
            else
            {
                games[i].SetActive(false);
            }
        }

        for (int i = 0; i < padNumber; i++)
        {
            if (padNumber == 3)
            {
                padColours[i] = games[0].transform.GetChild(i+1).GetComponent<Image>();
                padButtons[i] = games[0].transform.GetChild(i+1).GetComponent<Button>();
            }
            else if (padNumber == 4)
            {
                padColours[i] = games[1].transform.GetChild(i+1).GetComponent<Image>();
                padButtons[i] = games[1].transform.GetChild(i+1).GetComponent<Button>();
            }
            else if (padNumber == 5)
            {
                padColours[i] = games[2].transform.GetChild(i+1).GetComponent<Image>();
                padButtons[i] = games[2].transform.GetChild(i+1).GetComponent<Button>();
            }
        }

        pads = new int[padNumber];
        // Initialize the pads with random colors
        int redCount = 0;
        int blueCount = 0;

        // Assign random colors
        for (int i = 0; i < pads.Length; i++)
        {
            bool isRed = UnityEngine.Random.value > 0.5f;
            pads[i] = isRed ? 0 : 1;
            if (isRed) redCount++; else blueCount++;
        }

        // If all are the same, flip one pad
        if (redCount == pads.Length || blueCount == pads.Length)
        {
            int flipIndex = UnityEngine.Random.Range(0, pads.Length);
            pads[flipIndex] = (redCount == pads.Length) ? 1 : 0;
        }

        // Pads all gray until player interacts
        for (int i = 0; i < padColours.Length; i++)
        {
            padColours[i].color = unpressedColour;
        }
    }

    public int[] GetBestMovePads(gameState position)
    {
        double maxEval = double.NegativeInfinity;
        int[] bestPads = null;

        for (int newPadStart = 0; newPadStart < padNumber; newPadStart++)
        {
            gameState childState = new gameState
            {
                pads = new int[padNumber],
                padsPresses = (int[])position.padsPresses.Clone()
            };
            int count = 0;
            for (int i = newPadStart; i < newPadStart + padNumber; i++)
            {
                if (count >= padNumber) break;
                childState.pads[count] = position.pads[i % padNumber];
                count++;
            }
            double eval = evaluatePosition(childState); // Evaluate the child state
            if (eval > maxEval)
            {
                maxEval = eval;
                bestPads = (int[])childState.pads.Clone();
            }
        }
        return bestPads;
    }

    public double evaluatePosition(gameState position)
    {
        //Initialize a new gamestate
        gameState positionClone = new gameState
        {
            pads = (int[])position.pads.Clone(),
            padsPresses = (int[])position.padsPresses.Clone()
        };
        // Simulate player's turn
        // Find which pads can be changed colour
        int[] padsPressed = new int[handNumber];
        int count = 0;
        for (int i = 0; i < positionClone.padsPresses.Length; i++)
        {
            if (positionClone.padsPresses[i] > 0)
            {
                padsPressed[count] = i;
                count++;
            }
        }
        double minEval = double.PositiveInfinity;
        // Generate all permutations of colours for the pressed pads
        int total = 1 << handNumber; // 2^handNumber
        // Iterate through all combinations of pressed pads
        for (int i = 0; i < total; i++)
        {
            //Find all possible colourings of the pressed pads
            int[] bits = new int[handNumber];
            for (int j = 0; j < handNumber; j++)
            {
                bits[handNumber - j - 1] = (i >> j) & 1;
            }
            for (int j = 0; j < handNumber; j++)
            {
                int padIndex = padsPressed[j];
                positionClone.pads[padIndex] = bits[j]; // Change the colour of the pressed pad
            }
            // Implement the evaluation logic for the game state
            // Position evaluation equal to the number of pads of the same color
            int redCount = 0;
            int blueCount = 0;
            for (int j = 0; j < positionClone.pads.Length; j++)
            {
                if (positionClone.pads[j] == 0)
                {
                    redCount++;
                }
                else
                {
                    blueCount++;
                }
            }
            double score = Math.Max(redCount, blueCount); // Return the maximum count of pads of the same color
            Debug.Log("Evaluating position: Red Count = " + redCount + ", Blue Count = " + blueCount + ", Score = " + score);
            minEval = Math.Min(minEval, padNumber - score); // Update the minimum evaluation score
        }
        return minEval; // Return the minimum evaluation score for the minimizing player
    }

    // Update is called once per frame
    void Update()
    {
        while (isPlayerTurn1! && isPlayerTurn2!)
        {
            //disable button presses
            for (int i = 0; i < padButtons.Length; i++)
            {
                padButtons[i].interactable = false; // Disable button presses during AI turn
            }
        }
        if (isPlayerTurn1 || isPlayerTurn2)
        {
            // Player can press pads and change colours
            for (int i = 0; i < pads.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // Assuming pads are pressed with keys 1, 2, 3, 4
                {
                    OnPadPressed(i); // Call the method to handle pad press
                }
            }
            if (isPlayerTurn2)
            {
                for (int i = 0; i < padPresses.Length; i++)
                {
                    if (padPresses[i] > 0)
                    {
                        // Change color of the pressed pad
                        padColours[i].color = pads[i] == 0 ? Color.red : Color.blue;
                    }
                }
            }
            // Player can end their turn
            if (Input.GetKeyDown(KeyCode.KeypadEnter) && isPlayerTurn2)
            {
                EndPlayerTurn(); // End the player's turn when enter is pressed
            }
        }
        else
        {
            // AI logic can be implemented here
            Debug.Log("AI's turn to play");
            int[] bestPads = GetBestMovePads(new gameState { pads = pads, padsPresses = padPresses });
            if (bestPads != null)
            {
                Debug.Log("Best pads for AI: " + string.Join(", ", bestPads));
                // Update the pads with the best move
                for (int i = 0; i < padNumber; i++)
                {
                    pads[i] = bestPads[i];
                }
            }
            isPlayerTurn2 = true;
        }
    }

    public void OnPadPressed(int padIndex)
    {
        if (isPlayerTurn1 && padIndex >= 0 && padIndex < pads.Length)
        {
            if (distinctPadPresses < handNumber)
            {
                // Increment the pad press for the specific pad
                padPresses[padIndex]++;
                Debug.Log("Pad " + (padIndex + 1) + " pressed.");
                if (distinctPadPresses == handNumber)
                {
                    Debug.Log("Player has pressed enough pads. Ending turn.");
                    isPlayerTurn1 = false; // End the player's turn
                }
                padColours[padIndex].color = pressedColour; // Change color to indicate it has been pressed
            }
            if (padPresses[padIndex] == 1)
            {
                // Increment the distinct pad presses only if this pad hasn't been pressed before
                distinctPadPresses++;
            }
            if (distinctPadPresses >= handNumber)
            {
                isPlayerTurn1 = false; // End the player's turn if they press more pads than allowed
                isPlayerTurn2 = false;
                return; // Do not allow pressing more pads than the hand number
            }
        }
        else if (isPlayerTurn2 && padPresses[padIndex] > 0 && padIndex >= 0 && padIndex < pads.Length)
        {
            // Flip the color of the pressed pad
            pads[padIndex] = 1 - pads[padIndex]; // Toggle between 0 and 1
            padColours[padIndex].color = pads[padIndex] == 0 ? Color.red : Color.blue;
        }
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn2)
        {
            Debug.Log("It's not the player's turn to end.");
            return; // Do not allow ending the turn if it's not the player's turn
        }
        if (distinctPadPresses >= handNumber)
        {
            // Check if all pads are the same colour
            bool allSameColour = true;
            for (int i = 1; i < pads.Length; i++)
            {
                if (pads[i] != pads[0])
                {
                    allSameColour = false;
                    break;
                }
            }
            if (allSameColour)
            {
                Debug.Log("All pads are the same colour. Player wins!");
                // Handle win condition here (e.g., show win message, reset game, etc.)
                // Set all pad colours to the winning colour
                Color winningColor = pads[0] == 0 ? Color.red : Color.blue;
                for (int i = 0; i < padColours.Length; i++)
                {
                    padColours[i].color = winningColor;
                }
                //show the win message
                endGameMessage.SetActive(true);
                endGameHeader.text = "You Win!";
                endGameBody.text = "You have found the strategy that guarantees a win. Well done!";
                return;
            }
            // If the player has pressed enough pads, they can end their turn
            Debug.Log("Player ended their turn");

            isPlayerTurn1 = true;
            isPlayerTurn2 = false; // End the player's turn
            padPresses = new int[padNumber]; // Reset padPresses
            distinctPadPresses = 0; // Reset the total pad presses

            // Reset pad colours to gray
            for (int i = 0; i < padColours.Length; i++)
            {
                padColours[i].color = unpressedColour;
            }
        }
        else
        {
            Debug.Log("Not enough pad presses to end the turn. Minimum required: " + handNumber);
            return; // Do not end the turn if the condition is not met
        }
    }

    public void Forfeit()
    {
        endGameMessage.SetActive(true);
        endGameHeader.text = "You Lost.";
        endGameBody.text = "Is this selection of hands and pads even possible to win?";
    }

    public void ResetGame()
    {
        if (handNumber > padNumber)
        {
            handNumber = padNumber; // Ensure hand number does not exceed pad number
        }
        Start();
    }

    public void changeHandNumber(int newHandNumber)
    {
        handNumber = newHandNumber + minHandNumber;
    }
    public void changePadNumber(int newPadNumber)
    {
        padNumber = newPadNumber + minPadNumber;
    }
}   
