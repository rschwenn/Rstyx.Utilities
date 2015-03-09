
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Provides WPF Value Converters for Binding that are delivered <b>by static properties</b>. </summary>
    Public NotInheritable Class ConverterProvider
        
        #Region "Constructor"
            
            ''' <summary> Hides the Constructor. </summary>
            Private Sub New
            End Sub
            
        #End Region
        
        #Region "Shared Properties"
            
            ''' <summary> Returns an instance of CheckboxConverter. </summary>
             ''' <returns> CheckboxConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter converts between Boolean property and Checkbox state - and vice versa. </remarks>
            Public Shared ReadOnly Property CheckboxConverter() As IValueConverter
                Get
                    Return New CheckboxConverter()
                End Get
            End Property
            
            ''' <summary> Returns an instance of CultureInfoConverter. </summary>
             ''' <returns> CultureInfoConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter converts a CultureInfo to its NativeName String - and vice versa. </remarks>
            Public Shared ReadOnly Property CultureInfoConverter() As IValueConverter
                Get
                    Return New CultureInfoConverter()
                End Get
            End Property
            
            ''' <summary> Returns an instance of DebugConverter. </summary>
             ''' <returns> DebugConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter does nothing :-). </remarks>
            Public Shared ReadOnly Property DebugConverter() As IValueConverter
                Get
                    Return New DebugConverter()
                End Get
            End Property
            
            ''' <summary> Returns an instance of EnumDisplayConverter. </summary>
             ''' <returns> EnumDisplayConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter converts a given Enum to it's display string. </remarks>
            Public Shared ReadOnly Property EnumDisplayConverter() As IValueConverter
                Get
                    Return New EnumDisplayConverter()
                End Get
            End Property
            
            ''' <summary> Returns an instance of EnumIntConverter. </summary>
             ''' <returns> EnumIntConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter converts between a given Enum and Integer - and vice versa. </remarks>
            Public Shared ReadOnly Property EnumIntConverter() As IValueConverter
                Get
                    Return New EnumIntConverter()
                End Get
            End Property
            
            ''' <summary> Returns an instance of NanToNullConverter. </summary>
             ''' <returns> NanToNullConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter converts between NaN and Null string - and vice versa. </remarks>
            Public Shared ReadOnly Property NanToNullConverter() As IValueConverter
                Get
                    Return New NanToNullConverter()
                End Get
            End Property
            
            ''' <summary> Returns an instance of ScaleConverter. </summary>
             ''' <returns> ScaleConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter scales the input value applying the converter parameter as scale factor. </remarks>
            Public Shared ReadOnly Property ScaleConverter() As IValueConverter
                Get
                    Return New ScaleConverter()
                End Get
            End Property
            
            ''' <summary> Returns an instance of VisibilityToBooleanConverter in mode "TrueIfVisible". </summary>
             ''' <returns> VisibilityToBooleanConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter converts between Boolean property and Visibility state - and vice versa. </remarks>
            Public Shared ReadOnly Property TrueIfVisibleConverter() As IValueConverter
                Get
                    Return New VisibilityToBooleanConverter() With {.Inverted =False , .Not = False}
                End Get
            End Property
            
            ''' <summary> Returns an instance of VisibilityToBooleanConverter in mode "TrueIfVisible". </summary>
             ''' <returns> VisibilityToBooleanConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter converts between Boolean property and Visibility state - and vice versa. </remarks>
            Public Shared ReadOnly Property TrueIfNotVisibleConverter() As IValueConverter
                Get
                    Return New VisibilityToBooleanConverter() With {.Inverted =False , .Not = True}
                End Get
            End Property
            
            ''' <summary> Returns an instance of VisibilityToBooleanConverter in mode "TrueIfVisible". </summary>
             ''' <returns> VisibilityToBooleanConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter converts between Boolean property and Visibility state - and vice versa. </remarks>
            Public Shared ReadOnly Property VisibleIfTrueConverter() As IValueConverter
                Get
                    Return New VisibilityToBooleanConverter() With {.Inverted =True , .Not = False}
                End Get
            End Property
            
            ''' <summary> Returns an instance of VisibilityToBooleanConverter in mode "TrueIfVisible". </summary>
             ''' <returns> VisibilityToBooleanConverter (WPF Binding IValueConverter). </returns>
             ''' <remarks> The returned Converter converts between Boolean property and Visibility state - and vice versa. </remarks>
            Public Shared ReadOnly Property VisibleIfNotTrueConverter() As IValueConverter
                Get
                    Return New VisibilityToBooleanConverter() With {.Inverted =True , .Not = True}
                End Get
            End Property
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
