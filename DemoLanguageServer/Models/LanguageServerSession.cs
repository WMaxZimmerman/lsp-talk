using System.Collections.Concurrent;
using JsonRpc.DynamicProxy.Client;
using JsonRpc.Client;
using JsonRpc.Contracts;
using LanguageServer.VsCode.Contracts.Client;
using DemoLanguageServer.Providers;

namespace DemoLanguageServer.Models;

public class LanguageServerSession
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();

    public LanguageServerSession(JsonRpcClient rpcClient, IJsonRpcContractResolver contractResolver)
    {
        RpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
        var builder = new JsonRpcProxyBuilder {ContractResolver = contractResolver};
        Client = new ClientProxy(builder, rpcClient);
        Documents = new ConcurrentDictionary<Uri, SessionDocument>();
        DiagnosticProvider = new DiagnosticProvider();
    }

    public CancellationToken CancellationToken => cts.Token;

    public JsonRpcClient RpcClient { get; }

    public ClientProxy Client { get; }

    public ConcurrentDictionary<Uri, SessionDocument> Documents { get; }

    public DiagnosticProvider DiagnosticProvider { get; }

    public LanguageServerSettings Settings { get; set; } = new LanguageServerSettings();

    public void StopServer()
    {
        cts.Cancel();
    }

}
