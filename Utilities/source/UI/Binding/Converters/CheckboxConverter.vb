
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Converts between Boolean property and Checkbox state - and vice versa </summary>
    <ValueConversion(GetType(Boolean), GetType(Nullable(Of Boolean)))>
    Public Class CheckboxConverter
        Implements IValueConverter
        
        Public Sub New()
        End Sub
        
        ''' <summary> Converts a Boolean to a Checkbox state resp. Nullable(Of Boolean). </summary>
         ''' <param name="value">      Boolean value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 Nullable(Of Boolean): True or False. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Return New Nullable(Of Boolean)(CBool(value))
        End Function
        
        ''' <summary> Converts a Checkbox state resp. Nullable(Of Boolean) to a Boolean. </summary>
         ''' <param name="value">      Nullable(Of Boolean) value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 Boolean: True if input value is True, otherwise False. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return value
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
