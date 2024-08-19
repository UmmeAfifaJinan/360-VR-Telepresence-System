// 2023-11-02 by NW

using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TCP_Client : MonoBehaviour
{
    [SerializeField] private TMP_Text display_text, msg_text;
    [SerializeField] private Button button;

    private string TEST_MESSAGE = "";

    private void Display_Text(string s)
    {
        display_text.text += s + "\n";
    }

    private IEnumerator Client_Main()
    {
        IPAddress TCP_HOST_IP = IPAddress.Parse("127.0.0.1");
        int TCP_PORT = 5500;
        int BUFFER_SIZE = 1024;

        Debug.Log("Enter a message to send to the server: ");
        var EOM = "<|EOM|>";
        TEST_MESSAGE += EOM;
        var encoded_message = Encoding.UTF8.GetBytes(TEST_MESSAGE);

        IPEndPoint ip_endpoint = new IPEndPoint(TCP_HOST_IP, TCP_PORT);

        using (Socket client = new Socket(ip_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            string message = "Requesting to connect to server...";
            Debug.Log(message);
            Display_Text(message);

            client.BeginConnect(ip_endpoint, null, null);
            while (!client.Connected)
            {
                yield return null;
            }

            message = "Connected to server.";
            Debug.Log(message);
            Display_Text(message);

            while (true)
            {
                client.BeginSend(encoded_message, 0, encoded_message.Length, SocketFlags.None, null, null);
                message = "Sending message to server...";
                Debug.Log(message);
                Display_Text(message);

                message = $"Socket client sent message: \"{TEST_MESSAGE.Replace(EOM, "")}\"";
                Debug.Log(message);
                Display_Text(message);

                byte[] buffer = new byte[BUFFER_SIZE];
                int received = 0;
                while (received == 0)
                {
                    received = client.Receive(buffer);
                    yield return null;
                }

                string decoded_message = Encoding.UTF8.GetString(buffer, 0, received);
                if (decoded_message == "<|ACK|>")
                {
                    message = new string('-', 20);
                    Debug.Log(message);
                    Display_Text(message);

                    message = $"Received acknowledgement from server: \"{decoded_message}\"";
                    Debug.Log(message);
                    Display_Text(message);

                    message = new string('=', 20);
                    Debug.Log(message);
                    Display_Text(message);

                    break;
                }
            }
            message = "Closing connection...";
            Debug.Log(message);
            Display_Text(message);

            client.Shutdown(SocketShutdown.Both);
        }
    }

    private void Connect_Server()
    {
        Debug.Log("Client");
        TEST_MESSAGE = msg_text.text;
        StartCoroutine(Client_Main());
    }

    private void Start()
    {
        display_text.text = "";
        button.onClick.AddListener(Connect_Server);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Connect_Server();
        }
    }
}
