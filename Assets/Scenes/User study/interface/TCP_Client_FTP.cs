// created 2023-11-02 by NW
// last edited 2024-06-20 by NW

using System.Net.Sockets;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

public class TCP_Client_FTP : MonoBehaviour
{
    [SerializeField] private TMP_Text display_text;
    [SerializeField] private Button button;

    // static string path = "Assets/Screenshots";
    static string path = "Assets/Recordings";

    private void Display_Text(string s)
    {
        Debug.Log(s);
        display_text.text += s + "\n";
    }

    public IEnumerator Client_Main()
    {
        string TCP_HOST_IP = "127.0.0.1";
        int TCP_PORT = 5500;
        int BUFFER_SIZE = 1024;

        string SEPARATOR = "<SEPARATOR>";
        string message = "";

        message = "Files you can send: ";
        Debug.Log(message);

        string filepath = "";
        long size = 0;
        string filename = "";

        if (Directory.Exists(path))
        {
            string[] file_names = Directory.GetFiles(path);
            foreach (string file_path in file_names)
            {
                if (file_path.Contains("meta"))
                {
                    continue;
                }
                filepath = file_path;
                filename = filepath.Remove(0, path.Length);
                size = new FileInfo(filepath).Length;
                Debug.Log(filename);
            }
        }

        message = ("Requesting to connect to server...");
        Display_Text(message);

        using (TcpClient client = new TcpClient())
        {
            client.BeginConnect(TCP_HOST_IP, TCP_PORT, null, null);
            while (!client.Connected)
            {
                yield return null;
            }

            message = "Connected to server.";
            Display_Text(message);

            message = ("Sending file to server...");
            Display_Text(message);

            string file_string = filepath + SEPARATOR + size;
            var encoded = Encoding.UTF8.GetBytes(file_string);
            client.Client.Send(encoded);
            StreamWriter sWriter = new StreamWriter(client.GetStream());
            sWriter.Flush();
            client.Client.SendFile(filepath);
            message = ("Socket client sent file to server: \"" + filepath + "\"");
            Display_Text(message);

            while (true)
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                int received = 0;
                while (received == 0)
                {
                    received = client.Client.Receive(buffer);
                    yield return null;
                }

                string decoded_message = Encoding.UTF8.GetString(buffer, 0, received);
                if (decoded_message == "<|ACK|>")
                {
                    message = (new string('-', 20));
                    message += ($"Received acknowledgement from server: \"{decoded_message}\"");
                    message = (new string('=', 20));
                    Display_Text(message);
                    break;
                }
            }

            message = ("Closing connection...");
            Display_Text(message);
            client.Client.Shutdown(SocketShutdown.Both);
        }
    }

    private void Connect_Server()
    {
        Debug.Log("Client");
        StartCoroutine(Client_Main());
    }

    private void Start()
    {
        display_text.text = "";
        button.onClick.AddListener(Connect_Server);
    }
}
