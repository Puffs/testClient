using System.Net.Sockets;
using System.Text;


// Класс-обработчик клиента
class Client
{
    private const int port = 80;
    private const string host = "192.168.100.6";
    static TcpClient client;
    static NetworkStream stream;

    static void Main(string[] args)
    {

        Console.WriteLine("Введите путь до папки с входными данными");
        string dyrectoryPath = Console.ReadLine(); // Путь до директории с входными данными
        int numberOfFiles = 0; // Количество файлов в директории

        // Получаем количество файлов в директории
        try
        {
            numberOfFiles = Convert.ToInt32(new DirectoryInfo(dyrectoryPath).GetFiles().Length.ToString());
        }
        catch (Exception)
        {
            Console.WriteLine("Директория не найдена");
        }

        int count = 0;
        string[] text = new string[numberOfFiles]; // Массив строк из каждого файла
        int fileNumber = 1; // Номер обрабатываемого файла
        // Записываем содержимое каждого файла в массив
        while (count < numberOfFiles)
        {
            text[count] = File.ReadAllText(dyrectoryPath + fileNumber + ".txt");
            count++;
            fileNumber++;
        }

        // Подключаемся к серверу
        try
        {
            client = new TcpClient();
            client.Connect(host, port);

        }
        catch (Exception)
        {
            Console.WriteLine("Не удалось подключиться к серверу");
        }

        // Отправляем данные на сервер и получаем ответ
        try
        {
            while (true)
            {
                Console.WriteLine();
                int fileNum = 0;
                // Получаем номер файла, который хотим отправить на сервер
                while (true)
                {
                    Console.WriteLine("Введите номер файла от 1 до 15");
                    // Проверяем корректность введенных данных
                    try
                    {
                        fileNum = Convert.ToInt32(Console.ReadLine());
                        if (fileNum > 0 && fileNum <= numberOfFiles)
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Введите корректный номер файла");
                    }

                }
                // Отправляем строку из файла на сервер
                stream = client.GetStream(); // получаем поток
                Console.WriteLine(text[fileNum - 1]);
                byte[] data = Encoding.Unicode.GetBytes(text[fileNum - 1]);
                stream.Write(data, 0, data.Length);

                // Получаем ответ от сервера
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
                Thread.Sleep(100);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        // получение сообщений
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);//вывод сообщения
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Console.ReadLine();
                    Disconnect();
                }
            }

            // отключение от сервера
            static void Disconnect()
            {
                if (stream != null)
                    stream.Close();//отключение потока
                if (client != null)
                    client.Close();//отключение клиента
                Environment.Exit(0); //завершение процесса
            }
        }
    }
}

