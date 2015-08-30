//Copyright � 2014 Sony Computer Entertainment America LLC. See License.txt.

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
    public class picoLogOutputEditor : IInitializable, IControlHostClient
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
            IControlHostService controlHostService)
        {
            m_mainForm = mainForm;
            m_contextRegistry = contextRegistry;
			m_settingsService = settingsService;
			m_controlHostService = controlHostService;
        }

        #region IInitialize Interface

        /// <summary>
        /// Initialize component so it is displayed</summary>
        void IInitializable.Initialize()
        {
            // So the GUI will show up since nothing else imports it...

			m_icons = new Icons();

			//picoLogOutputForm form_All = new picoLogOutputForm( "All", m_contextRegistry, m_settingsService, m_controlHostService );
			//m_logForms.Add( form_All );

			m_inputThread = new InputThread( this );

			//picoLogOutputForm2 form_All = new picoLogOutputForm2();
			picoLogOutputForm3 form_All = new picoLogOutputForm3();
			m_logForms.Add( "All", form_All );
			picoLogDataTable data = new picoLogDataTable();
			//form_All.bindData( data.DataTableView );
			form_All.setup( data, m_icons );

			var info =
                    new ControlInfo(
					"All",
					"All" + " - TreeListView",
					StandardControlGroup.CenterPermanent );

			m_controlHostService.RegisterControl(
				form_All,
				info,
				this );
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

		public void addDataItem( DataItem dataItem, string channel )
		{
			lock( this )
			{
				picoLogOutputForm3 form;
				if ( m_logForms.TryGetValue( channel, out form ) )
				{
					//form.addDataItem( dataItem );
					if (form != null && form.IsHandleCreated)
						form.BeginInvoke( new MethodInvoker( () => form.addDataItem( dataItem ) ) );
				}
			}
		}

		//private void addDataItemUnsafe( DataItem dataItem )
		//{
		//	m_progressWindow.SetProgress( nDone, nTotal );
		//}

        private readonly MainForm m_mainForm;
        private readonly IContextRegistry m_contextRegistry;
		private readonly ISettingsService m_settingsService;
		private readonly IControlHostService m_controlHostService;

		//private List<picoLogOutputForm> m_logForms = new List<picoLogOutputForm>();
		private Dictionary<string, picoLogOutputForm3> m_logForms = new Dictionary<string, picoLogOutputForm3>();
		private Icons m_icons;
		private InputThread m_inputThread;
    }
}