using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Protocolo
{
    public class Pedido
    {
        // La clase Pedido tiene dos parametros, el comando del tipo de Pedido y los paramtros del comando
        public string Comando { get; set; }
        public string[] Parametros { get; set; }
        // se Crea el metodo de Procesar en la cual se divide el mensaje en parte por el caracter del espacio 
        public static Pedido Procesar(string mensaje)
        {
            var partes = mensaje.Split(' ');
            return new Pedido
            {
                // la primera parte del mensaje sera  el comadno y se pasa a MAYUSCULAS
                Comando = partes[0].ToUpper(),
                // el resto seran los parametros
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }
    // ****************
    // Creamos la clase Protocolo en la cual se usa las clases de Pedido y Respuesta
    public class Protocolo
    {
        // Creamos el metodo HazOperacion
        public static Respuesta HazOperacion(Pedido pedido, NetworkStream flujo)
        {
            // verificamos si el flujo de red no esta vacio
            if (flujo == null)
            {
                throw new InvalidOperationException("Error en la conexion");
            }
            try
            {
                //Enviamos el pedido al servidor por medio del buffer de tx
                byte[] bufferTx = Encoding.UTF8.GetBytes(
                    pedido.Comando + " " + string.Join(" ", pedido.Parametros));
                // lo enviamos mediante el comando Write donde se idnica el buffer, el byte de inicio y la longitud de lo que se va a enviar
                flujo.Write(bufferTx, 0, bufferTx.Length);
                // se inicializa el buffer de Rx
                byte[] bufferRx = new byte[1024];
                // Leemos lo que envia el servidor y lo guardamos en el buffer de RX
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                // lo decodificamos y lo guardamos en un string 
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                // lo separamos mediante el caracter de espacio
                var partes = mensaje.Split(' ');
                // creamos el objeto de tipo respuesta 
                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException ex)
            {

                throw new InvalidOperationException("Error al intentar transmitir: " + ex.Message);
            }
        }
        //Creamos el metodo ResolverPedido
        // Se tiene 3 parametros de entrada, el Pedido, la direccionCLiente y la listaCliente que guardara los registros del cliente
        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente, Dictionary<string, int> listadoClientes)
        {
            // se inicializa la respuesta  con un NOK y el mensaje 
            Respuesta respuesta = new Respuesta
            { Estado = "NOK", Mensaje = "Comando no reconocido" };

            switch (pedido.Comando)
            { //Se realiza un switch para cada caso que se tiene del pedido
                // en el caso de que el pedio sea INGRESO se validara el usuario y contrasenia
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        // se dara acceso o no aleatoriamente
                        respuesta = new Random().Next(2) == 0
                            ? new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = "ACCESO_CONCEDIDO"
                            }
                            : new Respuesta
                            {
                                Estado = "NOK",
                                Mensaje = "ACCESO_NEGADO"
                            };
                    }
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;
                // en el caso de que sea CALCULO  se guardara datos enviados que son el modelo, marca y placa
                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];
                        //validamos la plca con el metodo validarPlaca
                        if (ValidarPlaca(placa))
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = $"{placa} {indicadorDia}"
                            };
                            ContadorCliente(direccionCliente, listadoClientes);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;
                // si el pedido es CONTADOR varificamos si cliente tiene registro en la lista
                case "CONTADOR":
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = listadoClientes[direccionCliente].ToString()
                        };
                    }
                    else
                    {
                        respuesta.Mensaje = "No hay solicitudes previas";
                    }
                    break;
            }
            // al final retornamos la respuesta que envia el servidor al cliente
            return respuesta;
        }
        //Creamos el metodo ValidarPlaca que validara si la placa tiene 3 letras y 4 digitos
        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }
        // se crea el metodo que se obtiene el byte con el dia en la cual el auto no circula
        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));
            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes
                case 3:
                case 4:
                    return 0b00010000; // Martes
                case 5:
                case 6:
                    return 0b00001000; // Miércoles
                case 7:
                case 8:
                    return 0b00000100; // Jueves
                case 9:
                case 0:
                    return 0b00000010; // Viernes
                default:
                    return 0;
            }
        }
        private static void ContadorCliente(string direccionCliente, Dictionary<string, int> listadoClientes)
        {
            if (listadoClientes.ContainsKey(direccionCliente))
            {
                listadoClientes[direccionCliente]++;
            }
            else
            {
                listadoClientes[direccionCliente] = 1;
            }
        }
    }

}