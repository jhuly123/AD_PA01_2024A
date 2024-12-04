// *********************************************
// **************Encabezado*********************
// Nombre: Daniel Tapia
// Fecha de entrega: 26/06/2024

// *********************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        // Se declara el objeto TcpListener con el nombre escuchador
        private static TcpListener escuchador;
        // Se declara un diccionario en la cual va a tener como elementos un string relacionado con un int 
        private static Dictionary<string, int> listadoClientes
            = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            // Creamos un try y catch por si hay falla de conexion al cliente
            try
            {
                // Iniciamos con el escucha por parte del servidor en cualquier direccion IP en el puerto 8080
                escuchador = new TcpListener(IPAddress.Any, 8080);
                // Se comienza el Listener
                escuchador.Start();
                // Se muestra en consola que el servidor inicio 
                Console.WriteLine("Servidor inició en el puerto 5000...");

                while (true)
                {
                    // En un bucle infinito en la cual se crea el cliente TCP y sera aceptado por el escuchador
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    // Se imprime el cliente conectado en elpuerto  asignado
                    Console.WriteLine("Cliente conectado, puerto: {0}", cliente.Client.RemoteEndPoint.ToString());
                    // Se crea un hilo para el manejo del cliente en la cual se acepto la conexion  con el metodo ManipuladorCliente
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    // Se inicia el hilo 
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al iniciar el servidor: " +
                    ex.Message);
            }
            finally
            {
                // Si escuchador es difernte de null se para con el stop()
                escuchador?.Stop();
            }
        }

        // se crea un mentodo en la cual se manejara los mensajes enviados por el cliente con un parametro de entrada de tipo object
        private static void ManipuladorCliente(object obj)
        {
            // Se realiza un cast al objeto que se ingresa a clienteTCP
            TcpClient cliente = (TcpClient)obj;
            // Se crea el flujo de red
            NetworkStream flujo = null;
            try
            {
                // indicamos que el flujo creado sera el flujo del cliente
                flujo = cliente.GetStream();
                // Se crea los dos buffers de transimision y de recepcion
                byte[] bufferTx;
                byte[] bufferRx = new byte[1024];
                int bytesRx;
                // Verificamos si hay datos en el flujo de red
                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    // obtnenemos los datos enviados del cliente y lo decodificamos en UTF8
                    string mensajeRx =
                        Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    // realizamos el metodo Procesar de la clase Pedido del proyecto protocolo
                    Pedido pedido = Pedido.Procesar(mensajeRx);
                    // Se imprime en consola el mensaje procesado 
                    Console.WriteLine("Se recibio: " + pedido);
                    // Se crea un string en lacual se guardara la direccion del cliente
                    string direccionCliente =
                        cliente.Client.RemoteEndPoint.ToString();
                    // Al tener la direccion del cliente usamos el metodo ResolverPedido para enviar las respuesta 
                    // usando como parametros lo pedido y la direccion del cleinte

                    //Respuesta respuesta = ResolverPedido(pedido, direccionCliente);
                    //modificacion

                    Respuesta respuesta = Protocolo.Protocolo.ResolverPedido(pedido, direccionCliente, listadoClientes);
                    Console.WriteLine("Se envió: " + respuesta);
                    // Ya teniendo la respuesta se envia al buffer de trasmision lo codificamos usando UTF8
                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    //se envia usando el metodo Write
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }

            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                // Al finalizar se cierra el flujo y al cliente
                flujo?.Close();
                cliente?.Close();
            }
        }

    }
}