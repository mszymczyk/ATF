// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "timeline.xsd" "..\Schema.cs" "timeline" "picoTimelineEditor"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace picoTimelineEditor
{
    public static class Schema
    {
        public const string NS = "timeline";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            Initialize((ns,name)=>typeCollection.GetNodeType(ns,name),
                (ns,name)=>typeCollection.GetRootElement(ns,name));
        }

        public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
        {
            Initialize((ns,name)=>typeCollections[ns].GetNodeType(name),
                (ns,name)=>typeCollections[ns].GetRootElement(name));
        }

        private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
        {
            timelineType.Type = getNodeType("timeline", "timelineType");
            timelineType.nameAttribute = timelineType.Type.GetAttributeInfo("name");
            timelineType.fakeAttribute = timelineType.Type.GetAttributeInfo("fake");
            timelineType.groupChild = timelineType.Type.GetChildInfo("group");
            timelineType.markerChild = timelineType.Type.GetChildInfo("marker");
            timelineType.timelineRefChild = timelineType.Type.GetChildInfo("timelineRef");

            groupType.Type = getNodeType("timeline", "groupType");
            groupType.nameAttribute = groupType.Type.GetAttributeInfo("name");
            groupType.expandedAttribute = groupType.Type.GetAttributeInfo("expanded");
            groupType.trackChild = groupType.Type.GetChildInfo("track");

            trackType.Type = getNodeType("timeline", "trackType");
            trackType.nameAttribute = trackType.Type.GetAttributeInfo("name");
            trackType.intervalChild = trackType.Type.GetChildInfo("interval");
            trackType.keyChild = trackType.Type.GetChildInfo("key");

            intervalType.Type = getNodeType("timeline", "intervalType");
            intervalType.startAttribute = intervalType.Type.GetAttributeInfo("start");
            intervalType.descriptionAttribute = intervalType.Type.GetAttributeInfo("description");
            intervalType.nameAttribute = intervalType.Type.GetAttributeInfo("name");
            intervalType.lengthAttribute = intervalType.Type.GetAttributeInfo("length");
            intervalType.colorAttribute = intervalType.Type.GetAttributeInfo("color");

            eventType.Type = getNodeType("timeline", "eventType");
            eventType.startAttribute = eventType.Type.GetAttributeInfo("start");
            eventType.descriptionAttribute = eventType.Type.GetAttributeInfo("description");

            keyType.Type = getNodeType("timeline", "keyType");
            keyType.startAttribute = keyType.Type.GetAttributeInfo("start");
            keyType.descriptionAttribute = keyType.Type.GetAttributeInfo("description");
            keyType.specialEventAttribute = keyType.Type.GetAttributeInfo("specialEvent");

            markerType.Type = getNodeType("timeline", "markerType");
            markerType.startAttribute = markerType.Type.GetAttributeInfo("start");
            markerType.descriptionAttribute = markerType.Type.GetAttributeInfo("description");
            markerType.nameAttribute = markerType.Type.GetAttributeInfo("name");
            markerType.colorAttribute = markerType.Type.GetAttributeInfo("color");

            timelineRefType.Type = getNodeType("timeline", "timelineRefType");
            timelineRefType.nameAttribute = timelineRefType.Type.GetAttributeInfo("name");
            timelineRefType.startAttribute = timelineRefType.Type.GetAttributeInfo("start");
            timelineRefType.descriptionAttribute = timelineRefType.Type.GetAttributeInfo("description");
            timelineRefType.colorAttribute = timelineRefType.Type.GetAttributeInfo("color");
            timelineRefType.refAttribute = timelineRefType.Type.GetAttributeInfo("ref");

            controlPointType.Type = getNodeType("timeline", "controlPointType");
            controlPointType.xAttribute = controlPointType.Type.GetAttributeInfo("x");
            controlPointType.yAttribute = controlPointType.Type.GetAttributeInfo("y");
            controlPointType.tangentInAttribute = controlPointType.Type.GetAttributeInfo("tangentIn");
            controlPointType.tangentInTypeAttribute = controlPointType.Type.GetAttributeInfo("tangentInType");
            controlPointType.tangentOutAttribute = controlPointType.Type.GetAttributeInfo("tangentOut");
            controlPointType.tangentOutTypeAttribute = controlPointType.Type.GetAttributeInfo("tangentOutType");
            controlPointType.brokenTangentsAttribute = controlPointType.Type.GetAttributeInfo("brokenTangents");

            curveType.Type = getNodeType("timeline", "curveType");
            curveType.nameAttribute = curveType.Type.GetAttributeInfo("name");
            curveType.displayNameAttribute = curveType.Type.GetAttributeInfo("displayName");
            curveType.minXAttribute = curveType.Type.GetAttributeInfo("minX");
            curveType.maxXAttribute = curveType.Type.GetAttributeInfo("maxX");
            curveType.minYAttribute = curveType.Type.GetAttributeInfo("minY");
            curveType.maxYAttribute = curveType.Type.GetAttributeInfo("maxY");
            curveType.preInfinityAttribute = curveType.Type.GetAttributeInfo("preInfinity");
            curveType.postInfinityAttribute = curveType.Type.GetAttributeInfo("postInfinity");
            curveType.colorAttribute = curveType.Type.GetAttributeInfo("color");
            curveType.xLabelAttribute = curveType.Type.GetAttributeInfo("xLabel");
            curveType.yLabelAttribute = curveType.Type.GetAttributeInfo("yLabel");
            curveType.controlPointChild = curveType.Type.GetChildInfo("controlPoint");

            luaScriptType.Type = getNodeType("timeline", "luaScriptType");
            luaScriptType.startAttribute = luaScriptType.Type.GetAttributeInfo("start");
            luaScriptType.descriptionAttribute = luaScriptType.Type.GetAttributeInfo("description");
            luaScriptType.specialEventAttribute = luaScriptType.Type.GetAttributeInfo("specialEvent");
            luaScriptType.sourceCodeAttribute = luaScriptType.Type.GetAttributeInfo("sourceCode");

            groupCameraType.Type = getNodeType("timeline", "groupCameraType");
            groupCameraType.nameAttribute = groupCameraType.Type.GetAttributeInfo("name");
            groupCameraType.expandedAttribute = groupCameraType.Type.GetAttributeInfo("expanded");
            groupCameraType.trackChild = groupCameraType.Type.GetChildInfo("track");

            trackGroupCameraType.Type = getNodeType("timeline", "trackGroupCameraType");

            trackCameraAnimType.Type = getNodeType("timeline", "trackCameraAnimType");
            trackCameraAnimType.nameAttribute = trackCameraAnimType.Type.GetAttributeInfo("name");
            trackCameraAnimType.intervalChild = trackCameraAnimType.Type.GetChildInfo("interval");
            trackCameraAnimType.keyChild = trackCameraAnimType.Type.GetChildInfo("key");
            trackCameraAnimType.intervalCameraAnimTypeChild = trackCameraAnimType.Type.GetChildInfo("intervalCameraAnimType");

            intervalCameraAnimType.Type = getNodeType("timeline", "intervalCameraAnimType");
            intervalCameraAnimType.startAttribute = intervalCameraAnimType.Type.GetAttributeInfo("start");
            intervalCameraAnimType.descriptionAttribute = intervalCameraAnimType.Type.GetAttributeInfo("description");
            intervalCameraAnimType.nameAttribute = intervalCameraAnimType.Type.GetAttributeInfo("name");
            intervalCameraAnimType.lengthAttribute = intervalCameraAnimType.Type.GetAttributeInfo("length");
            intervalCameraAnimType.colorAttribute = intervalCameraAnimType.Type.GetAttributeInfo("color");

            intervalCurveType.Type = getNodeType("timeline", "intervalCurveType");
            intervalCurveType.startAttribute = intervalCurveType.Type.GetAttributeInfo("start");
            intervalCurveType.descriptionAttribute = intervalCurveType.Type.GetAttributeInfo("description");
            intervalCurveType.nameAttribute = intervalCurveType.Type.GetAttributeInfo("name");
            intervalCurveType.lengthAttribute = intervalCurveType.Type.GetAttributeInfo("length");
            intervalCurveType.colorAttribute = intervalCurveType.Type.GetAttributeInfo("color");
            intervalCurveType.curveChild = intervalCurveType.Type.GetChildInfo("curve");

            intervalFaderType.Type = getNodeType("timeline", "intervalFaderType");
            intervalFaderType.startAttribute = intervalFaderType.Type.GetAttributeInfo("start");
            intervalFaderType.descriptionAttribute = intervalFaderType.Type.GetAttributeInfo("description");
            intervalFaderType.nameAttribute = intervalFaderType.Type.GetAttributeInfo("name");
            intervalFaderType.lengthAttribute = intervalFaderType.Type.GetAttributeInfo("length");
            intervalFaderType.colorAttribute = intervalFaderType.Type.GetAttributeInfo("color");
            intervalFaderType.curveChild = intervalFaderType.Type.GetChildInfo("curve");

            timelineRootElement = getRootElement(NS, "timeline");
        }

        public static class timelineType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo fakeAttribute;
            public static ChildInfo groupChild;
            public static ChildInfo markerChild;
            public static ChildInfo timelineRefChild;
        }

        public static class groupType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static ChildInfo trackChild;
        }

        public static class trackType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
        }

        public static class intervalType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
        }

        public static class eventType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
        }

        public static class keyType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo specialEventAttribute;
        }

        public static class markerType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo colorAttribute;
        }

        public static class timelineRefType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo refAttribute;
        }

        public static class controlPointType
        {
            public static DomNodeType Type;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo tangentInAttribute;
            public static AttributeInfo tangentInTypeAttribute;
            public static AttributeInfo tangentOutAttribute;
            public static AttributeInfo tangentOutTypeAttribute;
            public static AttributeInfo brokenTangentsAttribute;
        }

        public static class curveType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo displayNameAttribute;
            public static AttributeInfo minXAttribute;
            public static AttributeInfo maxXAttribute;
            public static AttributeInfo minYAttribute;
            public static AttributeInfo maxYAttribute;
            public static AttributeInfo preInfinityAttribute;
            public static AttributeInfo postInfinityAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo xLabelAttribute;
            public static AttributeInfo yLabelAttribute;
            public static ChildInfo controlPointChild;
        }

        public static class luaScriptType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo specialEventAttribute;
            public static AttributeInfo sourceCodeAttribute;
        }

        public static class groupCameraType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static ChildInfo trackChild;
        }

        public static class trackGroupCameraType
        {
            public static DomNodeType Type;
        }

        public static class trackCameraAnimType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
            public static ChildInfo intervalCameraAnimTypeChild;
        }

        public static class intervalCameraAnimType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
        }

        public static class intervalCurveType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static ChildInfo curveChild;
        }

        public static class intervalFaderType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static ChildInfo curveChild;
        }

        public static ChildInfo timelineRootElement;
    }
}
