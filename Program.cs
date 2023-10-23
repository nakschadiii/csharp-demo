using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Completions;
using System.Diagnostics;
using System.ComponentModel;
using CliWrap;
using CliWrap.Buffered;
﻿using TextCopy;

class Program
{
    static async Task Main(string[] args)
    {
        string answer = "";
        string answerFunc = "";

        Dictionary<string, Func<Task>> commands = new Dictionary<string, Func<Task>>
        {
            {"--c", GetOpenAICorr},
            {"--t", GetOpenAITrad},
            {"-c", GetOpenAICorr},
            {"-t", GetOpenAITrad},
            {"create", CreateReactApp}
        };

        await ((args.Length == 0 || !commands.ContainsKey(args[0])) ? commands["--c"] : commands[args[0]])();

        while (answer != "y" && answer != "n") {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Press any key to continue... ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("(y/n) : ");
            Console.ForegroundColor = ConsoleColor.White;

            answer = Console.ReadLine();
            if (answer == "n") {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Bye...");
            } 

            if (answer == "y") {
                break;
            }
        }

        if (answer == "y") {
            while (!commands.ContainsKey(answerFunc)) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Choose a function (--t, --c, create) : ");
                Console.ForegroundColor = ConsoleColor.White;
                answerFunc = Console.ReadLine();
                Console.Clear();
            }
            string[] newArgs = new string[] { answerFunc };
            await Main(newArgs);
        }
    }

    static string GetUserInput(string prompt)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(prompt);
        Console.ForegroundColor = ConsoleColor.White;
        return Console.ReadLine() + "\n";
    }

    static async Task GetOpenAITrad()
    {
        string languageFrom = GetUserInput(">>> From which language : ");
        string input = GetUserInput(">>> Enter your message : ");
        string languageTo = GetUserInput(">>> To which language : ");

        Console.Clear();
        var apiKey = "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";
        var apiModel = "text-davinci-003";

        OpenAIAPI api = new OpenAIAPI(apiKey);
        var completionRequest = new CompletionRequest()
        {
            Prompt = "Translate this message from the language \"" + languageFrom + "\" into the language \"" + languageTo + "\" :\r\n" + input,
            Model = apiModel,
            Temperature = 0.3,
            MaxTokens = 100,
            TopP = 1.0,
            FrequencyPenalty = 0.0,
            PresencePenalty = 0.0,
        };

        Console.Clear();
        var result = await api.Completions.CreateCompletionsAsync(completionRequest);
        var resultText = result.Completions.FirstOrDefault()?.Text.Trim() ?? "";
        
        if (resultText != "") {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("Result : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(resultText);
            ClipboardService.SetText(resultText);
        } else {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Echec de l'opération");
        }
    }

    static async Task GetOpenAICorr()
    {
        string input = GetUserInput(">>> Enter your message : ");

        Console.Clear();
        var apiKey = "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";
        var apiModel = "text-davinci-003";

        OpenAIAPI api = new OpenAIAPI(apiKey);
        var completionRequest = new CompletionRequest()
        {
            Prompt = "Corrige ceci : '" + input + "'",
            Model = apiModel,
            Temperature = 0.3,
            MaxTokens = 100,
            TopP = 1.0,
            FrequencyPenalty = 0.0,
            PresencePenalty = 0.0,
        };

        Console.Clear();
        var result = await api.Completions.CreateCompletionsAsync(completionRequest);
        var resultText = result.Completions.FirstOrDefault()?.Text.Trim() ?? "";
        
        if (resultText != "") {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("Result : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(resultText);
            ClipboardService.SetText(resultText);
        } else {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Echec de l'opération");
        }
    }

    static async Task CreateReactApp()
    {
        string input = GetUserInput(">>> In which directory ? ");

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("Loading...");
        Console.ForegroundColor = ConsoleColor.White;


        var result = await Cli.Wrap("npx")
            .WithArguments(new[] {"create-react-app", input})
            .WithWorkingDirectory("./")
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        var exitCode = result.ExitCode;
        var stdOut = result.StandardOutput;
        var stdErr = result.StandardError;

        Console.Clear();
        Console.WriteLine(stdOut);
        Console.WriteLine(stdErr);

        await Cli.Wrap("code")
            .WithArguments(new[] {input})
            .WithWorkingDirectory("./")
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
    }
}