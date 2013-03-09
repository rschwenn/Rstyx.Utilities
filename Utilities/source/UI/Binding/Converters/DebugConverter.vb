
Imports System
Imports System.Windows.Data

Namespace UI.Binding.Converters
    
    ''' <summary> Pass through converter with <b>debugging breakpoints</b>. </summary>
    <ValueConversion(GetType(Object), GetType(Object))>
    Public Class DebugConverter
        Implements IValueConverter
        
        Public Sub New()
        End Sub
        
        
        ''' <summary> Passes through the input value. </summary>
         ''' <param name="value">      Object value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 Input Object value. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            System.Diagnostics.Debugger.Break()
            Return value
        End Function
        
        ''' <summary> Passes through the input value. </summary>
         ''' <param name="value">      Object value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 Input Object value. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            System.Diagnostics.Debugger.Break()
            Return value
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
