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
    public class TrackCharacterControllerAnim : Track
    {
        #region ITrack Members

        /// <summary>
        /// Gets or sets the track name</summary>
        public override string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.trackCharacterControllerAnimType.nameAttribute); }
			set { DomNode.SetAttribute( Schema.trackCharacterControllerAnimType.nameAttribute, value ); }
        }

        #endregion
    }
}



