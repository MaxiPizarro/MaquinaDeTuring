# Simulador de Máquina de Turing en Unity 3D

Este proyecto consiste en una simulación funcional e interactiva de una **Máquina de Turing** diseñada para realizar operaciones aritméticas básicas (Suma y Resta) utilizando lógica unaria. 

El proyecto recrea un prototipo físico basado en **Arduino**, sensores de color y actuadores, llevado a un entorno virtual mediante **Unity** y **C#**.

## Descripción del Proyecto

La máquina opera sobre una cinta finita de 21 espacios (LEDs), utilizando un cabezal móvil que lee y escribe colores para representar datos y estados.

* **Sistema Numérico:** Unario (La cantidad de LEDs encendidos representa el número).
* **Alfabeto:**
    * `0` (Apagado/Vacío)
    * `1` (LED Rojo - Dato)
    * `2` (LED Morado - Separador de argumentos)
* **Hardware Simulado:** Arduino Uno R3, Sensor de Color TCS3200, Motor Paso a Paso, Driver L298N.

## Funcionalidades Principales

1.  **Suma ($A + B$):** Fusiona dos bloques de LEDs eliminando el separador y ajustando el resultado.
2.  **Resta ($A - B$):** Realiza una comparación "ping-pong" borrando pares de LEDs.
3.  **Detección de Negativos:** Si la resta resulta negativa ($B > A$), la máquina entra en un estado de "limpieza" automática, borrando toda la cinta para indicar un resultado nulo/cero.
4.  **Simulación Física:** Movimiento suave del cabezal, tiempos de espera para lectura/escritura y representación fiel de las medidas reales del prototipo.

## Lógica de Estados (Algorithm)

La máquina funciona mediante una Máquina de Estados Finitos (FSM) implementada en C#.

### Tabla de Transiciones: SUMA
*Objetivo: Unir $A$ y $B$ convirtiendo el separador en unidad y borrando el exceso.*

| Estado | Lee | Escribe | Mueve | Acción Lógica |
| :---: | :---: | :---: | :---: | :--- |
| **q0** | S | S | R | Inicio. |
| **q1** | 1 | 1 | R | Avanza sobre el primer número. |
| **q1** | 2 | 1 | R | **Fusión:** Convierte Separador en 1. Pasa a q2. |
| **q2** | 1 | 1 | R | Avanza hasta el final. |
| **q2** | 0 | 0 | L | Encuentra vacío, retrocede. |
| **q3** | 1 | 0 | L | **Ajuste:** Borra el 1 sobrante. |
| **q4** | * | * | L | Retorno al inicio (Halt). |

### Tabla de Transiciones: RESTA
*Objetivo: Borrar un elemento de $B$ por cada elemento de $A$.*

| Estado | Acción Principal |
| :---: | :--- |
| **q0 - q2** | Avanza hasta el final del segundo número ($B$). |
| **q3** | Borra un '1' del final de $B$. Si $B$ está vacío, va a limpieza positiva ($q8$). |
| **q4** | Regresa al separador central. |
| **q5** | Cruza a la izquierda y busca un '1' en $A$. **Nota:** Si llega al inicio sin encontrar nada, detecta **Negativo** y salta a $q9$. |
| **q6** | Borra el '1' encontrado en $A$ y reinicia el ciclo. |
| **q8** | **Limpieza (+):** Borra el separador sobrante y regresa al inicio. |
| **q9** | **Limpieza (-):** Recorre toda la cinta hacia la derecha borrando todo (Panic Mode). |

##  Especificaciones Técnicas (Modelo 3D)

El modelo en Unity respeta las dimensiones del diseño físico original:

* **Base MDF:** $30 \text{ cm} \times 20 \text{ cm} \times 0.5 \text{ cm}$.
* **Riel (Cinta):** $25 \text{ cm}$ de largo.
* **Carrito + Cabezal:** $4 \times 4 \text{ cm}$ con sensor de $3 \times 2 \text{ cm}$.
* **Electrónica:** Arduino Uno R3 y Protoboard de 830 puntos ($16 \times 5 \text{ cm}$).

##  Estructura del Código

* `TuringMachine.cs`: El cerebro. Contiene las tablas de estados (Diccionarios), la corrutina de movimiento y la lógica de control.
* `Cell.cs`: Controla el estado visual y lógico de cada LED (cambio de materiales).
* `BotonFisico.cs`: Interfaz 3D que permite al usuario interactuar con los botones de Suma, Resta, Run y Reset.
* `MouseManager.cs`: Utilidad para manejar la interacción del mouse (Raycasting) con la cámara en modo Ortográfico.

##  Instrucciones de Uso

1.  **Ejecutar:** Dale al botón **Play** en Unity.
2.  **Configurar Cinta:** Haz clic sobre las esferas (LEDs) para encenderlas.
    * 1 Clic = Rojo (1)
    * 2 Clics = Morado (Separador)
    * 3 Clics = Apagado (0)
    * *Ejemplo Suma $2+1$:* `Rojo`, `Rojo`, `Morado`, `Rojo`.
3.  **Seleccionar Operación:** Haz clic en el botón físico **Suma** o **Resta** en la protoboard virtual.
4.  **Iniciar:** Haz clic en el botón **RUN**.
5.  **Reiniciar:** Usa el botón **Reset** para limpiar la cinta y volver al inicio.

---
**Integrantes:**
* Matías Gutiérrez Paz
* Maximiliano Pizarro Bravo


*Proyecto para la asignatura de Fundamentos de la Computación.*
