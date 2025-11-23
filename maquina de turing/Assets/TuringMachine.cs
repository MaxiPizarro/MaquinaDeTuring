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
        public int mover;    // -1 Izquierda, 1 Derecha, 0 Halt
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

    // --- FUNCIONES PÚBLICAS ---
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

    // --- NÚCLEO ---
    IEnumerator EjecutarProceso(bool esSuma)
    {
        ejecutando = true;
        indiceCabezal = 0;
        estadoActual = 0;

        Debug.Log("🚀 INICIANDO PROCESO...");

        yield return MoverCarritoSuave(0);

        while (ejecutando)
        {
            // 1. Verificar límites globales
            if (indiceCabezal < 0 || indiceCabezal >= cintaLeds.Length)
            {
                // Si estamos limpiando (q9) y nos salimos por la derecha, es un final correcto.
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

                // --- LOGICA ESPECIAL PARA BORDES ---
                // Si estamos en el inicio (0) y la máquina quiere ir a la izquierda (-1)
                if (indiceCabezal == 0 && r.mover == -1)
                {

                    // DETECCIÓN DE RESTA NEGATIVA:
                    // Si estábamos en q5 (buscando 1s en A) y chocamos con el inicio,
                    // significa que A se acabó y B todavía tiene números. ¡Es negativo!
                    if (!esSuma && estadoActual == 5)
                    {
                        Debug.Log("📉 RESTA NEGATIVA DETECTADA. Iniciando limpieza total (q9)...");
                        estadoActual = 9;  // Cambiamos a modo Borrador
                        indiceCabezal += 1; // Rebotamos hacia la derecha para empezar a borrar

                        // Movemos visualmente y forzamos el siguiente ciclo inmediatamente
                        yield return MoverCarritoSuave(indiceCabezal);
                        continue;
                    }

                    // Si no es negativo, es una parada normal en el inicio.
                    Debug.Log("🏠 FIN (Llegada al inicio). Resultado listo.");
                    ejecutando = false;
                    break;
                }
                // -----------------------------------------------

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
        // Permitimos ir uno más allá del largo solo si estamos limpiando para salirnos suavemente
        if (indiceDestino < 0 || indiceDestino > cintaLeds.Length) yield break;

        Vector3 destino;
        if (indiceDestino < cintaLeds.Length)
            destino = cintaLeds[indiceDestino].transform.position;
        else
            // Si se sale por la derecha (fin de limpieza), calculamos una posición imaginaria
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
        // === SUMA (Sin cambios) ===
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

        // === RESTA MEJORADA ===
        AgregarRegla(reglasResta, 0, 0, 0, 1, 1); // q0

        // q1: Buscar separador
        AgregarRegla(reglasResta, 1, 1, 1, 1, 1);
        AgregarRegla(reglasResta, 1, 2, 2, 1, 2);
        AgregarRegla(reglasResta, 1, 0, 0, 0, -1);

        // q2: Ir al final de B
        AgregarRegla(reglasResta, 2, 1, 1, 1, 2);
        AgregarRegla(reglasResta, 2, 0, 0, -1, 3);

        // q3: Restar en B
        AgregarRegla(reglasResta, 3, 1, 0, -1, 4); // Borra 1 en B -> q4
        AgregarRegla(reglasResta, 3, 2, 0, -1, 8); // B vacío -> Limpieza Normal (q8)
        AgregarRegla(reglasResta, 3, 0, 0, -1, 3); // Ignora huecos ya borrados

        // q4: Volver al separador
        AgregarRegla(reglasResta, 4, 1, 1, -1, 4);
        AgregarRegla(reglasResta, 4, 0, 0, -1, 4);
        AgregarRegla(reglasResta, 4, 2, 2, -1, 5); // Cruza Sep -> q5

        // q5: Buscar 1 en A
        AgregarRegla(reglasResta, 5, 0, 0, -1, 5); // Sigue buscando a la izq
        AgregarRegla(reglasResta, 5, 1, 0, 1, 6);  // Encuentra 1 en A, lo borra -> q6
        // (Si llega al indice 0 aquí, el código en "EjecutarProceso" detecta negativo y salta a q9)

        // q6: Reiniciar ciclo
        AgregarRegla(reglasResta, 6, 0, 0, 1, 6);
        AgregarRegla(reglasResta, 6, 2, 2, 1, 2);

        // q8: Limpieza Normal (Resultado Positivo)
        AgregarRegla(reglasResta, 8, 1, 1, -1, 8);
        AgregarRegla(reglasResta, 8, 0, 0, -1, 8);
        // Se detiene al llegar al inicio gracias al fix de borde.

        // === q9: EL BORRADOR (Limpieza Negativa) ===
        // Avanza hacia la derecha (1) borrando todo lo que encuentra
        AgregarRegla(reglasResta, 9, 0, 0, 1, 9); // Si lee 0, deja 0, avanza
        AgregarRegla(reglasResta, 9, 1, 0, 1, 9); // Si lee 1 (Rojo), lo apaga, avanza
        AgregarRegla(reglasResta, 9, 2, 0, 1, 9); // Si lee 2 (Morado), lo apaga, avanza
        // Seguirá así hasta salirse de la cinta por la derecha y el ciclo While lo detendrá.
    }

    void AgregarRegla(Dictionary<string, Regla> tabla, int q, int lee, int escribe, int mueve, int qNext)
    {
        string clave = q + "," + lee;
        Regla r = new Regla { escribir = escribe, mover = mueve, nuevoEstado = qNext };
        tabla[clave] = r;
    }
}