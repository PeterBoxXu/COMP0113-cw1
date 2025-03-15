 using UnityEngine;
 using System.Collections;
 
 public class LightFlicker : MonoBehaviour
 {
     public float maximumDim;
     public float maximumBoost;
     public float speed;
     public float strength;

     private bool noFlicker;
     private Light source;
     private float initialIntensity;
 
     public void Reset()
     {
         maximumDim = 0.2f;
         maximumBoost = 0.2f;
         speed = 5f;
         strength = 250;
     }
 
     public void Start()
     {
         source = GetComponent<Light>();
         initialIntensity = source.intensity;
         StartCoroutine(Flicker());
     }
 
 
     private IEnumerator Flicker()
     {
         while (!noFlicker)
         {
             source.intensity = Mathf.Lerp(initialIntensity *(1 - maximumDim),initialIntensity * (1 + maximumBoost), strength * Time.deltaTime);
             yield return new WaitForSeconds(speed);
         }
     }
 }



