using UnityEngine;

public class Cell : MonoBehaviour
{
    public int state = 0;

    public Material matOff;
    public Material matOn;
    public Material matSep;

    private Renderer _rend;

    void Awake()
    {
        _rend = GetComponent<Renderer>();
        UpdateColor(); 
    }

    void OnMouseDown()
    {
        Debug.Log("�ME HAS TOCADO! Soy la celda " + gameObject.name); // <--- AGREGA ESTO
        state++;
        if (state > 2) state = 0;
        UpdateColor();
    }

    public void SetState(int newState)
    {
        state = newState;
        UpdateColor();
    }

    public void UpdateColor()
    {
        if (_rend == null) return;

        if (state == 0) _rend.material = matOff;
        else if (state == 1) _rend.material = matOn;
        else if (state == 2) _rend.material = matSep;
    }
}