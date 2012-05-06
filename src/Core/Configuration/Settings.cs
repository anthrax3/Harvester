﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using Harvester.Core.Messaging;
using Harvester.Core.Messaging.Parsers;
using Harvester.Core.Processes;

/* Copyright (c) 2012 CBaxter
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 * IN THE SOFTWARE. 
 */

namespace Harvester.Core.Configuration
{
    public static class Settings
    {
        private static readonly IList<Func<IProcessMessages, MessageListener>> ListenerActivators;
        private static readonly IList<Func<IRetrieveProcesses, IParseMessages>> ParserActivators;
        private static readonly IDictionary<SystemEventLevel, ConsoleColor> ForeColors;
        private static readonly IDictionary<SystemEventLevel, ConsoleColor> BackColors;

        static Settings()
        {
            var levelsSection = (LevelsSection)ConfigurationManager.GetSection("levels") ?? new LevelsSection();
            
            ParserActivators = GetParserActivators();

            ListenerActivators = GetListenerActivators();

            ForeColors = new Dictionary<SystemEventLevel, ConsoleColor>
                             {
                                 { SystemEventLevel.Fatal, levelsSection.Fatal.ForeColor },
                                 { SystemEventLevel.Error, levelsSection.Error.ForeColor },
                                 { SystemEventLevel.Warning, levelsSection.Warning.ForeColor },
                                 { SystemEventLevel.Information, levelsSection.Information.ForeColor },
                                 { SystemEventLevel.Debug, levelsSection.Debug.ForeColor },
                                 { SystemEventLevel.Trace, levelsSection.Trace.ForeColor }
                             };

            BackColors = new Dictionary<SystemEventLevel, ConsoleColor>
                             {
                                 { SystemEventLevel.Fatal, levelsSection.Fatal.BackColor },
                                 { SystemEventLevel.Error, levelsSection.Error.BackColor },
                                 { SystemEventLevel.Warning, levelsSection.Warning.BackColor },
                                 { SystemEventLevel.Information, levelsSection.Information.BackColor },
                                 { SystemEventLevel.Debug, levelsSection.Debug.BackColor },
                                 { SystemEventLevel.Trace, levelsSection.Trace.BackColor }
                             };
        }
        
        public static ConsoleColor GetForeColor(SystemEventLevel level)
        {
            return ForeColors[level];
        }

        public static ConsoleColor GetBackeColor(SystemEventLevel level)
        {
            return BackColors[level];
        }

        public static IList<MessageListener> GetListeners(IProcessMessages messageProcessor)
        {
            return ListenerActivators.Select(activator => activator.Invoke(messageProcessor)).ToList();
        }

        private static IList<Func<IProcessMessages, MessageListener>> GetListenerActivators()
        {
            var hasElevatedPrivileges = new WindowsPrincipal(WindowsIdentity.GetCurrent() ?? WindowsIdentity.GetAnonymous()).IsInRole(WindowsBuiltInRole.Administrator);
            var listenersSection = (ListenersSection)ConfigurationManager.GetSection("listeners") ?? new ListenersSection();

            return listenersSection.Listeners
                                   .Cast<ListenerElement>()
                                   .Where(listener => !listener.ElevatedOnly || hasElevatedPrivileges)
                                   .Select(CreateListenerActivator)
                                   .ToList();  
        }

        private static Func<IProcessMessages, MessageListener> CreateListenerActivator(ListenerElement listenerElement)
        {
            return messageProcessor => (MessageListener)Activator.CreateInstance(Type.GetType(listenerElement.TypeName, true), messageProcessor, listenerElement);
        }

        public static IList<IParseMessages> GetParsers(IRetrieveProcesses processRetriever)
        {
            return ParserActivators.Select(activator => activator.Invoke(processRetriever)).ToList();
        }

        private static IList<Func<IRetrieveProcesses, IParseMessages>> GetParserActivators()
        {
            var parsersSection = (ParsersSection)ConfigurationManager.GetSection("parsers") ?? new ParsersSection();

            return parsersSection.Parsers
                                 .Cast<ParserElement>()
                                 .Select(CreateParserActivator)
                                 .ToList();
        }

        private static Func<IRetrieveProcesses, IParseMessages> CreateParserActivator(ParserElement parser)
        {
            return processRetriever => (IParseMessages)Activator.CreateInstance(Type.GetType(parser.TypeName, true), processRetriever, parser);
        }
    }
}
