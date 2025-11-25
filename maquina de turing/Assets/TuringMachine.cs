using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TuringMachine : MonoBehaviour
{
    [Header("Componentes Físicos")]
    public Transform carritoTransform;
    public Cell[] cintaLeds;

    [Header("Configuración")]
    public float velocidadMovimiento = 2.0f;
    public float tiempoEsperaPaso = 0.3f;

    private bool ejecutando = false;
    private int indiceCabezal = 0;
    private int estadoActual = 0;
    private bool? operacionSumaSeleccionada = null;

    private struct Regla
    {
        public int escribir;
        public int mover;    
        public int nuevoEstado;
    }

    private Dictionary<string, Regla> reglasSuma = new Dictionary<string, Regla>();
    private Dictionary<string, Regla> reglasResta = new Dictionary<string, Regla>();

    void Start()
    {
        CargarReglas();
        if (cintaLeds.Length > 0 && carritoTransform != null)
        {
            ActualizarPosicionVisual(0);
        }
    }
    public void SeleccionarSuma()
    {
        if (ejecutando) return;
        operacionSumaSeleccionada = true;
        Debug.Log("✅ MODO SUMA ACTIVADO.");
    }

    public void SeleccionarResta()
    {
        if (ejecutando) return;
        operacionSumaSeleccionada = false;
        Debug.Log("✅ MODO RESTA ACTIVADO.");
    }

    public void BotonRun()
    {
        if (ejecutando) return;
        if (operacionSumaSeleccionada == null)
        {
            Debug.LogError("⛔ ERROR: Selecciona SUMA o RESTA primero.");
            return;
        }
        StartCoroutine(EjecutarProceso(operacionSumaSeleccionada.Value));
    }

    public void BotonReset()
    {
        StopAllCoroutines();
        ejecutando = false;
        indiceCabezal = 0;
        estadoActual = 0;
        operacionSumaSeleccionada = null;
        foreach (var c in cintaLeds) c.SetState(0);
        ActualizarPosicionVisual(0);
        Debug.Log("🔄 RESET.");
    }

    IEnumerator EjecutarProceso(bool esSuma)
    {
        ejecutando = true;
        indiceCabezal = 0;
        estadoActual = 0;

        Debug.Log("🚀 INICIANDO PROCESO...");

        yield return MoverCarritoSuave(0);

        while (ejecutando)
        {
            if (indiceCabezal < 0 || indiceCabezal >= cintaLeds.Length)
            {
                if (estadoActual == 9 && indiceCabezal >= cintaLeds.Length)
                {
                    Debug.Log("🧹 LIMPIEZA COMPLETADA. Resultado: 0");
                }
                else
                {
                    Debug.LogError("Error: Cabezal fuera de límites.");
                }
                ejecutando = false;
                break;
            }

            int simboloLeido = cintaLeds[indiceCabezal].state;
            string clave = estadoActual + "," + simboloLeido;
            Dictionary<string, Regla> tablaUsada = esSuma ? reglasSuma : reglasResta;

            if (tablaUsada.ContainsKey(clave))
            {
                Regla r = tablaUsada[clave];

                yield return new WaitForSeconds(tiempoEsperaPaso * 0.5f);
                cintaLeds[indiceCabezal].SetState(r.escribir);

                if (r.mover == 0)
                {
                    Debug.Log("🏁 FIN (Halt por regla).");
                    ejecutando = false;
                    break;
                }

                if (indiceCabezal == 0 && r.mover == -1)
                {

                    if (!esSuma && estadoActual == 5)
                    {
                        Debug.Log("📉 RESTA NEGATIVA DETECTADA. Iniciando limpieza total (q9)...");
                        estadoActual = 9;  
                        indiceCabezal += 1; 

                        yield return MoverCarritoSuave(indiceCabezal);
                        continue;
                    }

                    Debug.Log("🏠 FIN (Llegada al inicio). Resultado listo.");
                    ejecutando = false;
                    break;
                }

                indiceCabezal += r.mover;
                estadoActual = r.nuevoEstado;

                yield return MoverCarritoSuave(indiceCabezal);
            }
            else
            {
                Debug.Log("🛑 HALT (Sin regla para q" + estadoActual + ")");
                ejecutando = false;
            }
        }
    }

    IEnumerator MoverCarritoSuave(int indiceDestino)
    {
        if (carritoTransform == null || cintaLeds == null || cintaLeds.Length == 0) yield break;

        if (indiceDestino < 0 || indiceDestino > cintaLeds.Length) yield break;

        Vector3 destino;
        if (indiceDestino < cintaLeds.Length)
            destino = cintaLeds[indiceDestino].transform.position;
        else
            destino = cintaLeds[cintaLeds.Length - 1].transform.position + (Vector3.right * 0.015f);

        Vector3 posicionFinal = new Vector3(destino.x, carritoTransform.position.y, carritoTransform.position.z);
        Vector3 posicionInicial = carritoTransform.position;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * velocidadMovimiento;
            carritoTransform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);
            yield return null;
        }
        carritoTransform.position = posicionFinal;
    }

    void ActualizarPosicionVisual(int index)
    {
        if (index >= 0 && index < cintaLeds.Length && carritoTransform != null)
        {
            Vector3 destino = cintaLeds[index].transform.position;
            carritoTransform.position = new Vector3(destino.x, carritoTransform.position.y, carritoTransform.position.z);
        }
    }

    void CargarReglas()
    {
        AgregarRegla(reglasSuma, 0, 0, 0, 1, 1);
        AgregarRegla(reglasSuma, 1, 1, 1, 1, 1);
        AgregarRegla(reglasSuma, 1, 2, 1, 1, 2);
        AgregarRegla(reglasSuma, 1, 0, 0, 0, -1);
        AgregarRegla(reglasSuma, 2, 1, 1, 1, 2);
        AgregarRegla(reglasSuma, 2, 0, 0, -1, 3);
        AgregarRegla(reglasSuma, 3, 1, 0, -1, 4);
        AgregarRegla(reglasSuma, 3, 2, 0, -1, 4);
        AgregarRegla(reglasSuma, 4, 1, 1, -1, 4);
        AgregarRegla(reglasSuma, 4, 2, 1, -1, 4);
        AgregarRegla(reglasSuma, 4, 0, 0, 0, -1);

        AgregarRegla(reglasResta, 0, 0, 0, 1, 1); 

        AgregarRegla(reglasResta, 1, 1, 1, 1, 1);
        AgregarRegla(reglasResta, 1, 2, 2, 1, 2);
        AgregarRegla(reglasResta, 1, 0, 0, 0, -1);

        AgregarRegla(reglasResta, 2, 1, 1, 1, 2);
        AgregarRegla(reglasResta, 2, 0, 0, -1, 3);

        AgregarRegla(reglasResta, 3, 1, 0, -1, 4); 
        AgregarRegla(reglasResta, 3, 2, 0, -1, 8); 
        AgregarRegla(reglasResta, 3, 0, 0, -1, 3); 

        AgregarRegla(reglasResta, 4, 1, 1, -1, 4);
        AgregarRegla(reglasResta, 4, 0, 0, -1, 4);
        AgregarRegla(reglasResta, 4, 2, 2, -1, 5); 

    
        AgregarRegla(reglasResta, 5, 0, 0, -1, 5); 
        AgregarRegla(reglasResta, 5, 1, 0, 1, 6);  

        AgregarRegla(reglasResta, 6, 0, 0, 1, 6);
        AgregarRegla(reglasResta, 6, 2, 2, 1, 2);

     
        AgregarRegla(reglasResta, 8, 1, 1, -1, 8);
        AgregarRegla(reglasResta, 8, 0, 0, -1, 8);

        AgregarRegla(reglasResta, 9, 0, 0, 1, 9); 
        AgregarRegla(reglasResta, 9, 1, 0, 1, 9); 
        AgregarRegla(reglasResta, 9, 2, 0, 1, 9); 
    }

    void AgregarRegla(Dictionary<string, Regla> tabla, int q, int lee, int escribe, int mueve, int qNext)
    {
        string clave = q + "," + lee;
        Regla r = new Regla { escribir = escribe, mover = mueve, nuevoEstado = qNext };
        tabla[clave] = r;
    }
}