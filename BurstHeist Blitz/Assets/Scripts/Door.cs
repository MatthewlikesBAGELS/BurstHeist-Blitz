using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public GameObject enterTooltip;
    public Animator transitionAnim;
    bool inDoorEnterZone = false;
    public int sceneBuildIndexToLoad;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enterTooltip.SetActive(true);
            inDoorEnterZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enterTooltip.SetActive(false);
            inDoorEnterZone = false;
        }
    }

    private void Update()
    {
        if (inDoorEnterZone && Input.GetKeyDown(KeyCode.E))
        {
            Invoke("LoadSceneFromBuildIndex", 1.5f);
            transitionAnim.SetBool("FadeIn", true);
        }

        enterTooltip.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void LoadSceneFromBuildIndex()
    {
        SceneManager.LoadScene(sceneBuildIndexToLoad);
    }
}
