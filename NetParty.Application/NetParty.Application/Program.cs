﻿#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using CommandLine;
using NetParty.Application.CommandLineOptions;
using NetParty.Application.CredentialsNS;
using NetParty.Application.DependencyInjection;
using Serilog;

#endregion

namespace NetParty.Application
    {
    internal class Program
        {
        private static void Main(string[] args)
            {
            using (var scope = DependencyContainer.Container.BeginLifetimeScope())
                {
                Log.Logger = scope.Resolve<ILogger>();

                var argumentParsingResult = Parser.Default.ParseArguments<StoreCredentialsOptions>(args);
                argumentParsingResult.MapResult(
                    opts => ExecuteConsoleCommand(() =>
                        scope.Resolve<ICredentialsRepository>().StoreAsync(new Credentials(opts.Username, opts.Password)).Wait()),
                    HandleParsingErrors);
                }
            }

        private static ConsoleCommandStatusCode HandleParsingErrors(IEnumerable<Error> errors)
            {
            errors.ToList().ForEach(e => Log.Logger.Error($"Console argument parsing error {e}"));
            return ConsoleCommandStatusCode.Failure;
            }

        private static ConsoleCommandStatusCode ExecuteConsoleCommand(Action consoleCommand)
            {
            try
                {
                consoleCommand.Invoke();
                return ConsoleCommandStatusCode.Success;
                }
            catch (Exception e)
                {
                Log.Logger.Fatal(e, "Error while executing console command");
                return ConsoleCommandStatusCode.Failure;
                }
            }
        }
    }
