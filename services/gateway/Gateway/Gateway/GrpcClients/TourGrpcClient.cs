using Grpc.Net.Client;
using TourService.Protos;

namespace Gateway.GrpcClients;

public class TourGrpcClient
{
    private readonly GrpcChannel _channel;
    private readonly TourService.Protos.TourService.TourServiceClient _client;

    public TourGrpcClient(string serviceUrl)
    {
        _channel = GrpcChannel.ForAddress(serviceUrl);
        _client = new TourService.Protos.TourService.TourServiceClient(_channel);
    }

    public TourService.Protos.TourService.TourServiceClient Client => _client;
}
