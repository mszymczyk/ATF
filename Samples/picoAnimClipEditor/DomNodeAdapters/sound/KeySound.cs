//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Adaptation;
using pico.Controls.PropertyEditing;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoAnimClipEditor.DomNodeAdapters
{
	class KeySoundPositionLister : DynamicLongEnumEditorLister
	{
		public string[] GetNames( PropertyEditorControlContext context )
		{
			KeySound keySound = context.LastSelectedObject.As<KeySound>();
			if ( keySound == null )
				// returning an non-empty string is necessary to avaid LongEnumEditorCrash
				//
				return new string[] { "fake" };

			return keySound.GetAvailablePositions();
		}
	}

    /// <summary>
    /// Adapts DomNode to a Key</summary>
	public class KeySound : DomNodeAdapter, AnimClipElementValidationInterface
    {
		/// <summary>
		/// Performs initialization when the adapter is connected to the DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates read only data for animdata
		/// </summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			if ( string.IsNullOrEmpty( SoundBank ) )
			{
				SoundBank = TimelineEditor.LastSoundBankFilename;
			}

			DomNode.AttributeChanged += DomNode_AttributeChanged;
		}

		private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			if ( e.AttributeInfo.Equivalent(Schema.keySoundType.soundBankAttribute) )
			{
				TimelineEditor.LastSoundBankFilename = SoundBank;
			}
		}

		/// <summary>
		/// Gets and sets the sound bank</summary>
		public string SoundBank
		{
			get { return (string)DomNode.GetAttribute( Schema.keySoundType.soundBankAttribute); }
			set { DomNode.SetAttribute( Schema.keySoundType.soundBankAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the sound name</summary>
		public string Sound
		{
			get { return (string)DomNode.GetAttribute( Schema.keySoundType.soundAttribute ); }
			set { DomNode.SetAttribute( Schema.keySoundType.soundAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets whether sound is positional or not</summary>
		public bool Positional
		{
			get { return (bool) DomNode.GetAttribute( Schema.keySoundType.positionalAttribute ); }
			set { DomNode.SetAttribute( Schema.keySoundType.positionalAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the local position on character</summary>
		public string Position
		{
			get { return (string) DomNode.GetAttribute( Schema.keySoundType.positionAttribute ); }
			set { DomNode.SetAttribute( Schema.keySoundType.positionAttribute, value ); }
		}

		public virtual bool CanParentTo( DomNode parent )
		{
			return ValidateImpl( parent, 0 );
		}

		public virtual bool Validate( DomNode parent )
		{
			return ValidateImpl( parent, 1 );
		}

		private bool ValidateImpl( DomNode parent, int validating )
		{
			if ( parent.Type != Schema.trackType.Type )
				return false;

			return true;
		}

		public string[] GetAvailablePositions()
		{
			TimelineContext tc = TimelineEditor.ContextRegistry.GetActiveContext<TimelineContext>();
			//DomNode timelineNode = DomNode.GetRoot();
			Timeline tim = tc.As<Timeline>();
			if ( tim.AnimCategory == "princess" || tim.AnimCategory == "monster" || tim.AnimCategory == "queen" )
			{
				string[] availablePositions = new string[] {
					"leftHand",
					"rightHand",
					"leftFoot",
					"rightFoot",
					"head",
					"pelvis"
				};

				return availablePositions;
			}

			// returning an non-empty string is necessary to avaid LongEnumEditorCrash
			//
			return new string[] { "fake2" };
		}
	}
}




