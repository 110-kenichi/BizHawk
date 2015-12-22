﻿using System;
using System.Linq;
using System.Windows.Forms;

using BizHawk.Client.Common;
using Emu = BizHawk.Emulation.Common;
using BizHawk.Emulation.Common.IEmulatorExtensions;


namespace BizHawk.Client.EmuHawk
{
	public partial class CheatEdit : UserControl
	{
		public Emu.IMemoryDomains MemoryDomains { get; set; }

		public CheatEdit()
		{
			InitializeComponent();
			AddressBox.Nullable = false;
			ValueBox.Nullable = false;
		}

		#region Privates

		private const string HexInd = "0x";

		private Cheat _cheat;
		private bool _loading;
		private bool _editmode;

		private Action _addCallback;
		private Action _editCallback;

		private void CheatEdit_Load(object sender, EventArgs e)
		{
			Restart();
		}

		public void Restart()
		{
			if (MemoryDomains != null) // the designer needs this check
			{
				DomainDropDown.Items.Clear();
				DomainDropDown.Items.AddRange(MemoryDomains
					.Where(d => d.CanPoke())
					.Select(d => d.ToString())
					.ToArray());

				if (MemoryDomains.HasSystemBus)
				{
					DomainDropDown.SelectedItem = MemoryDomains.SystemBus.ToString();
				}
				else
				{
					DomainDropDown.SelectedItem = MemoryDomains.MainMemory.ToString();
				}
			}

			SetFormToDefault();
		}

		private void SetFormToCheat()
		{
			_loading = true;
			SetSizeSelected(_cheat.Size);
			PopulateTypeDropdown();
			SetTypeSelected(_cheat.Type);
			SetDomainSelected(_cheat.Domain);

			AddressBox.SetHexProperties(_cheat.Domain.Size);

			ValueBox.ByteSize =
				CompareBox.ByteSize =
				_cheat.Size;

			ValueBox.Type =
				CompareBox.Type =
				_cheat.Type;

			ValueHexIndLabel.Text =
				CompareHexIndLabel.Text =
				_cheat.Type == DisplayType.Hex ? HexInd : string.Empty;

			BigEndianCheckBox.Checked = _cheat.BigEndian.Value;

			NameBox.Text = _cheat.Name;
			AddressBox.Text = _cheat.AddressStr;
			ValueBox.Text = _cheat.ValueStr;
			CompareBox.Text = _cheat.Compare.HasValue ? _cheat.CompareStr : String.Empty;

			CheckFormState();
			if (!_cheat.Compare.HasValue)
			{
				CompareBox.Text = String.Empty; // Necessary hack until WatchValueBox.ToRawInt() becomes nullable
			}

			_loading = false;
		}

		private void SetFormToDefault()
		{
			_loading = true;
			SetSizeSelected(WatchSize.Byte);
			PopulateTypeDropdown();

			NameBox.Text = string.Empty;

			if (MemoryDomains != null)
			{
				AddressBox.SetHexProperties(MemoryDomains.SystemBus.Size);
			}

			ValueBox.ByteSize = 
				CompareBox.ByteSize =
				WatchSize.Byte;

			ValueBox.Type = 
				CompareBox.Type =
				DisplayType.Hex;

			ValueBox.ResetText();
			CompareBox.ResetText();

			ValueHexIndLabel.Text =
				CompareHexIndLabel.Text =
				HexInd;

			BigEndianCheckBox.Checked = false;

			SetTypeSelected(DisplayType.Hex);

			CheckFormState();
			CompareBox.Text = string.Empty; // TODO: A needed hack until WatchValueBox.ToRawInt() becomes nullable
			_loading = false;
		}

		private void SetSizeSelected(WatchSize size)
		{
			switch (size)
			{
				default:
				case WatchSize.Byte:
					SizeDropDown.SelectedIndex = 0;
					break;
				case WatchSize.Word:
					SizeDropDown.SelectedIndex = 1;
					break;
				case WatchSize.DWord:
					SizeDropDown.SelectedIndex = 2;
					break;
			}
		}

		private void SetTypeSelected(Common.DisplayType type)
		{
			foreach (var item in DisplayTypeDropDown.Items)
			{
				if (item.ToString() == Watch.DisplayTypeToString(type))
				{
					DisplayTypeDropDown.SelectedItem = item;
					return;
				}
			}
		}

		private void SetDomainSelected(Emu.MemoryDomain domain)
		{
			foreach (var item in DomainDropDown.Items)
			{
				if (item.ToString() == domain.Name)
				{
					DomainDropDown.SelectedItem = item;
					return;
				}
			}
		}

		private void PopulateTypeDropdown()
		{
			DisplayTypeDropDown.Items.Clear();
			switch (SizeDropDown.SelectedIndex)
			{
				default:
				case 0:
					foreach(DisplayType t in ByteWatch.ValidTypes)
					{
						DisplayTypeDropDown.Items.Add(Watch.DisplayTypeToString(t));
					}
					break;
				case 1:
					foreach (DisplayType t in WordWatch.ValidTypes)
					{
						DisplayTypeDropDown.Items.Add(Watch.DisplayTypeToString(t));
					}
					break;
				case 2:
					foreach (DisplayType t in DWordWatch.ValidTypes)
					{
						DisplayTypeDropDown.Items.Add(Watch.DisplayTypeToString(t));
					}
					break;
			}

			DisplayTypeDropDown.SelectedItem = DisplayTypeDropDown.Items[0];
		}

		private void CheckFormState()
		{
			var valid = !String.IsNullOrWhiteSpace(AddressBox.Text) && !String.IsNullOrWhiteSpace(ValueBox.Text);
			AddButton.Enabled = valid;
			EditButton.Enabled = _editmode && valid;
		}

		private void SizeDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_loading)
			{
				PopulateTypeDropdown();

				ValueBox.ByteSize = 
					CompareBox.ByteSize =
					GetCurrentSize();
			}
		}

		private void DomainDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_loading)
			{
				var domain = MemoryDomains[DomainDropDown.SelectedItem.ToString()];
				AddressBox.SetHexProperties(domain.Size);
			}
		}

		private WatchSize GetCurrentSize()
		{
			switch (SizeDropDown.SelectedIndex)
			{
				default:
				case 0:
					return WatchSize.Byte;
				case 1:
					return WatchSize.Word;
				case 2:
					return WatchSize.DWord;
			}
		}

		private void DisplayTypeDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValueBox.Type =
				CompareBox.Type =
				Watch.StringToDisplayType(DisplayTypeDropDown.SelectedItem.ToString());
		}

		private void AddButton_Click(object sender, EventArgs e)
		{
			if (_addCallback != null)
			{
				_addCallback();
			}
		}

		private void EditButton_Click(object sender, EventArgs e)
		{
			if (_editCallback != null)
			{
				_editCallback();
			}
		}

		#endregion

		#region API

		public void SetCheat(Cheat cheat)
		{
			_editmode = true;
			_cheat = cheat;
			if (cheat.IsSeparator)
			{
				SetFormToDefault();
			}
			else
			{
				SetFormToCheat();
			}
		}

		public void ClearForm()
		{
			_cheat = Cheat.Separator;
			_editmode = false;
			SetFormToDefault();
		}

		public Cheat OriginalCheat
		{
			get { return _cheat; }
		}

		public Cheat Cheat
		{
			get
			{
				var domain = MemoryDomains[DomainDropDown.SelectedItem.ToString()];
				var address = AddressBox.ToRawInt().Value;
				if (address < domain.Size)
				{
					var watch = Watch.GenerateWatch(
						MemoryDomains[DomainDropDown.SelectedItem.ToString()],
						AddressBox.ToRawInt().Value,
						GetCurrentSize(),
						Watch.StringToDisplayType(DisplayTypeDropDown.SelectedItem.ToString()),
						BigEndianCheckBox.Checked,
                        NameBox.Text
						);

					return new Cheat(
						watch,
						ValueBox.ToRawInt().Value,
						 CompareBox.ToRawInt()
					);
				}
				else
				{
					MessageBox.Show(address.ToString() + " is not a valid address for the domain " + domain.Name, "Index out of range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return Cheat.Separator;
				}
			}
		}

		public void SetAddEvent(Action addCallback)
		{
			_addCallback = addCallback;
		}

		public void SetEditEvent(Action editCallback)
		{
			_editCallback = editCallback;
		}

		#endregion
	}
}
