using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        // Ahora, modificamos el cliente para que use la clase Protocolo
        private TcpClient remoto;
        private NetworkStream flujo;

        public FrmValidador()
        {
            InitializeComponent();
        }
        // Evento que se dispara cuando se carga el formulario
        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                // Intento de conexión al servidor en localhost en el puerto 8080
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
            }
            catch (SocketException ex)
            {
                // Mostrar un mensaje de error si no se puede establecer la conexió
                MessageBox.Show("No se puedo establecer conexión " + ex.Message,
                    "ERROR");
            }
            /*
            finally 
            {
                flujo?.Close();
                remoto?.Close();
            }
            */
            // Deshabilitar paneles y checkboxes al inicio.
            panPlaca.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkDomingo.Enabled = false;
            chkSabado.Enabled = false;
        }
        // Evento que se dispara cuando se hace clic en el botón Iniciar
        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;
            if (usuario == "" || contraseña == "")
            {
                // Mostrar advertencia si no se ingresan usuario o contraseña
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña",
                    "ADVERTENCIA");
                return;
            }
            // Crear un pedido de ingreso con los datos de usuario y contraseña
            Pedido pedido = new Pedido
            {
                Comando = "INGRESO",
                Parametros = new[] { usuario, contraseña }
            };
            // Ejecutar la operación de ingreso
            //Respuesta respuesta = HazOperacion(pedido);
            //modificacion
            Respuesta respuesta = Protocolo.Protocolo.HazOperacion(pedido, flujo);
            if (respuesta == null)
            {
                // Mostrar error si la respuesta es nula
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }
            // Verificar la respuesta y habilitar/deshabilitar paneles según el resultado
            if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
            {
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus();
            }
            else if (respuesta.Estado == "NOK" && respuesta.Mensaje == "ACCESO_NEGADO")
            {
                panPlaca.Enabled = false;
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales",
                    "ERROR");
                txtUsuario.Focus();
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;
            //Se crea el pedido con los el valor de comadno CALCULO y los parametros los ingresados 
            Pedido pedido = new Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { modelo, marca, placa }
            };
            // la respuesta tomara los devuelto con el metodo HazOperacion con el pedido 
            //Respuesta respuesta = HazOperacion(pedido);
            //Modificacion
            Respuesta respuesta = Protocolo.Protocolo.HazOperacion(pedido, flujo);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }
            // Procesar la respuesta
            if (respuesta.Estado == "NOK")
            {
                //Si la respues por parte del servidor es NOK se envia un messageBox
                MessageBox.Show("Error en la solicitud.", "ERROR");
                chkLunes.Checked = false;
                chkMartes.Checked = false;
                chkMiercoles.Checked = false;
                chkJueves.Checked = false;
                chkViernes.Checked = false;
            }
            else
            {
                // si la respuesta es OK se procesa la respuesta 
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("Se recibió: " + respuesta.Mensaje,
                    "INFORMACIÓN");
                byte resultado = Byte.Parse(partes[1]);
                switch (resultado)
                {
                    case 0b00100000:
                        chkLunes.Checked = true;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00010000:
                        chkMartes.Checked = true;
                        chkLunes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00001000:
                        chkMiercoles.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00000100:
                        chkJueves.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00000010:
                        chkViernes.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        break;
                    default:
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                }
            }
        }

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            String mensaje = "hola";

            Pedido pedido = new Pedido
            {
                Comando = "CONTADOR",
                Parametros = new[] { mensaje }
            };

            //Respuesta respuesta = HazOperacion(pedido);
            //modificacion
            Respuesta respuesta = Protocolo.Protocolo.HazOperacion(pedido, flujo);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");

            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("El número de pedidos recibidos en este cliente es " + partes[0],
                    "INFORMACIÓN");
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (flujo != null)
                flujo.Close();
            if (remoto != null)
                if (remoto.Connected)
                    remoto.Close();
        }
    }
}