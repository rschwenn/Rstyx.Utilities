
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic

Imports Rstyx.Utilities

Namespace UI.Binding.Converters
    
    ''' <summary> Converts an Enum value to it's display string using <see cref="Rstyx.Utilities.EnumExtensions.ToDisplayString"/>. </summary>
    <ValueConversion(GetType(System.Enum), GetType(String))>
    Public Class EnumDisplayConverter
        Implements IValueConverter
        
        Public Sub New()
        End Sub
        
        ''' <summary> Converts an Enum value to it's display string using <see cref="Rstyx.Utilities.EnumExtensions.ToDisplayString"/>. </summary>
         ''' <param name="value">      Input value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The display string. On error the input value itself is returned. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Try
                Return CType(value, System.Enum).ToDisplayString()
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
        
        ''' <summary> Not Implemented. </summary>
         ''' <param name="value">      Input value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The input value itself. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            System.Diagnostics.Trace.WriteLine("EnumDisplayConverter[ConvertBack]: This is Not Implemented => return input value!")
            Return value
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
