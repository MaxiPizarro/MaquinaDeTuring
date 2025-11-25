using UnityEngine;

public class BotonFisico : MonoBehaviour
{
    public enum TipoFuncion { Sumar, Restar, Run, Reset }
    public TipoFuncion funcion;

    public TuringMachine maquina;
    void OnMouseDown()
    {
        Debug.Log("🔘 Botón presionado: " + funcion);
        EjecutarAccion();
    }

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

