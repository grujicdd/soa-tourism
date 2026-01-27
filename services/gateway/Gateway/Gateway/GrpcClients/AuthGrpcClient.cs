using AuthService.Protos;
using Grpc.Net.Client;

namespace Gateway.GrpcClients;

public class AuthGrpcClient
{
    private readonly GrpcChannel _channel;
    private readonly AuthService.Protos.AuthService.AuthServiceClient _client;

    public AuthGrpcClient(string serviceUrl)
    {
        _channel = GrpcChannel.ForAddress(serviceUrl);
        _client = new AuthService.Protos.AuthService.AuthServiceClient(_channel);
    }

    public AuthService.Protos.AuthService.AuthServiceClient Client => _client;
}
