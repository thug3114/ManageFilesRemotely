using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/* Đồ án nhóm 9: Thêm, Xoá, Sửa File từ xa
   Thành viên: Phạm Huy Hoàng - 19521543
               Nguyễn Trung Thực - 19521008
               Trần Quang Huy - 19521637
 */
namespace Client
{
    public partial class Client : Form
    {
        public Client()
        {
            
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            Connect();

        }

        private void btnSend_Click(object sender, EventArgs e)
        {

            Send();
            AddMessage(txbMessage.Text);

        }

        /// <summary>
        /// port 1024 < port < 65535
        /// </summary>
        IPEndPoint IP;
        Socket client;

        void Connect()
        {
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            try
            {
                client.Connect(IP);
            }
            catch
            {
                MessageBox.Show("Khong the ket noi", "Thong bao");
                return;
            }

            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }

        void Close()
        {
            client.Close();
        }

        void Send()
        {
            if (txbMessage.Text != string.Empty)
            {
                client.Send(Serialize(0 + txbMessage.Text));
            }
        }

        void Mod(int a)
        {
            if (textBox1.Text != string.Empty)
            {
                client.Send(Serialize(a + textBox1.Text));
            } 
            else
            {
                MessageBox.Show("Vui lòng nhập đường dẫn!");
            }
        }
        

        void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5];
                    client.Receive(data);

                    string message = (string)Deserialize(data);

                    AddMessage(message);
                }
            }
            catch
            {
                Close();
            }

        }

        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
            txbMessage.Clear();
        }

        /// <summary>
        /// phân mảnh
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);

            return stream.ToArray();
        }

        /// <summary>
        /// gom mảnh
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
        }

        /// <summary>
        /// đóng kết nối khi đóng form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Mod(1);
            textBox1.Clear();
        }

        private void Client_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Mod(2);
            textBox1.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Mod(3);
            textBox1.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Mod(4);
            textBox1.Clear();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Rename(5);
            textBox1.Clear();
            textBox2.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {          
                Rename(6);
                textBox1.Clear();
                textBox2.Clear();
        }
        void Rename(int a)
        {
            if (textBox1.Text != string.Empty && textBox2.Text != string.Empty)
            {
                client.Send(Serialize(a + "$$$" + textBox1.Text +"$$$" +textBox2.Text));
            }
            else
            {
                MessageBox.Show("Vui lòng điền đầy đủ!");
            }
        }
        
    }
}
