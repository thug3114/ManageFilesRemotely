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

namespace Server
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Connect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            foreach (Socket item in clientList)
            {
                Send(item);
            }
            AddMessage(txbMessage.Text);
            txbMessage.Clear();

        }

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        IPEndPoint IP;
        Socket server;
        List<Socket> clientList;

        void Connect()
        {
            clientList = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            server.Bind(IP);

            Thread listen = new Thread(() => {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);

                        Thread receive = new Thread(Receive);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
            });
            listen.IsBackground = true;
            listen.Start();
        }

        void Close()
        {
            server.Close();
        }

        void Send(Socket client)
        {
            if (client != null && txbMessage.Text != string.Empty)
            {
                client.Send(Serialize(txbMessage.Text));
            }
        }

        void Addresponse(Socket client, string s)
        {
            client.Send(Serialize(s));
        }

        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        void Receive(object obj)
        {            
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    
                    byte[] data = new byte[1024 * 5];
                    client.Receive(data);

                    string message = (string)Deserialize(data);
                    string code = message.Substring(0, 1);
                    string path = message.Substring(1, message.Length - 1);
                    
                    // 1: Tạo file
                    if (code == "1")
                    {                                      
                        if (!File.Exists(path))
                        {
                            using (var file = File.Create(path))
                            {                               
                            }
                            AddMessage("Đã tạo thành công file: " + path);                            
                            foreach (Socket item in clientList)                            
                                Addresponse(item,"Đã tạo thành công file " + path);                            
                        }
                        else
                        {
                            // tạo thông báo trên Server
                            AddMessage("File " + path + " đã tồn tại!");
                            // tạo thông báo trên client
                            foreach (Socket item in clientList)                            
                                Addresponse(item, "Tạo thất bại, File " + path + " đã tồn tại");                            
                        }                                           
                    } 
                    // 2: Xoá file
                    else if (code == "2")
                    {                      
                        if (!File.Exists(path))
                        {                            
                            AddMessage("File " + path + " muốn xoá không tồn tại!");
                            foreach (Socket item in clientList)                            
                                Addresponse(item, "File " + path + " muốn xoá không tồn tại!");                                                       
                        }
                        else
                        {
                            File.Delete(path);
                            AddMessage("Đã xoá thành công File " + path + "!");
                            foreach (Socket item in clientList)                            
                                Addresponse(item, "Đã xóa File " + path + "!");                            
                        }
                    }
                    // 3: Tạo thư mục
                    if (code == "3")
                    {
                        
                        if (!Directory.Exists(path)) 
                        {                            
                            Directory.CreateDirectory(path);                                 
                            AddMessage("Đã tạo thành công thư mục " + path);
                            foreach (Socket item in clientList)                            
                                Addresponse(item, "Đã tạo thành công thư mục " + path);                            
                        }
                        else
                        {
                            AddMessage("Thư mục " + path + " đã tồn tại hoặc trùng với file có sẵn!");
                            foreach (Socket item in clientList)
                                Addresponse(item, "Tạo thất bại, Thư mục " + path + " đã tồn tại");
                        }
                    }
                    // 4: Xoá thư mục
                    else if (code == "4")
                    {
                        if (!Directory.Exists(path))
                        {
                            AddMessage("Thư mục " + path + " muốn xoá không tồn tại!");
                            foreach (Socket item in clientList)                            
                                Addresponse(item, "Thư mục " + path + " muốn xoá không tồn tại!");                            
                        }
                        else
                        {
                            System.IO.DirectoryInfo di = new DirectoryInfo(path);

                            foreach (FileInfo file in di.GetFiles())                            
                                file.Delete();                            
                            foreach (DirectoryInfo dir in di.GetDirectories())
                                dir.Delete(true);                          
                            Directory.Delete(path);
                            AddMessage("Đã xoá thư mục " + path + " thành công!");
                            foreach (Socket item in clientList)
                                Addresponse(item, "Đã xóa thư mục " + path + " thành công!");
                            
                        }
                    }
                    
                    //5: Đổi tên file
                    else if (code == "5")
                    {
                        string[] seq = message.Split("$$$");
                        path = seq[1];
                        string newnameF = seq[2];
                        string[] myP = path.Split('/');
                        int a = myP.Count();

                        if (myP[a - 1] == newnameF)
                        {
                            AddMessage("Lỗi: Tên muốn đổi trùng với tên gốc, vui lòng nhập lại");
                            foreach (Socket item in clientList)
                                Addresponse(item, path + " đổi tên không thành công " + newnameF);
                        }
                        else
                        {
                            if (File.Exists(path))
                            {
                                Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(path, newnameF);
                                AddMessage(path + " đã đổi tên thành " + newnameF);
                                foreach (Socket item in clientList)
                                    Addresponse(item, path + " đã đổi tên thành " + newnameF);
                            }
                            else
                            {
                                AddMessage("Không thể đổi tên do " + path + " không tồn tại!");
                                foreach (Socket item in clientList)
                                    Addresponse(item, "Không thể đổi tên do " + path + " không tồn tại!");
                            }
                        }


                    }                    
                    //6: Đổi tên thư mục
                    else if (code == "6")
                    {
                        string[] seq = message.Split("$$$");
                        path = seq[1];
                        string newnameD = seq[2];
                        string[] myP = path.Split('/');
                        int a = myP.Count();

                        if (myP[a-1] == newnameD)
                        {
                            AddMessage("Lỗi: Tên muốn đổi trùng với tên gốc, vui lòng nhập lại");
                            foreach (Socket item in clientList)
                                Addresponse(item, path + " đổi tên không thành công " + newnameD);
                        }
                        else
                        {
                            if (Directory.Exists(path))
                            {
                                Microsoft.VisualBasic.FileIO.FileSystem.RenameDirectory(path, newnameD);
                                AddMessage(path + " đã đổi tên thành " + newnameD);
                                foreach (Socket item in clientList)
                                    Addresponse(item, path + " đã đổi tên thành " + newnameD);
                            }
                            else
                            {
                                AddMessage("Không thể đổi tên do " + path + " không tồn tại!");
                                foreach (Socket item in clientList)
                                    Addresponse(item, "Không thể đổi tên do " + path + " không tồn tại!");
                            }

                        }                        

                    }
                    else if (code == "0")
                    {
                        foreach (Socket item in clientList)
                        {
                            if (item != null && item != client)
                                item.Send(Serialize(message));
                        }
                        AddMessage(message.Substring(1,message.Length-1));
                    }
                }
            }
            catch
            {
                clientList.Remove(client);
                client.Close();
            }

        }

        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
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
    }
}

    

