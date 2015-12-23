using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Applications;

namespace SettingsEditor
{
    /// <summary>
    /// Used for updating PropertyEditor on Undo/Redo</summary>
	public class DocumentEditingContext : EditingContext, IObservableContext, IPropertyEditingContext
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// Subscribes to events for DomNode tree changes and raises Reloaded event.</summary>
        protected override void OnNodeSet()
        {
			//DomNode.AttributeChanged += (sender, e) => ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
			DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += (sender, e) => ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
            DomNode.ChildRemoved += (sender,e)=> ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));

            Reloaded.Raise(this, EventArgs.Empty);
            base.OnNodeSet();
        }

		#region IPropertyEditingContext Members

		/// <summary>
		/// Gets an enumeration of the items with properties</summary>
		public IEnumerable<object> Items
		{
			get
			{
				return new object[] { DomNode };
			}
		}

		/// <summary>
		/// Gets an enumeration of the property descriptors for the items</summary>
		public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
		{
			//get { return GetPropertyDescriptors(); }
			get { return PropertyUtils.GetSharedProperties( Items ); }
		}

		#endregion

        #region IObservableContext Members
        /// <summary>
        /// Event handler for node inserted in DomNode tree.</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;
        /// <summary>
        /// Event handler for node removed from DomNode tree.</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;
        /// <summary>
        /// Event handler for node changed in DomNode tree.</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;
        /// <summary>
        /// Event that is raised when the DomNode tree has been reloaded.</summary>
        public event EventHandler Reloaded;
        #endregion

		#region TransactionContext Members
		/// <summary>
		/// Performs custom actions after a transaction ends</summary>
		protected override void OnEnded()
		{
			base.OnEnded();

			// the caveat here is that when user starts using sliders, this callback will get called
			// many times, every time slider value changes
			// ideally, there should be some mechanism of "merging" operations, this could be implementd similar to
			// what's in HistoryContext
			//
			Document document = this.Cast<Document>();
			document.SaveImpl();
		}

		#endregion

		void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			ItemChanged.Raise( this, new ItemChangedEventArgs<object>( e.DomNode ) );

			Document document = this.Cast<Document>();

			//ZMQHubMessage msg = new ZMQHubMessage( "settings" );
			//if (e.NewValue is bool)
			//{
			//	msg.appendString( "setInt" );
			//	msg.appendString( document.PathRelativeToData );
			//	msg.appendString( e.AttributeInfo.LongName );
			//	msg.appendInt( 1 );
			//	bool bval = (bool) e.NewValue;
			//	msg.appendInt( bval ? 1 : 0 );
			//}
			//else if (e.NewValue is int)
			//{
			//	msg.appendString( "setInt" );
			//	msg.appendString( document.PathRelativeToData );
			//	msg.appendString( e.AttributeInfo.LongName );
			//	msg.appendInt( 1 );
			//	msg.appendInt( (int) e.NewValue );
			//}
			//else if (e.NewValue is float)
			//{
			//	msg.appendString( "setFloat" );
			//	msg.appendString( document.PathRelativeToData );
			//	msg.appendString( e.AttributeInfo.LongName );
			//	msg.appendInt( 1 );
			//	msg.appendFloat( (float) e.NewValue );
			//}
			//else if (e.NewValue is string)
			//{
			//	msg.appendString( "setString" );
			//	msg.appendString( document.PathRelativeToData );
			//	msg.appendString( e.AttributeInfo.LongName );
			//	msg.appendString( (string) e.NewValue );
			//}
			//else if (e.NewValue is float[])
			//{
			//	msg.appendString( "setFloat" );
			//	msg.appendString( document.PathRelativeToData );
			//	msg.appendString( e.AttributeInfo.LongName );
			//	float[] farray = (float[]) e.NewValue;
			//	msg.appendInt( farray.Length );
			//	foreach (float f in farray)
			//		msg.appendFloat( f );
			//}
			//else
			//{
			//	Outputs.WriteLine( OutputMessageType.Error, "Unsupported attribute type!" );
			//	return;
			//}

			//ZMQHubService.send( msg );

			pico.Hub.HubMessage msg = new pico.Hub.HubMessage( "settings" );
			if (e.NewValue is bool)
			{
				msg.appendString( "setInt" );
				msg.appendString( document.PathRelativeToData );
				msg.appendString( e.AttributeInfo.Name );
				msg.appendInt( 1 );
				bool bval = (bool) e.NewValue;
				msg.appendInt( bval ? 1 : 0 );
			}
			else if (e.NewValue is int)
			{
				msg.appendString( "setInt" );
				msg.appendString( document.PathRelativeToData );
				msg.appendString( e.AttributeInfo.Name );
				msg.appendInt( 1 );
				msg.appendInt( (int) e.NewValue );
			}
			else if (e.NewValue is float)
			{
				msg.appendString( "setFloat" );
				msg.appendString( document.PathRelativeToData );
				msg.appendString( e.AttributeInfo.Name );
				msg.appendInt( 1 );
				msg.appendFloat( (float) e.NewValue );
			}
			else if (e.NewValue is string)
			{
				msg.appendString( "setString" );
				msg.appendString( document.PathRelativeToData );
				msg.appendString( e.AttributeInfo.Name );
				msg.appendString( (string) e.NewValue );
			}
			else if (e.NewValue is float[])
			{
				msg.appendString( "setFloat" );
				msg.appendString( document.PathRelativeToData );
				msg.appendString( e.AttributeInfo.Name );
				float[] farray = (float[]) e.NewValue;
				msg.appendInt( farray.Length );
				foreach (float f in farray)
					msg.appendFloat( f );
			}
			else
			{
				Outputs.WriteLine( OutputMessageType.Error, "Unsupported attribute type!" );
				return;
			}

			pico.Hub.HubService.sendS( msg );
		}

		//public SettingsFile ParamFile { get; set; }
    }
}
