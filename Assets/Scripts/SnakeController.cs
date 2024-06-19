using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnakeController : MonoBehaviour
{
    public Transform headPrefab;
    public Transform bodyPrefab;
    public Transform foodPrefab;
    public Transform wallPrefab;
    public int initialSize = 3;
    private List<Transform> snakeSegments = new List<Transform>();
    public float gridSize = 1.0f;
    private Vector3 direction = Vector3.right;
    private Vector3 lastDirection;
    public float updateTime = 0.15f; 
    private float lastMoveTime;
    private Vector2Int gridDimensions = new Vector2Int(23, 17);
    public Vector2 offset = new Vector2(-3.97f, -4.3f);

    private List<Transform> currentFood = new List<Transform>();
    private int foodCount = 1;
    private int foodEaten = 0; 
    public Text currentScoreText; 
    public Text bestScoreText; 
    private int currentScore = 0;
    private int bestScore = 0;
    private List<Vector3> wallPositions = new List<Vector3>(); 

    void Start()
    {
        InitializeSnake();
        LoadBestScore();
        UpdateScoreUI();
        GenerateFood();
    }

    void InitializeSnake()
    {
         Transform head = Instantiate(headPrefab, new Vector3(0, 2, 4), Quaternion.Euler(0, -90, 0));
        snakeSegments.Add(head);

        for (int i = 1; i < initialSize; i++)
        {
            Transform segment = Instantiate(bodyPrefab, new Vector3(-i * gridSize, 2, 4), Quaternion.identity);
            snakeSegments.Add(segment);
        }
        
    }

    void Update()
    {
        HandleInput();
        if (Time.time - lastMoveTime > updateTime)
        {
            Move();
            lastMoveTime = Time.time;
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && lastDirection != Vector3.back)
        {
            direction = Vector3.forward;
        }
        else if (Input.GetKeyDown(KeyCode.S) && lastDirection != Vector3.forward)
        {
            direction = Vector3.back;
        }
        else if (Input.GetKeyDown(KeyCode.A) && lastDirection != Vector3.right)
        {
            direction = Vector3.left;
        }
        else if (Input.GetKeyDown(KeyCode.D) && lastDirection != Vector3.left)
        {
            direction = Vector3.right;
        }
    }

    void Move()
    {
        lastDirection = direction;
        Vector3 previousPosition = snakeSegments[0].position;
        snakeSegments[0].position = new Vector3(
            Mathf.Round(snakeSegments[0].position.x + direction.x * gridSize),
            snakeSegments[0].position.y,
            Mathf.Round(snakeSegments[0].position.z + direction.z * gridSize)
        );
        if (snakeSegments[0].position.x < offset.x + 4 - gridDimensions.x / 2 * gridSize ||
            snakeSegments[0].position.x > offset.x + 4 + gridDimensions.x / 2 * gridSize ||
            snakeSegments[0].position.z < offset.y - 1 - gridDimensions.y / 2 * gridSize ||
            snakeSegments[0].position.z > offset.y - 1 + gridDimensions.y / 2 * gridSize)
        {
            RestartGame();
            return;
        }
        for (int i = 1; i < snakeSegments.Count; i++)
        {
            if (Vector3.Distance(snakeSegments[0].position, snakeSegments[i].position) < 0.1f)
            {
                RestartGame();
                return;
            }
        }


        if (direction == Vector3.right)
        {
            snakeSegments[0].rotation = Quaternion.Euler(0, -90, 0);
        }
        else if (direction == Vector3.left)
        {
            snakeSegments[0].rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (direction == Vector3.forward)
        {
            snakeSegments[0].rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (direction == Vector3.back)
        {
            snakeSegments[0].rotation = Quaternion.Euler(0, 0, 0);
        }

        for (int i = 1; i < snakeSegments.Count; i++)
        {
            Vector3 temp = snakeSegments[i].position;
            snakeSegments[i].position = previousPosition;
            previousPosition = temp;
        }
        IsEaten();
    }

    void IsEaten()
    {
        for (int i = 0; i < currentFood.Count; i++)
        {
            if (currentFood[i] != null && Vector3.Distance(snakeSegments[0].position, currentFood[i].position) < 0.1f)
            {
                Destroy(currentFood[i].gameObject);
                currentFood.RemoveAt(i);
                Grow();
                currentScore++;
                UpdateScoreUI();
                GenerateFood();
            }
        }
    }

    void GenerateFood()
    {
        while (currentFood.Count < foodCount)
        {
            Vector3 foodPosition;
            bool positionValid;

            do
            {
                positionValid = true;
                foodPosition = new Vector3(
                    offset.x + 4 + Mathf.Round(Random.Range(-gridDimensions.x / 2, gridDimensions.x / 2)) * gridSize,
                    2,
                    offset.y - 1 + Mathf.Round(Random.Range(-gridDimensions.y / 2, gridDimensions.y / 2)) * gridSize
                );

                foreach (Transform segment in snakeSegments)
                {
                    if (Vector3.Distance(segment.position, foodPosition) < 0.1f)
                    {
                        positionValid = false;
                        break;
                    }
                }

                foreach (Transform food in currentFood)
                {
                    if (Vector3.Distance(food.position, foodPosition) < 0.1f)
                    {
                        positionValid = false;
                        break;
                    }
                }
            }
            while (!positionValid);

            currentFood.Add(Instantiate(foodPrefab, foodPosition, Quaternion.identity));
        }
    }
    
    public void Grow()
    {
        Transform newSegment = Instantiate(bodyPrefab);
        newSegment.position = snakeSegments[snakeSegments.Count - 1].position;
        snakeSegments.Add(newSegment);
    }
    void UpdateScoreUI()
    {
        currentScoreText.text = "Score: " + currentScore;
        bestScoreText.text = "Best: " + bestScore;

        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            bestScoreText.text = "Best: " + bestScore;
            SaveBestScore();
        }
    }

    void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
    }

    void SaveBestScore()
    {
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.Save();
    }
    public void RestartGame()
{
    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); 
}
}
