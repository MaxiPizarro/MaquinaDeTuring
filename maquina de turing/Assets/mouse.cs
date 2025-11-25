using UnityEngine;

public class MouseManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("🔨 GOLPEASTE EL OBJETO: " + hit.collider.name);

                Cell celda = hit.collider.GetComponent<Cell>();
                if (celda != null)
                {
                    celda.SendMessage("OnMouseDown");
                }

                BotonFisico boton = hit.collider.GetComponent<BotonFisico>();
                if (boton != null)
                {
                    boton.SendMessage("OnMouseDown");
                }
            }
            else
            {
                Debug.Log("❌ CLIC EN EL AIRE (No tocó ningún Collider)");
            }
        }
    }
}