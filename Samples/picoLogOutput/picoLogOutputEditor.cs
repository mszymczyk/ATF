//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace pico.LogOutput
{
    /// <summary>
    /// Tree list view editor for hierarchical file system component</summary>
    [Export(typeof(IInitializable))]
    public class picoLogOutputEditor : IInitializable, IControlHostClient, ICommandClient
    {
        /// <summary>
        /// Constructor with parameters. Creates and registers UserControl and adds buttons to it.
        /// Creates a TreeListView to contain tree data.</summary>
        /// <param name="mainForm">Main form</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="settingsService">Settings service</param>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
		public picoLogOutputEditor(
            MainForm mainForm,
            IContextRegistry contextRegistry,
            ISettingsService settingsService,
            IControlHostService controlHostService,
			ICommandService commandService
			)
        {
            m_mainForm = mainForm;
            m_contextRegistry = contextRegistry;
			m_settingsService = settingsService;
			m_controlHostService = controlHostService;
			m_commandService = commandService;
        }

        #region IInitialize Interface

        /// <summary>
        /// Initialize component so it is displayed</summary>
        void IInitializable.Initialize()
        {
            // So the GUI will show up since nothing else imports it...
			m_commandService.RegisterCommand(
			   Command.ClearAll,
			   StandardMenu.View,
			   "LogCommands",
			   "Clear All",
			   "Clears All Outputs",
			   Keys.None,
			   null,
			   CommandVisibility.ApplicationMenu,
			   this );

			m_icons = new Icons();

			m_inputThread = new InputThread( this );
			//m_server = new AsynchronousSocketListener( this );
			//m_server.StartListening();

			//picoLogOutputForm3 form_All = new picoLogOutputForm3();
			//m_logForms.Add( "All", form_All );
			//picoLogDataTable data = new picoLogDataTable();
			//form_All.setup( data, m_icons );

			//var info =
			//		new ControlInfo(
			//		"All",
			//		"All",
			//		StandardControlGroup.CenterPermanent );

			//m_controlHostService.RegisterControl(
			//	form_All,
			//	info,
			//	this );

			_AddNewForm( "All" );
        }

        #endregion

        #region IControlHostClient Interface

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
			//if (ReferenceEquals(control, m_host))
			//	m_contextRegistry.ActiveContext = this;
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        public bool Close(Control control)
        {
            return true;
        }

        #endregion

		#region ICommandClient Members

		/// <summary>
		/// Logger commands
		/// </summary>
		protected enum Command
		{
			ClearAll
		};

		/// <summary>
		/// Can the client do the command?</summary>
		/// <param name="commandTag">Command</param>
		/// <returns>True iff client can do the command</returns>
		public bool CanDoCommand( object commandTag )
		{
			if (!(commandTag is Command))
				return false;

			switch ((Command) commandTag)
			{
				case Command.ClearAll:
					return true;
			}

			return false;
		}

		/// <summary>
		/// Do a command</summary>
		/// <param name="commandTag">Command</param>
		public void DoCommand( object commandTag )
		{
			if (commandTag is Command)
			{
				switch ((Command) commandTag)
				{
					case Command.ClearAll:
						_ClearAll();
						break;
				}
			}
		}

		/// <summary>
		/// Updates command state for given command</summary>
		/// <param name="commandTag">Command</param>
		/// <param name="state">Command state to update</param>
		public void UpdateCommand( object commandTag, Sce.Atf.Applications.CommandState state )
		{
		}

		#endregion

		public void addDataItem( DataItem dataItem, string channel )
		{
			lock( this )
			{
				picoLogOutputForm3 form;
				if ( m_logForms.TryGetValue( channel, out form ) )
				{
				}
				else
				{
					form = _AddNewForm( channel );
				}

				if (form.IsHandleCreated)
					form.BeginInvoke( new MethodInvoker( () => form.addDataItem( dataItem ) ) );
			}
		}

		private void _ClearAll()
		{
			lock (this)
			{
				foreach ( picoLogOutputForm3 form in m_logForms.Values )
				{
					//if (form.IsHandleCreated)
					//	form.BeginInvoke( new MethodInvoker( () => form.clearLog() ) );
					m_controlHostService.UnregisterControl( form );
				}

				m_logForms.Clear();
			}
		}

		private picoLogOutputForm3 _AddNewForm( string channel )
		{
			picoLogOutputForm3 form = new picoLogOutputForm3();
			m_logForms.Add( channel, form );
			//picoLogDataTable data = new picoLogDataTable();
			form.setup( m_icons );

			var info =
                    new ControlInfo(
					"All",
					"All",
					StandardControlGroup.CenterPermanent );

			m_controlHostService.RegisterControl(
				form,
				info,
				this );

			return form;
		}

        private readonly MainForm m_mainForm;
        private readonly IContextRegistry m_contextRegistry;
		private readonly ISettingsService m_settingsService;
		private readonly IControlHostService m_controlHostService;
		private readonly ICommandService m_commandService;

		private Dictionary<string, picoLogOutputForm3> m_logForms = new Dictionary<string, picoLogOutputForm3>();
		private Icons m_icons;
		private InputThread m_inputThread;
		//private AsynchronousSocketListener m_server;
    }
}