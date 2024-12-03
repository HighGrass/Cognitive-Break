using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    bool draggedPiece = false;
    bool deletedPiece = false;

    [SerializeField] Image mouseImage;
    [SerializeField] Image visualPiece;
    [SerializeField] GameManager gameManager;

    private void Start()
    {
        mouseImage.transform.position = new Vector3(1000, 1000, 0);
        visualPiece.transform.position = new Vector3(1000, 1000, 0);

        StartCoroutine(DragTutorial(2, 100));
    }
    IEnumerator DragTutorial(float time, int steps)
    {
        yield return new WaitForSeconds(3);
        mouseImage.color = new Color(1, 1, 1, 0);
        visualPiece.color = new Color(1, 1, 1, 0);

        Vector3 initialPosition = gameManager.PickPieces[0].transform.position + new Vector3(-10, -10);
        Vector3 finalPosition = gameManager.PickPieces[0].transform.position;

        mouseImage.transform.position = initialPosition;
        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(time / steps);
            mouseImage.color = new Color(1, 1, 1, (float)i / steps);

            float pt = (float)Mathf.Abs((float)steps / 2 - Mathf.Abs(i - (float)steps / 2));
            if (i >= steps / 2) pt = Mathf.Abs(steps / 2 - pt);
            else pt = pt / 2;

            pt = pt / ((float)steps / 2);

            Debug.Log("pt: " + pt);
            mouseImage.transform.position = Vector3.Lerp(initialPosition, finalPosition, pt);
        }
    }




    private void HideAll()
    {
        mouseImage.transform.position = new Vector3(1000, 1000, 0);
        visualPiece.transform.position = new Vector3(1000, 1000, 0);
    }



}
