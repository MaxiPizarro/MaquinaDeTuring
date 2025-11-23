using UnityEngine;

public class BotonFisico : MonoBehaviour
{
    // Las 4 funciones que pediste
    public enum TipoFuncion { Sumar, Restar, Run, Reset }
    public TipoFuncion funcion;

    public TuringMachine maquina;

    // Lógica simple para detectar clic sin depender de Colliders complejos
    // (Nota: Este método es llamado por el MouseManager si lo estás usando,
    //  o por el sistema de física de Unity si los colliders están bien)
    void OnMouseDown()
    {
        Debug.Log("🔘 Botón presionado: " + funcion);
        EjecutarAccion();
    }

    // Separamos la acción para poder llamarla desde otros scripts si hace falta
    public void EjecutarAccion()
    {
        if (maquina == null)
        {
            Debug.LogError("⚠️ ERROR: El botón " + gameObject.name + " no tiene conectada la 'Maquina' en el Inspector.");
            return;
        }

        switch (funcion)
        {
            case TipoFuncion.Sumar:
                maquina.SeleccionarSuma();
                break;
            case TipoFuncion.Restar:
                maquina.SeleccionarResta();
                break;
            case TipoFuncion.Run:
                maquina.BotonRun();
                break;
            case TipoFuncion.Reset:
                maquina.BotonReset();
                break;
        }
    }
}

