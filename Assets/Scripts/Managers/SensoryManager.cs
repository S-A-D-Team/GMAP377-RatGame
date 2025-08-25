using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class SensoryManager : MonoBehaviour
{
    protected List<Contaminable> contaminables;
    protected Dictionary<Contaminable, Color> originalColors;
    [SerializeField]
    protected SenseOfSmell sos;
    protected List<SenseOfHearing> listeners;

    // Start is called before the first frame update
    void Awake()
    {
        contaminables = new List<Contaminable>();
        originalColors = new Dictionary<Contaminable, Color>();
        listeners = new List<SenseOfHearing>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddToContaminablesSensed(Collider collider)
    {
        Debug.Log("Something Sensed");
        if (collider.GetComponent<Contaminable>() != null)
        {
            //Debug.Log("Contaminable Sensed");
            /*
            for (int i = 0; i < collider.gameObject.GetComponent<MeshRenderer>().materials[0].shader.GetPropertyCount(); i++)
            {
                Debug.Log("Property " + i + ": " + collider.gameObject.GetComponent<MeshRenderer>().materials[0].shader.GetPropertyName(i));
            }
            */
            //This list is more for if we want to do something with what is sensed later.
            contaminables.Add(collider.GetComponent<Contaminable>());
            int potency = (int)collider.GetComponent<Contaminable>().potency;
            //Debug.Log("Potency of object should is " +  potency);
            Color originalColor = collider.gameObject.GetComponent<MeshRenderer>().materials[0].GetColor("_Base_Color_Multiplier");
            //Debug.Log("Original Color: " + originalColor);
            if (!originalColors.ContainsKey(collider.GetComponent<Contaminable>())) 
            {
                originalColors.Add(collider.GetComponent<Contaminable>(), originalColor);
            }
            //For now it'll change the color completely, we can figure out to make it more like a tint or something
            if (potency == 1)
            {
                collider.gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_Base_Color_Multiplier", Color.green);
            }
            else if (potency == 2)
            {
                collider.gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_Base_Color_Multiplier", Color.yellow);
            }
            else if (potency == 3)
            {
                collider.gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_Base_Color_Multiplier", Color.red);
            }
        }
    }

    public void RemoveFromContaminablesSensed(Collider collider)
    {
        if (collider.GetComponent<Contaminable>() != null)
        {

            if (originalColors.ContainsKey(collider.GetComponent<Contaminable>()))
            {
                //Debug.Log("Should be in there");
                Color originalColor = originalColors[collider.GetComponent<Contaminable>()];
                collider.gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_Base_Color_Multiplier", originalColor);
                originalColors.Remove(collider.GetComponent<Contaminable>());
            }

            if (contaminables.Contains(collider.GetComponent<Contaminable>()))
            {
                contaminables.Remove(collider.GetComponent<Contaminable>());
            }
        }
    }

    public void addToListeners(SenseOfHearing listener)
    {
        listeners.Add(listener);
    }

    public void removeFromListeners(SenseOfHearing listener)
    {
        if (listeners.Contains(listener))
        {
           listeners.Remove(listener);
        }
    }

    public void emitSound(GameObject source, Vector3 location, float intensity)
    {
        //Debug.Log("Sensory Manager emit sound");
        Vector3 randomDirection = Random.insideUnitSphere * 100 / intensity;
        Debug.Log("Exact location: " + location + " General location: " + (location + randomDirection));
        Debug.Log("Intensity: " + intensity);
        foreach (var listener in listeners)
        {
            listener.hear(source, location + randomDirection, intensity);
        }
    }

    public void changeSmellRadius(float radius)
    {
        if (sos != null)
        {
            sos.changeRadius(radius);
        }
    }
}
