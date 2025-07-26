using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TarskiLogic : MonoBehaviour
{

    [Header("Referencias")]
    public List<GameObject> figures; 
    public List<PredicateUIElement> uiPredicates;

    [Header("Distancias")]
    public float minDistance = 0.1f; 
    public float maxDistance = 1.5f;  

    private Dictionary<string, System.Func<bool>> allPredicates;
    private List<string> activePredicates;


    void Start()
    {
        allPredicates = new Dictionary<string, System.Func<bool>>()
        {
            { "MismoColor", () => MismoColor(0, 1) },
            { "AlFrenteDe", () => AlFrenteDe(0, 1) },
            { "Entre", () => Entre(0, 1, 2) }
        };

        activePredicates = new List<string>() { "MismoColor", "AlFrenteDe", "Entre" };

        UpdateAllPredicatesUI();
    }

    public void OnFigureMoved()
    {
        UpdateAllPredicatesUI();
        CheckLevelCompletion();
    }

    private void UpdateAllPredicatesUI()
    {
        foreach (var uiElement in uiPredicates)
        {
            if (allPredicates.ContainsKey(uiElement.predicateName))
            {
                bool status = allPredicates[uiElement.predicateName].Invoke();
                uiElement.SetStatus(status);
            }
        }
    }

    private void CheckLevelCompletion()
    {
        foreach (var predicateName in activePredicates)
        {
            if (!allPredicates[predicateName].Invoke()) return;
        }
        Debug.Log("¡Nivel completado!");
    }

    private bool EstanCerca(int indexA, int indexB)
    {
        float distance = Vector3.Distance(figures[indexA].transform.position, figures[indexB].transform.position);
        return distance > minDistance && distance < maxDistance;
    }

    private bool MismoColor(int indexA, int indexB)
    {
        return figures[indexA].GetComponent<Renderer>().material.color ==
               figures[indexB].GetComponent<Renderer>().material.color;
    }

    private bool AlFrenteDe(int indexA, int indexB)
    {
        if (!EstanCerca(indexA, indexB)) return false;
        Vector3 dir = figures[indexB].transform.position - figures[indexA].transform.position;
        return Vector3.Dot(figures[indexA].transform.forward, dir.normalized) > 0.7f;
    }

    private bool Entre(int indexA, int indexB, int indexC)
    {
        if (!EstanCerca(indexA, indexB) || !EstanCerca(indexA, indexC)) return false;
        Vector3 a = figures[indexA].transform.position;
        Vector3 b = figures[indexB].transform.position;
        Vector3 c = figures[indexC].transform.position;
        return Vector3.Distance(a, b) + Vector3.Distance(a, c) == Vector3.Distance(b, c);
    }
}