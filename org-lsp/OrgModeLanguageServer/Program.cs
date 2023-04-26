﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;

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

        [JsonRpcMethod(Methods.InitializeName)]
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

        [JsonRpcMethod(Methods.TextDocumentDidOpenName)]
        public void OnDocumentOpened(DidOpenTextDocumentParams openParams)
        {
            ValidateDocument(openParams.TextDocument.Uri, openParams.TextDocument.Text);
        }

        [JsonRpcMethod(Methods.TextDocumentDidChangeName)]
        public void OnDocumentChanged(DidChangeTextDocumentParams changeParams)
        {
            var text = changeParams.ContentChanges.FirstOrDefault()?.Text;
            if (text != null)
            {
                ValidateDocument(changeParams.TextDocument.Uri, text);
            }
        }

        private void ValidateDocument(Uri documentUri, string text)
        {
            var diagnostics = text.Split('\n')
                .Select((line, index) => (line, lineNumber: index + 1))
                .Where(x => x.line.StartsWith("* TODO"))
                .Select(x => new Diagnostic
                {
                    Range = new Microsoft.VisualStudio.LanguageServer.Protocol.Range { Start = new Position(x.lineNumber, 0), End = new Position(x.lineNumber, x.line.Length) },
                    Message = "TODO item found",
                    Severity = DiagnosticSeverity.Information,
                    Source = "org-mode-ls"
                })
                .ToArray();

            _jsonRpc.NotifyAsync(
                Methods.TextDocumentPublishDiagnosticsName,
                new Microsoft.VisualStudio.LanguageServer.Protocol.PublishDiagnosticsParams { Uri = documentUri, Diagnostics = diagnostics });
        }
    }
}