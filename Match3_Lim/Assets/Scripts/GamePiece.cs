using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    bool m_isMoving = false;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /* if (Input.GetKeyDown(KeyCode.RightArrow))
         {
             Move((int)transform.position.x + 2, (int)transform.position.y, 0.5f);
         }
         else if (Input.GetKeyDown(KeyCode.LeftArrow))
         {
             Move((int)transform.position.x - 2, (int)transform.position.y, 0.5f);
         }*/
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    void Move(int destX, int destY, float timeToMove)
    {
        if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
        }
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        bool reachedDestination = false;

        float elapsedTime = 0f;

        m_isMoving = true;

        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true;
                transform.position = destination;
                SetCoord((int)destination.x, (int)destination.y);
                break;
            }

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / timeToMove;

            // Use sin wave as 'ease out' to get a smoother movement
            // t = Mathf.Sin(t * Mathf.PI * 0.5f); // multiplying by 0.5 will give us the first half of the curve (e.g. from 0 up to PI/2)

            // Use inverted cos wave as 'ease in' to get a smoother movement
            //t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);

            // Use exponential (smooth step) for ease-in-and-out
            t = t * t * (3 - 2 * t);

            transform.position = Vector3.Lerp(startPosition, destination, t);

            yield return null;
        }
        m_isMoving = false;
    }
}
