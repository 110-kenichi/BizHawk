using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using BizHawk.Client.Common;
using BizHawk.Emulation.Common;

namespace BizHawk.Client.EmuHawk
{
	public abstract class LuaLibraries
	{
		public readonly LuaDocumentation Docs = new LuaDocumentation();

		protected readonly Dictionary<Type, LuaLibraryBase> Libraries = new Dictionary<Type, LuaLibraryBase>();

		public readonly LuaFileList ScriptList = new LuaFileList();

		public abstract GuiLuaLibrary GuiLibrary { get; }

		/// <remarks>HACK: We need a Lua script to be able to restart the core and not restart itself in the process</remarks>
		public bool IsRebootingCore { get; set; }

		public EventWaitHandle LuaWait { get; protected set; }

		public abstract LuaFunctionList RegisteredFunctions { get; }

		public IEnumerable<LuaFile> RunningScripts => ScriptList.Where(lf => lf.Enabled);

		public abstract void CallExitEvent(LuaFile lf);

		public abstract void CallFrameAfterEvent();

		public abstract void CallFrameBeforeEvent();

		public abstract void CallLoadStateEvent(string name);

		public abstract void CallSaveStateEvent(string name);

		public abstract void Close();

		public abstract void EndLuaDrawing();

		public abstract void ExecuteString(string command);

		public abstract void Restart(IEmulatorServiceProvider newServiceProvider);

		public abstract ResumeResult ResumeScript(LuaFile lf);

		public abstract void SpawnAndSetFileThread(string pathToLoad, LuaFile lf);

		public abstract void StartLuaDrawing();

		public abstract void WindowClosed(IntPtr handle);

		public struct ResumeResult
		{
			public bool WaitForFrame;
			public bool Terminated;
		}
	}
}
