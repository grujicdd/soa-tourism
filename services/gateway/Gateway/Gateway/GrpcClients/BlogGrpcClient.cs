using BlogService.Protos;
using Grpc.Net.Client;

namespace Gateway.GrpcClients;

public class BlogGrpcClient
{
    private readonly GrpcChannel _channel;
    private readonly BlogService.Protos.BlogService.BlogServiceClient _client;

    public BlogGrpcClient(string serviceUrl)
    {
        _channel = GrpcChannel.ForAddress(serviceUrl);
        _client = new BlogService.Protos.BlogService.BlogServiceClient(_channel);
    }

    public BlogService.Protos.BlogService.BlogServiceClient Client => _client;
}
