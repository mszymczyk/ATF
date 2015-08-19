//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Track</summary>
    public class TrackAnimController : Track
    {
        #region ITrack Members

		///// <summary>
		///// Gets or sets the track name</summary>
		//public override string Name
		//{
		//	get { return (string)DomNode.GetAttribute(Schema.trackCameraAnimType.nameAttribute); }
		//	set { DomNode.SetAttribute( Schema.trackCameraAnimType.nameAttribute, value ); }
		//}

		///// <summary>
		///// Creates a new interval</summary>
		///// <returns>New interval</returns>
		//public IInterval CreateInterval()
		//{
		//	return new DomNode(Schema.intervalType.Type).As<IInterval>();
		//}

		///// <summary>
		///// Creates a new key</summary>
		///// <returns>New key</returns>
		//public IKey CreateKey()
		//{
		//	return new DomNode(Schema.keyType.Type).As<IKey>();
		//}

		///// <summary>
		///// Gets the list of all keys in the track</summary>
		//public IList<IKey> Keys
		//{
		//	get { return GetChildList<IKey>(Schema.trackType.keyChild); }
		//}

		/// <summary>
		/// Gets or sets the track name</summary>
		public string SkelFilename
		{
			get { return (string)DomNode.GetAttribute( Schema.trackAnimControllerType.skelFileAttribute); }
			set { DomNode.SetAttribute( Schema.trackAnimControllerType.skelFileAttribute, value ); }
		}

        #endregion
    }
}



