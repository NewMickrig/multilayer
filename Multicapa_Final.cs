using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Multicapa_Final : MonoBehaviour
{
    public GameObject[,] Planos = new GameObject[20, 20];
    public int[,] Almacen = new int[20, 20];
    public float[,] Distancias = new float[20, 20];
    public GameObject Prefab;
    public GameObject Prefab2;
    public GameObject Meta;

    public float xMeta;
    public float zMeta;

    public bool Win;

    public float x_Inicio;
    public float z_Inicio;


    public float Numero_Muestras;
    public float Precision_y;
    public float FactorAprendizaje_y;
    public float Precision_x;
    public float FactorAprendizaje_x;
    public int[] Perceptor = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };

    int Posicionx;
    int Posicionz;

    public int[,] Vista;
    public float[] Pesosx;
    public float[] Pesosy;
    public float[,] Datos_Entrada; //x1
    public List<float> Valores_Deseados_x;
    public List<float> Valores_Desados_y;

    public List<Nodo> Camino;
    public List<Nodo> OpenSet;
    public List<Nodo> ClosedSet;
    // Start is called before the first frame update

    public List<List<float>> Lista_DatosEntrada = new List<List<float>>();

    void Start()
    {
        Construir();
        Win = true;
        Posicionx = (int)transform.localPosition.x;
        Posicionz = (int)transform.localPosition.z;

        x_Inicio= transform.localPosition.x;
        z_Inicio= transform.localPosition.z; ;

        Numero_Muestras = 0;

        xMeta = 16;
        zMeta = 8;

        //                      x,z
        Vista = new int[,] { { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 } };

        Precision_x = 0.01f;
        FactorAprendizaje_x = 0.0001f;


        Precision_y = 0.01f;
        FactorAprendizaje_y = 0.0001f;


        Instantiate(Meta, new Vector3(xMeta, 0, zMeta), Quaternion.Euler(0, 0, 0));
        Valores_Deseados_x = new List<float> { };
        Valores_Desados_y = new List<float> { };

        Pesosx = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
        Pesosy = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

        Camino = new List<Nodo> { };
        OpenSet = new List<Nodo> { };
        ClosedSet = new List<Nodo> { };

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {

            A_Star();
            print("Inicia Entrenamiento");
            Entrenamiento(Pesosx, Valores_Deseados_x, Precision_x, FactorAprendizaje_x);
            Entrenamiento(Pesosy, Valores_Desados_y, Precision_y, FactorAprendizaje_y);
            print(Lista_DatosEntrada[0][0]+"," + Lista_DatosEntrada[0][1] + "," + Lista_DatosEntrada[0][2] + "," + Lista_DatosEntrada[0][3] + "," + Lista_DatosEntrada[0][4] + "," + Lista_DatosEntrada[0][5] + "," + Lista_DatosEntrada[0][6] + "," + Lista_DatosEntrada[0][7] + ",");
            print(Lista_DatosEntrada[1][0]+ "," + Lista_DatosEntrada[1][1] + "," + Lista_DatosEntrada[1][2] + "," + Lista_DatosEntrada[1][3] + "," + Lista_DatosEntrada[1][4] + "," + Lista_DatosEntrada[1][5] + "," + Lista_DatosEntrada[1][6] + "," + Lista_DatosEntrada[1][7] + ",");
            print(Lista_DatosEntrada[2][0] + "," + Lista_DatosEntrada[2][1] + "," + Lista_DatosEntrada[2][2] + "," + Lista_DatosEntrada[2][3] + "," + Lista_DatosEntrada[2][4] + "," + Lista_DatosEntrada[2][5] + "," + Lista_DatosEntrada[2][6] + "," + Lista_DatosEntrada[2][7] + ",");
            StartCoroutine(Movimiento());


        }
    }


    void Entrenamiento(float[] Arreglo_Pesos, List<float> Valores_Desados, float Precision, float Factor_Aprendizaje)
    {
        float error = 1;//Error Salida
        float E_ac = 0;//Error Actual
        float Error_prev = 0; // Error Anterior
        float Ew = 0; //Error Cuadratico
        float E_total = 0;
        float Valor_Salida = 0;
        int Iteraciones = 0;
        while (Math.Abs(error) > Precision)
        {
            Error_prev = Ew;
            for (int i = 0; i < Numero_Muestras; i++)
            {
                Valor_Salida = Calcular_SalidaRed(i, Arreglo_Pesos);
                E_ac = Valores_Desados[i] - Valor_Salida;

                Cambio_Peso(i, Arreglo_Pesos, E_ac, Factor_Aprendizaje);

                E_total = E_total + (float)Math.Pow(E_ac, 2);
            }
            Ew = ((1 / Numero_Muestras) * (E_total));
            error = (Ew - Error_prev);
            Iteraciones += 1;
        }
        print("Iteraciones: " + Iteraciones);
    }

    void Cambio_Peso(int i, float[] Arreglo_Pesos, float E_ac, float Factor_Aprendizaje)
    {
        int Valor_Peso = 0;
        foreach (float Peso in Arreglo_Pesos)
        {
            Arreglo_Pesos[Valor_Peso] = Peso + (Lista_DatosEntrada[i][Valor_Peso] * E_ac * Factor_Aprendizaje);
            Valor_Peso += 1;
        }

        //Cambio de peso = peso1 + (factor de aprendizaje *error * entrada1)
    }


    float Calcular_SalidaRed(int i, float[] Arreglo_Pesos)
    {
        float Salida = 0;
        int ValorEntrada = 0;
        foreach (float Peso in Arreglo_Pesos)
        {
            Salida += Lista_DatosEntrada[i][ValorEntrada] * Peso;
            ValorEntrada += 1;
        }

        return Salida;
    }

    void A_Star()
    {
        Nodo Inicio = new Nodo(Posicionx, Posicionz);
        Nodo Final = new Nodo((int)xMeta, (int)zMeta);

        OpenSet.Add(Inicio);


        while (Win)
        {

            if (OpenSet.Count > 0)
            {
                Nodo MenorPeso = OpenSet[0];
                for (int i = 0; i < OpenSet.Count; i++)
                {
                    if (OpenSet[i].f < MenorPeso.f || OpenSet[i].f == MenorPeso.f && OpenSet[i].h < MenorPeso.h)
                    {
                        MenorPeso = OpenSet[i];
                    }
                }

                Nodo actual = MenorPeso;
                Borrar(OpenSet, actual);
                ClosedSet.Add(actual);
                if (actual.z == Final.z && actual.x == Final.x)
                {

                    Camino.Add(actual);
                    while (actual.padre != null)
                    {
                        actual = actual.padre;
                        Camino.Add(actual);
                    }
                    Camino.Reverse();
                    Crear_Valores_Entrenamiento();
                    Win = false;
                }
                else
                {
                    if (actual.Hijos.Count < 1)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int x = actual.x + Vista[i, 0];
                            int z = actual.z + Vista[i, 1];
                            if ((x < 20 && z < 20) && (z > 0 && x > 0))
                            {
                                Nodo Hijo = new Nodo(x, z);
                                if (Planos[x, z].CompareTag("Obstaculo"))
                                {
                                    Hijo.pared = true;
                                }
                                actual.Hijos.Add(Hijo);
                            }
                        }
                    }
                    foreach (Nodo Vecino in actual.Hijos)
                    {

                        if (Si_esta(Vecino) || Vecino.pared == true)
                        {
                        }
                        else
                        {
                            int MovCosto = actual.g + actual.Heuristica(Vecino);
                            if (MovCosto < Vecino.g || !Si_esta(Vecino))
                            {
                                Vecino.g = MovCosto;
                                Vecino.h = Vecino.Heuristica(Final);
                                Vecino.padre = actual;
                                if (!Si_esta(Vecino))
                                {
                                    OpenSet.Add(Vecino);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void Crear_Valores_Entrenamiento()
    {
        float x = 0;
        float z = 0;
        for (int i = 0; i < Camino.Count; i++)
        {
            List<float> Entradas = new List<float>();
            for (int y = 0; y < 8; y++)
            {
                int x_Pos = (int)transform.localPosition.x + Vista[y, 0];
                int z_Pos = (int)transform.localPosition.z + Vista[y, 1];
                if ((x_Pos < 20 && z_Pos < 20) && (z_Pos > 0 && x_Pos > 0))
                {
                    if (Planos[x_Pos, z_Pos].CompareTag("Obstaculo"))
                    {
                         Entradas.Add(0f);
                    }
                    else
                    {
                        Entradas.Add(1f);
                    }
                    if(zMeta == z_Pos && xMeta == x_Pos)
                    {

                    }
                }
                else
                {
                    Entradas.Add(0f);
                }
            }

            Lista_DatosEntrada.Add(Entradas);
            //print(Entradas[0] + " " + Entradas[1] + " " + Entradas[2] + " " + Entradas[3] + " " + Entradas[4] + " " + Entradas[5] + " " + Entradas[6] + " " + Entradas[7] + " ");

            if(i == 0) 
            {
                x = (float)Camino[i].x;
                z = (float)Camino[i].z;
            }
            else 
            {
                x = (float)Camino[i].x - (float)Camino[i-1].x;
                z = (float)Camino[i].z - (float)Camino[i-1].z;
            }


            Valores_Deseados_x.Add(x);
            Valores_Desados_y.Add(z);

            //Se mueve
            float z_Mov = (float)Camino[i].z;
            float x_Mov = (float)Camino[i].x;
            this.transform.position = new Vector3(x_Mov, 0.5f, z_Mov);
            

            
        }
        transform.position = new Vector3(x_Inicio, 0.5f, z_Inicio);
        print(Lista_DatosEntrada.Count);

        Numero_Muestras = Lista_DatosEntrada.Count -1;

        Lista_DatosEntrada.RemoveAt(0);

        Valores_Desados_y.RemoveAt(0);

        Valores_Deseados_x.RemoveAt(0);
        print("Termine crear valores");

    }
    bool Si_esta(Nodo Vecino)
    {
        for (int i = ClosedSet.Count - 1; i >= 0; i--)
        {
            if (ClosedSet[i].z == Vecino.z && ClosedSet[i].x == Vecino.x)
            {
                return true;
            }
        }
        return false;
    }

    void Borrar(List<Nodo> array, Nodo Actual)
    {
        for (int i = array.Count - 1; i >= 0; i--)
        {
            if (array[i] == Actual)
            {
                array.RemoveAt(i);
            }
        }
    }

    //List<List<float>> Lista_DatosEntrada [1][] = new List<List<float>>();


    


    void Construir()
    {
        bool Forma = true;
        for (int x = 1; x < 20; x++)
        {
            for (int z = 1; z < 20; z++)
            {
                Forma = !Forma;
                if (Forma)
                {
                    Planos[x, z] = Instantiate(Prefab, new Vector3((float)x, 0, (float)z), Quaternion.Euler(0, 0, 0)) as GameObject;
                    Distancias[x, z] = Vector3.Distance(Planos[x, z].transform.localPosition, new Vector3(xMeta, 0, zMeta));

                }
                else
                {
                    Planos[x, z] = Instantiate(Prefab2, new Vector3((float)x, 0, (float)z), Quaternion.Euler(0, 0, 0)) as GameObject;
                    Distancias[x, z] = Vector3.Distance(Planos[x, z].transform.localPosition, new Vector3(xMeta, 0, zMeta));
                }
            }
        }
    }
    IEnumerator Movimiento()
    {
        Win = true;
        while (Win)
        {
            for (int i = 0; i < 8; i++)
            {
                int x = (int)transform.localPosition.x + Vista[i, 0];
                int z = (int)transform.localPosition.z + Vista[i, 1];
                if ((x < 20 && z < 20) && (z > 0 && x > 0))
                {
                    if (Planos[x, z].CompareTag("Obstaculo"))
                    {
                        Perceptor[i] = 0;
                    }
                    else
                    {
                        Perceptor[i] = 1;
                    }
                    if (zMeta == z && xMeta == x)
                    {
                        Win = false;
                    }
                }
                else
                {
                    Perceptor[i] = 0;
                }
                yield return null;
            }
            float Operacion_x = 0;
            float Operacion_z = 0;
            for (int i = 0; i < 8; i++)
            {
                Operacion_x += (float)Perceptor[i] * Pesosx[i];
                Operacion_z += (float)Perceptor[i] * Pesosy[i];
                yield return null;
            }
            //Operacion_x = Operacion_x / 8;
            //Operacion_z = Operacion_z / 8;

            print("X:" + Operacion_x + " Z:" + Operacion_z);

            Operacion_x = (float)Math.Round(Operacion_x);
            Operacion_z = (float)Math.Round(Operacion_z);

            print("X:" + Operacion_x + " Z:" + Operacion_z);
            
            yield return new WaitForSeconds(1f);

            transform.Translate(Operacion_x, 0, Operacion_z);

            if (Win==false)
            {
                this.transform.position = new Vector3(xMeta, 0.5f, zMeta);
            }
        }

    }
}
