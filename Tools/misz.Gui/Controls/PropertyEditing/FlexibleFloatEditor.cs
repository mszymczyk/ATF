using System;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf;

namespace misz.Gui
{
    /// <summary>
    /// Flexible bounded floats property editor that supplies FlexibleFloatControl instances to embed in complex
    /// property editing controls. These display a slider, textbox and customize button in the GUI.
    /// Based on Sce.Atf.Controls.PropertyEditing.BoundedFloatEditor
    /// </summary>
    public class FlexibleFloatEditor : IPropertyEditor
    {
        /// <summary>
        /// Constructs FlexibleFloatEditor using the given arguments</summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        public FlexibleFloatEditor( float min = 0, float max = 100, float softMin = 0, float softMax = 100, float stepSize = 0, bool checkBox = false )
        {
            if ( min >= max )
                throw new ArgumentOutOfRangeException( "min must be less than max" );
            if ( softMin >= softMax )
                throw new ArgumentOutOfRangeException( "softMin must be less than softMax" );
            if ( stepSize < 0 )
                throw new ArgumentOutOfRangeException( "stepSize must be greather-equal zero" );

            m_min = min;
            m_max = max;
            m_softMin = MathUtil.Max<float>( softMin, m_min );
            m_softMax = MathUtil.Min<float>( softMax, m_max );
            m_stepSize = stepSize > 0 ? stepSize : ( m_softMax - m_softMin ) * 0.01f;
            m_hasCheckBox = checkBox;
        }

        /// <summary>
        /// Gets the editor's minimum value</summary>
        public float Min
        {
            get { return m_min; }
        }

        /// <summary>
        /// Gets the editor's maximum value</summary>
        public float Max
        {
            get { return m_max; }
        }



        #region IPropertyEditor Members

        /// <summary>
        /// Obtains a control to edit a given property. Changes to the selection set
        /// cause this method to be called again (and passed a new 'context'),
        /// unless ICacheablePropertyControl is implemented on the control. For
        /// performance reasons, it is highly recommended that the control implement
        /// the ICacheablePropertyControl interface.</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public virtual Control GetEditingControl( PropertyEditorControlContext context )
        {
            var control = new FlexibleFloatControl( context, m_min, m_max, m_softMin, m_softMax, m_stepSize, m_hasCheckBox );
            SkinService.ApplyActiveSkin( control );
            return control;
        }

        #endregion

        /// <summary>
        /// Control for editing a bounded float</summary>
        protected class FlexibleFloatControl : FlexibleFloatInputControl, ICacheablePropertyControl
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="context">Context for property editing control</param>
            /// <param name="min">Minimum value</param>
            /// <param name="max">Maximum value</param>
            public FlexibleFloatControl( PropertyEditorControlContext context, float min, float max, float softMin, float softMax, float stepSize, bool checkBox )
                : base( softMin, min, max, softMin, softMax, stepSize, checkBox )
            {
                m_context = context;
                PropertyName = context.Descriptor.DisplayName;
                DrawBorder = false;
                DoubleBuffered = true;
                RefreshValue();
            }

            #region ICacheablePropertyControl

            /// <summary>
            /// Gets true iff this control can be used indefinitely, regardless of whether the associated
            /// PropertyEditorControlContext's SelectedObjects property changes, i.e., the selection changes. 
            /// This property must be constant for the life of this control.</summary>
            public virtual bool Cacheable
            {
                get { return true; }
            }

            #endregion

            /// <summary>
            /// Raises the ValueChanged event</summary>
            /// <param name="e">Event args</param>
            protected override void OnValueChanged( EventArgs e )
            {
                if ( !m_refreshing )
                {
                    float[] floats = new float[5];
                    floats[0] = Value;
                    floats[1] = SoftMin;
                    floats[2] = SoftMax;
                    floats[3] = StepSize;
                    floats[4] = CheckBoxEnabled ? 1.0f : 0.0f;
                    m_context.SetValue( floats );
                }

                base.OnValueChanged( e );
            }

            /// <summary>
            /// Refreshes the control</summary>
            public override void Refresh()
            {
                RefreshValue();

                base.Refresh();
            }

            private void RefreshValue()
            {
                try
                {
                    m_refreshing = true;

                    object value = m_context.GetValue();
                    if ( value == null )
                        Enabled = false;
                    else
                    {
                        if ( value is float )
                        {
                            Value = (float) value;
                        }
                        else if ( value is float[] )
                        {
                            float[] floats = value as float[];
                            if ( floats.Length != 5 )
                                throw new ArgumentException( "Expecting 5 floats" );
                            Value = floats[0];
                            // during transition phase to new float control, there will be only one float stored in file, the rest will be 3 zeros
                            //
                            if ( floats[1] == 0 && floats[2] == 0 && floats[3] == 0 && floats[4] == 0 )
                            {
                                // don't set attributes, leave them with default values
                                //
                            }
                            else
                            {
                                SoftMin = floats[1];
                                SoftMax = floats[2];
                                StepSize = floats[3];
                                CheckBoxEnabled = floats[4] != 0;
                            }
                        }

                        Enabled = !m_context.IsReadOnly;
                    }
                }
                finally
                {
                    m_refreshing = false;
                }
            }

            private readonly PropertyEditorControlContext m_context;
            private bool m_refreshing;
        }

        private float m_min;
        private float m_max = 100.0f;
        private float m_softMin;
        private float m_softMax = 100.0f;
        private float m_stepSize = 1.0f;
        private bool m_hasCheckBox = false;
    }
}
