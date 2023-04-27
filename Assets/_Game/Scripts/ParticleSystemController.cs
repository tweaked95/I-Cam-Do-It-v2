using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ParticleSystemController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var particleSystem = gameObject.GetComponent<ParticleSystem>();

        int seed = SceneManager.GetActiveScene().buildIndex+1;

        System.Random random = new System.Random(seed.GetHashCode());
        var initialColor = new Color(
            ((float)random.Next(0, 255))/255f,
            ((float)random.Next(0, 255))/255f,
            ((float)random.Next(0, 255))/255f
            );

        var particleSystemMain = particleSystem.main;
        Debug.Log(initialColor);
        particleSystemMain.startColor = new ParticleSystem.MinMaxGradient(initialColor);
        Debug.Log("PS Start Color after changing : " + particleSystemMain.startColor.color);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
