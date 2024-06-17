using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PAA_powerupControllerExample : MonoBehaviour
{
   
   public Mesh[] meshes;
    public Material[] materials;
    public GameObject powerup_object;
    public float respawnTimer = 2f;

   void Start()
    {
        // Loads a random mesh and material for demo purposes
       //getRandomPowerup();
    }

    void getRandomPowerup()
    {
          // Get the MeshFilter component of the object
        MeshFilter meshFilter = powerup_object.GetComponent<MeshFilter>();

        if (meshFilter != null)
        {
            // Select a random mesh from the array
            int randomMeshIndex = Random.Range(0, meshes.Length);
            meshFilter.mesh = meshes[randomMeshIndex];
        }

        // Get the Renderer component of the object
        Renderer renderer = powerup_object.GetComponent<Renderer>();

        if (renderer != null)
        {
            // Select a random material from the array
            int randomMaterialIndex = Random.Range(0, materials.Length);
            renderer.material = materials[randomMaterialIndex];

            if(randomMaterialIndex == 0){
                 Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
        
                // Assign the random color to the material
                renderer.material.color = randomColor;
            }
            else
            {
                
        
                // Assign the random color to the material
                renderer.material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);;
            }
        }
    }

    // Plays the "collect" animation when a player collides with the prefab
     private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player"))
        {
           collectPowerup();
        }
    }

    void collectPowerup()
    {
        GetComponent<Animator>().Play("collect");
        StartCoroutine(respawnPowerUp());
    }

    IEnumerator respawnPowerUp()
    {
        yield return new WaitForSeconds(respawnTimer);
        getRandomPowerup();
         GetComponent<Animator>().Play("spawn");
    }
}
