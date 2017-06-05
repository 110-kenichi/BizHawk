﻿using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.Atari.A7800Hawk
{
	public partial class A7800Hawk : IEmulator 
	{
		public IEmulatorServiceProvider ServiceProvider { get; }

		public ControllerDefinition ControllerDefinition { get; private set; }

		//Maria related variables
		public int cycle;
		public int cpu_cycle;
		public int scanline;

		public void FrameAdvance(IController controller, bool render, bool rendersound)
		{
			_frame++;

			if (controller.IsPressed("Power"))
			{
				// it seems that theMachine.Reset() doesn't clear ram, etc
				// this should leave hsram intact but clear most other things
				HardReset();
			}

			if (_islag)
			{
				_lagcount++;
			}

			scanline = 0;

			// actually execute the frame
			while (scanline < 263)
			{
				maria.Execute(cycle, scanline);
				cycle++;
				cpu_cycle++;


				if (cycle == 454)
				{
					scanline++;
					cycle = 0;
				}
			}


		}

		public int Frame => _frame;

		public string SystemId => "A7800"; 

		public bool DeterministicEmulation { get; set; }

		public void ResetCounters()
		{
			_frame = 0;
			_lagcount = 0;
			_islag = false;
		}

		public CoreComm CoreComm { get; }

		public void Dispose()
		{
			maria = null;
			tia = null;
			m6532 = null;
		}
	}
}
