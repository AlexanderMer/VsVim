﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vim.Extensions;
using Vim.Interpreter;
using Xunit;

namespace Vim.UnitTest
{
    public abstract class AutoCommandRunnerIntegrationTest : VimTestBase
    {
        private readonly IVimGlobalSettings _globalSettings;

        protected AutoCommandRunnerIntegrationTest()
        {
            _globalSettings = Vim.GlobalSettings;
        }

        protected void AddAutoCommand(EventKind eventKind, string pattern, string command)
        {
            var autoCommand = new AutoCommand(
                AutoCommandGroup.Default,
                eventKind,
                command,
                pattern);
            VimData.AutoCommands = VimData.AutoCommands.Concat(new[] { autoCommand }).ToFSharpList();
        }

        public sealed class BufEnterTest : AutoCommandRunnerIntegrationTest
        {
            [Fact]
            public void SimpleMatching()
            {
                AddAutoCommand(EventKind.BufEnter, "*.html", "set ts=14");
                var vimBuffer = CreateVimBufferWithName("foo.html");
                Assert.Equal(14, vimBuffer.LocalSettings.TabStop);
            }
        }

        public sealed class FileTypeTest : AutoCommandRunnerIntegrationTest
        {
            [Fact]
            public void Simple()
            {
                AddAutoCommand(EventKind.FileType, "html", "set ts=14");
                var vimBuffer = CreateVimBufferWithName("foo.html");
                Assert.Equal(14, vimBuffer.LocalSettings.TabStop);
            }

            /// <summary>
            /// This event shouldn't fire on close.  Just on buffer creation
            /// </summary>
            [Fact]
            public void NotOnClose()
            {
                AddAutoCommand(EventKind.FileType, "html", "set ts=14");
                var vimBuffer = CreateVimBufferWithName("foo.html");
                vimBuffer.LocalSettings.TabStop = 8;
                vimBuffer.Close();
                Assert.Equal(8, vimBuffer.LocalSettings.TabStop);
            }
        }

        public sealed class EnabledTest : AutoCommandRunnerIntegrationTest
        {
            [Fact]
            public void NoEventsOnDisable()
            {
                AddAutoCommand(EventKind.BufEnter, "*.html", "set ts=12");
                _globalSettings.AutoCommand = false;
                var vimBuffer = CreateVimBufferWithName("foo.html");
                Assert.Equal(8, vimBuffer.LocalSettings.TabStop);
            }

            [Fact]
            public void NoEventsOnDisableClose()
            {
                _globalSettings.AutoCommand = false;
                AddAutoCommand(EventKind.BufWinLeave, "*.html", "set ts=12");
                var vimBuffer = CreateVimBufferWithName("foo.html");
                vimBuffer.Close();
                Assert.Equal(8, vimBuffer.LocalSettings.TabStop);
            }

            [Fact]
            public void DisableChange()
            {
                _globalSettings.AutoCommand = false;
                AddAutoCommand(EventKind.BufWinLeave, "*.html", "set ts=12");
                var vimBuffer = CreateVimBufferWithName("foo.html");
                _globalSettings.AutoCommand = true;
                vimBuffer.Close();
                Assert.Equal(12, vimBuffer.LocalSettings.TabStop);
            }
        }
    }
}
