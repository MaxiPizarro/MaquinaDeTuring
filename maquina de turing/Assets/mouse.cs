using UnityEngine;

public class MouseManager : MonoBehaviour
{
    void Update()
    {
        // Detectar clic izquierdo (0)
        if (Input.GetMouseButtonDown(0))
        {
            // Lanzar rayo desde la cámara
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // 1. DIAGNÓSTICO: Imprimir qué golpeamos
                Debug.Log("🔨 GOLPEASTE EL OBJETO: " + hit.collider.name);

                // 2. ACCIÓN: Si es una Celda, activarla manualmente
                Cell celda = hit.collider.GetComponent<Cell>();
                if (celda != null)
                {
                    celda.SendMessage("OnMouseDown"); // Forzamos la ejecución
                }

                // 3. ACCIÓN: Si es un Botón, activarlo
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