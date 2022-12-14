using System.Text.Json;
using EconomyBlog.Settings;
using EconomyBlog.Structures;
using static System.GC;

namespace EconomyBlog.ServerLogic;

public class HttpServer : IDisposable
{
    public ServerStatus Status { get; private set; } = ServerStatus.Stop;
    private ServerSettings _serverSettings;

    private readonly HttpListener _httpListener;

    public HttpServer()
    {
        _serverSettings = new ServerSettings();
        _httpListener = new HttpListener();
    }

    public void Start()
    {
        if (Status is ServerStatus.Start)
        {
            Console.WriteLine("Already launched.");
            return;
        }

        if (File.Exists("./settings.json"))
            _serverSettings = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllBytes("./settings.json"))
                              ?? new ServerSettings();
        _httpListener.Prefixes.Clear();
        _httpListener.Prefixes.Add($"http://localhost:{_serverSettings.Port}/");

        Console.WriteLine("Launching...");
        _httpListener.Start();
        Status = ServerStatus.Start;
        Console.WriteLine("Launched.");

        Listen();
    }

    public void Stop()
    {
        if (Status is ServerStatus.Stop)
        {
            Console.WriteLine("Already stopped.");
            return;
        }

        Console.WriteLine("Stopping...");
        _httpListener.Stop();

        Status = ServerStatus.Stop;
        Console.WriteLine("Stopped.");
    }

    private void Listen()
        => _httpListener.BeginGetContext(ListenerCallback, _httpListener);

    private void ListenerCallback(IAsyncResult result)
    {
        try
        {
            if (!_httpListener.IsListening) return;
            var httpContext = _httpListener.EndGetContext(result);
            var response = ServerResponseProvider.GetResponse(_serverSettings.Path, httpContext);
            response.Close();
            Listen();
        }
        catch (Exception ex)
        {
            if (!_httpListener.IsListening)
                return;
            Console.WriteLine("An error occured. Try to start server again. " + ex.Message);
            Stop();
        }
    }

    public void Dispose()
    {
        Stop();
        SuppressFinalize(this);
    }
}