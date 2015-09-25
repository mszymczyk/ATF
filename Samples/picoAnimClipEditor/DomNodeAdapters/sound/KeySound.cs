//Copyright � 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoAnimClipEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Key</summary>
	public class KeySound : DomNodeAdapter, AnimClipElementValidationInterface
    {
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
	}
}



