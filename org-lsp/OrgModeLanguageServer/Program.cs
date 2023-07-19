using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using StreamJsonRpc;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace OrgModeLanguageServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var inputStream = Console.OpenStandardInput();
            var outputStream = Console.OpenStandardOutput();

            var server = new LanguageServer(inputStream, outputStream);
            await server.Run();
        }
    }

    public class LanguageServer
    {
        private readonly JsonRpc _jsonRpc;

        public LanguageServer(Stream input, Stream output)
        {
            _jsonRpc = JsonRpc.Attach(input, output, this);
            _jsonRpc.AddLocalRpcTarget(this);
        }

        public async Task Run()
        {
            _jsonRpc.StartListening();
            await _jsonRpc.Completion;
        }

        [JsonRpcMethod("initialize")]
        public InitializeResult Initialize(InitializeParams initializeParams)
        {
            return new InitializeResult
            {
                Capabilities = new ServerCapabilities
                {
                    TextDocumentSync = new TextDocumentSyncOptions
                    {
                        OpenClose = true,
                        Change = TextDocumentSyncKind.Full
                    }
                }
            };
        }

        [JsonRpcMethod("textDocument/didOpen")]
        public void OnDocumentOpened(DidOpenTextDocumentParams openParams)
        {
            ValidateDocument(openParams.TextDocument.Uri, openParams.TextDocument.Text);
        }

        [JsonRpcMethod("textDocument/didChange")]
        public void OnDocumentChanged(DidChangeTextDocumentParams changeParams)
        {
            var text = changeParams.ContentChanges.FirstOrDefault()?.Text;
            if (text != null)
            {
                ValidateDocument(changeParams.TextDocument.Uri, text);
            }
        }

        private void ValidateDocument(DocumentUri documentUri, string text)
        {
            var systemUri = new Uri(documentUri.ToString());

            var diagnostics = text.Split('\n')
                .Select((line, index) => (line, lineNumber: index + 1))
                .Where(x => x.line.StartsWith("* TODO"))
                .Select(x => new Diagnostic
                {
                    Range = new Range { Start = new Position(x.lineNumber, 0), End = new Position(x.lineNumber, x.line.Length) },
                    Message = "TODO item found",
                    Severity = DiagnosticSeverity.Information,
                    Source = "org-mode-ls"
                })
                .ToArray();

            _jsonRpc.NotifyAsync(
                "textDocument/publishDiagnostics",
                new PublishDiagnosticsParams { Uri = systemUri, Diagnostics = diagnostics });
        }
    }
}
