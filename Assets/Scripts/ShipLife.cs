using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipLife : MonoBehaviour
{
    public GameObject shipBody;

    private bool _isAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        if (shipBody == null || shipBody == this)
        {
            Debug.LogError("Invalid ship body, cannot manage ship life!");
            _isAlive = false;  // Prevent any further logic from running
            return;
        }

        foreach(Collider collider in shipBody.GetComponentsInChildren<Collider>(true))
        {
            collider.isTrigger = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isAlive)
        {
            Debug.Log("Ship crashed! Reloading scene...");
            _isAlive = false;
            shipBody.SetActive(false);
            StartCoroutine(ReloadScene());
        }
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
